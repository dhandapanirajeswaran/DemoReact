using System.Web;
using System.Web.Mvc;

namespace JsPlc.Ssc.PetrolPricing.Portal
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
        }
    }
}
