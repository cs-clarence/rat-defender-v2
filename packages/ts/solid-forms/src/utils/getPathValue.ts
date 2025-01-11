/* eslint-disable @typescript-eslint/no-explicit-any */
import type {
    FieldArrayPath,
    FieldArrayPathValue,
    FieldPath,
    FieldPathValue,
    FieldValues,
    IntersectValues,
    Maybe,
    PartialValues,
} from "../types";

/**
 * Returns the value of a dot path in an object.
 *
 * @param path The dot path.
 * @param object The object.
 *
 * @returns The value or undefined.
 */
export function getPathValue<
    TFormValues extends FieldValues,
    TFieldValues extends IntersectValues<TFormValues>,
    TFieldPath extends FieldPath<TFieldValues>,
>(
    path: TFieldPath,
    object: PartialValues<TFieldValues>,
): Maybe<FieldPathValue<TFieldValues, TFieldPath, true>>;

/**
 * Returns the value of a dot path in an object.
 *
 * @param path The dot path.
 * @param object The object.
 *
 * @returns The value or undefined.
 */
export function getPathValue<
    TFormValues extends FieldValues,
    TFieldValues extends IntersectValues<TFormValues>,
    TFieldArrayName extends FieldArrayPath<TFieldValues>,
>(
    path: TFieldArrayName,
    object: PartialValues<TFieldValues>,
): Maybe<FieldArrayPathValue<TFieldValues, TFieldArrayName, true>>;

// biome-ignore lint/suspicious/noExplicitAny: <explanation>
export function getPathValue(path: string, object: Record<string, any>): any {
    // biome-ignore lint/suspicious/noExplicitAny: <explanation>
    return path.split(".").reduce<any>((value, key) => value?.[key], object);
}
