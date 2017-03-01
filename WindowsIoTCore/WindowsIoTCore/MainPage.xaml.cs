using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using System.Text;
using Microsoft.Azure.Devices.Client;
using Microsoft.Devices.Tpm;
using System.Threading.Tasks;
using System.ComponentModel;
using Windows.System.Threading;
using System.Threading;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace WindowsIoTCore
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MyViewModel _viewModel { get; set; }
        private DispatcherTimer _timer;

        public MainPage()
        {
            this.InitializeComponent();
            this._viewModel = new MyViewModel();

            _viewModel.PropertyChanged += ViewModel_PropertyChanged;
            
            SetupTimer();
            StartStopTimer(_viewModel);
        }

        private void ViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var viewModel = (MyViewModel)sender;

            switch (e.PropertyName)
            {
                case "AutomaticSendingMessages":
                    StartStopTimer(viewModel);
                    break;
            }
        }

        void SetupTimer()
        {
            TimeSpan delay = TimeSpan.FromSeconds(1);

            _timer = new DispatcherTimer();
            _timer.Interval = delay;
            _timer.Tick += (object sender, object e) =>
                {
                    SendDeviceToCloudMessagesAsync();
                };
        }

        void StartStopTimer(MyViewModel viewModel)
        {
            if (viewModel.AutomaticSendingMessages)
                _timer.Start();
            else
                _timer.Stop();
        }

        async Task SendDeviceToCloudMessagesAsync()  
        {
            TpmDevice device = new TpmDevice(0); // Use logical device 0 on the TPM by default

            string iotHubUri = device.GetHostName();
            string deviceId = device.GetDeviceId();
            string sasToken = device.GetSASToken(validity: 3600);

            var deviceClient = DeviceClient.Create(
                iotHubUri,
                AuthenticationMethodFactory.CreateAuthenticationWithToken(deviceId, sasToken), TransportType.Http1);

            var str = _viewModel.Message;
            var message = new Message(Encoding.ASCII.GetBytes(str));

            await deviceClient.SendEventAsync(message);
        }

        async Task<string> ReceiveCloudToDeviceMessageAsync()
        {
            TpmDevice myDevice = new TpmDevice(0); // Use logical device 0 on the TPM by default
            string hubUri = myDevice.GetHostName();
            string deviceId = myDevice.GetDeviceId();
            string sasToken = myDevice.GetSASToken(validity: 3600);

            var deviceClient = DeviceClient.Create(
                hubUri,
                AuthenticationMethodFactory.
                    CreateAuthenticationWithToken(deviceId, sasToken), TransportType.Http1);

            while (true)
            {
                var receivedMessage = await deviceClient.ReceiveAsync();

                if (receivedMessage != null)
                {
                    var messageData = Encoding.ASCII.GetString(receivedMessage.GetBytes());
                    await deviceClient.CompleteAsync(receivedMessage);
                    return messageData;
                }

                await Task.Delay(TimeSpan.FromSeconds(1));
            }
        }

        private async void SendMessage_Click(object sender, RoutedEventArgs e)
        {
            await SendDeviceToCloudMessagesAsync();
        }
    }
}