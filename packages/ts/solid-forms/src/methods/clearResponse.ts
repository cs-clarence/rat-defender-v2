import type {
    FieldValues,
    FormStore,
    IntersectValues,
    ResponseData,
} from "../types";

/**
 * Clears the response of the form.
 *
 * @param form The form of the response.
 */
export function clearResponse<
    TFormValues extends FieldValues,
    TFieldValues extends IntersectValues<TFormValues>,
    TResponseData extends ResponseData,
>(form: FormStore<TFormValues, TFieldValues, TResponseData>): void {
    form.internal.response.set({});
}
