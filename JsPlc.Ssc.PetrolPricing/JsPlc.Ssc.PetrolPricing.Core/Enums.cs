using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JsPlc.Ssc.PetrolPricing.Core
{
    public enum FileUploadTypes
    {
        None = 0,
        DailyPriceData = 1,
        QuarterlySiteData = 2,
        LatestJsPriceData = 3,
        LatestCompPriceData = 4
    }

    public enum ImportProcessStatuses
    {

        Uploaded = 1,
        Warning = 2,
        Processing = 5,
        Success = 10,
        Calculating = 11,
        CalcFailed = 12,
        Failed = 15,
        ImportAborted = 16,
        CalcAborted = 17
    }

    public enum ExportExcelFileType
    {
        None = 0,
        ExportJSSites = 1,
        ExportAllSites = 2
    }

    public enum ReportExportFileType
    {
        None = 0,
        CompetitorSites,
        CompetitorsPriceRange,
        CompetitorsPriceRangeByCompany,
        NationalAverageReport,
        NationalAverageReport2,
        PriceMovementReport,
        PricePointsReport,
        QuarterlySiteAnalysis
    }
}
