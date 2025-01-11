/* eslint-disable @typescript-eslint/no-explicit-any */
import { type JSX, onCleanup, onMount } from "solid-js";
import { isServer } from "solid-js/web";
import { transformFiles } from "./helpers";
import type { FileUploaderDirective } from "./types";

declare module "solid-js" {
    // eslint-disable-next-line @typescript-eslint/no-namespace
    namespace JSX {
        interface Directives {
            fileUploader: FileUploaderDirective;
        }
    }
}

export const fileUploader = (
    element: HTMLInputElement,
    options: () => FileUploaderDirective,
) => {
    if (isServer) {
        return;
    }
    const { userCallback, setFiles } = options();

    onMount(() => {
        const onChange: JSX.EventHandler<HTMLInputElement, Event> = async (
            event,
        ) => {
            const parsedFiles = transformFiles(event.currentTarget.files);

            setFiles(parsedFiles);

            try {
                await userCallback(parsedFiles);
            } catch (error) {
                console.error(error);
            }
            return;
        };

        // biome-ignore lint/suspicious/noExplicitAny: <explanation>
        onCleanup(() => element.removeEventListener("change", onChange as any));

        // biome-ignore lint/suspicious/noExplicitAny: <explanation>
        element.addEventListener("change", onChange as any);
    });
};
