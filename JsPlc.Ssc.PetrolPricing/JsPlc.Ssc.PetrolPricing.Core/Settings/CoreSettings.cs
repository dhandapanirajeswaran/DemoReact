using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JsPlc.Ssc.PetrolPricing.Core.Settings
{
    public static class CoreSettings
    {
        public static class Logging
        {
            public static bool LogInformationMessages = false;
            public static bool LogDebugMessages = false;
            public static bool LogTraceMessages = true;
        }

        public static class RepositorySettings
        {
            public static class Dapper
            {
                public static bool LogDapperCalls = false;
            }
        }
    }
}
