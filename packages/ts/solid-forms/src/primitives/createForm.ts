/* eslint-disable @typescript-eslint/no-explicit-any */
import { mergeProps, type JSX } from "solid-js";
import type { FormProps, FieldProps, FieldArrayProps } from "../components";
import { Form, Field, FieldArray } from "../components";
import type {
    FieldArrayPath,
    FieldPath,
    FieldPathValue,
    FieldValues,
    FormOptions,
    FormStore,
    IntersectValues,
    MaybeValue,
    PartialKey,
    ResponseData,
} from "../types";
import { createFormStore } from "./createFormStore";

/**
 * Creates and returns the store of the form as well as a linked Form, Field
 * and FieldArray component.
 *
 * @param options The form options.
 *
 * @returns The store and linked components.
 */
export function createForm<
    TFormValues extends FieldValues,
    TResponseData extends ResponseData = undefined,
    TFieldValues extends
        IntersectValues<TFormValues> = IntersectValues<TFormValues>,
>(
    options?: FormOptions<TFormValues>,
): [
    FormStore<TFormValues, TFieldValues, TResponseData>,
    {
        Form: (
            props: Omit<
                FormProps<TFormValues, TFieldValues, TResponseData>,
                "of"
            >,
        ) => JSX.Element;
        Field: <TFieldName extends FieldPath<TFieldValues>>(
            props: FieldPathValue<
                TFieldValues,
                TFieldName,
                false
            > extends MaybeValue<string>
                ? PartialKey<
                      Omit<
                          FieldProps<
                              TFormValues,
                              TFieldValues,
                              TResponseData,
                              TFieldName
                          >,
                          "of"
                      >,
                      "type"
                  >
                : Omit<
                      FieldProps<
                          TFormValues,
                          TFieldValues,
                          TResponseData,
                          TFieldName
                      >,
                      "of"
                  >,
        ) => JSX.Element;
        FieldArray: <TFieldArrayName extends FieldArrayPath<TFieldValues>>(
            props: Omit<
                FieldArrayProps<
                    TFormValues,
                    TFieldValues,
                    TResponseData,
                    TFieldArrayName
                >,
                "of"
            >,
        ) => JSX.Element;
    },
];

export function createForm(options?: FormOptions<FieldValues>): [
    FormStore<FieldValues, FieldValues, ResponseData>,
    {
        Form: (
            props: Omit<
                FormProps<FieldValues, FieldValues, ResponseData>,
                "of"
            >,
        ) => JSX.Element;
        Field: (
            props: Omit<
                FieldProps<FieldValues, FieldValues, ResponseData, string>,
                "of"
            >,
        ) => JSX.Element;
        FieldArray: (
            props: Omit<
                FieldArrayProps<FieldValues, FieldValues, ResponseData, string>,
                "of"
            >,
        ) => JSX.Element;
    },
] {
    // Create form store
    const form = createFormStore<FieldValues, FieldValues, ResponseData>(
        options,
    );

    // Return form store and linked components
    return [
        form,
        {
            // biome-ignore lint/suspicious/noExplicitAny: <explanation>
            Form: (props) => Form(mergeProps({ of: form }, props) as any),
            // biome-ignore lint/suspicious/noExplicitAny: <explanation>
            Field: (props) => Field(mergeProps({ of: form }, props) as any),
            FieldArray: (props) =>
                // biome-ignore lint/suspicious/noExplicitAny: <explanation>
                FieldArray(mergeProps({ of: form }, props) as any),
        },
    ];
}
