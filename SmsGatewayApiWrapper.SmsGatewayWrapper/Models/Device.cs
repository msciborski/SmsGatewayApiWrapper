using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SmsGatewayApiWrapper.SmsGatewayWrapper.Utilities.Converters;

namespace SmsGatewayApiWrapper.SmsGatewayWrapper.Models {
    public class Device {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("make")]
        public string Maker { get; set; }

        [JsonProperty("model")]
        public string Model { get; set; }

        [JsonProperty("number")]
        public string Number { get; set; }

        [JsonProperty("provider")]
        public string Provider { get; set; }

        [JsonProperty("country")]
        public string Country { get; set; }

        [JsonProperty("connection_type")]
        public string ConnectionType { get; set; }

        [JsonProperty("battery")]
        public string Battery { get; set; }

        [JsonProperty("signal")]
        public string Signal { get; set; }

        [JsonProperty("lat")]
        public string Latitude { get; set; }

        [JsonProperty("lng")]
        public string Length { get; set; }

        [JsonProperty("last_seen")]
        [JsonConverter(typeof(DateTimeConverter))]
        public DateTime LastSeen { get; set; }
    }
}
