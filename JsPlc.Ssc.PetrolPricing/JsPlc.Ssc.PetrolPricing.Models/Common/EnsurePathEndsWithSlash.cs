namespace JsPlc.Ssc.PetrolPricing.Models.Common
{
    public class FileFunctions
    {
        public static string EnsurePathEndsWithSlash(string pathString)
        {
            var pathLastSlash = pathString.StartsWith("\\") ? "\\" : "/";

            return (pathString.EndsWith("/") || pathString.EndsWith("\\")) ? pathString : pathString + pathLastSlash;
        }
    }
}
