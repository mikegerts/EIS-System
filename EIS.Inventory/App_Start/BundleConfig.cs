using System.Web.Optimization;

namespace EIS.Inventory
{
    public class BundleConfig
    {
        // For more information on bundling, visit http://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                        "~/Scripts/jquery-3.1.1.min.js"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryui").Include(
                    "~/Scripts/jquery-ui.min.js"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
                        "~/Scripts/jquery.validate*"));

            // Use the development version of Modernizr to develop with and learn from. Then, when you're
            // ready for production, use the build tool at http://modernizr.com to pick only the tests you need.
            bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
                        "~/Scripts/modernizr-*"));

            bundles.Add(new ScriptBundle("~/bundles/bootstrap").Include(
                      "~/Scripts/bootstrap.min.js",
                      "~/Scripts/respond.min.js"));

            bundles.Add(new ScriptBundle("~/bundles/iCheck").Include(
                    "~/Scripts/plugins/iCheck/icheck.min.js"));

            bundles.Add(new ScriptBundle("~/bundles/AdminLTE").Include(
                    "~/Scripts/AdminLTE/app.js"));



            bundles.Add(new StyleBundle("~/Content/css/AdminLTE").Include(
                      "~/Content/css/skins/_all-skins.min.css",
                      "~/Content/css/AdminLTE.css",
                      "~/Content/css/Site.css"));  

            bundles.Add(new StyleBundle("~/Content/css/bootstrap").Include(
                      "~/Content/css/bootstrap.min.css"));
                      //"~/Content/fonts/font-awesome-4.3.0/css/font-awesome.css"));

            bundles.Add(new StyleBundle("~/Content/css/iCheck").Include(
                   "~/Content/css/iCheck/icheck.min.css"));

            bundles.Add(new StyleBundle("~/Content/css/calendar").Include(
                     "~/Content/css/plugins/calendar/fullCalendar.min.css",
                     "~/Content/css/plugins/calendar/fullCalendarPrint.css"));
        }
    }
}
