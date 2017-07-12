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

            if (daysAgo == -1)
                return "Tomorrow at " + formattedTime;
            if (daysAgo < 0)
                return String.Format("In {0} days at {1}", Math.Abs(daysAgo), formattedTime);
            if (daysAgo == 0)
                return "Today at " + formattedTime;
            if (daysAgo == 1)
                return "Yesterday at " + formattedTime;
            return datetime.ToString("dd-MMM-yyyy HH:mm:ss");
        }

        public static string FormatFriendlyTimeAgo(TimeSpan timeDiff)
        {
            var duration = timeDiff.Duration();
            var totalSeconds = duration.TotalSeconds;
            int wholeYearsAgo = (int)duration.TotalDays / 365;

            if (timeDiff != duration)
                return FormatFriendlyTimeInFuture(duration);

            if (totalSeconds < 5)
                return "a few seconds ago";
            if (totalSeconds <= 60)
                return "1 minute ago";

            if (duration.TotalHours < 1)
            {
                if (totalSeconds % 60 == 0)
                    return FormatPlural("{0} minute{1} ago", (int)duration.TotalMinutes);
                else
                    return FormatPlural("{0} minute{1}", (int)duration.TotalMinutes)
                        + FormatPlural(" {0} second{1} ago", duration.Seconds);
            }

            if (duration.TotalHours < 24)
                return FormatPlural("{0} hour{1} ago", (int)duration.TotalHours);
            if (wholeYearsAgo < 1)
                return FormatPlural("{0} day{1} ago", (int)duration.TotalDays);
            return FormatPlural("{0} year{1}", wholeYearsAgo)
                + FormatPlural(" and {0} day{1} ago", ((int)duration.TotalDays) % 365);
        }

        private static string FormatFriendlyTimeInFuture(TimeSpan duration)
        {
            if (duration.TotalSeconds < 5)
                return "in a few seconds";
            if (duration.TotalSeconds < 60)
                return "in a minute";
            if (duration.TotalHours < 1)
                return FormatPlural("in {0} minute{1}", (int)duration.TotalMinutes);
            if (duration.TotalHours < 24)
                return FormatPlural("in {0} hour{1}", (int)duration.TotalHours);
            if (duration.TotalDays < 365)
                return FormatPlural("in {0} day{1}", (int)duration.TotalDays);
            return FormatPlural("in {0} year{1}", (int)duration.TotalDays / 365)
                + FormatPlural(" and {0} day{1}", ((int)duration.TotalDays) % 365);
        }

        private static string FormatPlural(string format, int value)
        {
            var pural = value == 1 ? "" : "s";
            return String.Format(format, value, pural);
        }
    }
}