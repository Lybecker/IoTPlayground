using Microsoft.Azure.Devices.Client;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using WindowsIoTCore.ViewModels;

namespace WindowsIoTCore.Services
{
    public class IotService
    {
        public DeviceClient Connect()
        {
            var deviceClient = DeviceClient.CreateFromConnectionString("HostName=alyiothub.azure-devices.net;DeviceId=windows10iotcore;SharedAccessKey=PT6A0g3mJv0rcj09YuCA+r9bb9DuZ2i6s2ayYB0kwUM=");
            return deviceClient;
        }

        //DeviceClient Connect()
        //{
        //    // Get credentials from TPM store/chip
        //    TpmDevice device = new TpmDevice(0); // Use logical device 0 on the TPM by default

        //    string iotHubUri = device.GetHostName();
        //    string deviceId = device.GetDeviceId();
        //    string sasToken = device.GetSASToken(validity: 3600);

        //    // Connect
        //    var deviceClient = DeviceClient.Create(
        //        iotHubUri,
        //        AuthenticationMethodFactory.CreateAuthenticationWithToken(deviceId, sasToken), TransportType.Http1);

        //    return deviceClient;
        //}

        double GetCurrentWindSpeed()
        {
            double avgWindSpeed = 10; // m/s
            Random rand = new Random();
            double currentWindSpeed = avgWindSpeed + rand.NextDouble() * 4 - 2;

            return currentWindSpeed;
        }

        public async Task SendDeviceToCloudMessagesAsync(DeviceClient deviceClient, string message)
        {
            var windspeed = GetCurrentWindSpeed();

            var telemetryDataPoint = new
            {
                windSpeed = windspeed,
                message = message
            };

            var msgString = JsonConvert.SerializeObject(telemetryDataPoint);
            var msg = new Message(Encoding.ASCII.GetBytes(msgString));

            await deviceClient.SendEventAsync(msg);
        }

        public async Task ProcessIncommingMessagesAsync(DeviceClient deviceClient, MainViewModel viewModel)
        {
            while (true)
            {
                var msgString = await ReceiveCloudToDeviceMessageAsync(deviceClient);

                viewModel.LatestReceivedMessage = $"Received '{msgString}'";

                var regEx = Regex.Match(msgString, @"^SendFreqnecy (?<frequency>\d{1,2}?)$", RegexOptions.IgnoreCase);
                if (regEx.Success)
                    viewModel.SendFrequencyInSeconds = int.Parse(regEx.Groups["frequency"].Value);
            }
        }

        async Task<string> ReceiveCloudToDeviceMessageAsync(DeviceClient deviceClient)
        {
            var receivedMessage = await deviceClient.ReceiveAsync();

            var msgString = Encoding.ASCII.GetString(receivedMessage.GetBytes());
            await deviceClient.CompleteAsync(receivedMessage);

            return msgString;
        }
    }
}
