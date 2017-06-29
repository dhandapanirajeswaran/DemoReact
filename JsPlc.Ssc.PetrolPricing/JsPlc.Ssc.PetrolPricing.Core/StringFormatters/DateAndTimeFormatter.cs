using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JsPlc.Ssc.PetrolPricing.Core.StringFormatters
{
    public static class DateAndTimeFormatter
    {
        public static string FormatFriendlyDateTime(DateTime datetime)
        {
            var daysAgo = DateTime.Now.Date.Subtract(datetime.Date).TotalDays;
            var formattedTime = datetime.ToString("HH:mm:ss");
            if (daysAgo == 0)
                return "Today at " + formattedTime;
            if (daysAgo == 1)
                return "Yesterday at " + formattedTime;
            return datetime.ToString("dd-MMM-yyyy HH:mm:ss");
        }

        public static string FormatFriendlyTimeAgo(TimeSpan timeAgo)
        {
            var totalSeconds = timeAgo.TotalSeconds;
            int wholeYearsAgo = (int)timeAgo.TotalDays / 365;

            if (totalSeconds < 5)
                return "a few seconds";
            if (totalSeconds <= 60)
                return "1 minute ago";

            if (timeAgo.TotalHours < 1)
            {
                if (totalSeconds % 60 == 0)
                    return FormatPlural("{0} minute{1} ago", (int)timeAgo.TotalMinutes);
                else
                    return FormatPlural("{0} minute{1}", (int)timeAgo.TotalMinutes)
                        + FormatPlural(" {0} second{1} ago", timeAgo.Seconds);
            }

            if (timeAgo.TotalHours < 24)
                return FormatPlural("{0} hour{1} ago", (int)timeAgo.TotalHours);
            if (wholeYearsAgo < 1)
                return FormatPlural("{0} day{1} ago", (int)timeAgo.TotalDays);
            return FormatPlural("{0} year{1}", wholeYearsAgo)
                + FormatPlural(" and {0} day{1} ago", ((int)timeAgo.TotalDays) % 365);
        }

        private static string FormatPlural(string format, int value)
        {
            if (value == 1)
                return String.Format(format, value, "");
            else
                return String.Format(format, value, "s");
        }
    }
}