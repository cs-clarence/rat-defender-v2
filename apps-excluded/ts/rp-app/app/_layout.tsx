import {
    DarkTheme,
    DefaultTheme,
    ThemeProvider,
} from "@react-navigation/native";
import { useFonts } from "expo-font";
import { Stack } from "expo-router";
import * as SplashScreen from "expo-splash-screen";
import { useEffect } from "react";
import "react-native-reanimated";

import { useColorScheme } from "@/hooks/useColorScheme";
import { useStore } from "@/store";
import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { MD3DarkTheme, MD3LightTheme, PaperProvider } from "react-native-paper";

// Prevent the splash screen from auto-hiding before asset loading is complete.
SplashScreen.preventAutoHideAsync();

const queryClient = new QueryClient();

export default function RootLayout() {
    const colorScheme = useColorScheme();
    const [loaded] = useFonts({
        SpaceMono: require("../assets/fonts/SpaceMono-Regular.ttf"),
    });

    useEffect(() => {
        if (loaded) {
            SplashScreen.hideAsync();
        }
    }, [loaded]);

    const initialize = useStore((state) => state.initialize);

    useEffect(() => {
        initialize();
    }, [initialize]);

    if (!loaded) {
        return null;
    }

    return (
        <ThemeProvider
            value={colorScheme === "dark" ? DarkTheme : DefaultTheme}
        >
            <PaperProvider
                theme={colorScheme === "dark" ? MD3DarkTheme : MD3LightTheme}
            >
                <QueryClientProvider client={queryClient}>
                    <Stack>
                        <Stack.Screen
                            name="(onboard)/index"
                            options={{ headerShown: false, title: "Onboard" }}
                        />
                        <Stack.Screen
                            name="(tabs)"
                            options={{ headerShown: false }}
                        />
                        <Stack.Screen name="+not-found" />
                    </Stack>
                </QueryClientProvider>
            </PaperProvider>
        </ThemeProvider>
    );
}
