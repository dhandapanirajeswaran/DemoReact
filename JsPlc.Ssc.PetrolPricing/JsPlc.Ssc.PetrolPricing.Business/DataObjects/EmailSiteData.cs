using System;

namespace JsPlc.Ssc.PetrolPricing.Business
{
	public class EmailSiteData
	{
		public string SiteName { get; set; }
		public string EmailBody { get; set; }
		public DateTime ChangeDate { get; set; }
		public decimal PriceUnleaded { get; set; }
		public decimal PriceSuper { get; set; }
		public decimal PriceDiesel { get; set; }
		public bool AtLeastOnePriceAvailable { get; set; }
	}
}
