using JsPlc.Ssc.PetrolPricing.Models.Enums;
using System;

namespace JsPlc.Ssc.PetrolPricing.Business
{
	public class EmailSiteData
	{
		public string SiteName { get; set; }
		public string EmailBody { get; set; }
		public DateTime ChangeDate { get; set; }
        public decimal PriceUnleaded { get; set; } = 0;
        public decimal PriceSuper { get; set; } = 0;
        public decimal PriceDiesel { get; set; } = 0;
        public bool AtLeastOnePriceAvailable { get; set; } = false;
	}

    public class EmailSiteFuelPriceChange
    {
        public FuelTypeItem FuelType { get; set; }
        public decimal UpdatedPrice { get; set; }
    }
}
