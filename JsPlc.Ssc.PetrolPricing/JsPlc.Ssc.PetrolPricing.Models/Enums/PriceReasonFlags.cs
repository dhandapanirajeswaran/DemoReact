using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JsPlc.Ssc.PetrolPricing.Models.Enums
{
    [Flags]
    public enum PriceReasonFlags
    {
        None = 0,

        // bits 0-3
        CheapestPriceFound = 0x00000001,
        Rounded = 0x00000002,
        InsidePriceVariance = 0x00000004,
        OutsidePriceVariance = 0x00000008,

        // bits 4-7
        TodayPriceSnapBack = 0x00000010,
        HasGrocers = 0x00000020,
        HasIncompleteGrocers = 0x00000040,
        BasedOnUnleaded = 0x00000080,

        // bits 8-11
        MissingSiteCatNo = 0x00000100,
        MissingDailyCatalist = 0x00000200,
        NoMatchCompetitorPrice = 0x00000400,
        NoSuggestedPrice = 0x00000800,

        // bits 12-5
        PriceStuntFreeze = 0x00001000
    }
}
