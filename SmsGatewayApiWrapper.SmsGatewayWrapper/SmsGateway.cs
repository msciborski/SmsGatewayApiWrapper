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
using SmsGatewayApiWrapper.SmsGatewayWrapper.Utilities.Converters;
using SmsGatewayApiWrapper.SmsGatewayWrapper.Utilities.Exceptions;

namespace SmsGatewayApiWrapper.SmsGatewayWrapper {
    public class SmsGateway {
        private readonly string _baseUrl = "http://smsgateway.me/api/v3/";
        private readonly string _devicesUrl = "devices?email={0}&password={1}&page={2}";
        private readonly string _deviceUrl = "devices/view/{0}?email={1}&password={2}";
        private readonly string _messagesUrl = "messages?email={0}&password={1}";
        private readonly string _messageUrl = "messages/view/{0}?email={1}&password={2}";


        private static readonly TimeSpan lastSeenDeviceDuration = new TimeSpan(0,10,0);
        private Device storedLastSeenDevice = null;
        private DateTime storedLastSeenDeviceTime = DateTime.MinValue;

        /// <summary>
        /// Contains email for your account on site
        /// </summary>
        public string Email { get; private set; }

        /// <summary>
        /// Contains password for your account on site
        /// </summary>
        public string Password { get; private set; }

        public SmsGateway(string email, string password) {
            Email = email;
            Password = password;
        }

        public async Task<Device> GetLastSeenDeviceAsync() {
            if((DateTime.Now - storedLastSeenDeviceTime) < lastSeenDeviceDuration) {
                return storedLastSeenDevice;
            }

            try {
                var devices = await GetDevicesAsync();
                var lastSeenDevice = devices.OrderBy(d => d.LastSeen).FirstOrDefault();

                if(lastSeenDevice == null) {
                    throw new DeviceException("No device is avaiable.");
                }
                storedLastSeenDevice = lastSeenDevice;
                storedLastSeenDeviceTime = DateTime.Now;;
            }catch(AuthenticationException e) {
                Console.WriteLine(e.ToString());
            }

            return storedLastSeenDevice;
        }
        public Device GetLastSeenDevice() {
            var task = Task.Run(async () => {
                storedLastSeenDevice = await GetLastSeenDeviceAsync();
            });

            while (!task.IsCompleted) {
                System.Threading.Thread.Yield();
            }

            if (task.IsFaulted) {
                throw task.Exception;
            }else if (task.IsCanceled) {
                throw new Exception("Timeout.");
            }
            return storedLastSeenDevice;
        }

