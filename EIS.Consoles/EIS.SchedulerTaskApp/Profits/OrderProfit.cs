using EIS.SchedulerTaskApp.Models;

namespace EIS.SchedulerTaskApp.Profits
{
    public class OrderProfit : ProfitRule
    {
        public OrderProfit() { }

        public OrderProfit(int vendorId, decimal amount)
            : base(vendorId, amount)
        {

        }

        protected override void apply(PurchaseOrderItem result)
        {
            result.Profit += Amount;
        }
    }
}
