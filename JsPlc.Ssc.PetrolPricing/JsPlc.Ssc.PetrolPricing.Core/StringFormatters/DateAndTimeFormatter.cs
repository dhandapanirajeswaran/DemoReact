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
            if (totalSeconds < 60)
                return "less than 1 minute";
            if (totalSeconds == 60)
                return "1 minute ago";

            if (timeAgo.TotalHours < 1)
            {
                if (totalSeconds % 60 == 0)
                    return String.Format("{0} minutes ago", (int)timeAgo.TotalMinutes);
                else
                    return String.Format("{0} minutes {1} seconds ago", (int)timeAgo.TotalMinutes, timeAgo.Seconds);
            }

            if (timeAgo.TotalHours < 24)
                return String.Format("{0} hours ago", (int)timeAgo.TotalHours );
            if (wholeYearsAgo < 1)
                return String.Format("{0} days ago", (int)timeAgo.TotalDays);
            return String.Format("{0} years and {1} days ago", wholeYearsAgo, ((int)timeAgo.TotalDays) % 365);
        }
    }
}
