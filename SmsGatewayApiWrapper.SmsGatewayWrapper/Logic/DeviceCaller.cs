using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SmsGatewayApiWrapper.SmsGatewayWrapper.Models;
using SmsGatewayApiWrapper.SmsGatewayWrapper.Utilities.Converters;
using SmsGatewayApiWrapper.SmsGatewayWrapper.Utilities.Exceptions;

namespace SmsGatewayApiWrapper.SmsGatewayWrapper {

    public class DeviceCaller : SmsGatewayAbstract, IDeviceCaller {
        private static readonly TimeSpan lastSeenDeviceDuration = new TimeSpan(0, 10, 0);
        public Device storedLastSeenDevice = null;
        public DateTime storedLastSeenDeviceTime = DateTime.MinValue;

        public DeviceCaller(string email, string password) : base(email, password) {
            //storedLastSeenDevice = GetLastSeenDevice();
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
        /// Eactly like <see cref="GetDevicesAsync"/>, but synchronous.
        /// </summary>
        /// <exception cref="AuthenticationException">
        ///     If you provide wrong credentials for your account.
        /// </exception>
        /// <returns><see cref="PaginingList{Device}"/></returns>
        public PaginingList<Device> GetDevices(int page = 1) {
            PaginingList<Device> devices = null;

            var task = Task.Run(async () => {
                devices = await GetDevicesAsync(page);
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
                    var response = await MakeRequestAsync(client, String.Format(_devicesUrl, Email, Password, page), OperationType.GET);
                    var jObject = await CreateJObjectResult(response);
                    JsonSerializer serializer = new JsonSerializer();
                    serializer.Converters.Add(new PaginingListConverter<Device>());
                    devices = jObject.ToObject<PaginingList<Device>>(serializer);
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
                    var response =
                        await MakeRequestAsync(client, String.Format(_deviceUrl, id, Email, Password), OperationType.GET);
                        

                    var jObject = await CreateJObjectResult(response);
                    device = jObject["result"].ToObject<Device>();
                }
            } catch (HttpRequestException e) {
                Console.WriteLine(e.ToString());
            } catch (JsonException e) {
                Console.WriteLine(e.ToString());
            } catch (ArgumentException e) {
                Console.WriteLine(e.ToString());
            }
            return device;
        }

        private async Task<JObject> CreateJObjectResult(HttpResponseMessage response) {
            var jObject = await CreateJObjectFromResponse(response);
            if (!response.IsSuccessStatusCode) {
                CreateError(jObject);
            }
            return jObject;
        }
        private async Task<JObject> CreateJObjectFromResponse(HttpResponseMessage response) {
            var responseContent = await response.Content.ReadAsStringAsync();
            return JObject.Parse(responseContent);

        }
        private void CreateError(JObject jObject) {
            if (jObject["errors"]["login"] != null) {
                throw new AuthenticationException((string) jObject["errors"]["login"]);
            }
            if (jObject["errors"]["id"] != null) {
                throw new DeviceException((string) jObject["errors"]["id"]);
            }
            if (jObject["errors"] != null) {
                var error = jObject["errors"].Select(t => (string) t).FirstOrDefault();
                throw new AuthenticationException(error);
            }
        }


    }
}
