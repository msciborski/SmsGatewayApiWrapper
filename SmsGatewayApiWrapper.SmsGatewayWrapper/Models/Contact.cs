using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SmsGatewayApiWrapper.SmsGatewayWrapper.Utilities.Converters;

namespace SmsGatewayApiWrapper.SmsGatewayWrapper.Models {
    public class Contact {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Number { get; set; }
        [JsonProperty("created_at")]
        [JsonConverter(typeof(DateTimeConverter))]
        public DateTime CreatedAt { get; set; }
    }
}
