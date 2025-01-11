import { batch } from "solid-js";
import type {
    FieldPath,
    FieldPathValue,
    FieldValues,
    FormStore,
    IntersectValues,
    Maybe,
    ResponseData,
} from "../types";
import { initializeFieldStore, validateIfRequired } from "../utils";
import { reset } from "./reset";

/**
 * Value type of the set value options.
 */
export type SetInitialValueOptions = Partial<{
    shouldValidate: boolean;
    shouldReset: boolean;
}>;

/**
 * Sets the value of the specified field.
 *
 * @param form The form of the field.
 * @param name The name of the field.
 * @param value The value to bet set.
 * @param options The value options.
 */
export function setInitialValue<
    TFormValues extends FieldValues,
    TFieldValues extends IntersectValues<TFormValues>,
    TResponseData extends ResponseData,
    TFieldPath extends FieldPath<TFieldValues>,
>(
    form: FormStore<TFormValues, TFieldValues, TResponseData>,
    name: TFieldPath,
    value: FieldPathValue<TFieldValues, TFieldPath, true>,
    options?: Maybe<SetInitialValueOptions>,
): void;

export function setInitialValue(
    form: FormStore<FieldValues, FieldValues, ResponseData>,
    name: string,
    value: FieldPathValue<FieldValues, string, true>,
    {
        shouldValidate = false,
        shouldReset = true,
    }: Maybe<SetInitialValueOptions> = {},
): void {
    batch(() => {
        // Initialize store of specified field
        const field = initializeFieldStore(form, name);

        // Set new value
        field.initialValue.set(() => value);

        // Validate if set to "true" and necessary
        if (shouldValidate) {
            validateIfRequired(form, field, name, {
                on: ["blur"],
            });
        }

        if (shouldReset) {
            reset(form, name);
        }
    });
}
