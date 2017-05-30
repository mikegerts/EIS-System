using EIS.SchedulerTaskApp.Models;

namespace EIS.SchedulerTaskApp.Profits
{
    public abstract class ProfitRule
    {
        public ProfitRule() { }
        public ProfitRule(int vendorId, decimal amount)
        {
            VendorId = vendorId;
            Amount = amount;
        }

        public int VendorId { get; set; }

        public void Apply(PurchaseOrderItem result)
        {
            apply(result);
        }

        protected decimal Amount { get; set; }
        protected abstract void apply(PurchaseOrderItem result);
    }
}
