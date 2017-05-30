using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using EIS.Inventory.Core.ViewModels;
using EIS.Inventory.DAL.Database;

namespace EIS.Inventory.Core.Services
{
    public class OrderGroupService :IOrderGroupService
    {
        private readonly EisInventoryContext _context;
        public OrderGroupService()
        {
            _context = new EisInventoryContext();
        }

        public IEnumerable<OrderGroupListViewModel> GetAllOrderGroups()
        {
            var orderGroups = _context.ordergroupdetails.ToList();

            return Mapper.Map<IEnumerable<OrderGroupListViewModel>>(orderGroups);
        }

        public OrderGroupViewModel GetOrderGroup(long id)
        {
            var orderGroup = _context.ordergroupdetails.FirstOrDefault(x => x.Id == id);
            return Mapper.Map<ordergroupdetail, OrderGroupViewModel>(orderGroup);
        }

        public IEnumerable<OrderViewModel> GetOrderByGroup(long groupId)
        {
            var orderGroup = _context.ordergroupdetails.FirstOrDefault(x => x.Id == groupId);

            return Mapper.Map<IEnumerable<order>, IEnumerable<OrderViewModel>>(orderGroup.orders);

        }

        public OrderGroupViewModel CreateOrderGroup(OrderGroupViewModel model)
        {
            var orderGroup = Mapper.Map<OrderGroupViewModel, ordergroupdetail>(model);
            orderGroup.Modified = DateTime.UtcNow;
            orderGroup.Created = DateTime.UtcNow;

            orderGroup.orders.Clear();

            foreach (var orderId in model.OrderIds)
            {
                var order = _context.orders.FirstOrDefault(x => x.OrderId == orderId);
                if (order == null)
                    continue;

                orderGroup.orders.Add(order);
            }

            _context.ordergroupdetails.Add(orderGroup);
            _context.SaveChanges();

            return Mapper.Map<ordergroupdetail, OrderGroupViewModel>(orderGroup);
        }

        public int UpdateOrderGroupEisOrderIds(long id, List<string> orderIds)
        {
            var orderGroup = _context.ordergroupdetails.FirstOrDefault(x => x.Id == id);

            orderGroup.orders.Clear();

            var counter = 0;
            foreach(var orderId in orderIds)
            {
                var order = _context.orders.FirstOrDefault(x => x.OrderId == orderId);
                if (order == null)
                    continue;

                counter++;
                orderGroup.orders.Add(order);
            }

            _context.SaveChanges();

            return counter;
        }

        public bool DeleteOrderGroup(long id)
        {
            var orderGroup = _context.ordergroupdetails.FirstOrDefault(x => x.Id == id);

            orderGroup.orders.Clear();

            _context.ordergroupdetails.Remove(orderGroup);
            _context.SaveChanges();

            return true;
            
        }


        public OrderGroupViewModel UpdateOrderGroup(long id, OrderGroupViewModel model)
        {
            var oldOrderGroup = _context.ordergroupdetails.FirstOrDefault(x => x.Id == id);

            oldOrderGroup.orders.Clear();

            foreach (var orderId in model.OrderIds)
            {
                var order = _context.orders.FirstOrDefault(x => x.OrderId == orderId);
                if(order == null)
                    continue;

                oldOrderGroup.orders.Add(order);
            }

            oldOrderGroup.Modified = DateTime.UtcNow;
            _context.Entry(oldOrderGroup).CurrentValues.SetValues(model);
            _context.SaveChanges();

            return model;
        }
    }
}
