// import { useMcuApi } from "@/api/hooks";
// import { useEffect, useState } from "react";
// import {
//     ScrollView,
//     View,
//     Text,
//     StyleSheet,
//     Button,
//     TextInput,
//     Alert,
//     type InputModeOptions,
//     ActivityIndicator,
// } from "react-native";
// import type {
//     DispenserConfig,
//     MotionDetectorConfig,
//     ThermalDetectorConfig,
//     TwilioSmsConfig,
// } from "@/api/mcu";
// import { Checkbox } from "expo-checkbox";
// import type React from "react";
// import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";

// function getErrorMessage(error: unknown) {
//     if (error instanceof Error) {
//         return error.message;
//     }

//     return String(error);
// }

// type FieldConfig<TFieldNames extends string> = Readonly<{
//     id?: string;
//     name: TFieldNames;
//     label: string;
//     placeholder?: string;
//     initialValue?: string;
//     obscure?: boolean;
//     showUnobscureCheckbox?: boolean;
//     multiline?: boolean;
//     inputMode?: InputModeOptions | "boolean";
//     helperText?: string;
// }>;

// type SubmitButtonConfig<TFieldNames extends string> = Readonly<{
//     label: string;
//     onPress: (data: Record<TFieldNames, string>) => Promise<void>;
//     disabled?: boolean;
// }>;

// type FormConfig<TFieldNames extends string> = Readonly<{
//     id?: string;
//     name: string;
//     fields: FieldConfig<TFieldNames>[];
//     submitButton?: SubmitButtonConfig<TFieldNames>;
// }>;

// type FieldState = Readonly<{
//     value: string | null;
//     isUnobscured: boolean;
// }>;

// function hasAnyBlankValues<TFieldNames extends string>(
//     fieldStates: Record<TFieldNames, FieldState>,
//     fieldConfigs: FieldConfig<TFieldNames>[],
// ) {
//     return fieldConfigs.some(
//         (fieldConfig) => !fieldStates[fieldConfig.name]?.value,
//     );
// }

// function isUnchanged<TFieldNames extends string>(
//     fieldStates: Record<TFieldNames, FieldState>,
//     fieldConfigs: FieldConfig<TFieldNames>[],
// ) {
//     return fieldConfigs.every((fieldConfig) => {
//         return (
//             fieldStates[fieldConfig.name]?.value === fieldConfig.initialValue
//         );
//     });
// }

// function getOnlyValues<TFieldNames extends string>(
//     fieldValues: Record<TFieldNames, FieldState>,
// ) {
//     const values = {} as Record<TFieldNames, string>;
//     for (const [fieldName, fieldValue] of Object.entries(fieldValues)) {
//         values[fieldName as TFieldNames] =
//             (fieldValue as FieldState).value ?? "";
//     }

//     return values;
// }

// function FormErrorText(props: { error: string }) {
//     return <Text style={styles.errorMessageText}>{props.error}</Text>;
// }

// function useFormFieldStates<TFieldNames extends string>(
//     fieldConfigs: FieldConfig<TFieldNames>[],
// ) {
//     const [fieldStates, setFieldStates] = useState<
//         Record<TFieldNames, FieldState>
//     >(
//         Object.fromEntries(
//             fieldConfigs.map((field) => [
//                 field.name,
//                 { value: field.initialValue ?? "", isUnobscured: false },
//             ]),
//         ) as Record<TFieldNames, FieldState>,
//     );

//     return {
//         get() {
//             return fieldStates;
//         },
//         set: setFieldStates,
//     };
// }

// function createFieldConfigs<TFieldNames extends string>(
//     fields: FieldConfig<TFieldNames>[],
// ) {
//     return fields;
// }

