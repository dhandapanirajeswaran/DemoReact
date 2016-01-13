using System;
using System.Collections.Generic;

namespace JsPlc.Ssc.PetrolPricing.Models.ViewModels
{
    public class PriceMovementReportContainerViewModel
    {
        public int FuelTypeId { get; set; }
        public String FuelTypeName { get; set; }

        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public PriceMovementReportViewModel PriceMovementReport { get; set; }

        public PriceMovementReportContainerViewModel()
        {
            PriceMovementReport = new PriceMovementReportViewModel();
        }
    }

    public class PriceMovementReportViewModel
    {
        public List<PriceMovementReportRows> ReportRows { get; set; }

        public PriceMovementReportViewModel()
        {
            ReportRows = new List<PriceMovementReportRows>();
        }
    }

    /// <summary>
    /// Rows of Sites
    /// </summary>
    public class PriceMovementReportRows
    {
        public int SiteId { get; set; }
        public String SiteName { get; set; }
        public List<PriceMovementReportDataItems> DataItems { get; set; }

        public PriceMovementReportRows()
        {
            DataItems = new List<PriceMovementReportDataItems>();
        }
    }

    /// <summary>
    /// Date and Fuel items per site
    /// </summary>
    public class PriceMovementReportDataItems
    {
        public DateTime PriceDate { get; set; }
        public int PriceValue { get; set; }
    }
}
