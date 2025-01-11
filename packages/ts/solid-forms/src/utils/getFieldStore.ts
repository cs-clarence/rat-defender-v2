import type {
    FieldPath,
    FieldValues,
    FormStore,
    InternalFieldStore,
    IntersectValues,
    Maybe,
    ResponseData,
} from "../types";

/**
 * Returns the store of a field.
 *
 * @param form The form of the field.
 * @param name The name of the field.
 *
 * @returns The reactive store.
 */
export function getFieldStore<
    TFormValues extends FieldValues,
    TFieldValues extends IntersectValues<TFormValues>,
    TResponseData extends ResponseData,
    TFieldName extends FieldPath<TFieldValues>,
>(
    form: FormStore<TFormValues, TFieldValues, TResponseData>,
    name: TFieldName,
): Maybe<InternalFieldStore<TFieldValues, TFieldName>> {
    return form.internal.fields[name];
}
