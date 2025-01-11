/* eslint-disable @typescript-eslint/no-explicit-any */
import { batch } from "solid-js";
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
import { getUniqueId, initializeFieldArrayStore } from "../utils";
import { validate } from "./validate";
import { setInitialValue } from "./setInitialValue";

/**
 * Value type of the set values options.
 */
export type SetInitialValuesOptions = Partial<{
    shouldValidate: boolean;
}>;

/**
 * Sets multiple values of the form at once.
 *
 * @param form The form store.
 * @param values The values to be set.
 * @param options The values options.
 */
export function setInitialValues<
    TFormValues extends FieldValues,
    TFieldValues extends IntersectValues<TFormValues>,
    TResponseData extends ResponseData,
>(
    form: FormStore<TFormValues, TFieldValues, TResponseData>,
    values: PartialValues<TFormValues>,
    options?: Maybe<SetInitialValuesOptions>,
): void;

/**
 * Sets multiple values of a field array at once.
 *
 * @param form The form of the field array.
 * @param name The name of the field array.
 * @param values The values to be set.
 * @param options The values options.
 */
export function setInitialValues<
    TFormValues extends FieldValues,
    TFieldValues extends IntersectValues<TFormValues>,
    TResponseData extends ResponseData,
    TFieldArrayName extends FieldArrayPath<TFieldValues>,
>(
    form: FormStore<TFormValues, TFieldValues, TResponseData>,
    name: TFieldArrayName,
    values: FieldArrayPathValue<TFieldValues, TFieldArrayName, false>,
    options?: Maybe<SetInitialValuesOptions>,
): void;

export function setInitialValues<
    TFormValues extends FieldValues,
    TFieldValues extends IntersectValues<TFormValues>,
    TResponseData extends ResponseData,
    TFieldArrayName extends FieldArrayPath<TFieldValues>,
>(
    form: FormStore<TFormValues, TFieldValues, TResponseData>,
    arg2: PartialValues<TFieldValues> | TFieldArrayName,
    arg3?: Maybe<
        | SetInitialValuesOptions
        | FieldArrayPathValue<TFieldValues, TFieldArrayName, false>
    >,
    arg4?: Maybe<SetInitialValuesOptions>,
): void {
    // Check if values of a field array should be set
    const isFieldArray = typeof arg2 === "string";

    // Get values from arguments
    const values = (isFieldArray ? arg3 : arg2) as
        | PartialValues<TFormValues>
        | FieldArrayPathValue<TFieldValues, TFieldArrayName, true>;

    // Get options from arguments
    const options = ((isFieldArray ? arg4 : arg3) ||
        {}) as SetInitialValuesOptions;

    // Destructure options and set default values
    const { shouldValidate = false } = options;

    // Create list of field and field array names
    const names: (FieldPath<TFieldValues> | FieldArrayPath<TFieldValues>)[] =
        isFieldArray ? [arg2] : [];

    batch(() => {
        // Create function to set items of field array
        const setFieldArrayItems = (
            name: FieldArrayPath<TFieldValues>,
            // biome-ignore lint/suspicious/noExplicitAny: <explanation>
            value: any[],
        ) => {
            // Initialize store of specified field array
            const fieldArray = initializeFieldArrayStore(form, name);

            // Set array items
            fieldArray.items.set(value.map(() => getUniqueId()));
        };

        // Create recursive function to set nested values
        const setNestedValues = (data: object, prevPath?: string) =>
            // biome-ignore lint/complexity/noForEach: <explanation>
            Object.entries(data).forEach(([path, value]) => {
                // Create new compound path
                const compoundPath = prevPath ? `${prevPath}.${path}` : path;

                // Set value of fields
                if (
                    !value ||
                    typeof value !== "object" ||
                    Array.isArray(value)
                ) {
                    setInitialValue(
                        form,
                        compoundPath as FieldPath<TFieldValues>,
                        value,
                        {
                            shouldValidate: false,
                        },
                    );

                    // Add name of field or field array to list
                    names.push(
                        compoundPath as
                            | FieldPath<TFieldValues>
                            | FieldArrayPath<TFieldValues>,
                    );
                }

                // Set items of field arrays
                if (Array.isArray(value)) {
                    setFieldArrayItems(
                        compoundPath as FieldArrayPath<TFieldValues>,
                        value,
                    );
                }

                // Set values of nested fields and field arrays
                if (value && typeof value === "object") {
                    setNestedValues(value, compoundPath);
                }
            });

        // Set field array items if necessary
        if (isFieldArray) {
            setFieldArrayItems(
                arg2 as TFieldArrayName,
                arg3 as FieldArrayPathValue<
                    TFieldValues,
                    TFieldArrayName,
                    true
                >,
            );
        }

        // Set nested values of specified values
        setNestedValues(values, isFieldArray ? arg2 : undefined);

        // Validate if set to "true" and necessary
        if (
            shouldValidate &&
            ["touched", "input"].includes(
                form.internal.validateOn === "submit" && form.submitted
                    ? form.internal.revalidateOn
                    : form.internal.validateOn,
            )
        ) {
            validate(form, names, {});
        }
    });
}
