using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JsPlc.Ssc.PetrolPricing.Models.Enums
{
    // NOTE: This matches the database table [dbo].[UploadType]
    public enum FileUploadType
    {
        None = 0,
        DailyPriceData = 1,
        QuarterlySiteData = 2,
        LatestJsPriceData = 3,
        LatestCompetitorsPriceData = 4,
        JsPriceOverrideData = 5
    }
}
