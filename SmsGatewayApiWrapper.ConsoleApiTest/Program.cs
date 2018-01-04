using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmsGatewayApiWrapper.SmsGatewayWrapper;
using SmsGatewayApiWrapper.SmsGatewayWrapper.Models;

namespace SmsGatewayApiWrapper.ConsoleApiTest {
    class Program {
        private static SmsGateway smsGateway;
        private static string email = ConfigurationManager.AppSettings["email"];
        private static string password = ConfigurationManager.AppSettings["password"];
        private static string invalidEmail = "test@test.com";
        static async Task Main(string[] args) {
            var wrongEmail = "test@gmail.com";
            smsGateway = new SmsGateway(email, password);
            //await GetMessages();
            //await GetMessage();
            //await SendMessage();
            //await GetContacts();
            await SendMessageToContact();
            //await SendMessageToManyNumbers();
            Console.ReadLine();
        }
        static async Task GetDevicesTest() {
            var paginingDevices = await smsGateway.GetDevicesAsync();

            foreach (var device in paginingDevices) {
                Console.WriteLine(device.Id);
            }
        }
        static async Task GetMessages() {
            try {
                var paginingMesages = await smsGateway.GetMessagesAsync();
                foreach (var paginingMesage in paginingMesages) {
                    Console.WriteLine("Tresc -> {0}\nOd ->{1}", paginingMesage.MessageContent, paginingMesage.Contact.Number);
                }
            } catch (Exception e) {
                Console.WriteLine(e.ToString());
            }
        }
        static async Task SendMessageToContact() {
            var message = "test, test";
            var id = "10520922";
            try {
                var m = await smsGateway.SendMessageToContactAsync(id, message);
                Console.WriteLine(m.Contact.Name);
            } catch (Exception e) {
                Console.WriteLine(e);
            }
        }
        static async Task SendMessageToManyNumbers() {
            var message = "test, test";
            string[] numbers = new[] { "780273188", "515054859" };
            try {
                var messages = await smsGateway.SendMessageToManyAsync(numbers, message);
                foreach (var message1 in messages) {
                    Console.WriteLine(message1.Id);
                }
            }catch(Exception e) {
                Console.WriteLine(e.Message);
            }
        }
        static async Task GetMessage() {
            var message = await smsGateway.GetMessageAsync(51696153);
            Console.WriteLine("Text: {0}\n, Contact: {1}", message.MessageContent, message.Contact.Name);
        }
        static async Task SendMessage() {
            string number = "48515054859";
            string messageText = "Test, test";
            try {
                var message = await smsGateway.SendMessageAsync(number, messageText);
                Console.WriteLine("{0} {1}", message.Id, message.MessageContent);
            } catch (Exception e) {
                Console.WriteLine(e.ToString());
            }
        }
        static async Task GetContacts() {
            var contacts = await smsGateway.GetContactsAsync();
            foreach (var contact in contacts) {
                Console.WriteLine("Id -> {0}\nName -> {1}\nNumber -> {2}\nCreated at -> {3}", contact.Id, contact.Name, contact.Number, contact.CreatedAt);
            }
        }
    }
}
