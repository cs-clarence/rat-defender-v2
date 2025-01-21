// LoadingScreen.tsx
import React from 'react';
import { StyleSheet, View, ActivityIndicator, Text } from 'react-native';

const LoadingScreen: React.FC = () => {
  return (
    <View style={styles.container}>
      <ActivityIndicator size="large" color="#0000ff" />
      <Text style={styles.loadingText}>Loading...</Text>
    </View>
  );
};

const styles = StyleSheet.create({
  container: {
    flex: 1,
    justifyContent: 'center',
    alignItems: 'center',
    backgroundColor: '#fff', // Change to your preferred background color
  },
  loadingText: {
    marginTop: 10,
    fontSize: 18,
    color: '#333', // Change to your preferred text color
  },
});

export default LoadingScreen;
