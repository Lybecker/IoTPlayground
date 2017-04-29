using Microsoft.Azure.Devices.Client;
using Newtonsoft.Json;
using System.Text;
using System.Threading.Tasks;


namespace FruitKeyboard
{
    public class IotHubService
    {
        DeviceClient _deviceClient;

        public IotHubService(string connectionString)
        {
            _deviceClient = DeviceClient.CreateFromConnectionString(connectionString);
        }

        public async Task SendDeviceToCloudMessagesAsync(string message)
        {
            var telemetryDataPoint = new
            {
                message = message
            };

            var msgString = JsonConvert.SerializeObject(telemetryDataPoint);
            var msg = new Message(Encoding.ASCII.GetBytes(msgString));

            await _deviceClient.SendEventAsync(msg);
        }
    }
}