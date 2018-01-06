using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SmsGatewayApiWrapper.SmsGatewayWrapper.Logic.Interfaces;
using SmsGatewayApiWrapper.SmsGatewayWrapper.Models;
using SmsGatewayApiWrapper.SmsGatewayWrapper.Utilities.Converters;
using SmsGatewayApiWrapper.SmsGatewayWrapper.Utilities.Exceptions;

namespace SmsGatewayApiWrapper.SmsGatewayWrapper {
    public class ContactCaller : SmsGatewayAbstract, IContactCaller {
  
        public ContactCaller(string email, string password) : base(email, password) {
            
        }

        public async Task<PaginingList<Contact>> GetContactsAsync(int page = 1) {
            PaginingList<Contact> contacts = null;
            try {
                using (var client = new HttpClient()) {
                    var response = await MakeRequestAsync(client, String.Format(_contactsUrl, Email, Password, page),
                        OperationType.GET);
                    var responseContent = await response.Content.ReadAsStringAsync();

                    if (response.IsSuccessStatusCode) {
                        JObject jObject = JObject.Parse(responseContent);
                        JsonSerializer serializer = new JsonSerializer();
                        serializer.Converters.Add(new PaginingListConverter<Contact>());
                        contacts = jObject.ToObject<PaginingList<Contact>>(serializer);
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
            return contacts;
        }
        public PaginingList<Contact> GetContacts(int page = 1) {
            PaginingList<Contact> contacts = null;

            var task = Task.Run(async () => {
                contacts = await GetContactsAsync(page);
            });

            while (!task.IsCompleted) {
                System.Threading.Thread.Yield();
            }

            if (task.IsFaulted) {
                throw task.Exception;
            }

            if (task.IsCanceled) {
                throw new Exception("Timeout.");
            }

            return contacts;
        }

    }
}
