using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SmsGatewayApiWrapper.SmsGatewayWrapper.Models;

namespace SmsGatewayApiWrapper.SmsGatewayWrapper.Utilities.Converters {
    public class PaginingListConverter<T> : JsonConverter where T : class {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) {
            throw new NotImplementedException();
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer) {
            if(reader.TokenType == JsonToken.StartObject) {
                JObject item = JObject.Load(reader);

                if(item["result"]["data"] != null) {
                    var devices = item["result"]["data"].ToObject<IList<T>>();

                    int total = item["result"]["total"].Value<int>();
                    int perPage = item["result"]["per_page"].Value<int>();
                    int currentPage = item["result"]["current_page"].Value<int>();
                    int lastPage = item["result"]["last_page"].Value<int>();

                    return new PaginingList<T>(devices, new PaginingInformation(total, perPage, currentPage, lastPage));
                }
            }
            throw new Exception("There is no [\"resut\"][\"data\"]");
        }

        public override bool CanConvert(Type objectType) {
            return typeof(PaginingList<T>).GetTypeInfo().IsAssignableFrom(objectType.GetTypeInfo());
        }
    }
}
