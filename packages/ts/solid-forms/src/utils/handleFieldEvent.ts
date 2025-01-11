import { batch } from "solid-js";
import type {
    FieldEvent,
    FieldPath,
    FieldPathValue,
    FieldValues,
    FormStore,
    InternalFieldStore,
    IntersectValues,
    ResponseData,
    ValidationMode,
} from "../types";
import { updateFieldDirty } from "./updateFieldDirty";
import { validateIfRequired } from "./validateIfRequired";

/**
 * Handles the input, change and blur event of a field.
 *
 * @param form The form of the field.
 * @param field The store of the field.
 * @param name The name of the field.
 * @param event The event of the field.
 * @param validationModes The modes of the validation.
 * @param inputValue The value of the input.
 */
export function handleFieldEvent<
    TFormValues extends FieldValues,
    TFieldValues extends IntersectValues<TFormValues>,
    TResponseData extends ResponseData,
    TFieldName extends FieldPath<TFieldValues>,
>(
    form: FormStore<TFormValues, TFieldValues, TResponseData>,
    field: InternalFieldStore<TFieldValues, TFieldName>,
    name: TFieldName,
    event: FieldEvent,
    validationModes: Exclude<ValidationMode, "submit">[],
    inputValue?: FieldPathValue<TFieldValues, TFieldName, true>,
): void;

export function handleFieldEvent(
    form: FormStore<FieldValues, FieldValues, ResponseData>,
    field: InternalFieldStore<FieldValues, string>,
    name: string,
    event: FieldEvent,
    validationModes: Exclude<ValidationMode, "submit">[],
    inputValue?: FieldPathValue<FieldValues, string, false>,
): void {
    batch(() => {
        // Update value state
        field.value.set((prevValue) =>
            field.transform.reduce(
                (current, transformation) => transformation(current, event),
                inputValue ?? prevValue,
            ),
        );

        // Update touched state
        field.touched.set(true);
        form.internal.touched.set(true);

        // Update dirty state
        updateFieldDirty(form, field);

        // Validate value if required
        validateIfRequired(form, field, name, { on: validationModes });
    });
}
