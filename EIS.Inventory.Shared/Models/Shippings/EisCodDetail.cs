using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EIS.Inventory.Shared.Models.Shippings
{
    public class EisCodDetail
    {
        public EisCodCollectionType CollectionType;
        public EisMoney CodCollectionAmount;
    }

    public enum EisCodCollectionType
    {
        ANY, CASH, COMPANY_CHECK, GUARANTEED_FUNDS, PERSONAL_CHECK
    }
}
