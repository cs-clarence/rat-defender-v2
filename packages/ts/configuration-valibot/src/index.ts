import type { BaseIssue, BaseSchema, SafeParseResult } from "valibot";
import type { SchemaParser } from "@repo/configuration";
import * as v from "valibot";

export { safeParse } from "valibot";

function adaptResult<
    TSchema extends BaseSchema<unknown, unknown, BaseIssue<unknown>>,
>(result: SafeParseResult<TSchema>) {
    if (result.success) {
        return {
            success: true,
            output: result.output,
            issues: undefined,
        };
    }
    const flattened = v.flatten(result.issues);
    // biome-ignore lint/complexity/noForEach: <explanation>
    const issues = Object.entries(flattened.nested ?? {}).forEach(
        ([key, value]) => ({
            path: key,
            errors: value ?? [],
        }),
    );

    return {
        success: false,
        output: undefined,
        issues,
    };
}

export function valibotAdapter<
    TSchema extends BaseSchema<unknown, unknown, BaseIssue<unknown>>,
    TAsync extends boolean = false,
>(schema: TSchema): SchemaParser<v.InferOutput<TSchema>, TAsync> {
    return ((value: unknown, isRunAsync?: boolean) => {
        if (isRunAsync) {
            return v.safeParseAsync(schema, value).then(adaptResult);
        }

        return adaptResult(v.safeParse(schema, value));
    }) as unknown as SchemaParser<v.InferOutput<TSchema>, TAsync>;
}
