using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SmsGatewayApiWrapper.SmsGatewayWrapper;
using SmsGatewayApiWrapper.SmsGatewayWrapper.Logic.Interfaces;
using SmsGatewayApiWrapper.SmsGatewayWrapper.Models;
using SmsGatewayApiWrapper.SmsGatewayWrapper.Utilities.Converters;
using SmsGatewayApiWrapper.SmsGatewayWrapper.Utilities.Exceptions;

namespace SmsGatewayApiWrapper.SmsGatewayWrapper {
    /// <summary>
    /// <c>SmsGateway</c> is a class, which wraps SmsGateway API and provides you flexible and simple methods for using this API.
    /// </summary>
    public class SmsGateway : SmsGatewayAbstract {

        public IDeviceCaller Device { get; private set; }
        public IMessageCaller Message { get; private set; }
        public IContactCaller Contact { get; private set; }

        /// <summary>
        ///     Initializing a new instance of <c>SmsGateway</c>
        /// </summary>
        /// <example>
        ///     <code>
        ///         SmsGateway smsGateway = new SmsGateway("test@test.com", "testPassword");
        ///         Console.WriteLine(smsGateway.Email);
        ///     </code>
        /// </example>
        /// <param name="email">Email address for your account on https://smsgateway.me </param>
        /// <param name="password">Passwor for your account on https://smsgateway.me </param>
        public SmsGateway(string email, string password) 
            : base(email, password) {
            Device = new DeviceCaller(email, password);
            Message = new MessageCaller(email, password, Device);
            Contact = new ContactCaller(email, password);
        }
        public SmsGateway(string email, string password, IDeviceCaller device, IMessageCaller message, IContactCaller contact)
            : base(email, password) {
            Device = device;
            Message = message;
            Contact = contact;
        }
    }
}
