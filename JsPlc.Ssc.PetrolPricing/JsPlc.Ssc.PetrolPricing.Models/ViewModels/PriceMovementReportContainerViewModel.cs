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
            Brand = "SAINSBURYS";
            FromDate = DateTime.Today.AddDays(-30);
            ToDate = DateTime.Today;
            FuelTypeId = 2;
            ReportWidth = 0;
        }

        public string Brand { get; set; }

        public string SiteName { get; set; }

        public Dictionary<int, string> FuelTypes { get; set; }

        public List<string> Brands { get; set; }

        public int ReportWidth { get; set; }
    }

    public class PriceMovementReportViewModel
    {
        public List<DateTime> Dates { get; set; }
        public List<PriceMovementReportRows> ReportRows { get; set; }

        public PriceMovementReportViewModel()
        {
            Dates = new List<DateTime>();
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
        public List<PriceMovementFuelPriceItem> FuelPrices { get; set; }

        public PriceMovementReportDataItems()
        {
            this.FuelPrices = new List<PriceMovementFuelPriceItem>();
        }
    }

    public class PriceMovementFuelPriceItem
    {
        public int FuelTypeId { get; set; }
        public int PriceValue { get; set; } 
    }
}
