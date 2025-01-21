import { McuApi } from "@/api/mcu";
import { useStore } from "@/store";
import AsyncStorage from "@react-native-async-storage/async-storage";
import { router } from "expo-router";
import { useEffect, useState } from "react";
import {
    ScrollView,
    Text,
    StyleSheet,
    TextInput,
    Button,
    Alert,
} from "react-native";
import Zeroconf, { type Service } from "react-native-zeroconf";

const styles = StyleSheet.create({
    container: {
        flex: 1,
        backgroundColor: "#fff",
        alignItems: "center",
        justifyContent: "center",
    },
    setupText: {
        fontSize: 24,
        fontWeight: "bold",
    },
    baseUrlInput: {
        borderWidth: 1,
        borderColor: "#ccc",
        marginTop: 20,
        padding: 10,
        margin: 10,
        borderRadius: 5,
        width: "80%",
    },
});

function discoverMcu(): Promise<Service> {
    return new Promise((resolve, reject) => {
        const zeroconf = new Zeroconf({
            captureRejections: true,
        });

        Alert.alert("Scanning for devices...");

        zeroconf.scan("rat-defender", "tcp", ".local", "DNSSD");

        zeroconf.on("resolved", (service) => {
            Alert.alert("Service Resolved", JSON.stringify(service));
            resolve(service);
            zeroconf.stop();
        });

        zeroconf.on("found", async (service) => {
            Alert.alert("Service Found", JSON.stringify(service));
        });

        zeroconf.on("error", (error) => {
            Alert.alert("Error", error.message);
            reject(error);
        });
    });
}

export default function OnboardScreen() {
    const [baseUrl, setBaseUrl] = useState<string | null>(null);
    const [saving, setSaving] = useState(false);
    const setMcuBaseUrl = useStore.use.setMcuApiBaseUrl();

    // useEffect(() => {
    // 	discoverMcu()
    // 		.then(async (service) => {
    // 			const id = service.txt["x-rat-defender-id"];

    // 			if (typeof id === "string") {
    // 				const baseUrl = `http://${service.host}:${service.port}`;
    // 				Alert.alert("Service Resolved", JSON.stringify(service));
    // 				setMcuBaseUrl(baseUrl);
    // 			}
    // 		})
    // 		.catch(() => {});
    // }, [setMcuBaseUrl]);

    useEffect(() => {
        AsyncStorage.getItem("baseUrl").then(async (value) => {
            if (value !== null) {
                const mcu = new McuApi({
                    baseUrl: value,
                });
                try {
                    const response = await mcu.health();
                    if (response) {
                        setMcuBaseUrl(value);
                        router.replace("/(tabs)");
                    } else {
                        throw new Error("The URL provided is not valid");
                    }
                } catch (error) {
                    Alert.alert(
                        "The previous URL is now unavailable",
                        "Please provide a new URL.",
                    );
                }
            } else {
                await discoverMcu();
            }
        });
    }, [setMcuBaseUrl]);

    return (
        <ScrollView contentContainerStyle={styles.container}>
            <Text style={styles.setupText}>Setup Your Rat Defender App</Text>
            <TextInput
                style={styles.baseUrlInput}
                placeholder="Enter your base URL"
                onChangeText={(text) => setBaseUrl(text)}
            />
            <Button
                title={saving ? "Saving..." : "Continue"}
                disabled={saving || baseUrl === null || baseUrl === ""}
                onPress={async () => {
                    try {
                        if (baseUrl !== null) {
                            const mcu = new McuApi({
                                baseUrl,
                            });
                            setSaving(true);
                            if (await mcu.health()) {
                                await setMcuBaseUrl(baseUrl);
                                setSaving(false);
                                router.replace("/(tabs)");
                            } else {
                                throw new Error(
                                    "The URL provided is not valid",
                                );
                            }
                        }
                    } catch (error) {
                        console.error(error);
                        setSaving(false);

                        Alert.alert(
                            "The URL provided is not valid",
                            "Please enter a valid URL that is accessible from your device.",
                        );
                    }
                }}
            />
        </ScrollView>
    );
}
