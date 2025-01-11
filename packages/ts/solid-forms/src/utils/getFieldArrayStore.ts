import type {
    FieldArrayPath,
    FieldValues,
    FormStore,
    InternalFieldArrayStore,
    IntersectValues,
    Maybe,
    ResponseData,
} from "../types";

/**
 * Returns the store of a field array.
 *
 * @param form The form of the field array.
 * @param name The name of the field array.
 *
 * @returns The reactive store.
 */
export function getFieldArrayStore<
    TFormValues extends FieldValues,
    TFieldValues extends IntersectValues<TFormValues>,
    TResponseData extends ResponseData,
    TFieldArrayName extends FieldArrayPath<TFieldValues>,
>(
    form: FormStore<TFormValues, TFieldValues, TResponseData>,
    name: TFieldArrayName,
): Maybe<InternalFieldArrayStore> {
    return form.internal.fieldArrays[name];
}
