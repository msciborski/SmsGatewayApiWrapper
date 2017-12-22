using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace SmsGatewayApiWrapper.SmsGatewayWrapper.Models {
    public class Message {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("device_id")]
        public int DeviceId { get; set; }

        [JsonProperty("message")]
        public string MessageContent { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("send_at")]
        public DateTime SendAt { get; set; }

        [JsonProperty("queued_at")]
        public DateTime QueuedAt { get; set; }

        [JsonProperty("sent_at")]
        public DateTime SentAt { get; set; }

        [JsonProperty("delivered_at")]
        public DateTime DeliveredAt { get; set; }

        [JsonProperty("expires_at")]
        public DateTime ExpiresAt { get; set; }

        [JsonProperty("canceled_at")]
        public DateTime CanceledAt { get; set; }

        [JsonProperty("failed_at")]
        public DateTime FailedAt { get; set; }

        [JsonProperty("received_at")]
        public DateTime ReceivedAt { get; set; }

        [JsonProperty("created_at")]
        public DateTime CreatedAt { get; set; }

        [JsonProperty("contact")]
        public Contact Contact { get; set; }

    }
}
