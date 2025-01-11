import { untrack } from "solid-js";
import type {
    FieldArrayPath,
    FieldPath,
    FieldValues,
    FormStore,
    IntersectValues,
    Maybe,
    ResponseData,
} from "../types";
import { getFieldArrayNames } from "./getFieldArrayNames";
import { getFieldNames } from "./getFieldNames";

/**
 * Returns a tuple with filtered field and field array names. For each
 * specified field array name, the names of the contained fields and field
 * arrays are also returned. If no name is specified, the name of each field
 * and field array of the form is returned.
 *
 * @param form The form of the fields.
 * @param arg2 The name of the fields.
 * @param shouldValid Whether to be valid.
 *
 * @returns A tuple with filtered names.
 */
export function getFilteredNames<
    TFormValues extends FieldValues,
    TFieldValues extends IntersectValues<TFormValues>,
    TResponseData extends ResponseData,
    TOptions extends Record<string, unknown>,
>(
    form: FormStore<TFormValues, TFieldValues, TResponseData>,
    arg2?: Maybe<
        | FieldPath<TFieldValues>
        | FieldArrayPath<TFieldValues>
        | (FieldPath<TFieldValues> | FieldArrayPath<TFieldValues>)[]
        | TOptions
    >,
    shouldValid?: Maybe<boolean>,
): [FieldPath<TFieldValues>[], FieldArrayPath<TFieldValues>[]] {
    return untrack(() => {
        // Get all field and field array names of form
        const allFieldNames = getFieldNames(form, shouldValid);
        const allFieldArrayNames = getFieldArrayNames(form, shouldValid);

        // If names are specified, filter and return them
        if (typeof arg2 === "string" || Array.isArray(arg2)) {
            return (typeof arg2 === "string" ? [arg2] : arg2)
                .reduce(
                    (tuple, name) => {
                        // Destructure tuple
                        const [fieldNames, fieldArrayNames] =
                            tuple as unknown as [
                                Set<FieldPath<TFieldValues>>,
                                Set<FieldArrayPath<TFieldValues>>,
                            ];

                        // If it is name of a field array, add it and name of each field
                        // and field array it contains to field and field array names
                        if (
                            allFieldArrayNames.includes(
                                name as FieldArrayPath<TFieldValues>,
                            )
                        ) {
                            // biome-ignore lint/complexity/noForEach: <explanation>
                            allFieldArrayNames.forEach((fieldArrayName) => {
                                if (fieldArrayName.startsWith(name)) {
                                    fieldArrayNames.add(
                                        fieldArrayName as FieldArrayPath<TFieldValues>,
                                    );
                                }
                            });
                            // biome-ignore lint/complexity/noForEach: <explanation>
                            allFieldNames.forEach((fieldName) => {
                                if (fieldName.startsWith(name)) {
                                    fieldNames.add(
                                        fieldName as FieldPath<TFieldValues>,
                                    );
                                }
                            });

                            // If it is name of a field, add it to field name set
                        } else {
                            fieldNames.add(name as FieldPath<TFieldValues>);
                        }

                        // Return tuple
                        return tuple;
                    },
                    [new Set(), new Set()] as [
                        Set<FieldPath<TFormValues>>,
                        Set<FieldArrayPath<TFormValues>>,
                    ],
                )
                .map((set) => [...set]) as [
                FieldPath<TFormValues>[],
                FieldArrayPath<TFormValues>[],
            ];
        }

        // Otherwise return every field and field array name
        return [allFieldNames, allFieldArrayNames];
    }) as [FieldPath<TFieldValues>[], FieldArrayPath<TFieldValues>[]];
}