// function Form<TFieldNames extends string>(
//     props: FormConfig<TFieldNames> & {
//         children?: React.ReactNode;
//         onChange?: (data: Record<TFieldNames, string>) => void;
//         onChangeText?: (fieldName: TFieldNames, text: string) => void;
//         fieldStates: Record<TFieldNames, FieldState>;
//         setFieldStates: React.Dispatch<
//             React.SetStateAction<Record<TFieldNames, FieldState>>
//         >;
//     },
// ) {
//     function isShown(fieldConfig: FieldConfig<TFieldNames>) {
//         return props.fieldStates[fieldConfig.name]?.isUnobscured;
//     }
//     const id = props.id ?? `${props.name}-form`;
//     const [isSubmitting, setIsSubmitting] = useState(false);

//     return (
//         <View id={id} key={id} style={styles.sectionContainer}>
//             <Text style={styles.subTitle}>{props.name}</Text>
//             {props.fields.map((field) => {
//                 const id = field.id ?? `${field.name}-field`;
//                 return (
//                     <>
//                         <View
//                             id={id}
//                             key={id}
//                             style={styles.textInputContainer}
//                         >
//                             <Text style={styles.textInputLabel}>
//                                 {field.label}
//                             </Text>
//                             {field.inputMode === "boolean" && (
//                                 <Checkbox
//                                     value={
//                                         props.fieldStates[field.name]?.value ===
//                                         "true"
//                                     }
//                                     onValueChange={(value) => {
//                                         props.setFieldStates((state) => {
//                                             const f = state[field.name];

//                                             return {
//                                                 ...state,
//                                                 [field.name]: {
//                                                     ...f,
//                                                     value: value.toString(),
//                                                 },
//                                             };
//                                         });
//                                     }}
//                                 />
//                             )}
//                             {field.inputMode !== "boolean" && (
//                                 <TextInput
//                                     placeholder={field.placeholder}
//                                     style={styles.textInput}
//                                     value={
//                                         props.fieldStates[field.name]?.value ??
//                                         ""
//                                     }
//                                     onChangeText={(value) => {
//                                         props.setFieldStates((state) => {
//                                             const f = state[field.name];

//                                             return {
//                                                 ...state,
//                                                 [field.name]: {
//                                                     ...f,
//                                                     value,
//                                                 },
//                                             };
//                                         });
//                                         props.onChangeText?.(field.name, value);

//                                         props.onChange?.(
//                                             getOnlyValues(props.fieldStates),
//                                         );
//                                     }}
//                                     multiline={field.multiline}
//                                     secureTextEntry={
//                                         field.obscure && !isShown(field)
//                                     }
//                                     inputMode={field.inputMode}
//                                 />
//                             )}
//                             {field.helperText && (
//                                 <Text style={styles.helperText}>
//                                     {field.helperText}
//                                 </Text>
//                             )}
//                         </View>
//                         {field.showUnobscureCheckbox && (
//                             <View
//                                 id={`${id}-show-checkbox`}
//                                 key={`${id}-show-checkbox`}
//                                 style={styles.checkboxContainer}
//                             >
//                                 <Checkbox
//                                     style={styles.checkbox}
//                                     value={
//                                         props.fieldStates[field.name]
//                                             ?.isUnobscured ?? field.initialValue
//                                     }
//                                     onValueChange={(value) => {
//                                         props.setFieldStates((state) => {
//                                             const f = state[field.name];

//                                             return {
//                                                 ...state,
//                                                 [field.name]: {
//                                                     ...f,
//                                                     isUnobscured: value,
//                                                 },
//                                             };
//                                         });
//                                         props.onChange?.(
//                                             getOnlyValues(props.fieldStates),
//                                         );
//                                     }}
//                                 />
//                                 <Text style={styles.checkboxLabel}>
//                                     Show {field.label}
//                                 </Text>
//                             </View>
//                         )}
//                     </>
//                 );
//             })}
//             {props.children}

