/* eslint-disable @typescript-eslint/ban-ts-comment */
import { createSignal } from "../primitives";
import type {
    FieldElement,
    FieldPath,
    FieldPathValue,
    FieldValues,
    FormStore,
    InternalFieldStore,
    IntersectValues,
    Maybe,
    ResponseData,
} from "../types";
import { getFieldStore } from "./getFieldStore";
import { getPathValue } from "./getPathValue";

/**
 * Initializes and returns the store of a field.
 *
 * @param form The form of the field.
 * @param name The name of the field.
 *
 * @returns The reactive store.
 */
export function initializeFieldStore<
    TFormValues extends FieldValues,
    TFieldValues extends IntersectValues<TFormValues>,
    TResponseData extends ResponseData,
    TFieldName extends FieldPath<TFieldValues>,
>(
    form: FormStore<TFormValues, TFieldValues, TResponseData>,
    name: TFieldName,
): InternalFieldStore<TFieldValues, TFieldName>;

export function initializeFieldStore(
    form: FormStore<FieldValues, FieldValues, ResponseData>,
    name: string,
): InternalFieldStore<FieldValues, string> {
    // Initialize store on first request
    if (!getFieldStore(form, name)) {
        // Get initial value of field
        const initial = getPathValue(name, form.internal.initialValues);

        // Create signals of field store
        const elements = createSignal<FieldElement[]>([]);
        const initialValue =
            createSignal<Maybe<FieldPathValue<FieldValues, string, true>>>(
                initial,
            );
        const startValue =
            createSignal<Maybe<FieldPathValue<FieldValues, string, true>>>(
                initial,
            );
        const value =
            createSignal<Maybe<FieldPathValue<FieldValues, string, true>>>(
                initial,
            );
        const error = createSignal("");
        const active = createSignal(false);
        const touched = createSignal(false);
        const dirty = createSignal(false);

        // Add store of field to form
        // @ts-expect-error
        form.internal.fields[name] = {
            // Signals
            elements,
            initialValue,
            startValue,
            value,
            error,
            active,
            touched,
            dirty,

            // Other
            validate: [],
            transform: [],
            consumers: new Set(),
        };

        // Add name of field to form
        form.internal.fieldNames.set((names) => [...names, name]);
    }

    // Return store of field
    // biome-ignore lint/style/noNonNullAssertion: <explanation>
    return getFieldStore(form, name)!;
}
