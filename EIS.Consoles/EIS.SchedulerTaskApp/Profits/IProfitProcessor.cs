using EIS.SchedulerTaskApp.Models;

namespace EIS.SchedulerTaskApp.Profits
{
    public interface IProfitProcessor
    {
        void Apply(PurchaseOrderItem result, int? vendorId);
    }
}
