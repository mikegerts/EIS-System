namespace EIS.Inventory.Shared.Models.Shippings
{
    public partial class EisWeight
    {
        public EisWeightUnits Units { get; set; }
        public bool UnitsSpecified { get; set; }
        public decimal Value { get; set; }
        public bool ValueSpecified { get; set; }
    }

    public enum EisWeightUnits
    {
        KG, LB
    }

}
