import React, { useCallback, useRef, useState } from "react";
import {
    StyleSheet,
    View,
    Button,
    Dimensions,
    Alert,
    Platform,
} from "react-native";
import { ThemedText } from "@/components/ThemedText";
import { ThemedView } from "@/components/ThemedView";
import ParallaxScrollView from "@/components/ParallaxScrollView";
import Ionicons from "@expo/vector-icons/Ionicons";
import { useMcuApi } from "@/api/hooks";
import {
    Canvas,
    Rect,
    type SkiaDomView,
    Text,
    matchFont,
} from "@shopify/react-native-skia";
import * as FileSystem from "expo-file-system";
import { useQuery } from "@tanstack/react-query";
import { celsiusToThermalColor } from "@/utilities/temperature";
import { resizeImage, type PixelMatrix } from "@/utilities/image";
import { getContrastingColor } from "@/utilities/colors";
import { getGetCurrentImageMotionJpegUrl } from "@/api";
import { WebView } from "react-native-webview";

async function saveImageToDisk(bytes: string, name: string) {
    const path = `${FileSystem.documentDirectory}/${name}.png`;

    await FileSystem.writeAsStringAsync(path, bytes, {
        encoding: "base64",
    });

    return path;
}

function getCameraPreviewDimensions() {
    const { width, height } = Dimensions.get("window");

    const min = Math.min(width, height) - 96;

    const perPixel = min / 8;

    return {
        width: min,
        height: min,
        perPixel,
    };
}

const cameraPreviewDimensions = getCameraPreviewDimensions();

const fontFamily = Platform.select({
    default: "monospace",
});

const fontSize = cameraPreviewDimensions.perPixel * 0.25;

const font = matchFont({
    fontFamily,
    fontSize,
    fontStyle: "normal",
    fontWeight: "bold",
});

function TemperaturePixel({
    color,
    x,
    y,
    sizePerPixel,
}: { color: string; x: number; y: number; sizePerPixel: number }) {
    return (
        <Rect
            height={sizePerPixel}
            width={sizePerPixel}
            x={x * sizePerPixel}
            y={y * sizePerPixel}
            color={color}
        />
    );
}

function TemperatureText({
    celsius,
    x,
    y,
    sizePerPixel,
}: {
    celsius: number;
    x: number;
    y: number;
    sizePerPixel: number;
}) {
    const text = `${celsius}°C`;
    const color = getContrastingColor(celsiusToThermalColor(celsius));

    return (
        <Text
            text={text}
            x={x * sizePerPixel + fontSize}
            y={y * sizePerPixel + (sizePerPixel / 2 + fontSize / 2)}
            font={font}
            color={color}
        />
    );
}

type Matrix = number[][];

function interpolateMatrix(matrix: Matrix, factor = 2): PixelMatrix {
    return resizeImage(
        matrix.map((row) =>
            row.map((celsius) => celsiusToThermalColor(celsius)),
        ),
        factor,
    );
}

const imageResizeFactor = 4;

