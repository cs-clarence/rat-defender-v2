import type {
    FieldPath,
    FieldValues,
    FormStore,
    IntersectValues,
    RawFieldState,
    ResponseData,
} from "../types";
import { initializeFieldStore } from "./initializeFieldStore";

/**
 * Sets the store of a field to the specified state.
 *
 * @param form The form of the field.
 * @param name The name of the field.
 * @param state The new state to be set.
 */
export function setFieldState<
    TFormValues extends FieldValues,
    TFieldValues extends IntersectValues<TFormValues>,
    TResponseData extends ResponseData,
    TFieldName extends FieldPath<TFieldValues>,
>(
    form: FormStore<TFormValues, TFieldValues, TResponseData>,
    name: TFieldName,
    state: RawFieldState<TFieldValues, TFieldName>,
): void {
    const field = initializeFieldStore(form, name);
    field.startValue.set(() => state.startValue);
    field.value.set(() => state.value);
    field.error.set(state.error);
    field.touched.set(state.touched);
    field.dirty.set(state.dirty);
}
