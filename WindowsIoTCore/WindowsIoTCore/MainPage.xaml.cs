using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using System.Text;
using Microsoft.Azure.Devices.Client;
using Microsoft.Devices.Tpm;
using System.Threading.Tasks;
using System.ComponentModel;
using Newtonsoft.Json;
using System.Text.RegularExpressions;

namespace WindowsIoTCore
{
    /// <summary>
    /// The one and only page
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MyViewModel _viewModel { get; set; }
        private DispatcherTimer _timer;
        private DeviceClient _deviceClient;

        public MainPage()
        {
            this.InitializeComponent();
            this._viewModel = new MyViewModel();

            _viewModel.PropertyChanged += ViewModel_PropertyChanged;

            _deviceClient = Connect();

            SetupTimer(_viewModel);
            StartStopTimer(_viewModel);

            ProcessIncommingMessagesAsync(_deviceClient);
        }

        private void ViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var viewModel = (MyViewModel)sender;

            switch (e.PropertyName)
            {
                case "AutomaticSendingMessages":
                    StartStopTimer(viewModel);
                    break;
                case "SendFrequencyInSeconds":
                    UpdateSendFrequency(viewModel);
                    break;
            }
        }

        void SetupTimer(MyViewModel viewModel)
        {
            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromSeconds(viewModel.SendFrequencyInSeconds);
            _timer.Tick += (object sender, object e) =>
                {
                    SendDeviceToCloudMessagesAsync(_deviceClient);
                };
        }

        void StartStopTimer(MyViewModel viewModel)
        {
            if (viewModel.AutomaticSendingMessages)
                _timer.Start();
            else
                _timer.Stop();
        }

        void UpdateSendFrequency(MyViewModel viewModel)
        {
            _timer.Interval = TimeSpan.FromSeconds(viewModel.SendFrequencyInSeconds);
        }

        DeviceClient Connect()
        {
            // Get credentials from TPM store/chip
            TpmDevice device = new TpmDevice(0); // Use logical device 0 on the TPM by default

            string iotHubUri = device.GetHostName();
            string deviceId = device.GetDeviceId();
            string sasToken = device.GetSASToken(validity: 3600);

            // Connect
            var deviceClient = DeviceClient.Create(
                iotHubUri,
                AuthenticationMethodFactory.CreateAuthenticationWithToken(deviceId, sasToken), TransportType.Http1);

            return deviceClient;
        }

        double GetCurrentWindSpeed()
        {
            double avgWindSpeed = 10; // m/s
            Random rand = new Random();
            double currentWindSpeed = avgWindSpeed + rand.NextDouble() * 4 - 2;

            return currentWindSpeed;
        }

        async Task SendDeviceToCloudMessagesAsync(DeviceClient deviceClient)
        {
            var windspeed = GetCurrentWindSpeed();

            var telemetryDataPoint = new
            {
                windSpeed = windspeed,
                message = _viewModel.Message
            };

            var msgString = JsonConvert.SerializeObject(telemetryDataPoint);
            var msg = new Message(Encoding.ASCII.GetBytes(msgString));

            await deviceClient.SendEventAsync(msg);
        }

        async Task ProcessIncommingMessagesAsync(DeviceClient deviceClient)
        {
            while (true)
            {
               var msgString = await ReceiveCloudToDeviceMessageAsync(deviceClient);

                _viewModel.LatestReceivedMessage = $"Receive '{msgString}'";

                var regEx = Regex.Match(msgString, @"^SendFreqnecy (?<frequency>\d{1,2}?)$", RegexOptions.IgnoreCase);
                if (regEx.Success)
                    _viewModel.SendFrequencyInSeconds = int.Parse(regEx.Groups["frequency"].Value);
            }
        }

        async Task<string> ReceiveCloudToDeviceMessageAsync(DeviceClient deviceClient)
        {
            var receivedMessage = await deviceClient.ReceiveAsync();

            var msgString = Encoding.ASCII.GetString(receivedMessage.GetBytes());
            await deviceClient.CompleteAsync(receivedMessage);

            return msgString;
        }

        private async void SendMessage_Click(object sender, RoutedEventArgs e)
        {
            await SendDeviceToCloudMessagesAsync(_deviceClient);
        }
    }
}