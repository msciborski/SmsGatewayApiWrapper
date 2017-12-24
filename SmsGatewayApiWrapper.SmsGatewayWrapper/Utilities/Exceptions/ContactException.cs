using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmsGatewayApiWrapper.SmsGatewayWrapper.Utilities.Exceptions {
    class ContactException : SmsGatewayException {

        public ContactException() {
            
        }
        public ContactException(string message)
            : base(message) {
            
        }

        public ContactException(string message, Exception inner)
            : base(message, inner) {
            
        }
    }
}