//             <Button
//                 title={props.submitButton?.label ?? "Submit"}
//                 color="#007BFF"
//                 disabled={
//                     props.submitButton?.disabled ??
//                     (isUnchanged(props.fieldStates, props.fields) ||
//                         hasAnyBlankValues(props.fieldStates, props.fields) ||
//                         isSubmitting)
//                 }
//                 onPress={async () => {
//                     if (props.submitButton?.onPress) {
//                         setIsSubmitting(true);
//                         await props.submitButton?.onPress(
//                             getOnlyValues(props.fieldStates),
//                         );
//                         setIsSubmitting(false);
//                     }
//                 }}
//             />
//         </View>
//     );
// }

// function useReinitializeFieldStatesEffect<
//     TFieldNames extends string,
//     TData extends Record<TFieldNames, unknown>,
// >(props: {
//     data: TData | undefined;
//     fieldStates: Record<TFieldNames, FieldState>;
//     setFieldStates: React.Dispatch<
//         React.SetStateAction<Record<TFieldNames, FieldState>>
//     >;
// }) {
//     // biome-ignore lint/correctness/useExhaustiveDependencies: <explanation>
//     useEffect(() => {
//         if (props.data) {
//             const copy = { ...props.fieldStates };

//             for (const [key, original] of Object.entries(copy)) {
//                 const newValue = props.data[key as TFieldNames];

//                 if (newValue) {
//                     copy[key as TFieldNames] = {
//                         ...(original as FieldState),
//                         value: newValue.toString(),
//                     };
//                 }
//             }

//             props.setFieldStates(copy);
//         }
//     }, [props.data]);
// }

// function WiFiSettings() {
//     const mcuApi = useMcuApi();

//     const connect = useMutation({
//         mutationKey: ["connect-wifi"],
//         mutationFn: async (data: Record<"ssid" | "psk", string>) => {
//             const { ssid, psk } = data;
//             if (ssid !== null && psk !== null) {
//                 return await mcuApi.connectToWiFi({
//                     ssid,
//                     psk,
//                 });
//             }
//             return false;
//         },
//         onSuccess: (ok) => {
//             if (ok) {
//                 Alert.alert("Success", "WiFi connection successful");
//             } else {
//                 Alert.alert("Error", "Failed to connect to WiFi");
//             }
//         },
//         onError: (error) => {
//             Alert.alert("Error", getErrorMessage(error));
//         },
//     });

//     const fieldConfigs = createFieldConfigs([
//         {
//             name: "ssid",
//             label: "SSID",
//             placeholder: "Enter your WiFi SSID",
//         },
//         {
//             name: "psk",
//             label: "PSK",
//             placeholder: "Enter your WiFi PSK",
//             obscure: true,
//             showUnobscureCheckbox: true,
//         },
//     ]);

//     const { get: fieldStates, set: setFieldStates } =
//         useFormFieldStates(fieldConfigs);

//     return (
//         <Form
//             key="wifi-form"
//             id="wifi-form"
//             name="WiFi"
//             fields={fieldConfigs}
//             fieldStates={fieldStates()}
//             setFieldStates={setFieldStates}
//             submitButton={{
//                 label: "Connect",
//                 onPress: async (data) => {
//                     await connect.mutateAsync(data);
//                 },
//             }}
//         />
//     );
// }

// const SMS_CONFIG_KEY = "sms-config";

// function SmsSettings() {
//     const mcuApi = useMcuApi();

//     const queryClient = useQueryClient();

//     const smsConfig = useQuery({
//         queryKey: [SMS_CONFIG_KEY],
//         queryFn: () => mcuApi.getTwilioSmsConfig(),
//     });

//     const updateSmsConfig = useMutation({
//         mutationKey: ["update-sms-config"],
//         onSuccess: (ok) => {
//             if (ok) {
//                 queryClient.invalidateQueries({
//                     queryKey: [SMS_CONFIG_KEY],
//                 });
//                 Alert.alert(
//                     "Success",
//                     "Twilio SMS config updated successfully",
//                 );
//             } else {
//                 Alert.alert("Error", "Failed to update Twilio SMS config");
//             }
//         },
//         onError: (error) => {
//             Alert.alert("Error", getErrorMessage(error));
//         },
//         mutationFn: async (data: Record<keyof TwilioSmsConfig, string>) => {
//             return await mcuApi.setTwilioSmsConfig({
//                 accountSid: data.accountSid,
//                 authToken: data.authToken,
//                 fromPhoneNumber: data.fromPhoneNumber,
//                 messageBody: data.messageBody,
//                 throttle: smsConfig.data?.throttle ?? 0,
//                 toPhoneNumber: data.toPhoneNumber,
//             });
//         },
//     });

