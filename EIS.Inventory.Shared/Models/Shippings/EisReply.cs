using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EIS.Inventory.Shared.Models.Shippings
{
    public class EisReply
    {
        public string JobId { get; set; }
        public EisTransactionDetail TransactionDetail { get; set; }
        public EisVersionId Version { get; set; }
        public EisCompletedShipmentDetail CompletedShipmentDetail { get; set; }
    }
}
