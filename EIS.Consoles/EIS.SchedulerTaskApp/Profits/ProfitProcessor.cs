using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using EIS.SchedulerTaskApp.Helpers;
using EIS.SchedulerTaskApp.Models;
using EIS.Inventory.Shared.Helpers;
using MySql.Data.MySqlClient;
using EIS.Inventory.Shared.Models;

namespace EIS.SchedulerTaskApp.Profits
{
    public class ProfitProcessor : IProfitProcessor
    {
        private readonly string _inventoryConnectionString;
        private List<ProfitRule> _profitRules = null;

        public ProfitProcessor()
        {
            _inventoryConnectionString = ConfigurationManager.ConnectionStrings["InventoryConnection"].ConnectionString;
        }

        public void Apply(PurchaseOrderItem result, int? vendorId)
        {
            var rule = getVendorProfitRule(vendorId);
            rule.Apply(result);
        }

        private ProfitRule getVendorProfitRule(int? vendorId)
        {
            // load the profit rules
            if (_profitRules == null)
                _profitRules = getVendorProfitRules();

            return _profitRules.FirstOrDefault(x => x.VendorId == vendorId);
        }

        private List<ProfitRule> getVendorProfitRules()
        {
            var rules = new List<ProfitRule>();
            using (var conn = new MySqlConnection(_inventoryConnectionString))
            {
                var reader = MySqlHelper.ExecuteReader(conn, CommandType.Text,
                    "SELECT Id, DropShipFeeType, DropShipFee FROM vendors", null);

                while (reader.Read())
                {
                    var vendorId = Convert.ToInt32(reader[0]);
                    var feeType = EnumHelper.ParseEnum<DropShipFeeType>(reader[1].ToString());
                    var amount = Convert.ToDecimal(reader[2]);

                    var profitRule = createProfitRule(feeType, vendorId, amount);
                    rules.Add(profitRule);
                }
            }

            return rules;
        }

        private ProfitRule createProfitRule(DropShipFeeType type, int vendorId, decimal amount)
        {
            if (type == DropShipFeeType.Order)
                return new OrderProfit(vendorId, amount);
            else if (type == DropShipFeeType.Quantity)
                return new QuantityProfit(vendorId, amount);
            else
                return new ZeroProfit(vendorId, amount);
        }
    }
}
