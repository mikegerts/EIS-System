using System;

namespace EIS.DataUploaderApp.Models
{
    public class VendorProduct
    {
        public long Id { get; set; }

        public int VendorId { get; set; }

        /// <summary>
        /// Gets or sets the product SKU, the vendor SKU
        /// </summary>
        public string SKU { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public string ShortDescription { get; set; }

        public string Category { get; set; }

        public string UPCCode { get; set; }

        public decimal Cost { get; set; }

        public int Quantity { get; set; }

        public DateTime ResultDate { get; set; }

        public override string ToString()
        {
            return string.Format("{0}\t{1}\t{2}\t{3}\t{4}", VendorId, Name, Category, Cost, SKU);
        }
    }
}
