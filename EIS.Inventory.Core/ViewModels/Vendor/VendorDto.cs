using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using EIS.Inventory.Shared.Models;

namespace EIS.Inventory.Core.ViewModels
{
    public class VendorDto
    {
        public int Id { get; set; }

        [Required]
        [DisplayName("Vendor Name")]
        public string Name { get; set; }

        [EmailAddress]
        public string Email { get; set; }
        public string VendorAddress { get; set; }
        public string SuiteApartment { get; set; }

        [DisplayName("Full Name")]
        public string ContactPerson { get; set; }

        [DisplayName("City/Locality")]
        public string City { get; set; }

        [DisplayName("Zip Code")]
        public string ZipCode { get; set; }

        [DisplayName("Phone Number")]
        public string PhoneNumber { get; set; }

        [DisplayName("EIS SKU Code")]
        public string SKUCodeStart { get; set; }
        public DropShipFeeType DropShipFeeType { get; set; }
        public decimal? DropShipFee { get; set; }
        public int? SafetyQty { get; set; }
        public int? CompanyId { get; set; }
        public string CompanyName { get; set; }
        public string ReturnsAcceptedOption { get; set; }
        public string RefundOption { get; set; }
        public string ReturnsWithinOption { get; set; }
        public string ShippingCostPaidByOption { get; set; }
        public string ReturnPolicyDescription { get; set; }
        public string ShippingType { get; set; }
        public string ShippingService { get; set; }
        public decimal ShippingServiceCost { get; set; }
        public string State { get; set; }
        public string Website { get; set; }
        public string FaxField { get; set; }
        public string AvailableCarrier { get; set; }
        public string PickupFrequency { get; set; }
        public string ReturnPolicy { get; set; }
        public int? DaysToReturn { get; set; }
        public string ContactName { get; set; }
        public string ShipFromAddress { get; set; }
        public string ShipInfoCity { get; set; }
        public string ShipInfoState { get; set; }
        public string ShipInfoZipCode { get; set; }
        public string ShipInfoPhone { get; set; }
        public string PaymentTerms { get; set; }
        public string CreditLimit { get; set; }
        public string InventoryUpdateFrequency { get; set; }
        public int? AlwaysQuantity { get; set; }
        public string AccountNumber { get; set; }
        public string AccountType { get; set; }
        public string ShippingMethod { get; set; }
        public string ModifiedBy { get; set; }
        public virtual ICollection<VendorDepartmentDto> VendorDepartments { get; set; }
    }
}
