using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EIS.OrdersServiceApp.Models {
    public class BigCommerceCredential : Credential {
        public string Username { get; set; }
        public string ApiKey { get; set; }
    }
}
