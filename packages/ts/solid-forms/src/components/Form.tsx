import { batch, type JSX, splitProps } from "solid-js";
import { FormError } from "../exceptions";
import { getValues, setError, setResponse, validate } from "../methods";
import type {
	FieldArrayPath,
	FieldPath,
	FieldValues,
	FormStore,
	IntersectValues,
	Maybe,
	MaybePromise,
	ResponseData,
} from "../types";

/**
 * Value type of the submit event object.
 */
export type SubmitEvent = Event & {
	submitter: HTMLElement;
} & {
	currentTarget: HTMLFormElement;
	target: Element;
};

/**
 * Function type to handle the submission of the form.
 */
export type SubmitHandler<TFieldValues extends FieldValues> = (
	values: TFieldValues,
	event: SubmitEvent,
) => MaybePromise<unknown>;

/**
 * Value type of the form properties.
 */
export type FormProps<
	TFormValues extends FieldValues,
	TFieldValues extends IntersectValues<TFormValues>,
	TResponseData extends ResponseData,
> = Omit<JSX.FormHTMLAttributes<HTMLFormElement>, "onSubmit"> & {
	of: FormStore<TFormValues, TFieldValues, TResponseData>;
	onSubmit: SubmitHandler<TFormValues>;
	responseDuration?: Maybe<number>;
	keepResponse?: Maybe<boolean>;
	shouldActive?: Maybe<boolean>;
	shouldTouched?: Maybe<boolean>;
	shouldDirty?: Maybe<boolean>;
	shouldFocus?: Maybe<boolean>;
	children: JSX.Element;
};

/**
 * HTML form element that simplifies form submission and disables the browser's
 * default form validation.
 */
export function Form<
	TFormValues extends FieldValues,
	TFieldValues extends IntersectValues<TFormValues>,
	TResponseData extends ResponseData,
>(props: FormProps<TFormValues, TFieldValues, TResponseData>): JSX.Element {
	// Split props between local, options and other
	const [, options, other] = splitProps(
		props,
		["of"],
		[
			"keepResponse",
			"shouldActive",
			"shouldTouched",
			"shouldDirty",
			"shouldFocus",
		],
	);

	return (
		<form
			novalidate
			{...other}
			ref={props.of.internal.element.set}
			onSubmit={
				(async (event: SubmitEvent) => {
					// Prevent default behavior of browser
					event.preventDefault();

					// Destructure props
					const { of: form, onSubmit, responseDuration: duration } = props;

					batch(() => {
						// Reset response if it is not to be kept
						if (!options.keepResponse) {
							form.internal.response.set({});
						}

						// Increase submit count and set submitted and submitting to "true"
						form.internal.submitCount.set((count) => count + 1);
						form.internal.submitted.set(true);
						form.internal.submitting.set(true);
					});

					// Try to run submit actions if form is valid
					try {
						if (await validate(form, options as { shouldFocus: boolean })) {
							await onSubmit(
								getValues(
									form,
									options as { shouldActive: boolean },
								) as TFormValues,
								event,
							);
						}

						// If an error occurred, set error to fields and response
					} catch (error: unknown) {
						batch(() => {
							if (error instanceof FormError) {
								// biome-ignore lint/complexity/noForEach: <explanation>
								(
									Object.entries(error.errors) as [
										FieldPath<TFieldValues> | FieldArrayPath<TFieldValues>,
										Maybe<string>,
									][]
								).forEach(([name, error]) => {
									if (error) {
										setError(form, name, error, {
											...options,
											shouldFocus: false,
										} as {
											shouldFocus: boolean;
										});
									}
								});
							}
							if (!(error instanceof FormError) || error.message) {
								setResponse(
									form,
									{
										status: "error",
										message:
											error instanceof Error
												? error?.message
												: "An unknown error has occurred.",
									},
									{ duration: duration } as Maybe<
										Partial<{ duration: number }>
									>,
								);
							}
						});

						// Finally set submitting back to "false"
					} finally {
						form.internal.submitting.set(false);
					}
					// eslint-disable-next-line @typescript-eslint/no-explicit-any
					// biome-ignore lint/suspicious/noExplicitAny: <explanation>
				}) as any
			}
		/>
	);
}
