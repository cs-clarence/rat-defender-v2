/* eslint-disable @typescript-eslint/no-explicit-any */
import type {
    FieldArrayPath,
    FieldArrayPathValue,
    FieldPath,
    FieldValues,
    FormStore,
    IntersectValues,
    Maybe,
    PartialValues,
    ResponseData,
} from "../types";
import {
    getFieldArrayStore,
    getFieldStore,
    getFilteredNames,
    getOptions,
} from "../utils";

/**
 * Value type of the get values options.
 */
export type GetValuesOptions = Partial<{
    shouldActive: boolean;
    shouldTouched: boolean;
    shouldDirty: boolean;
    shouldValid: boolean;
}>;

/**
 * Returns the current values of the form fields.
 *
 * @param form The form of the fields.
 * @param options The values options.
 *
 * @returns The form field values.
 */
export function getValues<
    TFormValues extends FieldValues,
    TFieldValues extends IntersectValues<TFormValues>,
    TResponseData extends ResponseData,
>(
    form: FormStore<TFormValues, TFieldValues, TResponseData>,
    options?: Maybe<GetValuesOptions>,
): PartialValues<TFormValues>;

/**
 * Returns the values of the specified field array.
 *
 * @param form The form of the field array.
 * @param name The name of the field array.
 * @param options The values options.
 *
 * @returns The field array values.
 */
export function getValues<
    TFormValues extends FieldValues,
    TFIeldValues extends IntersectValues<TFormValues>,
    TResponseData extends ResponseData,
    TFieldArrayName extends FieldArrayPath<TFIeldValues>,
>(
    form: FormStore<TFormValues, TFIeldValues, TResponseData>,
    name: TFieldArrayName,
    options?: Maybe<GetValuesOptions>,
): PartialValues<FieldArrayPathValue<TFIeldValues, TFieldArrayName, false>>;

/**
 * Returns the current values of the specified fields and field arrays.
 *
 * @param form The form of the fields.
 * @param names The names of the fields and field arrays.
 * @param options The values options.
 *
 * @returns The form field values.
 */
export function getValues<
    TFormValues extends FieldValues,
    TFieldValues extends IntersectValues<TFormValues>,
    TResponseData extends ResponseData,
>(
    form: FormStore<TFormValues, TFieldValues, TResponseData>,
    names: (FieldPath<TFieldValues> | FieldArrayPath<TFieldValues>)[],
    options?: Maybe<GetValuesOptions>,
): PartialValues<TFormValues>;

export function getValues<
    TFormValues extends FieldValues,
    TFieldValues extends IntersectValues<TFormValues>,
    TResponseData extends ResponseData,
>(
    form: FormStore<TFormValues, TFieldValues, TResponseData>,
    arg2?: Maybe<
        | FieldArrayPath<TFieldValues>
        | (FieldPath<TFieldValues> | FieldArrayPath<TFieldValues>)[]
        | GetValuesOptions
    >,
    arg3?: Maybe<GetValuesOptions>,
): PartialValues<TFormValues> {
    // Get filtered field names to get value from
    const [fieldNames, fieldArrayNames] = getFilteredNames(form, arg2);

    // Destructure options and set default values
    const {
        shouldActive = true,
        shouldTouched = false,
        shouldDirty = false,
        shouldValid = false,
    } = getOptions(arg2, arg3);

    // If no field or field array name is specified, set listener to be notified
    // when a new field is added
    if (typeof arg2 !== "string" && !Array.isArray(arg2)) {
        form.internal.fieldNames.get();

        // Otherwise if a field array is included, set listener to be notified when
        // a new field array items is added
    } else {
        // biome-ignore lint/complexity/noForEach: <explanation>
        fieldArrayNames.forEach((fieldArrayName) =>
            // biome-ignore lint/style/noNonNullAssertion: <explanation>
            getFieldArrayStore(form, fieldArrayName)!.items.get(),
        );
    }

    // Create and return values of form or field array
    // biome-ignore lint/suspicious/noExplicitAny: <explanation>
    return fieldNames.reduce<any>(
        (values, name) => {
            // Get store of specified field
            // biome-ignore lint/style/noNonNullAssertion: <explanation>
            const field = getFieldStore(form, name)!;

            // Add value if field corresponds to filter options
            if (
                (!shouldActive || field.active.get()) &&
                (!shouldTouched || field.touched.get()) &&
                (!shouldDirty || field.dirty.get()) &&
                (!shouldValid || !field.error.get())
            ) {
                // Split name into keys to be able to add values of nested fields
                (typeof arg2 === "string" ? name.replace(`${arg2}.`, "") : name)
                    .split(".")
                    // biome-ignore lint/suspicious/noExplicitAny: <explanation>
                    .reduce<any>(
                        (object, key, index, keys) =>
                            // biome-ignore lint/suspicious/noAssignInExpressions: <explanation>
                            (object[key] =
                                index === keys.length - 1
                                    ? // If it is last key, add value
                                      field.value.get()
                                    : // Otherwise return object or array
                                      (typeof object[key] === "object" &&
                                          object[key]) ||
                                      (Number.isNaN(+(keys[index + 1] ?? 0))
                                          ? {}
                                          : [])),
                        values,
                    );
            }

            // Return modified values object
            return values;
        },
        typeof arg2 === "string" ? [] : {},
    );
}
