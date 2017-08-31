using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using JsPlc.Ssc.PetrolPricing.Core.StringFormatters;
using Newtonsoft.Json;

namespace JsPlc.Ssc.PetrolPricing.Portal.Helper.Extensions
{
    public static class HtmlHelperExtensions
    {
        public static string FormatFriendlyDateTime(this HtmlHelper helper, DateTime datetime)
        {
            return DateAndTimeFormatter.FormatFriendlyDateTime(datetime);
        }

        public static string FormatFriendlyDateTime(this HtmlHelper helper, DateTime? datetime)
        {
            return datetime.HasValue
                ? DateAndTimeFormatter.FormatFriendlyDateTime(datetime.Value)
                : "";
        }

        public static string FormatFriendlyTimeAgo(this HtmlHelper helper, DateTime? datetime)
        {
            if (datetime.HasValue)
                return FormatFriendlyTimeAgo(helper, datetime.Value);
            else
                return "";
        }

        public static string FormatFriendlyTimeAgo(this HtmlHelper helper, DateTime datetime)
        {
            return FormatFriendlyTimeAgo(helper, DateTime.Now.Subtract(datetime));
        }

        public static string FormatFriendlyTimeAgo(this HtmlHelper helper, TimeSpan timeAgo)
        {
            return DateAndTimeFormatter.FormatFriendlyTimeAgo(timeAgo);
        }

        public static IHtmlString ToJson(this HtmlHelper helper, object data)
        {
            var json = Newtonsoft.Json.JsonConvert.SerializeObject(data, Formatting.None);
            return MvcHtmlString.Create(json);
        }

        public static IHtmlString FormatEmailLink(this HtmlHelper helper, string emailAddress)
        {
            if (String.IsNullOrEmpty(emailAddress))
                return MvcHtmlString.Create("unknown");

            var fullname = ExtractFullNameFromEmail(emailAddress);

            var infotip = String.Format("Send an Email to [em]{0}[/em]",
                emailAddress);

            var html = String.Format("<a target=\"_blank\" href=\"mailto:{0}\" data-infotip=\"{1}\">{2}</a>",
                emailAddress,
                infotip,
                fullname
                );

            return MvcHtmlString.Create(html);
        }

        #region private methods

        private static string ExtractFullNameFromEmail(string emailAddress)
        {
            return String.IsNullOrEmpty(emailAddress)
                ? ""
                : emailAddress.Split('@')[0].Replace('.', ' ');
        }

        #endregion private methods
    }
}