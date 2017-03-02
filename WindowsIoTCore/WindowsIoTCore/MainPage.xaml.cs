﻿using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using System.Text;
using Microsoft.Azure.Devices.Client;
using Microsoft.Devices.Tpm;
using System.Threading.Tasks;
using System.ComponentModel;

namespace WindowsIoTCore
{
    /// <summary>
    /// The one and only page
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
            
            SetupTimer(_viewModel);
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

        void UpdateSendFrequency(MyViewModel viewModel)
        {
            _timer.Interval = TimeSpan.FromSeconds(viewModel.SendFrequencyInSeconds);
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