//     const getInitialValue = (fieldName: keyof TwilioSmsConfig) => {
//         return smsConfig.data?.[fieldName].toString();
//     };

//     const fieldConfigs = createFieldConfigs<keyof TwilioSmsConfig>([
//         {
//             name: "accountSid",
//             label: "Account SID",
//             placeholder: "Enter your Twilio Account SID",
//             initialValue: getInitialValue("accountSid"),
//         },
//         {
//             name: "authToken",
//             label: "Auth Token",
//             placeholder: "Enter your Twilio Auth Token",
//             initialValue: getInitialValue("authToken"),
//             obscure: true,
//             showUnobscureCheckbox: true,
//         },
//         {
//             name: "fromPhoneNumber",
//             label: "From Phone Number",
//             initialValue: getInitialValue("fromPhoneNumber"),
//             placeholder: "Enter your Twilio From Phone Number",
//         },
//         {
//             name: "toPhoneNumber",
//             label: "To Phone Number",
//             initialValue: getInitialValue("toPhoneNumber"),
//             placeholder: "Enter your Twilio To Phone Number",
//         },
//         {
//             name: "messageBody",
//             label: "Message Body",
//             initialValue: getInitialValue("messageBody"),
//             placeholder: "Enter your Twilio Message Body",
//             multiline: true,
//         },
//     ]);

//     const { get: fieldStates, set: setFieldStates } =
//         useFormFieldStates(fieldConfigs);

//     useReinitializeFieldStatesEffect({
//         data: smsConfig.data,
//         fieldStates: fieldStates(),
//         setFieldStates,
//     });

//     return (
//         <Form<keyof TwilioSmsConfig>
//             key="wifi-form"
//             id="sms-form"
//             name="Twilio SMS"
//             fields={fieldConfigs}
//             fieldStates={fieldStates()}
//             setFieldStates={setFieldStates}
//             submitButton={{
//                 label: "Save",
//                 onPress: async (data) => {
//                     await updateSmsConfig.mutateAsync(data);
//                 },
//             }}
//         >
//             {smsConfig.isError && (
//                 <FormErrorText error={smsConfig.error.message} />
//             )}
//             {smsConfig.isLoading && <ActivityIndicator />}
//         </Form>
//     );
// }

// const DISPENSER_CONFIG_KEY = "dispenser-config";

// function DispsenserSettings() {
//     const mcuApi = useMcuApi();
//     const queryClient = useQueryClient();

//     const dispenserConfig = useQuery({
//         queryKey: [DISPENSER_CONFIG_KEY],
//         queryFn: () => mcuApi.getDispenserConfig(),
//     });

//     const updateDispenserConfig = useMutation({
//         mutationFn: async (data: Record<keyof DispenserConfig, string>) => {
//             return await mcuApi.setDispenserConfig({
//                 angleDelayMs: Number.parseInt(data.angleDelayMs),
//                 dispenseAngle: Number.parseInt(data.dispenseAngle),
//                 dispenseIntervalMs:
//                     dispenserConfig.data?.dispenseIntervalMs ?? 0,
//             });
//         },
//         mutationKey: ["update-dispenser-config"],
//         onSuccess: (ok) => {
//             if (ok) {
//                 queryClient.invalidateQueries({
//                     queryKey: [DISPENSER_CONFIG_KEY],
//                 });
//                 Alert.alert("Success", "Dispenser config updated successfully");
//             } else {
//                 Alert.alert("Error", "Failed to update Dispenser config");
//             }
//         },
//         onError: (error) => {
//             Alert.alert("Error", getErrorMessage(error));
//         },
//     });

