/* eslint-disable @typescript-eslint/no-explicit-any */
import type {
    FieldArrayPath,
    FieldPath,
    FieldValues,
    IntersectValues,
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
export function hasPathValue<
    TFormValues extends FieldValues,
    TFieldValues extends IntersectValues<TFormValues>,
    TFieldPath extends FieldPath<TFieldValues>,
>(path: TFieldPath, object: PartialValues<TFieldValues>): boolean;

/**
 * Returns the value of a dot path in an object.
 *
 * @param path The dot path.
 * @param object The object.
 *
 * @returns The value or undefined.
 */
export function hasPathValue<
    TFormValues extends FieldValues,
    TFieldValues extends IntersectValues<TFormValues>,
    TFieldArrayName extends FieldArrayPath<TFieldValues>,
>(path: TFieldArrayName, object: PartialValues<TFieldValues>): boolean;

// biome-ignore lint/suspicious/noExplicitAny: <explanation>
export function hasPathValue(path: string, object: Record<string, any>): any {
    const split = path.split(".");

    function isLast(idx: number) {
        return idx === split.length - 1;
    }

    function isWithinBounds(idx: number, arr: unknown[]) {
        return idx >= 0 && idx < arr.length;
    }

    // biome-ignore lint/suspicious/noExplicitAny: <explanation>
    let current: any = object;

    for (const [idx, key] of split.entries()) {
        if (typeof current === "object") {
            if (!(key in current)) {
                return false;
            }

            if (isLast(idx)) {
                return true;
            }

            current = current[key];
            continue;
        }

        if (Array.isArray(current)) {
            const idx = +key;

            if (Number.isNaN(idx)) {
                return false;
            }

            if (!isWithinBounds(idx, current)) {
                return false;
            }
            if (isLast(idx)) {
                return true;
            }

            current = current[idx];
        }
    }

    return false;
}
