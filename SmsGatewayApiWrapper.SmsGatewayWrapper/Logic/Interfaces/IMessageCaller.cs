using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmsGatewayApiWrapper.SmsGatewayWrapper.Models;

namespace SmsGatewayApiWrapper.SmsGatewayWrapper {
    public interface IMessageCaller {
        IEnumerable<Message> GetMessages();
        Task<IEnumerable<Message>> GetMessagesAsync();

        Message GetMessage(int id);
        Task<Message> GetMessageAsync(int id);

        Message SendMessage(string number, string message, string deviceId = null);
        Task<Message> SendMessageAsync(string number, string message, string deviceId = null);

        IEnumerable<Message> SendMessageToMany(IEnumerable<string> numbers, string message, string deviceId = null);

        Task<IEnumerable<Message>> SendMessageToManyAsync(IEnumerable<string> numbers, string message,
            string deviceId = null);

        Message SendMessageToContact(string contactId, string message, string deviceId = null);
        Task<Message> SendMessageToContactAsync(string contactId, string message, string deviceId = null);
    }
}
