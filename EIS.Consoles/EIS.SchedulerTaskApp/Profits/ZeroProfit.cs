using EIS.SchedulerTaskApp.Models;

namespace EIS.SchedulerTaskApp.Profits
{
    public class ZeroProfit : ProfitRule
    {
        public ZeroProfit() { }

        public ZeroProfit(int vendorId, decimal amount)
            : base(vendorId, amount)
        {

        }

        protected override void apply(PurchaseOrderItem result)
        {
            return;
        }
    }
}
