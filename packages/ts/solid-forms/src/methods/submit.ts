import { untrack } from "solid-js";
import type {
    FieldValues,
    FormStore,
    IntersectValues,
    ResponseData,
} from "../types";

/**
 * Validates and submits the form.
 *
 * @param form The form to be submitted.
 */
export function submit<
    TFormValues extends FieldValues,
    TResponseData extends ResponseData,
    TFieldValues extends IntersectValues<TFormValues>,
>(form: FormStore<TFormValues, TFieldValues, TResponseData>): void {
    untrack(() => form.element)?.requestSubmit();
}
