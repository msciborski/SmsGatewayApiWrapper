using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmsGatewayApiWrapper.SmsGatewayWrapper.Utilities.Exceptions {
    public class SmsGatewayException : Exception {

        public SmsGatewayException() {
            
        }
        public SmsGatewayException(string message)
            : base(message) {
            
        }

        public SmsGatewayException(string message, Exception inner)
            : base(message, inner) {
            
        }
    }
}
