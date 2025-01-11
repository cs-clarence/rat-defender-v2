import { untrack } from "solid-js";
import type {
    FieldPath,
    FieldValues,
    FormStore,
    IntersectValues,
    Maybe,
    RawFieldState,
    ResponseData,
} from "../types";
import { getFieldStore } from "./getFieldStore";

/**
 * Returns the RAW state of the field.
 *
 * @param form The form of the field.
 * @param name The name of the field.
 *
 * @returns The state of the field.
 */
export function getFieldState<
    TFormValues extends FieldValues,
    TFieldValues extends IntersectValues<TFormValues>,
    TResponseData extends ResponseData,
    TFieldPath extends FieldPath<TFieldValues>,
>(
    form: FormStore<TFormValues, TFieldValues, TResponseData>,
    name: TFieldPath,
): Maybe<RawFieldState<TFieldValues, TFieldPath>> {
    const field = getFieldStore(form, name);
    return field
        ? untrack(() => ({
              startValue: field.startValue.get(),
              value: field.value.get(),
              error: field.error.get(),
              touched: field.touched.get(),
              dirty: field.dirty.get(),
          }))
        : undefined;
}
