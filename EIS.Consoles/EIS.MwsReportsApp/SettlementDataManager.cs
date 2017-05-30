using System;
using System.Collections.Generic;
using System.Linq;
using EIS.MwsReportsApp.Repositories;
using EIS.Inventory.Shared.ViewModels;

namespace EIS.MwsReportsApp
{
    public static class SettlementDataManager
    {
        public static void InsertSettlementReportToDb(List<SettlementReportDto> settlementList)
        {
            Console.WriteLine("Inserting settlement reports to DB from Amazon...");

            var repository = new SettlementReportRepository();

            foreach (var settlementReport in settlementList)
            {
                // get the settlement orders data
                var orders = settlementReport.Orders;
                var orderItems = orders.SelectMany(x => x.Items).ToList();
                var orderItemFees = orderItems.SelectMany(x => x.ItemFees).ToList();
                var orderItemPrices = orderItems.SelectMany(x => x.ItemPrices).ToList();
                
                // insert first the settlement orders data
                repository.InsertSettlementOrders(orders);
                repository.InsertSettlementOrderItems(orderItems);
                repository.InsertOrderItemFees(orderItemFees);
                repository.InsertOrderItemPrices(orderItemPrices);

                // then let's get the settlement refunds data
                var refunds = settlementReport.Refunds;
                var refundsItems = refunds.SelectMany(x => x.Items).ToList();
                var refundsItemFees = refundsItems.SelectMany(x => x.ItemFees).ToList();
                var refundsItemPrices = refundsItems.SelectMany(x => x.ItemPrices).ToList();

                // insert first the settlement refunds data
                repository.InsertSettlementRefunds(refunds);
                repository.InsertSettlementRefundItems(refundsItems);
                repository.InsertRefundItemFees(refundsItemFees);
                repository.InsertRefundItemPrices(refundsItemPrices);
            }

            Console.WriteLine("Inserting settlement reports to DB done!");
        }
    }
}