export default function TabTwoScreen() {
    const mcuApi = useMcuApi();
    const canvasRef = useRef<SkiaDomView>(null);

    const saveImage = useCallback(async () => {
        if (canvasRef?.current != null) {
            Alert.alert("Saving Image", "Please wait...");
            const bytes = await canvasRef.current?.makeImageSnapshotAsync();

            const name = `ThermalScan_${new Date().toISOString()}`;

            if (bytes != null) {
                const uri = await saveImageToDisk(bytes.encodeToBase64(), name);
                return uri;
            }
        } else {
            Alert.alert("Error", "CanvasRef is null");
        }
    }, []);

    const query = useQuery({
        queryKey: ["thermal-image"],
        queryFn: () => mcuApi.getThermalImageCelsius(),
        refetchInterval: 500,
    });

    const [cameraMode, setCameraMode] = useState<"thermal" | "vision">(
        "thermal",
    );

    return (
        <ParallaxScrollView
            headerBackgroundColor={{ light: "#D0D0D0", dark: "#353636" }}
            headerImage={
                <View style={styles.headerIconContainer}>
                    <Ionicons
                        size={310}
                        name="camera-outline"
                        style={styles.headerImage}
                    />
                </View>
            }
        >
            <ThemedView style={styles.titleContainer}>
                <ThemedText
                    type="title"
                    style={{
                        display: "flex",
                        textAlign: "center",
                        alignSelf: "center",
                        justifyContent: "center",
                    }}
                >
                    Camera Preview
                </ThemedText>
            </ThemedView>
            {/* Placeholder for Capture Button */}
            <View style={styles.buttonContainerHorizontal}>
                <Button
                    title="Thermal"
                    disabled={cameraMode === "thermal"}
                    onPress={() => {
                        setCameraMode("thermal");
                    }}
                    color="#007BFF"
                />
                <Button
                    title="Vision"
                    disabled={cameraMode === "vision"}
                    onPress={() => {
                        setCameraMode("vision");
                    }}
                    color="#007BFF"
                />
            </View>

            <View style={styles.cameraContainer}>
                {/* Placeholder for Camera Preview */}
                {cameraMode === "thermal" && query.data && (
                    <Canvas style={styles.matrixPreview} ref={canvasRef}>
                        {interpolateMatrix(
                            query.data.data.data?.image ?? [],
                            imageResizeFactor,
                        ).map((cols, x) =>
                            cols.map((color, y) => (
                                <TemperaturePixel
                                    key={`${x}-${y}`}
                                    color={color}
                                    x={x}
                                    y={y}
                                    sizePerPixel={
                                        cameraPreviewDimensions.perPixel /
                                        imageResizeFactor
                                    }
                                />
                            )),
                        )}
                        {/*query.data.map((row, x) =>
                            row.map((celsius, y) => (
                                <TemperatureText
                                    key={`${x}-${y}`}
                                    celsius={celsius}
                                    x={x}
                                    y={y}
                                    sizePerPixel={
                                        cameraPreviewDimensions.perPixel
                                    }
                                />
                            )),
                        )*/}
                    </Canvas>
                )}

                {cameraMode === "vision" && <VideoFeed />}

                {/* Placeholder for Capture Button */}
                {cameraMode === "thermal" && (
                    <View style={styles.buttonContainer}>
                        <Button
                            title="Capture"
                            disabled={!query.data || canvasRef === null}
                            onPress={async () => {
                                try {
                                    const url = await saveImage();
                                    Alert.alert(
                                        "Image Saved",
                                        `Saved to '${url}'`,
                                    );
                                } catch (error) {
                                    Alert.alert(
                                        "Error",
                                        error instanceof Error
                                            ? error.message
                                            : String(error),
                                    );
                                }
                            }}
                            color="#007BFF"
                        />
                    </View>
                )}
            </View>
            <View style={styles.bottomPadding} />
        </ParallaxScrollView>
    );
}

const styles = StyleSheet.create({
    headerIconContainer: {
        alignItems: "center",
        justifyContent: "center",
    },
    headerImage: {
        color: "#808080",
    },
    titleContainer: {
        flexDirection: "row",
        gap: 8,
        paddingHorizontal: 16,
        alignSelf: "center",
    },
    cameraContainer: {
        flex: 1,
        justifyContent: "center",
        alignItems: "center",
        paddingHorizontal: 16,
    },
    cameraPlaceholder: {
        width: "100%",
        backgroundColor: "#EFEFEF",
        borderRadius: 8,
        justifyContent: "center",
        alignItems: "center",
        borderWidth: 1,
        borderColor: "#CCCCCC",
    },
    buttonContainer: {
        marginTop: 8,
        marginBottom: 16,
    },
    buttonContainerHorizontal: {
        display: "flex",
        flexDirection: "row",
        alignItems: "center",
        alignSelf: "center",
    },
    previewContainer: {
        marginTop: 16,
        width: "100%",
        alignItems: "center",
    },
    errorMessageText: {
        fontSize: 14,
        fontWeight: "400",
        width: "100%",
        color: "#ff0000",
        marginBottom: 4,
        alignItems: "center",
        justifyContent: "center",
        textAlign: "center",
        display: "flex",
    },
    matrixPreview: {
        width: cameraPreviewDimensions.width,
        height: cameraPreviewDimensions.height,
    },
    backgroundVideo: {
        width: 340,
        height: 260,
    },
    bottomPadding: {
        height: 80,
    },
});

function VideoFeed() {
    const mcuApi = useMcuApi();

    function formatHtml() {
        const url = `${mcuApi.config.baseUrl}${getGetCurrentImageMotionJpegUrl()}`;
        return `<html><body><img src="${url}" width="100%" style="background-color: white; min-height: 100%; min-width: 100%; position: fixed; top: 0; left: 0;"></body></html>`;
    }

    return (
        <View>
            <WebView
                style={styles.backgroundVideo}
                automaticallyAdjustContentInsets={true}
                scalesPageToFit={true}
                startInLoadingState={false}
                contentInset={{ top: 0, right: 0, left: 0, bottom: 0 }}
                scrollEnabled={false}
                source={{ html: formatHtml(), baseUrl: "/" }}
            />
        </View>
    );
}
