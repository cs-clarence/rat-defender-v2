import AsyncStorage from "@react-native-async-storage/async-storage";
import { create } from "zustand";
import { combine } from "zustand/middleware";
import { createSelectors } from "./create-selectors";

type State = {
	mcuApiBaseUrl: string | null;
};

const initialState: State = {
	mcuApiBaseUrl: null,
};

export const useStoreBase = create(
	combine({ ...initialState }, (set) => ({
		setMcuApiBaseUrl: async (baseUrl: string) => {
			AsyncStorage.setItem("baseUrl", baseUrl);
			set({
				mcuApiBaseUrl: baseUrl,
			});
		},
		removeMcuApiBaseUrl: async () => {
			AsyncStorage.removeItem("baseUrl");
			set({
				mcuApiBaseUrl: null,
			});
		},
		initialize: async () => {
			const baseUrl = await AsyncStorage.getItem("baseUrl");
			set({
				mcuApiBaseUrl: baseUrl,
			});
		},
		reset: async () => {
			set({ ...initialState });
		},
	})),
);

export const useStore = createSelectors(useStoreBase);
