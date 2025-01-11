/* eslint-disable @typescript-eslint/no-explicit-any */
import { children, createMemo, mergeProps, type JSX } from "solid-js";
import { createLifecycle } from "../primitives";
import type {
	FieldArrayPath,
	FieldValues,
	FormStore,
	IntersectValues,
	Maybe,
	MaybeArray,
	ResponseData,
	ValidateFieldArray,
} from "../types";
import { initializeFieldArrayStore } from "../utils";

/**
 * Value type ot the field store.
 */
export type FieldArrayStore<
	TFormValues extends FieldValues,
	TFieldValues extends IntersectValues<TFormValues>,
	TFieldArrayName extends FieldArrayPath<TFieldValues>,
> = {
	get name(): TFieldArrayName;
	get items(): number[];
	get error(): string;
	get active(): boolean;
	get touched(): boolean;
	get dirty(): boolean;
};

/**
 * Value type of the field array props.
 */
export type FieldArrayProps<
	TFormValues extends FieldValues,
	TFieldValues extends IntersectValues<TFormValues>,
	TResponseData extends ResponseData,
	TFieldArrayName extends FieldArrayPath<TFieldValues>,
> = {
	of: FormStore<TFormValues, TFieldValues, TResponseData>;
	name: TFieldArrayName;
	children: (
		store: FieldArrayStore<TFormValues, TFieldValues, TFieldArrayName>,
	) => JSX.Element;
	validate?: Maybe<MaybeArray<ValidateFieldArray<number[]>>>;
	keepActive?: Maybe<boolean>;
	keepState?: Maybe<boolean>;
};

/**
 * Headless field array that provides reactive properties and state.
 */
export function FieldArray<
	TFormValues extends FieldValues,
	TFieldValues extends IntersectValues<TFormValues>,
	TResponseData extends ResponseData,
	TFieldArrayName extends FieldArrayPath<TFieldValues>,
>(
	props: FieldArrayProps<
		TFormValues,
		TFieldValues,
		TResponseData,
		TFieldArrayName
	>,
): JSX.Element {
	// Get store of specified field array
	const getFieldArray = createMemo(() =>
		initializeFieldArrayStore(props.of, props.name),
	);

	// Create lifecycle of field array
	// biome-ignore lint/suspicious/noExplicitAny: <explanation>
	createLifecycle(mergeProps({ getStore: getFieldArray }, props) as any);
	const content = children(() =>
		props.children({
			get name() {
				return props.name;
			},
			get items() {
				return getFieldArray().items.get();
			},
			get error() {
				return getFieldArray().error.get();
			},
			get active() {
				return getFieldArray().active.get();
			},
			get touched() {
				return getFieldArray().touched.get();
			},
			get dirty() {
				return getFieldArray().dirty.get();
			},
		}),
	);

	return <>{content()}</>;
}
