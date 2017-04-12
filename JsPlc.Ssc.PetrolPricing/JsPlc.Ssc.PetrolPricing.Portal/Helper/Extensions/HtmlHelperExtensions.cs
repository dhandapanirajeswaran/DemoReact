using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using JsPlc.Ssc.PetrolPricing.Core.StringFormatters;

namespace JsPlc.Ssc.PetrolPricing.Portal.Helper.Extensions
{
    public static class HtmlHelperExtensions
    {
        public static string FormatFriendlyDateTime(this HtmlHelper helper,DateTime datetime)
        {
            return DateAndTimeFormatter.FormatFriendlyDateTime(datetime);
        }

        public static string FormatFriendlyTimeAgo(this HtmlHelper helper, DateTime datetime)
        {
            return FormatFriendlyTimeAgo(helper, DateTime.Now.Subtract(datetime));
        }

        public static string FormatFriendlyTimeAgo(this HtmlHelper helper, TimeSpan timeAgo)
        {
            return DateAndTimeFormatter.FormatFriendlyTimeAgo(timeAgo);
        }
    }
}