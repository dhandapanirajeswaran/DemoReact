using System;
using System.Collections.Generic;

namespace JsPlc.Ssc.PetrolPricing.Models.ViewModels.SelfTest
{
    public class DataSanityCheckSummaryViewModel
    {
        public bool SitesExist { get; set; }
        public bool DailyPriceFileUploadExist { get; set; }
        public bool QuarterlyFileUploadExist { get; set; }
        public bool LatestJSPriceFileUploadExist { get; set; }
        public bool LatestCompPriceFileUploadExist { get; set; }
        public bool SitePricesExist { get; set; }
        public bool SiteCompetitorsExist { get; set; }
        public bool LatestCompPricesExist { get; set; }
        public bool LatestPricesExist { get; set; }
   }
}