        public async Task<PaginingList<Device>> GetDevicesAsync(int page = 1) {
            PaginingList<Device> devices = null;
            try {
                using (var client = new HttpClient()) {
                    BaseConfigurationHttpClient(client);
                    var response =
                        await client.GetAsync(String.Format(_devicesUrl, Email, Password, page));
                    var responseContent = await response.Content.ReadAsStringAsync();

                    if (response.IsSuccessStatusCode) {
                        JObject jObject = JObject.Parse(responseContent);
                        JsonSerializer serializer = new JsonSerializer();
                        serializer.Converters.Add(new PaginingListConverter<Device>());
                        devices = jObject.ToObject<PaginingList<Device>>(serializer);
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
        public PaginingList<Device> GetDevices() {
            PaginingList<Device> devices = null;

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

        public async Task<Device> GetDeviceAsync(int id) {
            Device device = null;
            try {
                using (var client = new HttpClient()) {
                    BaseConfigurationHttpClient(client);
                    var response = await client.GetAsync(String.Format(_deviceUrl,
                        id, Email, Password));
                    var responseContent = await response.Content.ReadAsStringAsync();

                    if (response.IsSuccessStatusCode) {
                        JObject jObject = JObject.Parse(responseContent);
                        device = jObject["result"].ToObject<Device>();
                    } else {
                        JObject jObject = JObject.Parse(responseContent);
                        if (jObject["errors"]["login"] != null) {
                            throw new AuthenticationException((string) jObject["errors"]["login"]);
                        }
                        if (jObject["errors"]["id"] != null) {
                            throw new DeviceException((string) jObject["errors"]["id"]);
                        }
                    }

                }
            } catch (HttpRequestException e) {
                Console.WriteLine(e.ToString());
            } catch (JsonException e) {
                Console.WriteLine(e.ToString());
            }
            return device;
        }

        public Device GetDevice(int id) {
            Device device = null;

            var task = Task.Run(async () => {
                device = await GetDeviceAsync(id);
            });

            while (!task.IsCompleted) {
                System.Threading.Thread.Yield();
            }

            if (task.IsFaulted) {
                throw task.Exception;
            } else if (task.IsCanceled) {
                throw new Exception("Timeout.");
            }
            return device;
        }

        /*
         * Documentation don't respond to the result of query. In documentation /messages should return json with messages
         * and pagining information. In real, just return messages
         */
        public async Task<IEnumerable<Message>> GetMessagesAsync() {
            IEnumerable<Message> messages = null;
            try {
                using (var client = new HttpClient()) {
                    BaseConfigurationHttpClient(client);
                    var response = await client.GetAsync(String.Format(_messagesUrl, Email, Password));
                    var responseContent = await response.Content.ReadAsStringAsync();
                    if (response.IsSuccessStatusCode) {
                        JObject jObject = JObject.Parse(responseContent);
                        messages = jObject["result"].ToObject<IList<Message>>();
                        //JObject jObject = JObject.Parse(responseContent);
                        //JsonSerializer serializer = new JsonSerializer();
                        //serializer.Converters.Add(new PaginingListConverter<Message>());
                        //messages = jObject.ToObject<PaginingList<Message>>(serializer);
                        Console.WriteLine(responseContent);
                    } else {
                        JObject jObject = JObject.Parse(responseContent);
                        var error = jObject["errors"].Select(t => (string) t).FirstOrDefault();
                        throw new AuthenticationException(error);
                    }
                }
            } catch (HttpRequestException e) {
                Console.WriteLine(e.ToString());
            } catch (JsonException e) {
                Console.WriteLine(e.ToString());
            }
            return messages;
        }

        public IEnumerable<Message> GetMessages() {
            IEnumerable<Message> messages = null;

            var task = Task.Run(async () => {
                messages = await GetMessagesAsync();
            });

            while (!task.IsCompleted) {
                System.Threading.Thread.Yield();
            }

            if (task.IsFaulted) {
                throw task.Exception;
            } else if (task.IsCanceled) {
                throw new Exception("Timeout.");
            }

            return messages;
        }

        public async Task<Message> GetMessageAsync(int id) {
            Message message = null;
            try {
                using(var client = new HttpClient()) {
                    BaseConfigurationHttpClient(client);
                    var response = await client.GetAsync(String.Format(_messageUrl, id, Email, Password));
                    var responseContent = await response.Content.ReadAsStringAsync();

                    if (response.IsSuccessStatusCode) {
                        JObject jObject = JObject.Parse(responseContent);
                        message = jObject["result"].ToObject<Message>();
                    } else {
                        JObject jObject = JObject.Parse(responseContent);
                        if (jObject["errors"]["login"] != null) {
                            throw new AuthenticationException((string) jObject["errors"]["login"]);
                        }
                        if (jObject["errors"]["id"] != null) {
                            throw new DeviceException((string) jObject["errors"]["id"]);
                        }
                    }
                }
            }catch(HttpRequestException e) {
                Console.WriteLine(e.ToString());
            }catch(JsonException e) {
                Console.WriteLine(e.ToString());
            }
            return message;
        }

        public Message GetMessage(int id) {
            Message message = null;

            var task = Task.Run(async () => {
                message = await GetMessageAsync(id);
            });

            while (!task.IsCompleted) {
                System.Threading.Thread.Yield();
            }

            if (task.IsFaulted) {
                throw task.Exception;
            } else if (task.IsCanceled) {
                throw new Exception("Timeout.");
            }

            return message;
        }

        private void BaseConfigurationHttpClient(HttpClient client) {
            client.BaseAddress = new Uri(_baseUrl);
            client.DefaultRequestHeaders.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        }
    }
}
