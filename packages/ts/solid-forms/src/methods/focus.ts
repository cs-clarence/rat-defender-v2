import { untrack } from "solid-js";
import type {
    FieldPath,
    FieldValues,
    FormStore,
    IntersectValues,
    ResponseData,
} from "../types";
import { getFieldStore } from "../utils";

/**
 * Focuses the specified field of the form.
 *
 * @param form The form of the field.
 * @param name The name of the field.
 */
export function focus<
    TFormValues extends FieldValues,
    TFieldValues extends IntersectValues<TFormValues>,
    TResponseData extends ResponseData,
>(
    form: FormStore<TFormValues, TFieldValues, TResponseData>,
    name: FieldPath<TFieldValues>,
): void {
    untrack(() => getFieldStore(form, name)?.elements.get()[0]?.focus());
}
