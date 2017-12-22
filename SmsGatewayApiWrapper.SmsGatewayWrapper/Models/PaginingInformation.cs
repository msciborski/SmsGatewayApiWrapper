using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace SmsGatewayApiWrapper.SmsGatewayWrapper.Models {
    public class PaginingInformation {
        [JsonProperty("total")]
        public int Total { get; set; }

        [JsonProperty("per_page")]
        public int DevicesPerPage { get; set; }

        [JsonProperty("current_page")]
        public int CurrentPage { get; set; }
        
        [JsonProperty("last_page")]
        public int LastPage { get; set; }

        public PaginingInformation(int total, int devicesPerPage, int currentPage, int lastPage) {
            Total = total;
            DevicesPerPage = devicesPerPage;
            CurrentPage = currentPage;
            LastPage = lastPage;
        }
    }
}
