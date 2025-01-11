import { batch, untrack } from "solid-js";
import type {
    FieldValues,
    FormStore,
    IntersectValues,
    ResponseData,
} from "../types";
import { getFieldAndArrayStores } from "./getFieldAndArrayStores";

/**
 * Updates the touched, dirty and invalid state of the form.
 *
 * @param form The store of the form.
 */
export function updateFormState<
    TFormValues extends FieldValues,
    TFieldValues extends IntersectValues<TFormValues>,
    TResponseData extends ResponseData,
>(form: FormStore<TFormValues, TFieldValues, TResponseData>): void {
    // Create state variables
    // biome-ignore lint/style/useSingleVarDeclarator: <explanation>
    let touched = false,
        dirty = false,
        invalid = false;

    // Check each field and field array and update state if necessary
    untrack(() => {
        for (const fieldOrFieldArray of getFieldAndArrayStores(form)) {
            if (fieldOrFieldArray.active.get()) {
                if (fieldOrFieldArray.touched.get()) {
                    touched = true;
                }
                if (fieldOrFieldArray.dirty.get()) {
                    dirty = true;
                }
                if (fieldOrFieldArray.error.get()) {
                    invalid = true;
                }
            }

            // Break loop if all state values are "true"
            if (touched && dirty && invalid) {
                break;
            }
        }
    });

    // Update state of form
    batch(() => {
        form.internal.touched.set(touched);
        form.internal.dirty.set(dirty);
        form.internal.invalid.set(invalid);
    });
}
