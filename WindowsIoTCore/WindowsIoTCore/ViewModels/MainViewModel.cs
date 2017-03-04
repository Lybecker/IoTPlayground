using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using Microsoft.Azure.Devices.Client;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Windows.UI.Xaml;
using WindowsIoTCore.Services;

namespace WindowsIoTCore.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private readonly IotService _iotService;

        private DispatcherTimer _timer;
        private DeviceClient _deviceClient;

        public MainViewModel(IotService iotService)
        {
            _iotService = iotService;

            MessengerInstance.Register<Messages.UnhandledExceptionMessage>(this, m => { LatestReceivedMessage = m.Exception.ToString(); });

            _deviceClient = _iotService.Connect();

            SetupTimer();
            StartStopTimer();

            _iotService.ProcessIncommingMessagesAsync(_deviceClient, this);

        }

        #region Private Methods

        void SetupTimer()
        {
            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromSeconds(SendFrequencyInSeconds);
            _timer.Tick += async (object sender, object e) =>
            {
                await _iotService.SendDeviceToCloudMessagesAsync(_deviceClient, Message);
            };
        }

        void StartStopTimer()
        {
            if (AutomaticSendingMessages)
                _timer.Start();
            else
                _timer.Stop();
        }

        void UpdateSendFrequency()
        {
            _timer.Interval = TimeSpan.FromSeconds(SendFrequencyInSeconds);
        }
        #endregion

        #region Properties
        private bool _automaticSendingMessages = true;
        public bool AutomaticSendingMessages
        {
            get { return _automaticSendingMessages; }
            set
            {
                if (Set(ref _automaticSendingMessages, value))
                {
                    StartStopTimer();
                }
            }
        }

        int _sendFrequencyInSeconds = 1;
        public int SendFrequencyInSeconds
        {
            get { return _sendFrequencyInSeconds; }
            set
            {
                if (Set(ref _sendFrequencyInSeconds, value))
                {
                    UpdateSendFrequency();
                }
            }
        }

        string _message = "Hello, Cloud from Windows 10 IoT";
        public string Message
        {
            get { return _message; }
            set { Set(ref _message, value); }
        }

        string _latestReceivedMessage = "(no message received yet)";
        public string LatestReceivedMessage
        {
            get { return _latestReceivedMessage; }
            set { Set(ref _latestReceivedMessage, value); }
        }
        #endregion

        #region Commands

        public RelayCommand SendMessageCommand => new RelayCommand(async () =>
        {
            await _iotService.SendDeviceToCloudMessagesAsync(_deviceClient, Message);
        });

        #endregion
    }
}