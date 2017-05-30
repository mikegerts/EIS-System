using EIS.Inventory.Shared.Models;

namespace EIS.Inventory.Shared.ViewModels
{
    public class ShippingLocationDto
    {
        public ShippingLocationDto()
        {
            // need to instantiate this object for the knockout mapping
            IsReturnSame = true;
            FromAddressDetails = new Address();
            ReturnAddressDetails = new Address();
        }

        public int Id { get;set; }
        public string Name { get; set; }
        public string FromCompanyName { get; set; }
        public string FromPhone { get; set; }
        public string ReturnCompanyName { get; set; }
        public string ReturnPhone { get; set; }
        public bool IsReturnSame { get; set; }
        public bool IsDefault { get; set; }
        public virtual Address FromAddressDetails { get; set; }
        public virtual Address ReturnAddressDetails { get; set; }
        public string ModifiedBy { get; set; }
        public bool HasId { get { return Id > 0; } }
    }
}
