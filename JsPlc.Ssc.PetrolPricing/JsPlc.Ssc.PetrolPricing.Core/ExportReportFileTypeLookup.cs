using System;
using System.Collections.Generic;

namespace JsPlc.Ssc.PetrolPricing.Core
{
    public class ExportReportFileTypeLookup
    {
        private static readonly Dictionary<string, ReportExportFileType> ReportFilenameToFileTypeMap = new Dictionary<string, ReportExportFileType>()
        {
            {"CompetitorSites", ReportExportFileType.CompetitorSites},
            {"CompetitorsPriceRange", ReportExportFileType.CompetitorsPriceRange},
            {"CompetitorsPriceRangeByCompany", ReportExportFileType.CompetitorsPriceRangeByCompany},
            {"NationalAverageReport", ReportExportFileType.NationalAverageReport},
            {"NationalAverageReport2", ReportExportFileType.NationalAverageReport2},
            {"PriceMovementReport", ReportExportFileType.PriceMovementReport},
            {"PricePointsReport", ReportExportFileType.PricePointsReport},
            {"QuarterlySiteAnalysisReport", ReportExportFileType.QuarterlySiteAnalysis },
            {"LastSitePrices" , ReportExportFileType.LastSitePrices},
            {"Compliance", ReportExportFileType.Compliance }
        };

        public ReportExportFileType GetReportType(string fileName)
        {
            if (ReportFilenameToFileTypeMap.ContainsKey(fileName))
            {
                return ReportFilenameToFileTypeMap[fileName];
            }
            else if (fileName.EndsWith("PriceMovementReport"))
            {
                return ReportExportFileType.PriceMovementReport;
            }
            else
            {
                throw new ArgumentException($"Unknown report type for filename: {fileName}");
            }
        }
    }
}
