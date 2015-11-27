using System;

namespace JsPlc.Ssc.PetrolPricing.Models.Common
{
    public static class Functions
    {
        public static string EnsurePathEndsWithSlash(string pathString)
        {
            var pathLastSlash = pathString.StartsWith("\\") ? "\\" : "/";

            return (pathString.EndsWith("/") || pathString.EndsWith("\\")) ? pathString : pathString + pathLastSlash;
        }
        public static DateTime? TryParseDateTime(this string input)
        {
            DateTime outDt;
            return DateTime.TryParse(input, out outDt) ? (DateTime?)outDt : null;
        }
        public static int? TryParseInt(this string input)
        {
            int outVal;
            return Int32.TryParse(input, out outVal) ? (int?)outVal : null;
        }
    }
}
