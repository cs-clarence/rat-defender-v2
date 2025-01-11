import { untrack } from "solid-js";
import type {
    FieldArrayPath,
    FieldPath,
    FieldValues,
    FormErrors,
    FormStore,
    IntersectValues,
    Maybe,
    ResponseData,
} from "../types";
import { getFieldArrayStore } from "./getFieldArrayStore";
import { getFieldStore } from "./getFieldStore";

/**
 * Value type of the error response options.
 */
type ErrorResponseOptions = Partial<{
    shouldActive: boolean;
}>;

/**
 * Sets an error response if a form error was not set at any field or field
 * array.
 *
 * @param form The form of the errors.
 * @param formErrors The form errors.
 * @param options The error options.
 */
export function setErrorResponse<
    TFormValues extends FieldValues,
    TFieldValues extends IntersectValues<TFormValues>,
    TResponseData extends ResponseData,
>(
    form: FormStore<TFormValues, TFieldValues, TResponseData>,
    formErrors: FormErrors<TFieldValues>,
    { shouldActive = true }: ErrorResponseOptions,
): void {
    // Combine errors that were not set for any field or field array into one
    // general form error response message
    const message = Object.entries<Maybe<string>>(formErrors)
        .reduce<string[]>((errors, [name, error]) => {
            if (
                [
                    getFieldStore(form, name as FieldPath<TFieldValues>),
                    getFieldArrayStore(
                        form,
                        name as FieldArrayPath<TFieldValues>,
                    ),
                ].every(
                    (fieldOrFieldArray) =>
                        !fieldOrFieldArray ||
                        (shouldActive &&
                            !untrack(fieldOrFieldArray.active.get)),
                )
            ) {
                // biome-ignore lint/style/noNonNullAssertion: <explanation>
                errors.push(error!);
            }
            return errors;
        }, [])
        .join(" ");

    // If there is a error message, set it as form response
    if (message) {
        form.internal.response.set({
            status: "error",
            message,
        });
    }
}
