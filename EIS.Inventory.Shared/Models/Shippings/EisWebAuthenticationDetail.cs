using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EIS.Inventory.Shared.Models.Shippings
{
    public class EisWebAuthenticationDetail
    {
        public EisWebAuthenticationCredential ParentCredential { get; set; }
        public EisWebAuthenticationCredential UserCredential { get; set; }
    }
}
