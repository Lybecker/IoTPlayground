using GalaSoft.MvvmLight.Ioc;
using Microsoft.Practices.ServiceLocation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WindowsIoTCore.Services;

namespace WindowsIoTCore.ViewModels
{
    public class ViewModelLocator
    {
        public ViewModelLocator()
        {
            ServiceLocator.SetLocatorProvider(() => SimpleIoc.Default);

            //Register Services
            SimpleIoc.Default.Register<IotService>();

            //Register ViewModels
            SimpleIoc.Default.Register<MainViewModel>();
        }

        public MainViewModel MainViewModel => ServiceLocator.Current.GetInstance<MainViewModel>();

    }
}
