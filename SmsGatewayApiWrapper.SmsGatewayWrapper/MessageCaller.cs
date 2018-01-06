using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmsGatewayApiWrapper.SmsGatewayWrapper {
    public class MessageCaller : SmsGatewayAbstract {
        private DeviceCaller Device { get; set; }
        internal MessageCaller(string email, string password)
            : base(email, password) {
            
        }
    }
}
