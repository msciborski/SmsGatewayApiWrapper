using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmsGatewayApiWrapper.SmsGatewayWrapper {
    public class SmsGateway {
        /// <summary>
        /// Contains email for your account on site
        /// </summary>
        public string Email { get; private set; }

        /// <summary>
        /// Contains password for your account on site
        /// </summary>
        public string Password { get; private set; }

        public SmsGateway(string email, string password ) {
            Email = email;
            Password = password;
        }

        public async Task<IEnumerable<>>




    }
}
