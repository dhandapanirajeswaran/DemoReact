using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace JsPlc.Ssc.PetrolPricing.Portal.Helper.Extensions
{
    public static class HtmlHelperExtensions
    {
        public static string FormatFriendlyDateTime(this HtmlHelper helper,DateTime datetime)
        {
            var daysAgo = DateTime.Now.Date.Subtract(datetime.Date).TotalDays;
            var formattedTime = datetime.ToString("HH:mm:ss");
            if (daysAgo == 0)
                return "Today at " + formattedTime;
            if (daysAgo == 1)
                return "Yesterday at " + formattedTime;
            return datetime.ToString("dd-MMM-yyyy HH:mm:ss");
        }
    }
}