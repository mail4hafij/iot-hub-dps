﻿// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Azure.Devices.Client;
using Microsoft.Azure.Devices.Provisioning.Client;
using Microsoft.Azure.Devices.Provisioning.Client.Transport;
using Microsoft.Azure.Devices.Shared;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Security.Cryptography.X509Certificates;

namespace ContainerDevice
{
    class Program
    {
        // Azure Device Provisioning Service (DPS) ID Scope
        private static string dpsIdScope = "";

        // Certificate (PFX) File Name
        private static string certificateFileName = "new-device.cert.pfx";

        // Certificate (PFX) Password
        private static string certificatePassword = "1234";
        // NOTE: For the purposes of this example, the certificatePassword is
        // hard coded. In a production device, the password will need to be stored
        // in a more secure manner. Additionally, the certificate file (PFX) should
        // be stored securely on a production device using a Hardware Security Module.

        private const string GlobalDeviceEndpoint = "global.azure-devices-provisioning.net";

        private static int telemetryDelay = 1;

        private static DeviceClient deviceClient;

        // INSERT Main method below here
        public static async Task Main(string[] args)
        {
            X509Certificate2 certificate = LoadProvisioningCertificate();

            using (var security = new SecurityProviderX509Certificate(certificate))
            using (var transport = new ProvisioningTransportHandlerAmqp(TransportFallbackType.TcpOnly))
            {
                ProvisioningDeviceClient provClient =
                    ProvisioningDeviceClient.Create(GlobalDeviceEndpoint, dpsIdScope, security, transport);

                using (deviceClient = await ProvisionDevice(provClient, security))
                {
                    await deviceClient.OpenAsync().ConfigureAwait(false);

                    // INSERT Setup OnDesiredPropertyChanged Event Handling below here
                    await deviceClient.SetDesiredPropertyUpdateCallbackAsync(OnDesiredPropertyChanged, null).ConfigureAwait(false);

                    // INSERT Load Device Twin Properties below here
                    var twin = await deviceClient.GetTwinAsync().ConfigureAwait(false);
                    await OnDesiredPropertyChanged(twin.Properties.Desired, null);

                    // Start reading and sending device telemetry
                    Console.WriteLine("Start reading and sending device telemetry...");
                    await SendDeviceToCloudMessagesAsync();

                    await deviceClient.CloseAsync().ConfigureAwait(false);
                }
            }
        }

        // INSERT LoadProvisioningCertificate method below here
        private static X509Certificate2 LoadProvisioningCertificate()
        {
            var certificateCollection = new X509Certificate2Collection();
            certificateCollection.Import(certificateFileName, certificatePassword, X509KeyStorageFlags.UserKeySet);

            X509Certificate2 certificate = null;

            foreach (X509Certificate2 element in certificateCollection)
            {
                Console.WriteLine($"Found certificate: {element?.Thumbprint} {element?.Subject}; PrivateKey: {element?.HasPrivateKey}");
                if (certificate == null && element.HasPrivateKey)
                {
                    certificate = element;
                }
                else
                {
                    element.Dispose();
                }
            }

            if (certificate == null)
            {
                throw new FileNotFoundException($"{certificateFileName} did not contain any certificate with a private key.");
            }

            Console.WriteLine($"Using certificate {certificate.Thumbprint} {certificate.Subject}");
            return certificate;
        }

        // INSERT ProvisionDevice method below here
        private static async Task<DeviceClient> ProvisionDevice(ProvisioningDeviceClient provisioningDeviceClient, SecurityProviderX509Certificate security)
        {
            var result = await provisioningDeviceClient.RegisterAsync().ConfigureAwait(false);
            Console.WriteLine($"ProvisioningClient AssignedHub: {result.AssignedHub}; DeviceID: {result.DeviceId}");
            if (result.Status != ProvisioningRegistrationStatusType.Assigned)
            {
                throw new Exception($"DeviceRegistrationResult.Status is NOT 'Assigned'");
            }

            var auth = new DeviceAuthenticationWithX509Certificate(
                result.DeviceId,
                security.GetAuthenticationCertificate());

            return DeviceClient.Create(result.AssignedHub, auth, TransportType.Amqp);
        }

        private static async Task SendDeviceToCloudMessagesAsync()
        {
            var sensor = new EnvironmentSensor();

            while (true)
            {
                var currentTemperature = sensor.ReadTemperature();
                var currentHumidity = sensor.ReadHumidity();
                var currentPressure = sensor.ReadPressure();
                var currentLocation = sensor.ReadLocation();

                var messageString = CreateMessageString(currentTemperature,
                                                        currentHumidity,
                                                        currentPressure,
                                                        currentLocation);

                var message = new Message(Encoding.ASCII.GetBytes(messageString));

                // Add a custom application property to the message.
                // An IoT hub can filter on these properties without access to the message body.
                message.Properties.Add("temperatureAlert", (currentTemperature > 30) ? "true" : "false");

                // Send the telemetry message
                await deviceClient.SendEventAsync(message);
                Console.WriteLine("{0} > Sending message: {1}", DateTime.Now, messageString);

                // Delay before next Telemetry reading
                await Task.Delay(telemetryDelay * 1000);
            }
        }

        private static string CreateMessageString(double temperature, double humidity, double pressure, EnvironmentSensor.Location location)
        {
            // Create an anonymous object that matches the data structure we wish to send
            var telemetryDataPoint = new
            {
                temperature = temperature,
                humidity = humidity,
                pressure = pressure,
                latitude = location.Latitude,
                longitude = location.Longitude
            };
            var messageString = JsonConvert.SerializeObject(telemetryDataPoint);

            // Create a JSON string from the anonymous object
            return JsonConvert.SerializeObject(telemetryDataPoint);
        }

        // INSERT OnDesiredPropertyChanged method below here
        private static async Task OnDesiredPropertyChanged(TwinCollection desiredProperties, object userContext)
        {
            Console.WriteLine("Desired Twin Property Changed:");
            Console.WriteLine($"{desiredProperties.ToJson()}");

            // Read the desired Twin Properties
            if (desiredProperties.Contains("telemetryDelay"))
            {
                string desiredTelemetryDelay = desiredProperties["telemetryDelay"];
                if (desiredTelemetryDelay != null)
                {
                    telemetryDelay = int.Parse(desiredTelemetryDelay);
                }
                // if desired telemetryDelay is null or unspecified, don't change it
            }


            // Report Twin Properties
            var reportedProperties = new TwinCollection();
            reportedProperties["telemetryDelay"] = telemetryDelay.ToString();
            await deviceClient.UpdateReportedPropertiesAsync(reportedProperties).ConfigureAwait(false);
            Console.WriteLine("Reported Twin Properties:");
            Console.WriteLine($"{reportedProperties.ToJson()}");
        }
    }

    internal class EnvironmentSensor
    {
        // Initial telemetry values
        double minTemperature = 20;
        double minHumidity = 60;
        double minPressure = 1013.25;
        double minLatitude = 39.810492;
        double minLongitude = -98.556061;
        Random rand = new Random();

        internal class Location
        {
            internal double Latitude;
            internal double Longitude;
        }

        internal double ReadTemperature()
        {
            return minTemperature + rand.NextDouble() * 15;
        }
        internal double ReadHumidity()
        {
            return minHumidity + rand.NextDouble() * 20;
        }
        internal double ReadPressure()
        {
            return minPressure + rand.NextDouble() * 12;
        }
        internal Location ReadLocation()
        {
            return new Location { Latitude = minLatitude + rand.NextDouble() * 0.5, Longitude = minLongitude + rand.NextDouble() * 0.5 };
        }
    }
}