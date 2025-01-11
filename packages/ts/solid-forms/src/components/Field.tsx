/* eslint-disable @typescript-eslint/no-explicit-any */
import {
    createEffect,
    createMemo,
    type JSX,
    untrack,
    mergeProps,
    children,
} from "solid-js";
import { createLifecycle } from "../primitives";
import type {
    FieldElement,
    FieldPath,
    FieldPathValue,
    FieldType,
    FieldValues,
    FormStore,
    IntersectValues,
    Maybe,
    MaybeArray,
    MaybeValue,
    PartialKey,
    ResponseData,
    TransformField,
    ValidateField,
} from "../types";
import {
    getElementInput,
    handleFieldEvent,
    initializeFieldStore,
} from "../utils";

/**
 * Value type ot the field store.
 */
export type FieldStore<
    TFieldValues extends FieldValues,
    TFieldName extends FieldPath<TFieldValues>,
> = {
    get name(): TFieldName;
    get value(): Maybe<FieldPathValue<TFieldValues, TFieldName, true>>;
    get error(): string;
    get active(): boolean;
    get touched(): boolean;
    get dirty(): boolean;
};

/**
 * Value type of the field element props.
 */
export type FieldElementProps<
    TFieldValues extends FieldValues,
    TFieldName extends FieldPath<TFieldValues>,
> = {
    get name(): TFieldName;
    get autofocus(): boolean;
    ref: (element: FieldElement) => void;
    onInput: JSX.EventHandler<FieldElement, InputEvent>;
    onChange: JSX.EventHandler<FieldElement, Event>;
    onBlur: JSX.EventHandler<FieldElement, FocusEvent>;
};

/**
 * Value type of the field props.
 */
export type FieldProps<
    TFormValues extends FieldValues,
    TFieldValues extends IntersectValues<TFormValues>,
    TResponseData extends ResponseData,
    TFieldPath extends FieldPath<TFieldValues>,
> = {
    of: FormStore<TFormValues, TFieldValues, TResponseData>;
    name: TFieldPath;
    type: FieldType<FieldPathValue<TFieldValues, TFieldPath, true>>;
    children: (
        store: FieldStore<TFieldValues, TFieldPath>,
        props: FieldElementProps<TFieldValues, TFieldPath>,
    ) => JSX.Element;
    validate?: Maybe<
        MaybeArray<
            ValidateField<FieldPathValue<TFieldValues, TFieldPath, true>>
        >
    >;
    transform?: Maybe<
        MaybeArray<
            TransformField<FieldPathValue<TFieldValues, TFieldPath, true>>
        >
    >;
    keepActive?: boolean;
    keepState?: boolean;
};

/**
 * Headless form field that provides reactive properties and state.
 */
export function Field<
    TFormValues extends FieldValues,
    TFieldValues extends IntersectValues<TFormValues>,
    TResponseData extends ResponseData,
    TFieldPath extends FieldPath<TFieldValues>,
>(
    props: FieldPathValue<
        TFieldValues,
        TFieldPath,
        true
    > extends MaybeValue<string>
        ? PartialKey<
              FieldProps<TFormValues, TFieldValues, TResponseData, TFieldPath>,
              "type"
          >
        : FieldProps<TFormValues, TFieldValues, TResponseData, TFieldPath>,
): JSX.Element {
    // Get store of specified field
    const getField = createMemo(() =>
        initializeFieldStore(props.of, props.name),
    );

    // Create lifecycle of field
    // biome-ignore lint/suspicious/noExplicitAny: <explanation>
    createLifecycle(mergeProps({ getStore: getField }, props) as any);

    const content = children(() =>
        props.children(
            {
                get name() {
                    return props.name;
                },
                get value() {
                    return getField().value.get();
                },
                get error() {
                    return getField().error.get();
                },
                get active() {
                    return getField().active.get();
                },
                get touched() {
                    return getField().touched.get();
                },
                get dirty() {
                    return getField().dirty.get();
                },
            },
            {
                get name() {
                    return props.name;
                },
                get autofocus() {
                    return !!getField().error.get();
                },
                ref(element) {
                    // Add element to elements
                    getField().elements.set((elements) => [
                        ...elements,
                        element,
                    ]);

                    // Create effect that replaces initial input and input of field with
                    // initial input of element if both is "undefined", so that dirty
                    // state also resets to "false" when user removes input
                    createEffect(() => {
                        if (
                            element.type !== "radio" &&
                            getField().startValue.get() === undefined &&
                            untrack(getField().value.get) === undefined
                        ) {
                            const input = getElementInput(
                                element,
                                getField(),
                                // biome-ignore lint/suspicious/noExplicitAny: <explanation>
                                props.type as any,
                            );
                            getField().startValue.set(() => input);
                            getField().value.set(() => input);
                        }
                    });
                },
                onInput(event) {
                    handleFieldEvent(
                        props.of,
                        getField(),
                        props.name,
                        event,
                        ["touched", "input"],
                        getElementInput(
                            event.currentTarget,
                            getField(),
                            // biome-ignore lint/suspicious/noExplicitAny: <explanation>
                            props.type as any,
                        ),
                    );
                },
                onChange(event) {
                    handleFieldEvent(
                        props.of,
                        getField(),
                        props.name,
                        event,
                        ["change"],
                        getElementInput(
                            event.currentTarget,
                            getField(),
                            // biome-ignore lint/suspicious/noExplicitAny: <explanation>
                            props.type as any,
                        ),
                    );
                },
                onBlur(event) {
                    handleFieldEvent(props.of, getField(), props.name, event, [
                        "touched",
                        "blur",
                    ]);
                },
            },
        ),
    );

    return <>{content()}</>;
}
