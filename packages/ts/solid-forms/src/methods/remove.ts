import { batch, untrack } from "solid-js";
import type {
    FieldArrayPath,
    FieldValues,
    FormStore,
    IntersectValues,
    ResponseData,
} from "../types";
import {
    getFieldArrayNames,
    getFieldArrayState,
    getFieldArrayStore,
    getFieldNames,
    getFieldState,
    getPathIndex,
    setFieldArrayState,
    setFieldState,
    sortArrayPathIndex,
    updateFieldArrayDirty,
    validateIfRequired,
} from "../utils";

/**
 * Value type of the remove options.
 */
export type RemoveOptions = {
    at: number;
};

/**
 * Removes a item of the field array.
 *
 * @param form The form of the field array.
 * @param name The name of field array.
 * @param options The remove options.
 */
export function remove<
    TFormValues extends FieldValues,
    TFieldValues extends IntersectValues<TFormValues>,
    TResponseData extends ResponseData,
>(
    form: FormStore<TFormValues, TFieldValues, TResponseData>,
    name: FieldArrayPath<TFieldValues>,
    { at: index }: RemoveOptions,
): void {
    // Get store of specified field array
    const fieldArray = getFieldArrayStore(form, name);

    // Continue if specified field array exists
    if (fieldArray) {
        untrack(() => {
            // Get last index of field array
            const lastIndex = fieldArray.items.get().length - 1;

            // Continue if specified index is valid
            if (index >= 0 && index <= lastIndex) {
                // Create function to filter a name
                const filterName = (value: string) =>
                    value.startsWith(`${name}.`) &&
                    getPathIndex(name, value) > index;

                // Create function to get previous index name
                const getPrevIndexName = <T extends string>(
                    fieldOrFieldArrayName: T,
                    fieldOrFieldArrayIndex: number,
                ): T =>
                    fieldOrFieldArrayName.replace(
                        `${name}.${fieldOrFieldArrayIndex}`,
                        `${name}.${fieldOrFieldArrayIndex - 1}`,
                    ) as T;

                batch(() => {
                    // Move state of each field after the removed index back by one index
                    // biome-ignore lint/complexity/noForEach: <explanation>
                    getFieldNames(form)
                        .filter(filterName)
                        .sort(sortArrayPathIndex(name))
                        .forEach((fieldName) => {
                            setFieldState(
                                form,
                                getPrevIndexName(
                                    fieldName,
                                    getPathIndex(name, fieldName),
                                ),
                                // biome-ignore lint/style/noNonNullAssertion: <explanation>
                                getFieldState(form, fieldName)!,
                            );
                        });

                    // Move state of each field array after the removed index back by one index
                    // biome-ignore lint/complexity/noForEach: <explanation>
                    getFieldArrayNames(form)
                        .filter(filterName)
                        .sort(sortArrayPathIndex(name))
                        .forEach((fieldArrayName) => {
                            setFieldArrayState(
                                form,
                                getPrevIndexName(
                                    fieldArrayName,
                                    getPathIndex(name, fieldArrayName),
                                ),
                                // biome-ignore lint/style/noNonNullAssertion: <explanation>
                                getFieldArrayState(form, fieldArrayName)!,
                            );
                        });

                    // Delete item from field array
                    fieldArray.items.set((prevItem) => {
                        const nextItems = [...prevItem];
                        nextItems.splice(index, 1);
                        return nextItems;
                    });

                    // Set touched at field array and form to true
                    fieldArray.touched.set(true);
                    form.internal.touched.set(true);

                    // Update dirty state at field array and form
                    updateFieldArrayDirty(form, fieldArray);

                    // Validate field array if necessary
                    validateIfRequired(form, fieldArray, name, {
                        on: ["touched", "input"],
                    });
                });
            }
        });
    }
}
