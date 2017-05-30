using System.Collections.Generic;
using System.Configuration;
using System.Text;
using EIS.Inventory.Shared.ViewModels;
using MySql.Data.MySqlClient;

namespace EIS.MwsReportsApp.Repositories
{
    public class SettlementReportRepository
    {
        private string _inventoryConnectionString;

        public SettlementReportRepository()
        {
            _inventoryConnectionString = ConfigurationManager.ConnectionStrings["InventoryConnection"].ConnectionString;
        }

        public bool InsertSettlementOrders(List<SettlementOrderDto> settlementOrders)
        {

            using (var conn = new MySqlConnection(_inventoryConnectionString))
            {
                var sCommand = new StringBuilder(@"INSERT INTO settlementorders(OrderId,Marketplace,ShipmentId,
                                MerchantFulfillmentId,PostedDate,SettlementId) VALUES");
                var rows = new List<string>();

                // iterate and add it into the settlement reports
                foreach (var item in settlementOrders)
                {
                    rows.Add(string.Format(@"('{0}','{1}','{2}','{3}','{4:yyyy-MM-dd HH:mm:ss}','{5}')",
                        item.OrderId, item.Marketplace, item.ShipmentId,
                        item.MerchantFulfillmentId, item.PostedDate, item.SettlementId));
                }

                sCommand.Append(string.Join(",", rows));
                sCommand.Append(";");

                MySqlHelper.ExecuteNonQuery(conn, sCommand.ToString(), null);
            }

            return true;
        }

        public void InsertSettlementOrderItems(List<SettlementItemDto> orderItems)
        {
            using (var conn = new MySqlConnection(_inventoryConnectionString))
            {
                var sCommand = new StringBuilder(@"INSERT INTO settlementorderitems
                    (OrderItemCode, OrderId, SKU, Quantity) VALUES");
                var rows = new List<string>();

                // iterate and add it into the settlement reports
                foreach (var item in orderItems)
                {
                    rows.Add(string.Format(@"('{0}','{1}','{2}',{3})",
                        item.OrderItemCode, item.OrderId, item.SKU, item.Quantity));
                }

                sCommand.Append(string.Join(",", rows));
                sCommand.Append(";");

                MySqlHelper.ExecuteNonQuery(conn, sCommand.ToString(), null);
            }
        }

        public void InsertOrderItemFees(List<ItemChargeDto> orderItemFees)
        {
            using (var conn = new MySqlConnection(_inventoryConnectionString))
            {
                var sCommand = new StringBuilder("INSERT INTO settlementorderitemfees(OrderItemCode, FeeType, CurrencyCode, Amount) VALUES");
                var rows = new List<string>();

                // iterate and add it into the settlement reports
                foreach (var item in orderItemFees)
                {
                    rows.Add(string.Format(@"('{0}','{1}','{2}',{3})", item.OrderItemCode, item.Type, item.CurrencyCode, item.Amount));
                }

                sCommand.Append(string.Join(",", rows));
                sCommand.Append(";");

                MySqlHelper.ExecuteNonQuery(conn, sCommand.ToString(), null);
            }
        }

        public void InsertOrderItemPrices(List<ItemChargeDto> orderItemPrices)
        {
            using (var conn = new MySqlConnection(_inventoryConnectionString))
            {
                var sCommand = new StringBuilder("INSERT INTO settlementorderitemprices(OrderItemCode, PriceType, CurrencyCode, Amount) VALUES");
                var rows = new List<string>();

                // iterate and add it into the settlement reports
                foreach (var item in orderItemPrices)
                {
                    rows.Add(string.Format(@"('{0}','{1}','{2}',{3})", item.OrderItemCode, item.Type, item.CurrencyCode, item.Amount));
                }

                sCommand.Append(string.Join(",", rows));
                sCommand.Append(";");

                MySqlHelper.ExecuteNonQuery(conn, sCommand.ToString(), null);
            }
        }

        public bool InsertSettlementRefunds(List<SettlementRefundDto> setllementRefunds)
        {

            using (var conn = new MySqlConnection(_inventoryConnectionString))
            {
                var sCommand = new StringBuilder(@"INSERT INTO settlementrefunds(OrderId,Marketplace,AdjustmentId,
                                MerchantFulfillmentId,PostedDate,SettlementId) VALUES");
                var rows = new List<string>();

                // iterate and add it into the settlement reports
                foreach (var item in setllementRefunds)
                {
                    rows.Add(string.Format(@"('{0}','{1}','{2}','{3}','{4:yyyy-MM-dd HH:mm:ss}','{5}')",
                        item.OrderId, item.Marketplace, item.AdjustmentId,
                        item.MerchantFulfillmentId, item.PostedDate, item.SettlementId));
                }

                sCommand.Append(string.Join(",", rows));
                sCommand.Append(";");

                MySqlHelper.ExecuteNonQuery(conn, sCommand.ToString(), null);
            }

            return true;
        }

        public void InsertSettlementRefundItems(List<SettlementItemDto> refundItems)
        {
            using (var conn = new MySqlConnection(_inventoryConnectionString))
            {
                var sCommand = new StringBuilder(@"INSERT INTO settlementrefunditems
                    (OrderItemCode, OrderId, SKU, MerchantAdjustmentItemId) VALUES");
                var rows = new List<string>();

                // iterate and add it into the settlement reports
                foreach (var item in refundItems)
                {
                    rows.Add(string.Format(@"('{0}','{1}','{2}','{3}')",
                        item.OrderItemCode, item.OrderId, item.SKU, item.MerchantAdjustmentItemId));
                }

                sCommand.Append(string.Join(",", rows));
                sCommand.Append(";");

                MySqlHelper.ExecuteNonQuery(conn, sCommand.ToString(), null);
            }
        }
        
        public void InsertRefundItemFees(List<ItemChargeDto> refundItemFees)
        {
            using (var conn = new MySqlConnection(_inventoryConnectionString))
            {
                var sCommand = new StringBuilder("INSERT INTO settlementrefunditemfees(OrderItemCode, FeeType, CurrencyCode, Amount) VALUES");
                var rows = new List<string>();

                // iterate and add it into the settlement reports
                foreach (var item in refundItemFees)
                {
                    rows.Add(string.Format(@"('{0}','{1}','{2}',{3})", item.OrderItemCode, item.Type, item.CurrencyCode, item.Amount));
                }

                sCommand.Append(string.Join(",", rows));
                sCommand.Append(";");

                MySqlHelper.ExecuteNonQuery(conn, sCommand.ToString(), null);
            }
        }

        public void InsertRefundItemPrices(List<ItemChargeDto> refundItemPrices)
        {
            using (var conn = new MySqlConnection(_inventoryConnectionString))
            {
                var sCommand = new StringBuilder("INSERT INTO settlementrefunditemprices(OrderItemCode, PriceType, CurrencyCode, Amount) VALUES");
                var rows = new List<string>();

                // iterate and add it into the settlement reports
                foreach (var item in refundItemPrices)
                {
                    rows.Add(string.Format(@"('{0}','{1}','{2}',{3})", item.OrderItemCode, item.Type, item.CurrencyCode, item.Amount));
                }

                sCommand.Append(string.Join(",", rows));
                sCommand.Append(";");

                MySqlHelper.ExecuteNonQuery(conn, sCommand.ToString(), null);
            }
        }
    }
}
