import { useStore } from "@/store";
import { McuApi } from "./mcu";

const mcuApiChange = new Map<string, McuApi>();

export function useMcuApi() {
	const mcuApi = useMcuApiOptional();

	if (mcuApi === null) {
		throw new Error("No base URL set");
	}

	return mcuApi;
}

export function useMcuApiOptional() {
	const baseUrl = useStore.use.mcuApiBaseUrl();

	if (baseUrl === null) {
		return null;
	}

	if (!mcuApiChange.has(baseUrl)) {
		mcuApiChange.set(
			baseUrl,
			new McuApi({
				baseUrl,
			}),
		);
	}

	// biome-ignore lint/style/noNonNullAssertion: <explanation>
	return mcuApiChange.get(baseUrl)!;
}