//     function getInitialValue(fieldName: keyof DispenserConfig) {
//         return dispenserConfig.data?.[fieldName].toString();
//     }

//     const fieldConfigs = createFieldConfigs<keyof DispenserConfig>([
//         {
//             name: "angleDelayMs",
//             label: "Rotation Delay (ms)",
//             inputMode: "numeric",
//             helperText: "Delay between each rotation in milliseconds",
//             initialValue: getInitialValue("angleDelayMs"),
//         },
//         {
//             name: "dispenseAngle",
//             label: "Rotation Angle (°)",
//             inputMode: "numeric",
//             helperText: "Angle at which the dispenser rotates in degrees",
//             initialValue: getInitialValue("dispenseAngle"),
//         },
//     ]);

//     const { get: fieldStates, set: setFieldStates } =
//         useFormFieldStates(fieldConfigs);

//     useReinitializeFieldStatesEffect({
//         data: dispenserConfig.data,
//         fieldStates: fieldStates(),
//         setFieldStates,
//     });

//     return (
//         <Form<keyof DispenserConfig>
//             name="Dispenser"
//             key="dispsenser-form"
//             id="dispsenser-form"
//             submitButton={{
//                 label: "Save",
//                 onPress: async (data) => {
//                     await updateDispenserConfig.mutateAsync(data);
//                 },
//             }}
//             fieldStates={fieldStates()}
//             setFieldStates={setFieldStates}
//             fields={fieldConfigs}
//         >
//             {dispenserConfig.isError && (
//                 <FormErrorText error={dispenserConfig.error.message} />
//             )}
//             {dispenserConfig.isLoading && <ActivityIndicator />}
//         </Form>
//     );
// }

// const THERMAL_DETECTOR_CONFIG_KEY = "thermal-detector-config";

// function ThermalDetectorSettings() {
//     const mcuApi = useMcuApi();
//     const queryClient = useQueryClient();

//     const thermalDetectorConfig = useQuery({
//         queryKey: [THERMAL_DETECTOR_CONFIG_KEY],
//         queryFn: () => mcuApi.getThermalDetectorConfig(),
//     });

//     const updateThermalDetectorConfig = useMutation({
//         mutationFn: async (
//             data: Record<keyof ThermalDetectorConfig, string>,
//         ) => {
//             return await mcuApi.setThermalDetectorConfig({
//                 enabled: data.enabled === "true",
//                 minTempCelsius: Number.parseInt(data.minTempCelsius),
//                 maxTempCelsius: Number.parseInt(data.maxTempCelsius),
//             });
//         },
//         mutationKey: ["update-thermal-detector-config"],
//         onSuccess: (ok) => {
//             if (ok) {
//                 queryClient.invalidateQueries({
//                     queryKey: [THERMAL_DETECTOR_CONFIG_KEY],
//                 });
//                 Alert.alert(
//                     "Success",
//                     "Thermal detector config updated successfully",
//                 );
//             } else {
//                 Alert.alert(
//                     "Error",
//                     "Failed to update Thermal detector config",
//                 );
//             }
//         },
//         onError: (error) => {
//             Alert.alert("Error", getErrorMessage(error));
//         },
//     });

//     function getInitialValue(fieldName: keyof ThermalDetectorConfig) {
//         return thermalDetectorConfig.data?.[fieldName].toString();
//     }

//     const fieldConfigs = createFieldConfigs<keyof ThermalDetectorConfig>([
//         {
//             name: "enabled",
//             label: "Enabled",
//             inputMode: "boolean",
//             helperText: "Enable or disable the thermal detector",
//             initialValue: getInitialValue("enabled"),
//         },
//         {
//             name: "minTempCelsius",
//             label: "Minimum Temperature (°C)",
//             inputMode: "numeric",
//             helperText: "Minimum temperature in Celsius",
//             initialValue: getInitialValue("minTempCelsius"),
//         },
//         {
//             name: "maxTempCelsius",
//             label: "Maximum Temperature (°C)",
//             inputMode: "numeric",
//             helperText: "Maximum temperature in Celsius",
//             initialValue: getInitialValue("maxTempCelsius"),
//         },
//     ]);

