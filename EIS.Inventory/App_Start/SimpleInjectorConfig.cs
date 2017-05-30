using System.Configuration;
using System.Reflection;
using System.Web;
using System.Web.Mvc;
using EIS.Inventory.Core;
using EIS.Inventory.Core.Managers;
using EIS.Inventory.Core.Services;
using EIS.Inventory.Shared.Helpers;
using SimpleInjector;
using SimpleInjector.Integration.Web;
using SimpleInjector.Integration.Web.Mvc;

namespace EIS.Inventory
{
    public class SimpleInjectorConfig
    {
        public static void RegisterDependencies()
        {
            // create the container
            var container = new Container();
            Core.Core.Resolver = new CoreResolver(container);

            registerDalServices(container);

            // this is an extension method from the integration package
            container.RegisterMvcControllers(Assembly.GetExecutingAssembly());

            // this is an extension method from the integration package as well
            container.RegisterMvcIntegratedFilterProvider();
            
            // bind the MEF objects
            Core.Core.RegisterMEF();

            // optionally, check the configuration for correctness,
            // such as missing dependencies and recursive dependency graphs.
            // This will prevent the application from starting in case of an error
            container.Verify();
            
            DependencyResolver.SetResolver(new SimpleInjectorDependencyResolver(container));
        }

        private static void registerDalServices(Container container)
        {
            var vendorConnectionString = ConfigurationManager.ConnectionStrings["InventoryConnection"].ConnectionString;
            
            // set the scope lifestyle one directly after creating the container
            container.Options.DefaultScopedLifestyle = new WebRequestLifestyle();

            var hybridLifestyle = Lifestyle.CreateHybrid(
                        () => HttpContext.Current != null,
                        new WebRequestLifestyle(),
                        Lifestyle.Transient);

            //container.RegisterPerWebRequest<IAccountService, AccountService>();
            container.Register<IProductService, ProductService>(hybridLifestyle);
            container.Register<IVendorService, VendorService>(hybridLifestyle);
            container.Register<IProductTypeService, ProductTypeService>(hybridLifestyle);
            container.Register<IOrderService, OrderService>(hybridLifestyle);
            container.Register<ILogService, LogService>(hybridLifestyle);
            container.Register<IReportLogService>(() => new ReportLogService(vendorConnectionString), hybridLifestyle);
            container.Register<IScheduledTaskService, ScheduledTaskService>(hybridLifestyle);
            container.Register<ISystemJobService, SystemJobService>(hybridLifestyle);
            container.Register<IKitService, KitService>(hybridLifestyle);
            container.Register<IVendorProductLinkService, VendorProductLinkService>(hybridLifestyle);
            container.Register<IShippingService, ShippingService>(hybridLifestyle);

            container.Register<IBillingService, BillingService>(hybridLifestyle);
            container.Register<IProductGroupService, ProductGroupService>(hybridLifestyle);
            container.Register<IOrderGroupService, OrderGroupService>(hybridLifestyle);

            container.Register<IPersistenceHelper, PersistenceHelper>(hybridLifestyle);
            container.Register<IImageHelper, ImageHelper>(hybridLifestyle);
            container.Register<IFileSettingService>(() => new FileSettingService(vendorConnectionString), hybridLifestyle);
            container.Register<ICompanyService, CompanyService>(hybridLifestyle);
            container.Register<ICredentialService, CredentialService>(hybridLifestyle);
            container.Register<IShippingRateService, ShippingRateService>(hybridLifestyle);
            container.Register<IMessageTemplateService, MessageTemplateService>(hybridLifestyle);
            container.Register<IReportTemplateService, ReportTemplateService>(hybridLifestyle);
            container.Register<IVendorProductService, VendorProductService>(hybridLifestyle);
            container.Register<IExportDataService, ExportDataService>(hybridLifestyle);
            container.Register<ISystemEmailsService, SystemEmailsService>(hybridLifestyle);
            // managers
            container.Register<IShippingRateManager, ShippingRateManager>(hybridLifestyle);
            container.Register<IMarketplaceOrdersManager, MarketplaceOrdersManager>(hybridLifestyle);
            container.Register<IMarketplaceInventoryManager, MarketplaceInventoryManager>(hybridLifestyle);
            container.Register<IMarketplaceProductManager, MarketplaceProductManager>(hybridLifestyle);
            container.Register<ISavedSearchFilterService, SavedSearchFilterService>(hybridLifestyle);
            container.Register<ICustomerService, CustomerService>(hybridLifestyle);
            container.Register<ICustomerScheduledTaskService, CustomerScheduledTaskService>(hybridLifestyle);
            container.Register<IFileHelper, FileHelper>(hybridLifestyle);
        }
    }
}