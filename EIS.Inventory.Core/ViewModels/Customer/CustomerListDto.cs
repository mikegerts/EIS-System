using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EIS.Inventory.Core.ViewModels
{
   public class CustomerListDto
    {
        public int CustomerId { get; set; }
        
        public int CustomerNumber { get; set; }
        
        public string CompanyName { get; set; }
        
        public int CompanyId { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string EmailAddress { get; set; }

        public string OfficeNumber { get; set; }

        public string AccountType { get; set; }

        public decimal CostPlusBasedWholeSalePrice { get; set; }

        public string CostPlusBasedWholeSalePriceType { get; set; }
        
        public decimal? CreditLimit { get; set; }

        public int? CreditTerms { get; set; }

        public int PriceType { get; set; }

        public string ModifiedBy { get; set; }

        public string Modified { get; set; }

        public string CreatedBy { get; set; }

        public string Created { get; set; }
    }
}
