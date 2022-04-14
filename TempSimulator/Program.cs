using Microsoft.Azure.Devices.Client;
using Newtonsoft.Json;
using System;
using System.Text;
using System.Threading.Tasks;

namespace TempSimulator
{
    class Program
    {
        private const string HostName = "YOUR_IOTHUB_HOST_NAME";
        private const string DeviceId = "DEVICE_ID";
        private const string DeviceKey = "YOUR_DEVICE_KEY";

        private static DeviceClient _tempSimulator;

        private static void Main(string[] args)
        {
            _tempSimulator = DeviceClient.Create(HostName, new DeviceAuthenticationWithRegistrySymmetricKey(DeviceId, DeviceKey), TransportType.Mqtt);
            SendDeviceToCloudMessagesAsync();
            Console.ReadLine();
        }

        private static async void SendDeviceToCloudMessagesAsync()
        {
            Random rand = new Random();
            while (true)
            {
                var temp = rand.Next(-50, 50);
                var telemetryDataPoint = new
                {
                    deviceId = DeviceId,
                    temperature = temp
                };

                var messageString = JsonConvert.SerializeObject(telemetryDataPoint);
                var message = new Message(Encoding.ASCII.GetBytes(messageString));

                // You can query the following property in the iot-hub message routing.
                message.Properties.Add("city", "stockholm");
                message.Properties.Add("country", "sweden");
                await _tempSimulator.SendEventAsync(message);
                
                Console.WriteLine("{0} {1}: {2}", DateTime.Now, DeviceId, messageString);
                await Task.Delay(5000);
            }
        }
    }
}
