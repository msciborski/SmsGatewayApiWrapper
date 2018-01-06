using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SmsGatewayApiWrapper.SmsGatewayWrapper.Models;
using SmsGatewayApiWrapper.SmsGatewayWrapper.Utilities.Exceptions;

namespace SmsGatewayApiWrapper.SmsGatewayWrapper {
    public class MessageCaller : SmsGatewayAbstract, IMessageCaller {
        private IDeviceCaller Device { get; set; }
        internal MessageCaller(string email, string password, IDeviceCaller device)
            : base(email, password) {
            Device = device;

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
                    var response = await MakeRequestAsync(client, String.Format(_messagesUrl, Email, Password), OperationType.GET);
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
        /// Return message form provided ID. It's asynchronous method.
        /// </summary>
        /// <param name="id"><see cref="int"/>Message ID</param>
        /// <returns><see cref="Message"/></returns>
        public async Task<Message> GetMessageAsync(int id) {
            Message message = null;
            try {
                using (var client = new HttpClient()) {
                    var response =
                        await MakeRequestAsync(client, String.Format(_messageUrl, Email, Password, id), OperationType.GET);
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
        /// Exactly, like <see cref="SendMessageAsync"/>, but synchronous.
        /// </summary>
        /// <param name="number"></param>
        /// <param name="message"></param>
        /// <param name="deviceId"></param>
        /// <returns></returns>
        public Message SendMessage(string number, string message, string deviceId = null) {
            return SendMessage(message, number, MessageType.ToNumber, deviceId);
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
            return await SendMessageAsync(message, number, MessageType.ToNumber, deviceId);
        }

        public IEnumerable<Message> SendMessageToMany(IEnumerable<string> numbers, string message, string deviceId = null) {
            return SendMessage(message, numbers, MessageType.ToNumber, deviceId);
        }
        public async Task<IEnumerable<Message>> SendMessageToManyAsync(IEnumerable<string> numbers, string message, string deviceId = null) {
            return await SendMessageAsync(message, numbers, MessageType.ToNumber, deviceId);
        }

        public Message SendMessageToContact(string contactId, string message, string deviceId = null) {
            return SendMessage(message, contactId, deviceId);
        }
        public async Task<Message> SendMessageToContactAsync(string contactId, string message, string deviceId = null) {
            return await SendMessageAsync(message, contactId, MessageType.ToContact, deviceId);
        }

        public IEnumerable<Message> SendMessageToManyContacts(IEnumerable<string> contactIds, string message, string deviceId = null) {
            return SendMessage(message, contactIds, MessageType.ToContact, deviceId);
        }
        public async Task<IEnumerable<Message>> SendMessageToManyContactsAsync(IEnumerable<string> contactIds, string message, string deviceId = null) {
            return await SendMessageAsync(message, contactIds, MessageType.ToContact, deviceId);
        }


        private Message SendMessage(string message, string recipient, MessageType messageType, string deviceId = null) {
            Message result = null;
            var task = Task.Run(async () => {
                result = await SendMessageAsync(message, recipient, messageType, deviceId);
            });

            while (!task.IsCompleted) {
                System.Threading.Thread.Yield();
            }

            if (task.IsFaulted) {
                throw task.Exception;
            } else if (task.IsCanceled) {
                throw new Exception("Timeout.");
            }
            return result;
        }
        private async Task<Message> SendMessageAsync(string message, string recipient, MessageType messageType, string deviceId = null) {
            IEnumerable<string> recipients = new List<string>() { recipient };
            var result = await SendMessageAsync(message, recipients, messageType, deviceId);
            return result.FirstOrDefault();
        }

        private IEnumerable<Message> SendMessage(string message, IEnumerable<string> recipients, MessageType messageType, string deviceId = null) {
            IEnumerable<Message> result = null;
            var task = Task.Run(async () => {
                result = await SendMessageAsync(message, recipients, messageType, deviceId);
            });

            while (!task.IsCompleted) {
                System.Threading.Thread.Yield();
            }

            if (task.IsFaulted) {
                throw task.Exception;
            } else if (task.IsCanceled) {
                throw new Exception("Timeout.");
            }
            return result;
        }
        private async Task<IEnumerable<Message>> SendMessageAsync(string message, IEnumerable<string> recipients, MessageType messageType, string deviceId = null) {
            List<Message> sentMessage = null;
            try {
                using (var client = new HttpClient()) {
                    string tempDeviceId = String.Empty;
                    if (deviceId == null) {

                        var device = await Device.GetLastSeenDeviceAsync();
                        tempDeviceId = device.Id.ToString();
                    } else {
                        tempDeviceId = deviceId;
                    }

                    var jsonContent = CreateJsonMessage(recipients, message, tempDeviceId, messageType);

                    var response = await MakeRequestAsync(client, _sendMessageUrl, OperationType.POST, jsonContent);
                    var responseContent = await response.Content.ReadAsStringAsync();

                    if (response.IsSuccessStatusCode) {
                        sentMessage = CreateResult(responseContent).ToList();
                    }
                }
            } catch (HttpRequestException e) {
                Console.WriteLine(e.ToString());
            } catch (JsonException e) {
                Console.WriteLine(e.ToString());
            }
            return sentMessage;
        }

        private IEnumerable<Message> CreateResult(string responseContent) {
            JObject jObject = JObject.Parse(responseContent);
            if (jObject["result"]["fails"].HasValues) {
                if (jObject["result"]["fails"][0]["errors"]["device"] != null && jObject["result"]["fails"][0]["errors"]["contact"] != null) {
                    throw new SmsGatewayException((string) jObject["result"]["fails"][0]["errors"]["device"] + "\n" +
                                                  (string) jObject["result"]["fails"][0]["errors"]["contact"]);
                }
                if (jObject["result"]["fails"][0]["errors"]["device"] != null) {
                    throw new DeviceException((string) jObject["result"]["fails"][0]["errors"]["device"][0]);
                }
                if (jObject["result"]["fails"][0]["errors"]["contact"] != null) {
                    throw new ContactException((string) jObject["result"]["fails"][0]["errors"]["contact"]);
                }
            }
            if (jObject["errors"] != null) {
                if (jObject["errors"]["login"] != null) {
                    throw new AuthenticationException((string) jObject["errors"]["login"]);
                }
            }
            if (jObject["result"]["fails"].HasValues) {
                if (jObject["result"]["fails"][0]["errors"]["device"].HasValues) {
                    throw new DeviceException((string) jObject["result"]["fails"][0]["errors"]["device"]);
                }
            }
            return jObject["result"]["success"].ToObject<List<Message>>();
        }
        private string CreateJsonMessage(IEnumerable<string> recipients, string message, string deviceId, MessageType messageType) {
            switch (messageType) {
                case MessageType.ToNumber:
                    return JsonConvert.SerializeObject(new {
                        email = Email,
                        password = Password,
                        device = deviceId,
                        number = recipients,
                        message
                    });
                case MessageType.ToContact:
                    return JsonConvert.SerializeObject(new {
                        email = Email,
                        password = Password,
                        device = deviceId,
                        contact = recipients,
                        message
                    });
            }
            throw new ArgumentException("Operation type is not defined.");
        }
    }
}
