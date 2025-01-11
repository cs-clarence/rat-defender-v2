import {
    type BaseIssue,
    type Config,
    type GenericSchema,
    type GenericSchemaAsync,
    getDotPath,
    safeParseAsync,
} from "valibot";
import type {
    FieldValues,
    ValidateForm,
    PartialValues,
    FormErrors,
    ValidateField,
    FieldValue,
    Maybe,
} from "@repo/solid-forms";

type Schema = GenericSchema | GenericSchemaAsync;
type SchemaSignal = () => Schema;
/**
 * Creates a validation functions that parses the Valibot schema of a form.
 *
 * @param schema A Valibot schema.
 *
 * @returns A validation function.
 */
export function valiForm<TFormValues extends FieldValues>(
    schema: Schema | SchemaSignal,
    config: Config<BaseIssue<unknown>> = {},
): ValidateForm<TFormValues> {
    return async (values: PartialValues<TFormValues>) => {
        const s = schema instanceof Function ? schema() : schema;

        const result = await safeParseAsync(s, values, {
            ...config,
            abortPipeEarly: true,
        });
        const formErrors: Record<string, string> = {};
        if (result.issues) {
            for (const issue of result.issues) {
                // biome-ignore lint/style/noNonNullAssertion: <explanation>
                formErrors[getDotPath(issue)!] = issue.message;
            }
        }
        return formErrors as FormErrors<TFormValues>;
    };
}

/**
 * Creates a validation functions that parses the Valibot schema of a field.
 *
 * @param schema A Valibot schema.
 *
 * @returns A validation function.
 */
export function valiField<TFieldValue extends FieldValue>(
    schema: Schema | SchemaSignal,
    config: Config<BaseIssue<unknown>> = {},
): ValidateField<TFieldValue> {
    return async (value: Maybe<TFieldValue>) => {
        const s = schema instanceof Function ? schema() : schema;

        const result = await safeParseAsync(s, value, {
            abortPipeEarly: true,
            ...config,
        });
        return result.issues?.[0]?.message || "";
    };
}
