using System;
using System.Collections.Generic;

namespace JsPlc.Ssc.PetrolPricing.Models.ViewModels
{
    public class ComplianceReportContainerViewModel
    {
        public DateTime? ForDate { get; set; }
        public ComplianceReportViewModel ComplianceReport { get; set; }

        public ComplianceReportContainerViewModel()
        {
            ComplianceReport = new ComplianceReportViewModel();
        }
    }

    public class ComplianceReportViewModel
    {
        public List<ComplianceReportRow> ReportRows { get; set; }

        public ComplianceReportViewModel()
        {
            ReportRows = new List<ComplianceReportRow>();
        }
    }

    /// <summary>
    /// Rows of Sites
    /// </summary>
    public class ComplianceReportRow
    {
        public int SiteId { get; set; }
        public String PfsNo { get; set; }
        public String CatNo { get; set; }
        public String StoreNo { get; set; }
        public String SiteName { get; set; }
        public List<ComplianceReportDataItem> DataItems { get; set; } // 3 fuels - each being {DailyPrice, SitePrice, Diff}

        public ComplianceReportRow()
        {
            DataItems = new List<ComplianceReportDataItem>();
        }
    }

    /// <summary>
    /// Fuel and Catalist and Expected prices..
    /// </summary>
    public class ComplianceReportDataItem
    {
        public bool FoundExpectedPrice { get; set; }
        public bool FoundCatPrice { get; set; }

        public int FuelTypeId { get; set; }
        public string FuelTypeName { get; set; }
        //public DateTime PriceDate { get; set; }
        public int CatPriceValue { get; set; } // ModalPrice value from daily price (render this as value/10 format ###0.0)
        public int ExpectedPriceValue { get; set; } // SitePrice value from site price (render this as value/10 format ###0.0)
        public double Diff { get; set; } // Diff in value (Cat - Expected) e.g. 106.9 - 108.9 = -2.0

        public bool DiffValid { get; set; } // Diff will be invalid if either one value of Price is not found in the respective prices
    }
}
