import React from 'react';
import { View, Text, StyleSheet, ScrollView } from 'react-native';

export default function InfoScreen() {
  return (
    <ScrollView contentContainerStyle={styles.container}>
      <View style={styles.content}>
        <Text style={styles.title}>App Information</Text>

        <Text style={styles.sectionTitle}>Version</Text>
        <Text style={styles.text}>1.0.0</Text>

        <Text style={styles.sectionTitle}>Developer</Text>
        <Text style={styles.text}>The Syntax Error</Text>

        <Text style={styles.sectionTitle}>Description</Text>
        <Text style={styles.text}>
          This app provides a platform for exploring various features like camera integration, navigation, and more.
        </Text>

        <Text style={styles.sectionTitle}>Contact</Text>
        <Text style={styles.text}>Email: einjhelaquino02@gmail.com</Text>

        <Text style={styles.sectionTitle}>Acknowledgments</Text>
        <Text style={styles.text}>
          Special thanks to the contributors and supporters who helped build this app.
        </Text>
      </View>
    </ScrollView>
  );
}

const styles = StyleSheet.create({
  container: {
    flexGrow: 1,
    padding: 20,
    backgroundColor: '#fff',
  },
  content: {
    marginBottom: 20,
  },
  title: {
    fontSize: 28,
    fontWeight: 'bold',
    marginBottom: 20,
  },
  sectionTitle: {
    fontSize: 18,
    fontWeight: '600',
    marginTop: 20,
  },
  text: {
    fontSize: 16,
    marginTop: 8,
    color: '#555',
  },
});
