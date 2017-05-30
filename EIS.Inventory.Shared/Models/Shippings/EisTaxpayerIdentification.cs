namespace EIS.Inventory.Shared.Models.Shippings
{
    public class EisTaxpayerIdentification
    {
        public EisTinType TinType { get; set; }
        public string Number { get; set; }
    }

    public enum EisTinType
    {
        BUSINESS_NATIONAL, BUSINESS_STATE, BUSINESS_UNION, PERSONAL_NATIONAL, PERSONAL_STATE
    }

}
