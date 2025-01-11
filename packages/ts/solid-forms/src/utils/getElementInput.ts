import { untrack } from "solid-js";
import type {
    FieldElement,
    FieldPath,
    FieldPathValue,
    FieldType,
    FieldValues,
    InternalFieldStore,
    IntersectValues,
    Maybe,
} from "../types";

/**
 * Returns the current input of the element.
 *
 * @param element The field element.
 * @param field The store of the field.
 * @param type The data type to capture.
 *
 * @returns The element input.
 */
export function getElementInput<
    TFormValues extends FieldValues,
    TFieldValues extends IntersectValues<TFormValues>,
    TFieldPath extends FieldPath<TFieldValues>,
>(
    element: FieldElement,
    field: InternalFieldStore<TFieldValues, TFieldPath>,
    type: Maybe<FieldType<unknown>>,
): FieldPathValue<TFieldValues, TFieldPath, true> {
    const { checked, files, options, value, valueAsDate, valueAsNumber } =
        element as HTMLInputElement & HTMLSelectElement & HTMLTextAreaElement;
    return untrack(() =>
        !type || type === "string"
            ? value
            : type === "string[]"
              ? options
                  ? [...options]
                        .filter((e) => e.selected && !e.disabled)
                        .map((e) => e.value)
                  : checked
                    ? [...((field.value.get() || []) as string[]), value]
                    : ((field.value.get() || []) as string[]).filter(
                          (v) => v !== value,
                      )
              : type === "number"
                ? valueAsNumber
                : type === "boolean"
                  ? checked
                  : type === "File" && files
                    ? files[0]
                    : type === "File[]" && files
                      ? [...files]
                      : type === "Date" && valueAsDate
                        ? valueAsDate
                        : field.value.get(),
    ) as FieldPathValue<TFieldValues, TFieldPath, true>;
}
