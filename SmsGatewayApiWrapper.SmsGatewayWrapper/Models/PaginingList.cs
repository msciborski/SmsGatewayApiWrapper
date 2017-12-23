using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace SmsGatewayApiWrapper.SmsGatewayWrapper.Models {
    public class PaginingList<T> : List<T> where T : class {
        public PaginingInformation Pagining { get; set; }

        public PaginingList() {
            
        }

        public PaginingList(IList<Device> devices, IEnumerable<T> collection)
            : base(collection) {
            
        }

        public PaginingList(IEnumerable<T> collection, PaginingInformation pagining)
            : base(collection) {
            Pagining = pagining;
        }
        public PaginingList(int capacity)
            : base(capacity) {
            
        }
    }
}
