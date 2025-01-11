import { untrack } from "solid-js";
import type {
    FieldValues,
    FormStore,
    IntersectValues,
    Maybe,
    ResponseData,
} from "../types";
import { getFieldAndArrayStores } from "./getFieldAndArrayStores";

/**
 * Updates the invalid state of the form.
 *
 * @param form The store of the form.
 * @param dirty Whether there is an error.
 */
export function updateFormInvalid<
    TFormValues extends FieldValues,
    TFieldValues extends IntersectValues<TFormValues>,
    TResponseData extends ResponseData,
>(
    form: FormStore<TFormValues, TFieldValues, TResponseData>,
    invalid?: Maybe<boolean>,
): void {
    untrack(() => {
        form.internal.invalid.set(
            invalid ||
                getFieldAndArrayStores(form).some(
                    (fieldOrFieldArray) =>
                        fieldOrFieldArray.active.get() &&
                        fieldOrFieldArray.error.get(),
                ),
        );
    });
}
