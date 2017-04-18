using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.ServiceBus.Messaging;
using System.Threading;

namespace TiSensorTagIoTHubConsole
{
    class Program
    {
        static string connectionString = "<Azure IoT Hub connectionstring>";
        static string iotHubD2cEndpoint = "messages/events";
        static EventHubClient eventHubClient;
        static TemperatureScale temperatureScale = TemperatureScale.Celsius;
        static TiSensorTagTemperatureCalculator calculator = new TiSensorTagTemperatureCalculator();

        static void Main(string[] args)
        {
            Console.WriteLine("Receive messages. Ctrl-C to exit.\n");
            eventHubClient = EventHubClient.CreateFromConnectionString(connectionString, iotHubD2cEndpoint);

            var d2cPartitions = eventHubClient.GetRuntimeInformation().PartitionIds;

            CancellationTokenSource cts = new CancellationTokenSource();

            System.Console.CancelKeyPress += (s, e) =>
            {
                e.Cancel = true;
                cts.Cancel();
                Console.WriteLine("Exiting...");
            };

            var tasks = new List<Task>();
            foreach (string partition in d2cPartitions)
            {
                tasks.Add(ReceiveMessagesFromDeviceAsync(partition, cts.Token));
            }
            Task.WaitAll(tasks.ToArray());
        }

        private static async Task ReceiveMessagesFromDeviceAsync(string partition, CancellationToken ct)
        {
            var eventHubReceiver = eventHubClient.GetDefaultConsumerGroup().CreateReceiver(partition, DateTime.UtcNow);
            while (true)
            {
                if (ct.IsCancellationRequested) break;

                EventData eventData = await eventHubReceiver.ReceiveAsync();

                if (eventData == null) continue;

                var sensorData = eventData.GetBytes();

                // this is how to calculate the temperature according to the TI docs
                //var ambientTemp = calculator.CalculateAmbientTemperature(sensorData, temperatureScale);
                //var targetTemp = calculator.CalculateTargetTemperature(sensorData, ambientTemp, temperatureScale);

                //Console.WriteLine($"Partion {partition} - {targetTemp}, {ambientTemp}");

                // this is how the Azure IoT Hub Gateway module ble_printer does it 
                double ambient = 0, target = 0;

                sensortag_temp_convert(BitConverter.ToUInt16(sensorData, 2), BitConverter.ToUInt16(sensorData, 0), ref ambient, ref target);

                Console.WriteLine($"Partion {partition} - {target}, {ambient}");
            }
        }

        // https://github.com/Azure/azure-iot-gateway-sdk/blob/master/samples/ble_gateway/ble_printer/src/ble_printer.c
        static void sensortag_temp_convert(UInt16 rawAmbTemp, UInt16 rawObjTemp, ref double tAmb, ref double tObj)
        {
            const double SCALE_LSB = 0.03125;
            double t;
            int it;

            it = (int)((rawObjTemp) >> 2);
            t = ((double)(it)) * SCALE_LSB;
            tObj = t;

            it = (int)((rawAmbTemp) >> 2);
            t = (double)it;
            tAmb = t * SCALE_LSB;
        }
    }
}
