import { untrack } from "solid-js";
import type {
    FieldArrayPath,
    FieldPath,
    FieldValues,
    FormStore,
    IntersectValues,
    ResponseData,
} from "../types";
import { getFieldArrayNames } from "./getFieldArrayNames";
import { getFieldArrayStore } from "./getFieldArrayStore";
import { getPathIndex } from "./getPathIndex";

/**
 * Removes invalid field or field array names of field arrays.
 *
 * @param form The form of the field array.
 * @param names The names to be filtered.
 */
export function removeInvalidNames<
    TFormValues extends FieldValues,
    TFieldValues extends IntersectValues<TFormValues>,
    TResponseData extends ResponseData,
>(
    form: FormStore<TFormValues, TFieldValues, TResponseData>,
    names: (FieldPath<TFieldValues> | FieldArrayPath<TFieldValues>)[],
) {
    // biome-ignore lint/complexity/noForEach: <explanation>
    getFieldArrayNames(form, false).forEach((fieldArrayName) => {
        const lastIndex =
            // biome-ignore lint/style/noNonNullAssertion: <explanation>
            untrack(getFieldArrayStore(form, fieldArrayName)!.items.get)
                .length - 1;
        // biome-ignore lint/complexity/noForEach: <explanation>
        names
            .filter(
                (name) =>
                    name.startsWith(`${fieldArrayName}.`) &&
                    getPathIndex(fieldArrayName, name) > lastIndex,
            )
            .forEach((name) => {
                names.splice(names.indexOf(name), 1);
            });
    });
}
