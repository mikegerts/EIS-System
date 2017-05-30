using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using EIS.Inventory.Helpers;

namespace EIS.Inventory
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            GlobalConfiguration.Configure(WebApiConfig.Register);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            AutoMapperConfig.RegisterAutoMappers();
            SimpleInjectorConfig.RegisterDependencies();
            
            // bind the decimal binders
            bindDecimalModelBinder();
        }

        private void bindDecimalModelBinder()
        {
            //These two model binders have been added to solve an issue MVC has when dealing with
            //decimals passed as part of a JSON object.  When a decimal with 0 decimal places is 
            //converted to a JSON object the decimal places are removed and the MVC binder sees it
            //as an integer.  For some reason it decides it cannot convert this to a decimal so it
            //is set to zero.  The following custom bindings resolve this issue.  See link:
            //http://digitalbush.com/2011/04/24/asp-net-mvc3-json-decimal-binding-woes/

            // register our model binders
            ModelBinders.Binders.Add(typeof(decimal), new DecimalModelBinder());
            ModelBinders.Binders.Add(typeof(decimal?), new DecimalModelBinder());
        }
    }
}
