import type {
    FieldPath,
    FieldValues,
    FormStore,
    InternalFieldArrayStore,
    InternalFieldStore,
    IntersectValues,
    ResponseData,
} from "../types";

/**
 * Returns a tuple with all field and field array stores of a form.
 *
 * @param form The form of the stores.
 *
 * @returns The store tuple.
 */
export function getFieldAndArrayStores<
    TFormValues extends FieldValues,
    TFieldValues extends IntersectValues<TFormValues>,
    TResponseData extends ResponseData,
>(
    form: FormStore<TFormValues, TFieldValues, TResponseData>,
): (
    | InternalFieldStore<TFieldValues, FieldPath<TFieldValues>>
    | InternalFieldArrayStore
)[] {
    return [
        ...Object.values(form.internal.fields),
        ...Object.values(form.internal.fieldArrays),
    ] as (
        | InternalFieldStore<TFieldValues, FieldPath<TFieldValues>>
        | InternalFieldArrayStore
    )[];
}
