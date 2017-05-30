using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Entity.Validation;
using System.Linq;
using EIS.Inventory.DAL.Database;
using EIS.Inventory.Shared.Helpers;
using EIS.Inventory.Shared.Models;
using EIS.Inventory.Shared.ViewModels;
using MySql.Data.MySqlClient;
using AutoMapper;
namespace EIS.SystemJobApp.Repositories
{
    public class ShippingRateRepository
    {
        private readonly MySqlConnection _connection;
        private readonly IImageHelper _imageHelper;
        protected readonly LoggerRepository _logger;

        public ShippingRateRepository(LoggerRepository logger)
        {
            var connectionString = ConfigurationManager.ConnectionStrings["InventoryConnection"].ConnectionString;
            _connection = new MySqlConnection(connectionString);
            _logger = logger;
            _imageHelper = new ImageHelper(new PersistenceHelper());
        }

        public int DoUpadateOrInsertShippingRate(ShippingRateDB model,bool isCreate)
        {
            using (var context = new EisInventoryContext())
            {

                // get the exising product from db
                var shippingRate = context.shippingrates.FirstOrDefault(x => x.Id == model.Id);

                if (shippingRate != null)
                {
                    // let's update its data except for Products.Name
                    shippingRate.Rate = model.Rate;
                    shippingRate.Unit = model.Unit;
                    shippingRate.WeightFrom = model.WeightFrom;
                    shippingRate.WeightTo = model.WeightTo;
                }
                else
                {
                    if (!isCreate) return 0;
                    // add first the product item
                    context.shippingrates.Add(new shippingrate
                    {
                        Rate = model.Rate,
                        Unit = model.Unit,
                        WeightFrom = model.WeightFrom,
                        WeightTo = model.WeightTo,
                    });

                }
                // save the change first to the product
                context.SaveChanges();

                return 1;
            }
        }
    }
}