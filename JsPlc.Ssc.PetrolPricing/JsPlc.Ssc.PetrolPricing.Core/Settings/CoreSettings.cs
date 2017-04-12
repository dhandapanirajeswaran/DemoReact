using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JsPlc.Ssc.PetrolPricing.Core.Settings
{
    public static class CoreSettings
    {
        public static class RepositorySettings
        {
            public static class Dapper
            {
                public static bool LogDapperCalls = false;
            }

            public static class SitePrices
            {
                public static bool UseStoredProcedure = true;
                public static bool ShouldCompareWithOldCode = false;

                public static string CompareOutputFilename = @"C:\tempfile\logs\fuel_prices_old_vs_new.txt";
            }

            public static class CompetitorPrices
            {
                public static bool UseStoredProcedure = true;
                public static bool ShouldCompareWithOldCode = false;

                public static string CompareOutputFilename = @"C:\tempfile\logs\competitor_prices_old_vs_new.txt";
            }
        }
    }
}
