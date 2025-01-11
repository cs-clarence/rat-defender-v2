import type {
    FieldArrayPath,
    FieldPath,
    FieldValues,
    FormErrors,
    FormStore,
    IntersectValues,
    Maybe,
    ResponseData,
} from "../types";
import {
    getFieldArrayStore,
    getFieldStore,
    getFilteredNames,
    getOptions,
} from "../utils";

/**
 * Value type of the get errors options.
 */
export type GetErrorsOptions = Partial<{
    shouldActive: boolean;
    shouldTouched: boolean;
    shouldDirty: boolean;
}>;

/**
 * Returns the current errors of the form fields.
 *
 * @param form The form of the fields.
 * @param options The errors options.
 *
 * @returns The form errors.
 */
export function getErrors<
    TFormValues extends FieldValues,
    TFieldValues extends IntersectValues<TFormValues>,
    TResponseData extends ResponseData,
>(
    form: FormStore<TFormValues, TFieldValues, TResponseData>,
    options?: Maybe<GetErrorsOptions>,
): FormErrors<TFieldValues>;

/**
 * Returns the errors of the specified field array.
 *
 * @param form The form of the field array.
 * @param name The name of the field array.
 * @param options The errors options.
 *
 * @returns The form errors.
 */
export function getErrors<
    TFormValues extends FieldValues,
    TFieldValues extends IntersectValues<TFormValues>,
    TResponseData extends ResponseData,
    TFieldArrayName extends FieldArrayPath<TFormValues>,
>(
    form: FormStore<TFormValues, TFieldValues, TResponseData>,
    name: TFieldArrayName,
    options?: Maybe<GetErrorsOptions>,
): FormErrors<TFieldValues>;

/**
 * Returns the current errors of the specified fields and field arrays.
 *
 * @param form The form of the fields.
 * @param names The names of the fields and field arrays.
 * @param options The errors options.
 *
 * @returns The form errors.
 */
export function getErrors<
    TFormValues extends FieldValues,
    TFieldValues extends IntersectValues<TFormValues>,
    TResponseData extends ResponseData,
>(
    form: FormStore<TFormValues, TFieldValues, TResponseData>,
    names: (FieldPath<TFieldValues> | FieldArrayPath<TFieldValues>)[],
    options?: Maybe<GetErrorsOptions>,
): FormErrors<TFieldValues>;

export function getErrors<
    TFormValues extends FieldValues,
    TFieldValues extends IntersectValues<TFormValues>,
    TResponseData extends ResponseData,
>(
    form: FormStore<TFormValues, TFieldValues, TResponseData>,
    arg2?: Maybe<
        | FieldArrayPath<TFieldValues>
        | (FieldPath<TFieldValues> | FieldArrayPath<TFieldValues>)[]
        | GetErrorsOptions
    >,
    arg3?: Maybe<GetErrorsOptions>,
): FormErrors<TFieldValues> {
    // Get filtered field names to get error from
    const [fieldNames, fieldArrayNames] = getFilteredNames(form, arg2);

    // Destructure options and set default values
    const {
        shouldActive = true,
        shouldTouched = false,
        shouldDirty = false,
    } = getOptions(arg2, arg3);

    // If no field or field array name is specified, set listener to be notified
    // when a new field or field array is added
    if (typeof arg2 !== "string" && !Array.isArray(arg2)) {
        form.internal.fieldNames.get();
        form.internal.fieldArrayNames.get();

        // Otherwise if a field array is included, set listener to be notified when
        // a new field array items is added
    } else {
        // biome-ignore lint/complexity/noForEach: <explanation>
        fieldArrayNames.forEach((fieldArrayName) =>
            // biome-ignore lint/style/noNonNullAssertion: <explanation>
            getFieldArrayStore(form, fieldArrayName)!.items.get(),
        );
    }

    // Create and return object with form errors
    return [
        ...fieldNames.map(
            // biome-ignore lint/style/noNonNullAssertion: <explanation>
            (name) => [name, getFieldStore(form, name)!] as const,
        ),
        ...fieldArrayNames.map(
            // biome-ignore lint/style/noNonNullAssertion: <explanation>
            (name) => [name, getFieldArrayStore(form, name)!] as const,
        ),
    ].reduce<FormErrors<TFieldValues>>(
        (formErrors, [name, fieldOrFieldArray]) => {
            if (
                fieldOrFieldArray.error.get() &&
                (!shouldActive || fieldOrFieldArray.active.get()) &&
                (!shouldTouched || fieldOrFieldArray.touched.get()) &&
                (!shouldDirty || fieldOrFieldArray.dirty.get())
            ) {
                formErrors[name] = fieldOrFieldArray.error.get();
            }
            return formErrors;
        },
        {},
    );
}
