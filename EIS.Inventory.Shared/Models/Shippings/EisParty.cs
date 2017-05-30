using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EIS.Inventory.Shared.Models.Shippings
{
    public class EisParty
    {
        public string AccountNumber { get; set; }
        public IList<EisTaxpayerIdentification> Tins { get; set; }
        public EisContact Contact { get; set; }
        public EisAddress Address { get; set; }

    }
}
