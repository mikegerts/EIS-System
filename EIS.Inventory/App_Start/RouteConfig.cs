using System.Web.Mvc;
using System.Web.Routing;

namespace EIS.Inventory
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");
            routes.IgnoreRoute("{*favicon}", new { favicon = @"(.*/)?favicon.ico(/.*)?" });

            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "home", action = "index", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "products",
                url: "products",
                defaults: new { controller = "product", action = "index", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "vendors",
                url: "vendors",
                defaults: new { controller = "Vendor", action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}
