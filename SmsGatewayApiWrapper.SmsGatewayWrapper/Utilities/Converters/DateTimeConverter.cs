using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace SmsGatewayApiWrapper.SmsGatewayWrapper.Utilities.Converters {
    public class DateTimeConverter : JsonConverter {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) {
            DateTime epoch = new DateTime(1970,1,1,0,0,0, DateTimeKind.Utc);
            writer.WriteRawValue(((DateTime) value - epoch).TotalSeconds.ToString());
        }

        /// <summary>
        /// Converts epoch to DateTime
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="objectType"></param>
        /// <param name="existingValue"></param>
        /// <param name="serializer"></param>
        /// <returns>DateTime from epoch</returns>
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer) {
            var t = Convert.ToInt64(reader.Value);
            return new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds(t);
        }

        public override bool CanConvert(Type objectType) {
            return objectType == typeof(DateTime);
        }
    }
}
