using System;
using System.Collections.Generic;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.Devices.Enumeration;
using Windows.Devices.I2c;
using Windows.IoT.Core.HWInterfaces.MPR121;
using Windows.UI.Core;

namespace FruitKeyboard
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        MPR121 _mpr121 = null;
        UIElement[] _pinStatusUIElements = null;
        IotHubService _iotHubService = null;

        bool ApplemanNodes
        {
            get; set;
        }


        public MainPage()
        {
            this.InitializeComponent();

            //uses the default mpr121 address and Pin #5 on the RaspberryPi as IRQ Pin
            _mpr121 = new MPR121();
            InitMPR121();

            _pinStatusUIElements = new UIElement[] { pin0Status,pin1Status, pin2Status, pin3Status,
                                                    pin4Status, pin5Status,pin6Status, pin7Status,
                                                    pin8Status,pin9Status, pin10Status, pin11Status };

            _iotHubService = new IotHubService("<Azure IoT Hub device connection string>");
        }

        async void InitMPR121()
        {
            //Get the I2C device list on the Raspberry Pi.
            string aqs = I2cDevice.GetDeviceSelector(); //get the device selector AQS  (adavanced query string)
            var i2cDeviceList = await DeviceInformation.FindAllAsync(aqs); //get the I2C devices that match the device selector aqs

            //if the device list is not null, try to establish I2C connection between the master and the MPR121
            if (i2cDeviceList != null && i2cDeviceList.Count > 0)
            {
                bool connected = await _mpr121.OpenConnection(i2cDeviceList[0].Id);
                if (connected)
                {
                    txtStatus.Text = "Connected.";

                    //MPR121 will raise Touched and Released events if the IRQ pin is connected and configured corectly.. 
                    //Adding event handlers for those events
                    _mpr121.PinTouched += Mpr121_PinTouched;
                    _mpr121.PinReleased += Mpr121_PinReleased;
                }
            }
        }

        async void Mpr121_PinTouched(object sender, PinTouchedEventArgs e)
        {
            var sound = PinToCordSound(e.Touched[0]);
            await _iotHubService.SendDeviceToCloudMessagesAsync(sound);

            var task = Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                txtStatus.Text = e.Touched[0].ToString() + " Touched"; //just the first touched pin

                UpdatePinStatusUI(e.Touched, true);
                
                PlaySound(sound);
            });
        }

        void Mpr121_PinReleased(object sender, PinReleasedEventArgs e)
        {
            var task = Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
           {
               txtStatus.Text = e.Released[0].ToString() + " Released"; //just the first touched pin

               UpdatePinStatusUI(e.Released, false);
           });
        }

        void UpdatePinStatusUI(List<PinId> pins, bool turnOn)
        {
            SolidColorBrush pinBrush = new SolidColorBrush(Windows.UI.Colors.Red);
            if (turnOn)
                pinBrush = new SolidColorBrush(Windows.UI.Color.FromArgb(100, 38, 247, 5));

            foreach (PinId pin in pins)
            {
                (_pinStatusUIElements[(Array.IndexOf(Enum.GetValues(typeof(PinId)), pin) - 1)] as Windows.UI.Xaml.Shapes.Ellipse).Fill = pinBrush;
            }
        }

        async void PlaySound(string sound)
        {
            try
            {
                MediaElement mysong = new MediaElement();
                Windows.Storage.StorageFolder folder = await Windows.ApplicationModel.Package.Current.InstalledLocation.GetFolderAsync(@"Assets");
                Windows.Storage.StorageFile file = await folder.GetFileAsync($"{sound}.mp3");
                var stream = await file.OpenAsync(Windows.Storage.FileAccessMode.Read);
                mysong.SetSource(stream, file.ContentType);
                mysong.Play();
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        string PinToCordSound(PinId pin)
        {
            switch (pin)
            {
                case PinId.PIN_0:
                    return "WhatTheHeck";
                case PinId.PIN_1:
                    return ApplemanNodes ? "Appleman_1(C)" : "1 (C1)";
                case PinId.PIN_2:
                    return ApplemanNodes ? "Appleman_2(D)" : "2 (Eb)";
                case PinId.PIN_3:
                    return ApplemanNodes ? "Appleman_3(E)" : "3 (F1)";
                case PinId.PIN_4:
                    return ApplemanNodes ? "Appleman_4(F)" : "4 (Ab)";
                case PinId.PIN_5:
                    return ApplemanNodes ? "Appleman_5(G)" : "5 (Bb)";
                case PinId.PIN_6:
                    return "6 (C2)";
                case PinId.PIN_7:
                    return "7 (Dd)";
                case PinId.PIN_8:
                    return "8 (F2)";
                default:
                    return "KeyboardPlayingSkill";
            }
        }
    }
}