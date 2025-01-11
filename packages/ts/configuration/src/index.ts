import type { FlattenKeyOf } from "@repo/type-utils";
type StringRecord = Record<string, unknown>;

export type BaseFlatIssue<TKeys extends string = string> = {
    path: "" | TKeys; // When empty string, it means the issue is related to the whole value
    errors: string[];
};

export type ParseResultSuccess<T> = {
    success: true;
    output: T;
    issues: never;
};

export type ParseResultFailure<T> = {
    success: false;
    output: never;
    issues: BaseFlatIssue<FlattenKeyOf<T, true>>[];
};

export type SchemaParseResult<T> =
    | ParseResultSuccess<T>
    | ParseResultFailure<T>;

export type SchemaParser<T, IsAsync extends boolean = false> = (
    value: unknown,
    runAsync?: IsAsync,
) => IsAsync extends true
    ? Promise<SchemaParseResult<T>>
    : SchemaParseResult<T>;

export type ConfigValidationIssues<TConfig> = BaseFlatIssue<
    FlattenKeyOf<TConfig>
>[];

type BaseOptions<TConfig extends StringRecord, IsAsync extends boolean> = {
    isServerConfig?: boolean;
    isRunningOnServer?: boolean;
    validate: SchemaParser<TConfig, IsAsync>;
    skipValidation?: boolean;
    onValidationFailure?: (issues: ConfigValidationIssues<TConfig>) => void;
    onInvalidAccess?: (key: string) => void;
};

type LooseOptions<TConfig extends StringRecord, IsAsync extends boolean> = {
    runtimeConfig?: unknown;
} & BaseOptions<TConfig, IsAsync>;

type ChangeAllValueToUnknownRecursive<T> = T extends Record<string, unknown>
    ? {
          [K in keyof T]: ChangeAllValueToUnknownRecursive<T[K]>;
      }
    : unknown;

type StrictOptions<TConfig extends StringRecord, IsAsync extends boolean> = {
    runtimeConfigStrict: ChangeAllValueToUnknownRecursive<TConfig>;
} & BaseOptions<TConfig, IsAsync>;

export type CreateConfigOptions<
    TConfig extends StringRecord,
    IsAsync extends boolean = false,
> = LooseOptions<TConfig, IsAsync> | StrictOptions<TConfig, IsAsync>;

type StrictCreateConfigOptions<
    TConfig extends StringRecord,
    IsAsync extends boolean = false,
> = CreateConfigOptions<TConfig, IsAsync>;

const configInternals: unique symbol = Symbol("config_internals");

export type Configuration<TConfig extends StringRecord> = Readonly<TConfig> & {
    [configInternals]: unknown;
};

function defaultFailureHandler(issues: BaseFlatIssue[]) {
    // biome-ignore lint/complexity/noForEach: <explanation>
    issues.forEach((issue) => {
        const key = issue.path || "root";

        console.error(`Configuration issue at '${key}'':`, ...issue.errors);
    });

    throw new Error("Invalid configuration");
}

function defaultInvalidAccessHandler(key: string) {
    throw new Error(
        `Invalid access to key '${key}'. This key is only available on the server.`,
    );
}

function parseConfig<TConfig extends StringRecord>(
    config: unknown,
    parse: SchemaParser<TConfig, false>,
    onValidationFailure?: (issues: BaseFlatIssue[]) => void,
): TConfig {
    let issues: BaseFlatIssue[] = [];

    const result = parse(config, false);

    if (!result.success) {
        issues = [...(result?.issues ?? [])];
    }

    const onFailure =
        (onValidationFailure as unknown as
            | ((p: unknown) => void)
            | undefined) ?? defaultFailureHandler;

    if (issues.length > 0 || !result.success) {
        onFailure(issues);
    }

    return result.output;
}

export function createConfig<TConfig extends StringRecord>(
    options: StrictCreateConfigOptions<TConfig, false>,
): Configuration<TConfig> {
    const o = options as CreateConfigOptions<TConfig, false>;

    let config =
        "runtimeConfigStrict" in o
            ? o.runtimeConfigStrict
            : "runtimeConfig" in o
              ? o.runtimeConfig
              : {};

    const isServerConfig = o.isServerConfig ?? false;
    const isRunningOnServer =
        o.isRunningOnServer ?? typeof window === "undefined";

    function isValidAccess() {
        if (isServerConfig) {
            return isRunningOnServer;
        }

        return true;
    }

    const skipValidation = o.skipValidation ?? false;

    const onInvalidAccess = o.onInvalidAccess ?? defaultInvalidAccessHandler;

    if (!skipValidation) {
        config = parseConfig(
            config,
            o.validate,
            o.onValidationFailure as (issues: BaseFlatIssue[]) => void,
        ) as Configuration<TConfig>;
    }

    const proxy = new Proxy(config as TConfig, {
        get(target, prop, receiver) {
            if (!isValidAccess()) {
                return onInvalidAccess(prop as string);
            }

            if (prop === configInternals) {
                return undefined;
            }

            return Reflect.get(target, prop, receiver);
        },
    });

    return Object.freeze(proxy) as Configuration<TConfig>;
}