//     const { get: fieldStates, set: setFieldStates } =
//         useFormFieldStates(fieldConfigs);

//     useReinitializeFieldStatesEffect({
//         data: thermalDetectorConfig.data,
//         fieldStates: fieldStates(),
//         setFieldStates,
//     });

//     return (
//         <Form<keyof ThermalDetectorConfig>
//             name="Thermal Detector"
//             key="thermal-detector-form"
//             id="thermal-detector-form"
//             submitButton={{
//                 label: "Save",
//                 onPress: async (data) => {
//                     await updateThermalDetectorConfig.mutateAsync(data);
//                 },
//             }}
//             fields={fieldConfigs}
//             fieldStates={fieldStates()}
//             setFieldStates={setFieldStates}
//         >
//             {thermalDetectorConfig.isError && (
//                 <FormErrorText error={thermalDetectorConfig.error.message} />
//             )}
//             {thermalDetectorConfig.isLoading && <ActivityIndicator />}
//         </Form>
//     );
// }

// const MOTION_DETECTOR_CONFIG_KEY = "motion-detector-config";

// function MotionDetectorSettings() {
//     const mcuApi = useMcuApi();
//     const queryClient = useQueryClient();

//     const motionDetectorConfig = useQuery({
//         queryKey: [MOTION_DETECTOR_CONFIG_KEY],
//         queryFn: () => mcuApi.getMotionDetectorConfig(),
//     });

//     const updateMotionDetectorConfig = useMutation({
//         mutationFn: async (
//             data: Record<keyof MotionDetectorConfig, string>,
//         ) => {
//             return await mcuApi.setMotionDetectorConfig({
//                 enabled: data.enabled === "true",
//                 buzzDelayMs: Number.parseInt(data.buzzDelayMs),
//                 buzzDurationMs: Number.parseInt(data.buzzDurationMs),
//                 buzzTone: Number.parseInt(data.buzzTone),
//                 buzzSpawnTask: data.buzzSpawnTask === "true",
//                 buzzSpawnTaskMaxCount:
//                     motionDetectorConfig.data?.buzzSpawnTaskMaxCount ?? 3,
//             });
//         },
//         mutationKey: ["update-motion-detector-config"],
//         onSuccess: (ok) => {
//             if (ok) {
//                 queryClient.invalidateQueries({
//                     queryKey: [MOTION_DETECTOR_CONFIG_KEY],
//                 });
//                 Alert.alert(
//                     "Success",
//                     "Motion detector config updated successfully",
//                 );
//             } else {
//                 Alert.alert("Error", "Failed to update Motion detector config");
//             }
//         },
//         onError: (error) => {
//             Alert.alert("Error", getErrorMessage(error));
//         },
//     });

//     function getInitialValue(fieldName: keyof MotionDetectorConfig) {
//         return motionDetectorConfig.data?.[fieldName].toString();
//     }

//     const fieldConfigs = createFieldConfigs<keyof MotionDetectorConfig>([
//         {
//             name: "enabled",
//             label: "Enabled",
//             inputMode: "boolean",
//             helperText: "Enable or disable the motion detector",
//             initialValue: getInitialValue("enabled"),
//         },
//         {
//             name: "buzzDelayMs",
//             label: "Buzz Delay (ms)",
//             inputMode: "numeric",
//             helperText: "Time before Buzz is triggered in milliseconds",
//             initialValue: getInitialValue("buzzDelayMs"),
//         },
//         {
//             name: "buzzDurationMs",
//             label: "Buzz Duration (ms)",
//             inputMode: "numeric",
//             helperText: "How long to buzz for in milliseconds",
//             initialValue: getInitialValue("buzzDurationMs"),
//         },
//         {
//             name: "buzzTone",
//             label: "Buzz Tone",
//             inputMode: "numeric",
//             initialValue: getInitialValue("buzzTone"),
//         },
//         {
//             name: "buzzSpawnTask",
//             label: "Buzz Spawn Task",
//             inputMode: "boolean",
//             initialValue: getInitialValue("buzzSpawnTask"),
//         },
//         {
//             name: "buzzSpawnTaskMaxCount",
//             label: "Buzz Spawn Task Max Count",
//             inputMode: "numeric",
//             initialValue: getInitialValue("buzzSpawnTaskMaxCount"),
//         },
//     ]);

