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
    /// <summary>
    /// <c>SmsGateway</c> is a class, which wraps SmsGateway API and provides you flexible and simple methods for using this API.
    /// </summary>
    public class SmsGateway {
        /// <summary>
        /// Base url for API
        /// </summary>
        private readonly string _baseUrl = "http://smsgateway.me/api/v3/";
        /// <summary>
        /// Url for fetching devices
        /// </summary>
        private readonly string _devicesUrl = "devices?email={0}&password={1}&page={2}";
        /// <summary>
        /// Url for fetching single device
        /// </summary>
        private readonly string _deviceUrl = "devices/view/{0}?email={1}&password={2}";
        /// <summary>
        /// Url for fetching whole messages.
        /// </summary>
        private readonly string _messagesUrl = "messages?email={0}&password={1}";
        /// <summary>
        /// Url for fething 
        /// </summary>
        private readonly string _messageUrl = "messages/view/{0}?email={1}&password={2}";
        /// <summary>
        /// Url for sending messages
        /// </summary>
        private readonly string _sendMessageUrl =
            "messages/send?email={0}&password={1}&device={2}&number={3}&message={4}";

        /// <summary>
        /// Field, which holds TimeStamp for refreshing last seen device
        /// </summary>
        private static readonly TimeSpan lastSeenDeviceDuration = new TimeSpan(0, 10, 0);
        /// <summary>
        /// Fields for storing last seen device
        /// </summary>
        private Device storedLastSeenDevice = null;
        /// <summary>
        /// DateTime of device, which was fetched(marked as last seen)
        /// </summary>
        private DateTime storedLastSeenDeviceTime = DateTime.MinValue;

        /// <summary>
        /// Contains email for your account on site
        /// </summary>
        public string Email { get; private set; }

        /// <summary>
        /// Contains password for your account on site
        /// </summary>
        public string Password { get; private set; }

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
        public SmsGateway(string email, string password) {
            Email = email;
            Password = password;
        }

        /// <summary>
        /// Get all devices from API and select last seen device and return it. Method looking for newest device every 10 minutes. 
        /// This is async implementation.
        /// </summary>
        /// <exception cref="DeviceException">
        ///     If there is no avaiable device
        /// </exception>
        /// <exception cref="AuthenticationException">
        ///     If you provide wrong credentials.
        /// </exception>
        /// <returns><see cref="Device"/>
        ///     Return Task.
        /// </returns>
        public async Task<Device> GetLastSeenDeviceAsync() {
            if (( DateTime.Now - storedLastSeenDeviceTime ) < lastSeenDeviceDuration) {
                return storedLastSeenDevice;
            }

            try {
                var devices = await GetDevicesAsync();
                var lastSeenDevice = devices.OrderBy(d => d.LastSeen).FirstOrDefault();

                if (lastSeenDevice == null) {
                    throw new DeviceException("No device is avaiable.");
                }
                storedLastSeenDevice = lastSeenDevice;
                storedLastSeenDeviceTime = DateTime.Now; ;
            } catch (AuthenticationException e) {
                Console.WriteLine(e.ToString());
                throw;
            }

            return storedLastSeenDevice;
        }

        /// <summary>
        /// Exactly like <paramref cref="GetLastSeenDeviceAsync"/>, but synchronous.
        /// </summary>
        /// <exception cref="DeviceException">
        ///     If there is no avaiable device
        /// </exception>
        /// <exception cref="AuthenticationException">
        ///     If you provide wrong credentials.
        /// </exception>
        /// <returns><paramref cref="Device"/></returns>
        public Device GetLastSeenDevice() {
            var task = Task.Run(async () => {
                storedLastSeenDevice = await GetLastSeenDeviceAsync();
            });

            while (!task.IsCompleted) {
                System.Threading.Thread.Yield();
            }

            if (task.IsFaulted) {
                throw task.Exception;
            } else if (task.IsCanceled) {
                throw new Exception("Timeout.");
            }
            return storedLastSeenDevice;
        }

        /// <summary>
        /// Method <c>GetDevicesAsync</c> returns list of devices with pagination informations. It's asynchronous method.
        /// </summary>
        /// <param name="page"><see cref="int"> representing page. Default 1.</see>/></param>
        /// <exception cref="AuthenticationException">
        ///     If you provide wrong credentials for your account.
        /// </exception>
        /// <returns>
        ///     <see cref="Task{PaginingList{Device}}"/> 
        /// </returns>
        public async Task<PaginingList<Device>> GetDevicesAsync(int page = 1) {
            PaginingList<Device> devices = null;
            try {
                using (var client = new HttpClient()) {
                    BaseConfigurationHttpClient(client);
                    var response = await MakeRequestAsync(client, String.Format(_devicesUrl, Email, Password, page), "get");
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
            } catch (ArgumentException e) {
                Console.WriteLine(e.ToString());
            }
            return devices;
        }

        /// <summary>
        /// Eactly like <see cref="GetDevicesAsync"/>, but synchronous.
        /// </summary>
        /// <exception cref="AuthenticationException">
        ///     If you provide wrong credentials for your account.
        /// </exception>
        /// <returns><see cref="PaginingList{Device}"/></returns>
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
        /// <summary>
        /// Method <c>GetDeviceAsync</c> returns device for provided id. It's asynchronous method.
        /// </summary>
        /// <param name="id"><see cref="int"/> Id of device.</param>
        /// <exception cref="AuthenticationException">
        /// It's thrown when you provided wrong credentials.
        /// </exception>
        /// <exception cref="DeviceException">
        /// It's thrown when you provided ID of device, which dosen't exist.
        /// </exception>
        /// <returns><see cref="Task{Device}"/></returns>
        public async Task<Device> GetDeviceAsync(int id) {
            Device device = null;
            try {
                using (var client = new HttpClient()) {
                    BaseConfigurationHttpClient(client);
                    var response =
                        await MakeRequestAsync(client, String.Format(_deviceUrl, Email, Password, id), "get");
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
            } catch(ArgumentException e) {
                Console.WriteLine(e.ToString());
            }
            return device;
        }
        /// <summary>
        /// Exactly like <see cref="GetDeviceAsync"/>, but synchronously.
        /// </summary>
        /// <param name="id"></param>
        /// <exception cref="AuthenticationException">
        /// It's thrown when you provided wrong credentials.
        /// </exception>
        /// <exception cref="DeviceException">
        /// It's thrown when you provided ID of device, which dosen't exist.
        /// </exception>
        /// <returns><see cref="Device"/></returns>
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
        /// <summary>
        /// Return list of messages. 
        /// API's documentation don't respond to the result of query. In documentation /messages should return json with messages
        /// and pagining information. In real, just return messages.
        /// So, I can't provide you information about paginatio.
        /// </summary>
        /// <exception cref="AuthenticationException">
        ///     When you provide wrong credentials.
        /// </exception>
        /// <returns><see cref="IEnumerable{Messages}"/></returns>
        public async Task<IEnumerable<Message>> GetMessagesAsync() {
            IEnumerable<Message> messages = null;
            try {
                using (var client = new HttpClient()) {
                    BaseConfigurationHttpClient(client);
                    var response = await MakeRequestAsync(client, String.Format(_messagesUrl, Email, Password), "get");
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
            } catch (ArgumentException e) {
                Console.WriteLine(e.ToString());
            }
            return messages;
        }
        /// <summary>
        /// Exactyly, like <see cref="GetMessagesAsync"/>
        /// </summary>
        /// <exception cref="AuthenticationException">
        ///     When you provide wrong credentials.
        /// </exception>
        /// <returns><see cref="IEnumerable{Message}"/></returns>
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
        /// <summary>
        /// Return message form provided ID. It's asynchronous method.
        /// </summary>
        /// <param name="id"><see cref="int"/>Message ID</param>


        /// <returns><see cref="Message"/></returns>
        public async Task<Message> GetMessageAsync(int id) {
            Message message = null;
            try {
                using (var client = new HttpClient()) {
                    BaseConfigurationHttpClient(client);
                    var response =
                        await MakeRequestAsync(client, String.Format(_messageUrl, Email, Password, id), "get");
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
            } catch (HttpRequestException e) {
                Console.WriteLine(e.ToString());
            } catch (JsonException e) {
                Console.WriteLine(e.ToString());
            } catch (ArgumentException e) {
                Console.WriteLine(e.ToString());
            }
            return message;
        }
        /// <summary>
        /// Exactly, like <see cref="GetMessageAsync"/>, but synchronous.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
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
        /// <summary>
        /// <c>SendMessageAsync</c> is method used for sending message to provided number. It's using POST. It's asynchronous.
        /// </summary>
        /// <param name="number"><see cref="String"/> Number</param>
        /// <param name="message"><see cref="String"/> Message to send</param>
        /// <param name="deviceId"><see cref="String"/>Id of device(Used to send message). Default null, provided lastSeen device.</param>
        /// <exception cref="AuthenticationException">
        ///     Thrown when you provide wrong credentials.
        /// </exception>
        /// <exception cref="DeviceException">
        ///     Thrown when you provide ID of device, which dosen't exist.
        /// </exception>
        /// <returns><see cref="Message"/></returns>
        public async Task<Message> SendMessageAsync(string number, string message, string deviceId = null) {
            Message sentMessage = null;
            try {
                using (var client = new HttpClient()) {
                    BaseConfigurationHttpClient(client);

                    string deviceIdTemp = String.Empty;
                    if (deviceId == null) {
                        var device = await GetLastSeenDeviceAsync();
                        deviceIdTemp = device.Id.ToString();
                    } else {
                        deviceIdTemp = deviceId;
                    }

                    var response = await MakeRequestAsync(client,
                        String.Format(_sendMessageUrl, Email, Password, deviceIdTemp, number, message), "post");
                    var responseContent = await response.Content.ReadAsStringAsync();

                    Console.WriteLine(responseContent);

                    if (response.IsSuccessStatusCode) {
                        JObject jObject = JObject.Parse(responseContent);
                        if (jObject["result"]["fails"].HasValues) {
                            if (jObject["result"]["fails"][0]["errors"]["device"].HasValues) {
                                throw new DeviceException((string) jObject["result"]["fails"][0]["errors"]["device"]);
                            }
                        } else {
                            sentMessage = jObject["result"]["success"][0].ToObject<Message>();
                        }
                    }
                }
            } catch (HttpRequestException e) {
                Console.WriteLine(e.ToString());
            } catch (JsonException e) {
                Console.WriteLine(e.ToString());
            } catch(ArgumentException e) {
                Console.WriteLine(e.ToString());
            } catch (AuthenticationException e) {
                Console.WriteLine(e.ToString());
                throw;
            }

            return sentMessage;
        }
        /// <summary>
        /// Exactly, like <see cref="SendMessageAsync"/>, but synchronous.
        /// </summary>
        /// <param name="number"></param>
        /// <param name="message"></param>
        /// <param name="deviceId"></param>
        /// <returns></returns>
        public Message SendMessage(string number, string message, string deviceId = null) {
            Message sentMessage = null;

            var task = Task.Run(async () => {
                sentMessage = await SendMessageAsync(number, message, deviceId);
            });

            while (!task.IsCompleted) {
                System.Threading.Thread.Yield();
            }

            if (task.IsFaulted) {
                throw task.Exception;
            } else if (task.IsCanceled) {
                throw new Exception("Timeout.");
            }
            return sentMessage;
        }

        public Task<Message> SendMessageToContact(int contactId, string message, string deviceId = null) {
            return null;
        }

        private async Task<HttpResponseMessage> MakeRequestAsync(HttpClient client, string url, string httpMethod) {
            if (httpMethod.ToLowerInvariant() == "get") {
                var response = await client.GetAsync(url);
                return response;

            } else if (httpMethod.ToLowerInvariant() == "post") {
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, url);
                var response = await client.SendAsync(request);
                return response;
            }

            throw new ArgumentException("Provided wrong httpMethod.");

        }

        private void BaseConfigurationHttpClient(HttpClient client) {
            client.BaseAddress = new Uri(_baseUrl);
            client.DefaultRequestHeaders.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        }
    }
}
