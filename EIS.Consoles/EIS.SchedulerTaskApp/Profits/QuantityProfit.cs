using EIS.SchedulerTaskApp.Models;
using System;

namespace EIS.SchedulerTaskApp.Profits
{
    public class QuantityProfit : ProfitRule
    {
        public QuantityProfit() { }

        public QuantityProfit(int vendorId, decimal amount)
            : base(vendorId, amount)
        {

        }

        protected override void apply(PurchaseOrderItem result)
        {
            result.Profit += (result.SupplierQtyOrdered * Amount);
        }
    }
}
