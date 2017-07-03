using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JsPlc.Ssc.PetrolPricing.Core.ExtensionMethods
{
    public static class StringExtensions
    {
        public static bool IsInteger(this string input)
        {
            int temp = 0;
            return Int32.TryParse(input, out temp);
        }

        public static bool IsDouble(this string input)
        {
            double temp = 0.0;
            return Double.TryParse(input, out temp);
        }

        public static DateTime FromTicks(this string input)
        {
            long ticks = long.Parse(input);
            return new DateTime(ticks);
        }

        public static DateTime FromJavaScriptTicks(this string javascriptString)
        {
            long javascriptTicks = long.Parse(javascriptString);
            long dotNetTicks = javascriptTicks + DateTimeExtensions.DotNetEpochTicks01Jan1900;
            return new DateTime(dotNetTicks);
        }
    }
}
