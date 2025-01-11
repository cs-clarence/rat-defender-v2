import { untrack } from "solid-js";
import type {
    FieldArrayPath,
    FieldValues,
    FormStore,
    IntersectValues,
    Maybe,
    RawFieldArrayState,
    ResponseData,
} from "../types";
import { getFieldArrayStore } from "./getFieldArrayStore";

/**
 * Returns the RAW state of the field array.
 *
 * @param form The form of the field array.
 * @param name The name of the field array.
 *
 * @returns The state of the field array.
 */
export function getFieldArrayState<
    TFormValues extends FieldValues,
    TFieldValues extends IntersectValues<TFormValues>,
    TResponseData extends ResponseData,
>(
    form: FormStore<TFormValues, TFieldValues, TResponseData>,
    name: FieldArrayPath<TFieldValues>,
): Maybe<RawFieldArrayState> {
    const fieldArray = getFieldArrayStore(form, name);
    return fieldArray
        ? untrack(() => ({
              startItems: fieldArray.startItems.get(),
              items: fieldArray.items.get(),
              error: fieldArray.error.get(),
              touched: fieldArray.touched.get(),
              dirty: fieldArray.dirty.get(),
          }))
        : undefined;
}
