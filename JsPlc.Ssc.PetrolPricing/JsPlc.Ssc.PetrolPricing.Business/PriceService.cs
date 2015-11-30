using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using JsPlc.Ssc.PetrolPricing.Models;

namespace JsPlc.Ssc.PetrolPricing.Business
{
    public class PriceService : BaseService
    {
        // As per flow diagram 30 Nov 2015
        public IEnumerable<SitePrice> CalcPrice(int siteId, DateTime? usingPricesforDate)
        {
            if (!usingPricesforDate.HasValue) usingPricesforDate = DateTime.Now; // Uses dailyPrices of competitors Upload date matching this date

            var sitePrices = new List<SitePrice>();
            var site = _db.GetSite(siteId);

            // 0-2 miles
            List<SiteToCompetitor> competitorsZeroToTwoMiles = _db.GetCompetitors(siteId, 0, 2, false).ToList(); // Only Non-JS competitors (2nd arg false)
            if (competitorsZeroToTwoMiles.Any())
            {
                //List<int, int> competitorPrices = GetCheapestCompetitor(competitorsZeroToTwoMiles, usingPricesforDate);

                //KeyValuePair<int, int> cheapestPrice = competitorsZeroToTwoMiles.Sort(
                //    (competitor, toCompetitor) => competitor.Competitor.Prices);
                //;
            }

            return sitePrices;
        }

        // Returns a list of Competitor sites, 
        private List<KeyValuePair<Site, DailyPrice>> GetCheapestCompetitor(List<SiteToCompetitor> competitorsZeroToTwoMiles, DateTime? usingPricesforDate)
        {
            return null;
            //var dailyPrices = _db.GetDailyPricesFor
        }

        private class DailyPricesComparisonList
        {
            public Site JSSite { get; set; }
            public Site Competitor { get; set; }
            public SiteToCompetitor CompetitorDistanceInfo { get; set; }
            public DailyPrice DailyPrice { get; set; }
        }

    }
}