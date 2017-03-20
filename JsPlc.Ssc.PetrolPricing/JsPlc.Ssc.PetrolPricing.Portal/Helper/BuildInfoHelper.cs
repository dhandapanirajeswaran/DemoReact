using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace JsPlc.Ssc.PetrolPricing.Portal.Helper
{
    public static class BuildInfoHelper
    {
        public static DateTime BuildDateTime { get; set; }
        public static string BuildVersion { get; set; }
    }
}