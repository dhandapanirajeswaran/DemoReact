using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JsPlc.Ssc.PetrolPricing.Models
{
    public class SitePricingSettings
    {
        public double MinUnleadedPrice { get; set; }
        public double MaxUnleadedPrice { get; set; }
        public double MinDieselPrice { get; set; }
        public double MaxDieselPrice { get; set; }
        public double MinSuperUnleadedPrice { get; set; }
        public double MaxSuperUnleadedPrice { get; set; }
        public double MinUnleadedPriceChange { get; set; }
        public double MaxUnleadedPriceChange { get; set; }
        public double MinDieselPriceChange { get; set; }
        public double MaxDieselPriceChange { get; set; }
        public double MinSuperUnleadedPriceChange { get; set; }
        public double MaxSuperUnleadedPriceChange { get; set; }
        public int MaxGrocerDriveTimeMinutes { get; set; }
        public double PriceChangeVarianceThreshold { get; set; }
        public double SuperUnleadedMarkupPrice { get; set; }
        public int DecimalRounding { get; set; }
        public bool EnableSiteEmails { get; set; }
        public string SiteEmailTestAddresses { get; set; }
    }
}
