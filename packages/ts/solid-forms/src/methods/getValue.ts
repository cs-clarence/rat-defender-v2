import type {
    FieldPath,
    FieldPathValue,
    FieldValues,
    FormStore,
    IntersectValues,
    Maybe,
    ResponseData,
} from "../types";
import { initializeFieldStore } from "../utils";

/**
 * Value type if the get value options.
 */
export type GetValueOptions = Partial<{
    shouldActive: boolean;
    shouldTouched: boolean;
    shouldDirty: boolean;
    shouldValid: boolean;
}>;

/**
 * Returns the value of the specified field.
 *
 * @param form The form of the field.
 * @param name The name of the field.
 * @param options The value options.
 *
 * @returns The value of the field.
 */
export function getValue<
    TFormValues extends FieldValues,
    TFieldValues extends IntersectValues<TFormValues>,
    TResponseData extends ResponseData,
    TFieldPath extends FieldPath<TFieldValues>,
>(
    form: FormStore<TFormValues, TFieldValues, TResponseData>,
    name: TFieldPath,
    options?: Maybe<GetValueOptions>,
): Maybe<FieldPathValue<TFieldValues, TFieldPath, true>>;

export function getValue(
    form: FormStore<FieldValues, FieldValues, ResponseData>,
    name: string,
    {
        shouldActive = true,
        shouldTouched = false,
        shouldDirty = false,
        shouldValid = false,
    }: Maybe<GetValueOptions> = {},
): Maybe<FieldPathValue<FieldValues, string, false>> {
    // Get store of specified field
    const field = initializeFieldStore(form, name);

    // Continue if field corresponds to filter options
    if (
        (!shouldActive || field.active.get()) &&
        (!shouldTouched || field.touched.get()) &&
        (!shouldDirty || field.dirty.get()) &&
        (!shouldValid || !field.error.get())
    ) {
        // Return value of field
        return field.value.get();
    }

    // Otherwise return undefined
    return undefined;
}
