using EIS.Inventory;
using EIS.Inventory.Core;
using EIS.Inventory.Core.Managers;
using EIS.Inventory.Core.Services;
using EIS.Marketplace.Amazon;
using EIS.Marketplace.ConsoleApp;
using SimpleInjector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EIS.Marketplaces.ConsoleApp
{
    class Program
    {
        private static string accessKey = "AKIAJ2S4WUDF5FFYKCJA";
        private static string signature = "HmacSHA256";
        private static IMarketplaceProductManager manager;


        static void Main(string[] args)
        {
            // create the container
            var container = new Container();
            // init the Core Resolver
            Core.Resolver = new EIS.Inventory.Core.CoreResolver(container);

            // bind the MEF objects
            Core.RegisterMEF();

            ProdutInfoTest();

            Console.WriteLine("Submitting feed done!");
            Console.ReadKey();
        }

        public static void ProdutInfoTest()
        {
            var marketplaceService = new MarketplaceSettingService(null);
            //manager = new MarketplaceProductManager(marketplaceService);

            var result = manager.GetMarketplaceProductInfo("Amazon", "CJ10800000035350");
        }
    }
}
