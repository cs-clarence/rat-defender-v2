import { untrack } from "solid-js";
import type {
    FieldArrayPath,
    FieldPath,
    FieldValues,
    FormStore,
    InternalFieldArrayStore,
    InternalFieldStore,
    IntersectValues,
    Maybe,
    ResponseData,
    ValidationMode,
} from "../types";
import { validate } from "../methods";

/**
 * Value type of the validate otions.
 */
type ValidateOptions = {
    on: Exclude<ValidationMode, "submit">[];
    shouldFocus?: Maybe<boolean>;
};

/**
 * Validates a field or field array only if required.
 *
 * @param form The form of the field or field array.
 * @param fieldOrFieldArray The store of the field or field array.
 * @param name The name of the field or field array.
 * @param options The validate options.
 */
export function validateIfRequired<
    TFormValues extends FieldValues,
    TFieldValues extends IntersectValues<TFormValues>,
    TResponseData extends ResponseData,
    TFieldName extends FieldPath<TFieldValues>,
    TFieldArrayName extends FieldArrayPath<TFieldValues>,
>(
    form: FormStore<TFormValues, TFieldValues, TResponseData>,
    fieldOrFieldArray:
        | InternalFieldStore<TFieldValues, TFieldName>
        | InternalFieldArrayStore,
    name: TFieldName | TFieldArrayName,
    { on: modes, shouldFocus = false }: ValidateOptions,
): void {
    untrack(() => {
        if (
            (modes as string[]).includes(
                (
                    form.internal.validateOn === "submit"
                        ? form.internal.submitted.get()
                        : fieldOrFieldArray.error.get()
                )
                    ? form.internal.revalidateOn
                    : form.internal.validateOn,
            )
        ) {
            validate(form, name, { shouldFocus });
        }
    });
}
