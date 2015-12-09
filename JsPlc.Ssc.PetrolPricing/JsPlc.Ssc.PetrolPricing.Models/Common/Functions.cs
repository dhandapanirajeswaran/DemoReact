using System;
using System.ComponentModel;

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

        /// <summary>
        /// Attempts to convert a string to a double or Int value, failing which it converts it to the value of defaultValue of the type
        /// </summary>
        /// <param name="s"></param>
        /// <returns>Either the successfully converted value or the defaultValue for type (if the cast fails)</returns>
        public static T? ToNullable<T>(this string s) where T : struct
        {
            var result = new T?();
            try
            {
                if (!string.IsNullOrEmpty(s) && s.Trim().Length > 0)
                {
                    var conv = TypeDescriptor.GetConverter(typeof (T));
                    var convertFrom = conv.ConvertFrom(s);
                    if (convertFrom != null) result = (T) convertFrom;
                    else result = default(T);
                }
            }
            catch
            {
                return default(T); }
            return result;
        }

    }
}
