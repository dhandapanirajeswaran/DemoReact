using System;
using System.Collections.Generic;
using System.Linq;
using JsPlc.Ssc.PetrolPricing.Models;

namespace JsPlc.Ssc.PetrolPricing.Business
{
    public class PriceService : BaseService
    {
        // As per flow diagram 30 Nov 2015
        public IEnumerable<SitePrice> CalcPrice(int siteId, DateTime? forDate)
        {
            var sitePrices = new List<SitePrice>();
            var site = _db.GetSite(siteId);

            // 0-2 miles
            var competitorsZeroToTwoMiles = _db.GetCompetitors(siteId, 0, 2, false).ToList(); // Only Non-JS competitors (2nd arg false)
            if (competitorsZeroToTwoMiles.Any())
            {
                //competitorsZeroToTwoMiles.
            }
            return sitePrices;
        }
    }
}