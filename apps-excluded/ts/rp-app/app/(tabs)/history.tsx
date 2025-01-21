import { useMcuApi } from "@/api/hooks";
import { useQuery } from "@tanstack/react-query";
import {
    Text,
    StyleSheet,
    View,
    ScrollView,
    type StyleProp,
    type ViewStyle,
} from "react-native";
import {
    Card,
    DataTable,
    ActivityIndicator,
    Text as PaperText,
    useTheme,
} from "react-native-paper";

function Placeholder({
    children,
    style,
}: { children: React.ReactNode; style?: StyleProp<ViewStyle> }) {
    return (
        <View
            style={[
                {
                    display: "flex",
                    flexDirection: "column",
                    justifyContent: "center",
                    alignItems: "center",
                    gap: 8,
                    height: 128,
                },
                style,
            ]}
        >
            {children}
        </View>
    );
}

export default function History() {
    const mcuApi = useMcuApi();
    const theme = useTheme();

    const data = useQuery({
        queryKey: ["daily-statistics"],
        queryFn: () => mcuApi.getDailyStatistics(),
        select: (data) => {
            return data.data.data;
        },
        refetchInterval: 3000,
    });

    return (
        <View style={styles.container}>
            <Text style={styles.titleText}>History</Text>
            <Card style={styles.tableContainer}>
                <Card.Content>
                    <DataTable>
                        <DataTable.Header>
                            <DataTable.Title>Date</DataTable.Title>
                            <DataTable.Title numeric>
                                Detection Count
                            </DataTable.Title>
                        </DataTable.Header>
                        {data.isPending && (
                            <Placeholder>
                                <ActivityIndicator />
                                <PaperText variant="bodySmall">
                                    Loading...
                                </PaperText>
                            </Placeholder>
                        )}
                        {data.isError && (
                            <Placeholder
                                style={{
                                    gap: 8,
                                }}
                            >
                                <PaperText
                                    style={{
                                        color: theme.colors.error,
                                    }}
                                >
                                    Error: {data.error.message}
                                </PaperText>
                            </Placeholder>
                        )}
                        {data.data && (
                            <ScrollView>
                                {data.data.map((data) => (
                                    <DataTable.Row key={data.date}>
                                        <DataTable.Cell>
                                            {new Date(
                                                data.date,
                                            ).toLocaleDateString()}
                                        </DataTable.Cell>
                                        <DataTable.Cell numeric>
                                            {data.count}
                                        </DataTable.Cell>
                                    </DataTable.Row>
                                ))}
                                <View
                                    style={{
                                        height: 196,
                                    }}
                                />
                            </ScrollView>
                        )}
                    </DataTable>
                </Card.Content>
            </Card>
        </View>
    );
}

const styles = StyleSheet.create({
    container: {
        display: "flex",
        flexDirection: "column",
        maxHeight: "100%",
    },
    titleText: {
        margin: 8,
        fontSize: 24,
        fontWeight: "500",
    },
    tableContainer: {
        flexShrink: 1,
        marginInline: 12,
        height: "100%",
    },
});
