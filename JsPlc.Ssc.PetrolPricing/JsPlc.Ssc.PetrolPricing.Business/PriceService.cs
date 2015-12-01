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
        private readonly bool _includeJsSitesAsCompetitors = false;

        public PriceService()
        {
            
        }
        public PriceService(bool includeJsSitesAsCompetitors)
        {
            _includeJsSitesAsCompetitors = includeJsSitesAsCompetitors;
        }

        /// <summary>
        /// Calculate price of a Fuel for a Given JS Site based on Pricing Rules
        ///  As per flow diagram 30 Nov 2015
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="fuelId"></param>
        /// <param name="usingPricesforDate"></param>
        /// <returns></returns>
        public SitePrice CalcPrice(int siteId, int fuelId, DateTime? usingPricesforDate = null)
        {
            if (!usingPricesforDate.HasValue) usingPricesforDate = DateTime.Now; // Uses dailyPrices of competitors Upload date matching this date

            var site = _db.GetSite(siteId);
            if (site == null) return null;

            // APPLY PRICING RULES:

            // 0-2 miles
            var cheapestCompetitor = GetCheapestPriceUsingParams(siteId, 0, 2, fuelId,
                usingPricesforDate.Value, 0, _includeJsSitesAsCompetitors);
            // 2-3 miles
            if (!cheapestCompetitor.HasValue)
                cheapestCompetitor = GetCheapestPriceUsingParams(siteId, 2, 3, fuelId,
                usingPricesforDate.Value, 1, _includeJsSitesAsCompetitors);
            // 3-5 miles
            if (!cheapestCompetitor.HasValue)
                cheapestCompetitor = GetCheapestPriceUsingParams(siteId, 3, 5, fuelId,
                usingPricesforDate.Value, 2, _includeJsSitesAsCompetitors);
            // 5-10 miles
            if (!cheapestCompetitor.HasValue)
                cheapestCompetitor = GetCheapestPriceUsingParams(siteId, 5, 10, fuelId,
                usingPricesforDate.Value, 3, _includeJsSitesAsCompetitors);

            if (!cheapestCompetitor.HasValue) return null;

            var competitor = cheapestCompetitor.Value.Key;
            var markup = cheapestCompetitor.Value.Value;
            return new SitePrice
            {
                SiteId = siteId,
                JsSite = site,
                DateOfPrice = competitor.DailyPrice.DateOfPrice,
                SuggestedPrice = competitor.DailyPrice.ModalPrice + markup,
                DateOfCalc = DateTime.Now.Date // Only date component
            };
        }

        /// <summary>
        /// 1. Find competitors within distance criteria
        /// 2. If none found returns null
        /// 3. Else Find 
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="distFrom"></param>
        /// <param name="distTo"></param>
        /// <param name="fuelId"></param>
        /// <param name="usingPricesForDate"></param>
        /// <param name="markup"></param>
        /// <param name="includeJsSiteAsComp"></param>
        /// <returns></returns>
        private KeyValuePair<CheapestCompetitor, int>? GetCheapestPriceUsingParams(
            int siteId, int distFrom, int distTo, int fuelId, 
            DateTime usingPricesForDate, int markup, bool includeJsSiteAsComp = false)
        {
            var competitorsXtoYmiles = _db.GetCompetitors(siteId, distFrom, distTo, includeJsSiteAsComp).ToList(); // Only Non-JS competitors (2nd arg false)
            if (!competitorsXtoYmiles.Any()) return null;

            var cheapestCompetitor = GetCheapestCompetitor(competitorsXtoYmiles, fuelId, usingPricesForDate);

            return (cheapestCompetitor != null)
                ? (KeyValuePair<CheapestCompetitor, int>?)
                    new KeyValuePair<CheapestCompetitor, int>(cheapestCompetitor, markup)
                : null;
        }

        // Returns the cheapest competitor
        private CheapestCompetitor GetCheapestCompetitor(List<SiteToCompetitor> competitors, int fuelId,
            DateTime usingPricesforDate)
        {
            var competitorCatNos = competitors.Where(x => x.Competitor.CatNo.HasValue)
                .Select(x => x.Competitor.CatNo.Value);

            var dailyPricesForFuelByCompetitors = GetDailyPricesForFuelByCompetitors(competitorCatNos, fuelId,
                usingPricesforDate);

            var pricesForFuelByCompetitors = dailyPricesForFuelByCompetitors as IList<DailyPrice> ?? dailyPricesForFuelByCompetitors.ToList();
            if (!pricesForFuelByCompetitors.Any()) return null;

            // Sort asc and pick first (i.e. cheapest)
            var cheapestPrice = pricesForFuelByCompetitors.OrderBy(x => x.ModalPrice).First();
            var competitor = competitors.First(x => x.Competitor.CatNo == cheapestPrice.CatNo);

            return new CheapestCompetitor
            {
                CompetitorWithDistance = competitor, 
                DailyPrice = cheapestPrice
            };
        }

        private IEnumerable<DailyPrice> GetDailyPricesForFuelByCompetitors(IEnumerable<int> competitorCatNos, int fuelId, 
            DateTime usingPricesforDate)
        {
            return _db.GetDailyPricesForFuelByCompetitors(competitorCatNos, fuelId, usingPricesforDate);
        }
    }
}