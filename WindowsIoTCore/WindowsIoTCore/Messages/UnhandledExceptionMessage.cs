using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsIoTCore.Messages
{
    public class UnhandledExceptionMessage
    {
        public UnhandledExceptionMessage(Exception exception)
        {
            Exception = exception;
        }

        public Exception Exception { get; set; }
    }
}
