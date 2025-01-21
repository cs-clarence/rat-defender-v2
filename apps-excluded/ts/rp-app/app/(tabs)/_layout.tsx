import { Tabs } from "expo-router";
import type React from "react";

import { TabBarIcon } from "@/components/navigation/TabBarIcon"; // Ensure this is correct
import { Colors } from "@/constants/Colors";
import { useColorScheme } from "@/hooks/useColorScheme";

const TabLayout: React.FC = () => {
    const colorScheme = useColorScheme();

    return (
        <Tabs
            screenOptions={{
                tabBarActiveTintColor: Colors[colorScheme ?? "light"].tint,
                headerShown: false,
                tabBarStyle: {
                    position: "absolute", // Floating tab bar
                    bottom: 20, // Adjust distance from the bottom
                    left: 20, // Adjust distance from the left
                    right: 20, // Adjust distance from the right
                    elevation: 5, // Add shadow for Android
                    shadowColor: "#000", // Shadow for iOS
                    shadowOffset: { width: 0, height: 5 },
                    shadowOpacity: 0.3,
                    shadowRadius: 10,
                    borderRadius: 15, // Rounded corners
                    height: 60, // Adjust height of the tab bar
                    backgroundColor: Colors[colorScheme ?? "light"].background, // Background color based on theme
                },
            }}
        >
            <Tabs.Screen
                name="index"
                options={{
                    title: "Home",
                    tabBarIcon: ({ color, focused }) => (
                        <TabBarIcon
                            name={focused ? "home" : "home-outline"} // Ionicons home icons
                            color={color}
                        />
                    ),
                }}
            />
            <Tabs.Screen
                name="history"
                options={{
                    title: "History",
                    tabBarIcon: ({ color, focused }) => (
                        <TabBarIcon
                            name={focused ? "list" : "list-outline"} // Ionicons camera icons
                            color={color}
                        />
                    ),
                }}
            />
            <Tabs.Screen
                name="explore"
                options={{
                    title: "Camera",
                    tabBarIcon: ({ color, focused }) => (
                        <TabBarIcon
                            name={focused ? "camera" : "camera-outline"} // Ionicons camera icons
                            color={color}
                        />
                    ),
                }}
            />
            <Tabs.Screen
                name="info"
                options={{
                    title: "Info",
                    tabBarIcon: ({ color, focused }) => (
                        <TabBarIcon
                            name={
                                focused
                                    ? "information-circle"
                                    : "information-circle-outline"
                            } // Ionicons info icons
                            color={color}
                        />
                    ),
                }}
            />

            {/* <Tabs.Screen
				name="settings"
				options={{
					title: "Settings",
					tabBarIcon: ({ color, focused }) => (
						<TabBarIcon
							name={focused ? "settings" : "settings-outline"} // Ionicons info icons
							color={color}
						/>
					),
				}}
			/> */}
        </Tabs>
    );
};

export default TabLayout;
