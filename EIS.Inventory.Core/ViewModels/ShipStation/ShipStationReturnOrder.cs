
namespace EIS.Inventory.Core.ViewModels
{
    public class ShipStationReturnOrder
    {
        public int OrderId { get; set; }
        public string OrderNumber { get; set; }
        public string OrderKey { get; set; }
        public bool Success { get; set; }
        public string ErrorMessage { get; set; }
    }
}
