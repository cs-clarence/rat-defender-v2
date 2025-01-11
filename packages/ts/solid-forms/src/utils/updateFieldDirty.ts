import { batch, untrack } from "solid-js";
import type {
    FieldPath,
    FieldValue,
    FieldValues,
    FormStore,
    InternalFieldStore,
    IntersectValues,
    Maybe,
    ResponseData,
} from "../types";
import { isFieldDirty } from "./isFieldDirty";
import { updateFormDirty } from "./updateFormDirty";

/**
 * Updates the dirty state of a field.
 *
 * @param form The form of the field.
 * @param field The store of the field.
 */
export function updateFieldDirty<
    TFormValues extends FieldValues,
    TFieldValues extends IntersectValues<TFormValues>,
    TResponseData extends ResponseData,
    TFielName extends FieldPath<TFormValues>,
>(
    form: FormStore<TFormValues, TFieldValues, TResponseData>,
    field: InternalFieldStore<TFormValues, TFielName>,
): void {
    untrack(() => {
        // Check if field is dirty
        const dirty = isFieldDirty(
            field.startValue.get() as Maybe<FieldValue>,
            field.value.get() as Maybe<FieldValue>,
        );

        // Update dirty state of field if necessary
        if (dirty !== field.dirty.get()) {
            batch(() => {
                field.dirty.set(dirty);

                // Update dirty state of form
                updateFormDirty(form, dirty);
            });
        }
    });
}