//     const { get: fieldStates, set: setFieldStates } =
//         useFormFieldStates(fieldConfigs);

//     useReinitializeFieldStatesEffect({
//         data: motionDetectorConfig.data,
//         fieldStates: fieldStates(),
//         setFieldStates,
//     });

//     return (
//         <Form<keyof MotionDetectorConfig>
//             name="Motion Detector"
//             key="motion-detector-form"
//             id="motion-detector-form"
//             submitButton={{
//                 label: "Save",
//                 onPress: async (data) => {
//                     await updateMotionDetectorConfig.mutateAsync(data);
//                 },
//             }}
//             fields={fieldConfigs}
//             fieldStates={fieldStates()}
//             setFieldStates={setFieldStates}
//         >
//             {motionDetectorConfig.isError && (
//                 <FormErrorText error={motionDetectorConfig.error.message} />
//             )}
//             {motionDetectorConfig.isLoading && <ActivityIndicator />}
//         </Form>
//     );
// }

// function DeviceSettings() {
//     const mcuApi = useMcuApi();

//     const restartDevice = useMutation({
//         mutationKey: ["restart-device"],
//         retry: false,
//         mutationFn: async () => {
//             return await mcuApi.deviceRestart();
//         },
//     });

//     const resetDevice = useMutation({
//         mutationKey: ["reset-device"],
//         retry: false,
//         mutationFn: async () => {
//             return await mcuApi.deviceReset();
//         },
//     });

//     return (
//         <View style={styles.sectionContainer}>
//             <Text style={styles.subTitle}>Device Settings</Text>
//             <Button
//                 title="Restart"
//                 color="#007BFF"
//                 disabled={restartDevice.isPending}
//                 onPress={() => {
//                     Alert.alert(
//                         "Restart Device",
//                         "Are you sure you want to restart the device?",
//                         [
//                             {
//                                 text: "Cancel",
//                                 style: "cancel",
//                             },
//                             {
//                                 text: "Restart",
//                                 onPress: async () => {
//                                     await restartDevice.mutateAsync();
//                                 },
//                                 style: "destructive",
//                             },
//                         ],
//                     );
//                 }}
//             />
//             <Button
//                 title="Reset"
//                 color="red"
//                 disabled={resetDevice.isPending}
//                 onPress={() => {
//                     Alert.alert(
//                         "Reset Device",
//                         "Are you sure you want to reset the device? This will erase all data on the device.",
//                         [
//                             {
//                                 text: "Cancel",
//                                 style: "cancel",
//                             },
//                             {
//                                 text: "Reset",
//                                 onPress: async () => {
//                                     await resetDevice.mutateAsync();
//                                 },
//                                 style: "destructive",
//                             },
//                         ],
//                     );
//                 }}
//             />
//         </View>
//     );
// }

// function DebugSection() {
//     const mcuApi = useMcuApi();

//     const recordDetection = useMutation({
//         mutationFn: async () => {
//             return await mcuApi.recordDetection();
//         },
//         onSuccess: () => {
//             Alert.alert("Detection recorded");
//         },
//         onError: () => {
//             Alert.alert("Couldn't record detection");
//         },
//     });

