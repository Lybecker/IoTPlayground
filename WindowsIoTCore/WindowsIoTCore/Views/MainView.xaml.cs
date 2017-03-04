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
using Microsoft.Practices.ServiceLocation;
using WindowsIoTCore.ViewModels;

namespace WindowsIoTCore
{
    /// <summary>
    /// The one and only page
    /// </summary>
    public sealed partial class MainView : Page
    {
        public MainViewModel ViewModel => ServiceLocator.Current.GetInstance<MainViewModel>();

        public MainView()
        {
            this.InitializeComponent();
        }
    }
}