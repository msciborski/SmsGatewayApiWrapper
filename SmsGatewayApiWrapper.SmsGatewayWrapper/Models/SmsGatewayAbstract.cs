using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmsGatewayApiWrapper.SmsGatewayWrapper.Models {
    public abstract class SmsGatewayAbstract {
        protected enum OperationType {
            GET,
            POST
        }
        protected enum MessageType {
            ToNumber,
            ToContact
        }

        /// <summary>
        /// Base url for API
        /// </summary>
        protected readonly string _baseUrl = "http://smsgateway.me/api/v3/";
        /// <summary>
        /// Url for fetching devices
        /// </summary>
        protected readonly string _devicesUrl = "devices?email={0}&password={1}&page={2}";
        /// <summary>
        /// Url for fetching single device
        /// </summary>
        protected readonly string _deviceUrl = "devices/view/{0}?email={1}&password={2}";
        /// <summary>
        /// Url for fetching whole messages.
        /// </summary>
        protected readonly string _messagesUrl = "messages?email={0}&password={1}";
        /// <summary>
        /// Url for fething 
        /// </summary>
        protected readonly string _messageUrl = "messages/view/{0}?email={1}&password={2}";
        /// <summary>
        /// Url for sending messages
        /// </summary>
        protected readonly string _sendMessageUrl = "messages/send";

        protected readonly string _contactsUrl = "contacts?email={0}&password={1}&page={2}";

        /// <summary>
        /// Field, which holds TimeStamp for refreshing last seen device
        /// </summary>
        protected static readonly TimeSpan lastSeenDeviceDuration = new TimeSpan(0, 10, 0);
        /// <summary>
        /// Fields for storing last seen device
        /// </summary>
        protected Device storedLastSeenDevice = null;
        /// <summary>
        /// DateTime of device, which was fetched(marked as last seen)
        /// </summary>
        protected DateTime storedLastSeenDeviceTime = DateTime.MinValue;
    }
}
