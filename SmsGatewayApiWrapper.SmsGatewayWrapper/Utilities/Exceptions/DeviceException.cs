using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmsGatewayApiWrapper.SmsGatewayWrapper.Utilities.Exceptions {
    public class DeviceException : SmsGatewayException{

        public DeviceException () {
            
        }

        public DeviceException (string message)
            : base(message) {
            
        }

        public DeviceException(string message, Exception inner)
            : base(message, inner) {
            
        }
    }
}
