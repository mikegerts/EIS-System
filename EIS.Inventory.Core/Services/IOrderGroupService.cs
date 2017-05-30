using EIS.Inventory.Core.ViewModels;
using System.Collections.Generic;

namespace EIS.Inventory.Core.Services
{
    public interface IOrderGroupService
    {
        IEnumerable<OrderGroupListViewModel> GetAllOrderGroups();
        OrderGroupViewModel GetOrderGroup(long id);
        IEnumerable<OrderViewModel> GetOrderByGroup(long groupId);
        OrderGroupViewModel CreateOrderGroup(OrderGroupViewModel model);
        OrderGroupViewModel UpdateOrderGroup(long id, OrderGroupViewModel model);
        int UpdateOrderGroupEisOrderIds(long id, List<string> orderIds);
        bool DeleteOrderGroup(long id);

    }
}
