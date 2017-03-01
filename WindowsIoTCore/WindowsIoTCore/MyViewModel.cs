using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace WindowsIoTCore
{
    public class MyViewModel : INotifyPropertyChanged
    {
        public MyViewModel()
        {
            _automaticSendingMessages = true;
            _message = "Hello, Cloud from Windows 10 IoT";
        }

        private bool _automaticSendingMessages;
        public bool AutomaticSendingMessages
        {
            get { return _automaticSendingMessages; }
            set
            {
                _automaticSendingMessages = value;
                this.OnPropertyChanged();
            }
        }

        string _message;
        public string Message
        {
            get { return _message; }
            set
            {
                _message = value;
                this.OnPropertyChanged();
            }
        }
        
        public void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            // Raise the PropertyChanged event, passing the name of the property whose value has changed.
            this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        public event PropertyChangedEventHandler PropertyChanged;

        private void Do()
        {


        }
    }
}