//     const forceThermalDetection = useMutation({
//         mutationFn: async () => {
//             return await mcuApi.forceThermalDetection();
//         },
//         onSuccess: () => {
//             Alert.alert("Thermal detection forced");
//         },
//         onError: () => {
//             Alert.alert("Couldn't force thermal detection");
//         },
//     });

//     const forceMotionDetection = useMutation({
//         mutationFn: async () => {
//             return await mcuApi.forceMotionDetection();
//         },
//         onSuccess: () => {
//             Alert.alert("Motion detection forced");
//         },
//         onError: () => {
//             Alert.alert("Couldn't motion thermal detection");
//         },
//     });

//     const dispense = useMutation({
//         mutationFn: async () => {
//             return await mcuApi.dispense(1);
//         },
//         onError: () => {
//             Alert.alert("Dispense error");
//         },
//     });

//     return (
//         <View style={styles.sectionContainer}>
//             <Text style={styles.subTitle}>Debug</Text>

//             <Button
//                 title="Record detection"
//                 disabled={recordDetection.isPending}
//                 onPress={async () => await recordDetection.mutateAsync()}
//             />
//             <Button
//                 title="Dispense"
//                 disabled={dispense.isPending}
//                 onPress={async () => await dispense.mutateAsync()}
//             />
//             <Button
//                 title="Force thermal detection"
//                 disabled={forceThermalDetection.isPending}
//                 onPress={async () => await forceThermalDetection.mutateAsync()}
//             />
//             <Button
//                 title="Force motion detection"
//                 disabled={forceMotionDetection.isPending}
//                 onPress={async () => await forceMotionDetection.mutateAsync()}
//             />
//         </View>
//     );
// }

// const SHOW_DEBUG_SECTION = true;

// export default function SettingsScreen() {
//     return (
//         <ScrollView contentContainerStyle={styles.container}>
//             <Text style={styles.title}>Settings</Text>
//             <WiFiSettings />
//             <SmsSettings />
//             <DispsenserSettings />
//             <ThermalDetectorSettings />
//             <MotionDetectorSettings />
//             {SHOW_DEBUG_SECTION && <DebugSection />}
//             <DeviceSettings />
//             <View
//                 style={{
//                     marginBottom: 48,
//                 }}
//             />
//         </ScrollView>
//     );
// }

// const styles = StyleSheet.create({
//     container: {
//         flexGrow: 1,
//         padding: 20,
//         backgroundColor: "#fff",
//         gap: 24,
//     },
//     title: {
//         fontSize: 24,
//     },
//     subTitle: {
//         fontSize: 18,
//     },
//     sectionContainer: {
//         padding: 24,
//         borderWidth: 1,
//         borderColor: "#ccc",
//         display: "flex",
//         flexDirection: "column",
//         gap: 12,
//     },
//     textInputContainer: {
//         display: "flex",
//         flexDirection: "column",
//         alignItems: "flex-start",
//         gap: 4,
//     },
//     textInput: {
//         borderWidth: 1,
//         borderColor: "#ccc",
//         padding: 10,
//         borderRadius: 5,
//         width: "100%",
//     },
//     textInputLabel: {
//         // grey
//         color: "#8F8F8F",
//         fontSize: 12,
//     },
//     helperText: {
//         // grey
//         color: "#8F8F8F",
//         fontSize: 10,
//     },
//     checkboxContainer: {
//         display: "flex",
//         flexDirection: "row",
//         alignItems: "center",
//         justifyContent: "flex-start",
//         gap: 8,
//     },
//     checkboxLabel: {
//         fontSize: 12,
//         color: "#8F8F8F",
//     },
//     checkbox: {
//         width: 20,
//         height: 20,
//         borderColor: "#ccc",
//         borderWidth: 1,
//         borderRadius: 5,
//     },
//     errorMessageText: {
//         fontSize: 14,
//         fontWeight: "400",
//         width: "100%",
//         color: "#ff0000",
//         marginBottom: 4,
//         alignItems: "center",
//         justifyContent: "center",
//         textAlign: "center",
//         display: "flex",
//     },
// });
