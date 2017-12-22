using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace SmsGatewayApiWrapper.SmsGatewayWrapper.Models {
    public class DeviceList<T> : List<T> where T : class {
        public PaginingInformation Pagining { get; set; }

        public DeviceList() {
            
        }

        public DeviceList(IEnumerable<T> collection)
            : base(collection) {
            
        }

        public DeviceList(IEnumerable<T> collection, PaginingInformation pagining)
            : base(collection) {
            Pagining = pagining;
        }
        public DeviceList(int capacity)
            : base(capacity) {
            
        }

    }
}
