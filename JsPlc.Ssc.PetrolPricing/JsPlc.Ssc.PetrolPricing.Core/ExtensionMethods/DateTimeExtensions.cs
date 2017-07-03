using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JsPlc.Ssc.PetrolPricing.Core.ExtensionMethods
{
    public static class DateTimeExtensions
    {
        public const long DotNetEpochTicks01Jan1900 = 621355968000000000;

        public static long ToDateTimeTicks(this DateTime value)
        {
            return value.Ticks;
        }

        public static long ToJavaScriptTicks(this DateTime value)
        {
            var dotNetTicks = value.Ticks;
            var javascriptTicks = dotNetTicks - DotNetEpochTicks01Jan1900;
            if (javascriptTicks < 0)
                throw new ArgumentException("Javascript DateTime cannot be before 01 Jan 1970");
            return javascriptTicks;
        }
    }
}
