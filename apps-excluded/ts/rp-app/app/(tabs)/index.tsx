import React from "react";
import {
    StyleSheet,
    View,
    ScrollView,
    Image,
    Dimensions,
    RefreshControl,
} from "react-native";
import { LinearGradient } from "expo-linear-gradient"; // Ensure this is installed
import { HelloWave } from "@/components/HelloWave";
import { ThemedText } from "@/components/ThemedText";
import { ThemedView } from "@/components/ThemedView";
import LoadingScreen from "@/components/LoadingScreen"; // Adjust the import path as necessary
import { useMcuApi } from "@/api/hooks";
import { useQuery } from "@tanstack/react-query";
import type {
    getDailySummariesResponse,
    RatDetectionDaySummaryDto,
} from "@/api";

const { width } = Dimensions.get("window");

function getDailyAverage(data: getDailySummariesResponse) {
    const daily = data.data.data ?? [];

    return Math.round(
        daily.reduce((acc, cur) => {
            return acc + cur.count;
        }, 0) / daily.length,
    );
}

function groupByMonth(data: getDailySummariesResponse) {
    const daily = data.data.data ?? [];
    const grouped = daily.reduce(
        (acc, cur) => {
            const date = new Date(cur.date);
            const month = date.getMonth();
            const year = date.getFullYear();
            const key = `${year}-${month}`;

            if (!acc[key]) {
                acc[key] = [];
            }

            acc[key].push(cur);
            return acc;
        },
        {} as Record<string, RatDetectionDaySummaryDto[]>,
    );

    return Object.entries(grouped).map(([key, value]) => {
        return {
            date: key,
            detections: value,
        };
    });
}

function getMonthlyAverage(data: getDailySummariesResponse) {
    const byMonth = groupByMonth(data);

    return Math.round(
        byMonth.reduce((acc, cur) => {
            return (
                acc +
                cur.detections
                    .map((item) => {
                        return item.count;
                    })
                    .reduce((acc, cur) => {
                        return acc + cur;
                    }, 0)
            );
        }, 0) / byMonth.length,
    );
}

function getTotal(data: getDailySummariesResponse) {
    const daily = data.data.data ?? [];
    const numbers = daily.map((item) => item.count);

    return numbers.reduce((acc, cur) => {
        return acc + cur;
    }, 0);
}

export default function HomeScreen() {
    const mcuApi = useMcuApi();

    const query = useQuery({
        queryKey: ["daily-statistics"],
        queryFn: () => mcuApi.getDailyStatistics(),
        select: (data) => {
            return {
                total: getTotal(data),
                dailyAverage: getDailyAverage(data),
                monthlyAverage: getMonthlyAverage(data),
                lastUpdated: new Date().toLocaleTimeString(),
            };
        },
        refetchInterval: 3000,
    });

    if (query.isPending) {
        return <LoadingScreen />;
    }

    return (
        <ScrollView
            style={styles.container}
            contentContainerStyle={styles.contentContainer} // Use contentContainerStyle to manage the padding
            refreshControl={
                <RefreshControl
                    refreshing={query.isRefetching}
                    onRefresh={() => query.refetch()}
                    colors={["#0000ff"]} // Color for Android spinner
                />
            }
        >
            {/* Static Image with Gradient Overlay */}
            <View style={styles.headerContainer}>
                <Image
                    source={require("@/assets/images/rattt.png")} // Ensure correct image path
                    style={styles.headerImage}
                    resizeMode="cover"
                />
                <LinearGradient
                    colors={["rgba(0,0,0,0.3)", "rgba(0,0,0,0.6)"]}
                    style={styles.headerOverlay}
                />
            </View>

            {/* Content Section */}
            <ThemedView style={styles.titleContainer}>
                <ThemedText type="title" style={styles.titleText}>
                    Rat Detection Overview
                </ThemedText>
                <HelloWave />
            </ThemedView>

            {/* Live Preview Section */}
            <ThemedView style={styles.livePreviewContainer}>
                <ThemedText type="subtitle" style={styles.subtitleText}>
                    Live Preview:
                </ThemedText>
                <View style={styles.livePreviewCircleContainer}>
                    <View style={styles.livePreviewCircle}>
                        <View style={styles.ratCountContainer}>
                            <ThemedText
                                type="defaultSemiBold"
                                style={styles.dataValue}
                            >
                                {query.data?.total} {/* Live rat count */}
                            </ThemedText>
                            <ThemedText style={styles.dataLabel}>
                                Rats Detected
                            </ThemedText>
                        </View>
                        <ThemedText style={styles.timeLabel}>
                            Last Updated: {query.data?.lastUpdated}{" "}
                            {/* Timestamp */}
                        </ThemedText>
                    </View>
                </View>
            </ThemedView>

            {query.error !== null && (
                <ThemedText type="title" style={styles.errorMessageText}>
                    Error: {query.error.message}
                </ThemedText>
            )}

            {/* Data Sections - Static Display */}
            {["Daily", "Monthly"].map((period, index) => (
                <ThemedView key={period} style={styles.dataContainer}>
                    <ThemedText type="subtitle" style={styles.subtitleText}>
                        {`${period} Average:`}
                    </ThemedText>
                    <View style={styles.dataBox}>
                        <ThemedText
                            type="defaultSemiBold"
                            style={styles.dataValue}
                        >
                            {index === 0
                                ? query.data?.dailyAverage
                                : query.data?.monthlyAverage}{" "}
                            {/* Static values for design */}
                        </ThemedText>
                        <ThemedText style={styles.dataLabel}>
                            Rats Detected
                        </ThemedText>
                    </View>
                </ThemedView>
            ))}
        </ScrollView>
    );
}

