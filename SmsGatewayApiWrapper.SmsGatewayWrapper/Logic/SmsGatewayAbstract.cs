using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using SmsGatewayApiWrapper.SmsGatewayWrapper.Models;

namespace SmsGatewayApiWrapper.SmsGatewayWrapper {
    public abstract class SmsGatewayAbstract {
        protected enum OperationType {
            GET,
            POST
        }
        protected enum MessageType {
            ToNumber,
            ToContact
        }

        public string Email { get; private set; }
        public string Password { get; private set; }

        protected readonly string _baseUrl = "http://smsgateway.me/api/v3/";
        protected readonly string _devicesUrl = "devices?email={0}&password={1}&page={2}";
        protected readonly string _deviceUrl = "devices/view/{0}?email={1}&password={2}";
        protected readonly string _messagesUrl = "messages?email={0}&password={1}";
        protected readonly string _messageUrl = "messages/view/{0}?email={1}&password={2}";
        protected readonly string _sendMessageUrl = "messages/send";
        protected readonly string _contactsUrl = "contacts?email={0}&password={1}&page={2}";

        internal SmsGatewayAbstract(string email, string password) {
            this.Email = email;
            this.Password = password;
        }

        protected async Task<HttpResponseMessage> MakeRequestAsync(HttpClient client, string url, OperationType type, string body = null) {
            BaseConfigurationHttpClient(client);
            if (type == OperationType.GET) {
                var response = await client.GetAsync(url);
                return response;

            } else if (type == OperationType.POST) {
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, url);
                if (body != null) {
                    request.Content = new StringContent(body, Encoding.UTF8, "application/json");
                }
                var response = await client.SendAsync(request);
                return response;
            }

            throw new ArgumentException("Provided wrong httpMethod.");

        }

        protected void BaseConfigurationHttpClient(HttpClient client) {
            client.BaseAddress = new Uri(_baseUrl);
            client.DefaultRequestHeaders.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        }
    }
}
