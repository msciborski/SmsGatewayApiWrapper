using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SmsGatewayApiWrapper.SmsGatewayWrapper.Models;
using SmsGatewayApiWrapper.SmsGatewayWrapper.Utilities.Exceptions;

namespace SmsGatewayApiWrapper.SmsGatewayWrapper {
    public class SmsGateway {
        private readonly string _baseUrl = "http://smsgateway.me/api/v3/";
        /// <summary>
        /// Contains email for your account on site
        /// </summary>
        public string Email { get; private set; }

        /// <summary>
        /// Contains password for your account on site
        /// </summary>
        public string Password { get; private set; }

        public SmsGateway ( string email, string password ) {
            Email = email;
            Password = password;
        }

        public async Task<IEnumerable<Device>> GetDevicesAsync () {
            IEnumerable<Device> devices = null;
            try {
                using (var client = new HttpClient()) {
                    BaseConfigurationHttpClient(client);
                    var response =
                        await client.GetAsync(String.Format("devices?email={0}&password={1}", Email, Password));
                    var responseContent = await response.Content.ReadAsStringAsync();

                    if (response.IsSuccessStatusCode) {
                        JObject jObject = JObject.Parse(responseContent);
                        var devicesJson = jObject["result"]["data"];
                        devices = devicesJson.ToObject<List<Device>>();
                    } else {
                        JObject jObject = JObject.Parse(responseContent);
                        var error = jObject["errors"].Select(t => (string) t).FirstOrDefault();
                        throw new AuthenticationException(error);
                    }
                }
            } catch (HttpRequestException e) {
                Console.WriteLine("Problem with connection.");
                Console.WriteLine(e.ToString());
            } catch (JsonReaderException e) {
                Console.WriteLine(e.ToString());
            }
            return devices;
        }
        public IEnumerable<Device> GetDevices () {
            IEnumerable<Device> devices = null;

            var task = Task.Run(async () => {
                devices = await GetDevicesAsync();
            });

            while (!task.IsCompleted) {
                System.Threading.Thread.Yield();
            }

            if (task.IsFaulted) {
                throw task.Exception;
            }

            if (task.IsCanceled) {
                throw new Exception("Timeout obtaining device information.");
            }

            return devices;

        }

        private void BaseConfigurationHttpClient ( HttpClient client ) {
            client.BaseAddress = new Uri(_baseUrl);
            client.DefaultRequestHeaders.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        }
    }
}