const styles = StyleSheet.create({
    container: {
        flex: 1,
        backgroundColor: "#f0f0f0", // Light grey background for the entire app
    },
    contentContainer: {
        paddingBottom: 100, // Added padding to ensure content is not hidden behind the tab layout
    },
    headerContainer: {
        height: 300,
        width: "100%",
        position: "relative",
    },
    headerImage: {
        height: "100%",
        width: "100%",
        borderBottomLeftRadius: 30,
        borderBottomRightRadius: 30,
    },
    headerOverlay: {
        position: "absolute",
        height: "100%",
        width: "100%",
        top: 0,
        left: 0,
        borderBottomLeftRadius: 30,
        borderBottomRightRadius: 30,
        backgroundColor: "rgba(0, 0, 0, 0.5)", // Darker overlay for grey tone
    },
    titleContainer: {
        flexDirection: "row",
        alignItems: "center",
        justifyContent: "space-between",
        paddingHorizontal: 20,
        paddingVertical: 20,
        backgroundColor: "#e0e0e0", // Light grey
        marginTop: -30,
        borderRadius: 20,
        elevation: 5,
        shadowColor: "#000",
        shadowOffset: { width: 0, height: 5 },
        shadowOpacity: 0.15,
        shadowRadius: 10,
        marginHorizontal: 20,
    },
    titleText: {
        fontSize: 28,
        fontWeight: "700",
        color: "#333", // Dark grey text
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
    subtitleText: {
        fontSize: 20,
        fontWeight: "600",
        color: "#555", // Medium grey text
        marginBottom: 8,
    },
    dataContainer: {
        paddingHorizontal: 20,
        paddingVertical: 10,
    },
    dataBox: {
        backgroundColor: "#e0e0e0", // Light grey box
        borderRadius: 16,
        padding: 20,
        alignItems: "center",
        elevation: 5,
        shadowColor: "#000",
        shadowOffset: { width: 0, height: 4 },
        shadowOpacity: 0.2,
        shadowRadius: 8,
        marginBottom: 15,
        width: width - 40,
    },
    dataValue: {
        fontSize: 36,
        fontWeight: "bold",
        color: "#333", // Dark grey text
        lineHeight: 40,
        marginBottom: 4,
    },
    dataLabel: {
        fontSize: 16,
        color: "#666", // Medium grey text
    },
    livePreviewContainer: {
        paddingHorizontal: 20,
        paddingVertical: 10,
        marginTop: 20,
    },
    livePreviewCircleContainer: {
        alignItems: "center",
        marginVertical: 20,
    },
    livePreviewCircle: {
        width: 220,
        height: 220,
        borderRadius: 125,
        backgroundColor: "#f0f0f0", // Light grey background
        justifyContent: "center",
        alignItems: "center",
        borderColor: "#666", // Medium grey border
        borderWidth: 3,
        position: "relative",
    },
    ratCountContainer: {
        justifyContent: "center",
        alignItems: "center",
    },
    timeLabel: {
        fontSize: 14,
        color: "#888", // Lighter grey for the timestamp
        marginTop: 8,
    },
});
