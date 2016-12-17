using System.Web;
using System.Web.Optimization;

namespace JsPlc.Ssc.PetrolPricing.Portal
{
    public class BundleConfig
    {
        // For more information on bundling, visit http://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
          //  bundles.IgnoreList.Clear();
            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                        "~/Scripts/jquery-{version}.js",
                        "~/Scripts/chosen/chosen.jquery.js",
                       "~/Scripts/bootstrap-datepicker.min.js"));

            bundles.Add(new ScriptBundle("~/bundles/datatables").Include(
                     "~/Scripts/jquery.dataTables.min.js",
                     "~/Scripts/jquery.dataTables.js",
                      "~/Scripts/dataTables.bootstrap.min.js"));

            
            //Unobtrusive validation doesn't work. Temporary disabling
            bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
                        //"~/Scripts/jquery.validate*")
                        ));

            // Use the development version of Modernizr to develop with and learn from. Then, when you're
            // ready for production, use the build tool at http://modernizr.com to pick only the tests you need.
            bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
                        "~/Scripts/modernizr-*"));

            bundles.Add(new ScriptBundle("~/bundles/bootstrap").Include(
                      //"~/Scripts/bootstrap.js",
                      "~/Scripts/respond.js"));

            bundles.Add(new StyleBundle("~/Content/css").Include(
                      "~/Content/chosen.css",                                                              
                      "~/Content/bootstrap.css",
                      "~/Content/font-awesome.min.css",
                      "~/Content/bootstrap-datepicker.min.css",
                      "~/Content/site.css",
                      "~/Content/jquery.dataTables.min.css"));
        }
    }
}
