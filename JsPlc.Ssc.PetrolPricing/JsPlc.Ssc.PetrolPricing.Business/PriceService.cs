using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Data.Entity;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using JsPlc.Ssc.PetrolPricing.Models;

namespace JsPlc.Ssc.PetrolPricing.Business
{
    public class PriceService : BaseService
    {
        private readonly bool _includeJsSitesAsCompetitors; // false by default (excludes JS sites)

        public PriceService()
        {
            
        }
        public PriceService(bool includeJsSitesAsCompetitors)
        {
            _includeJsSitesAsCompetitors = includeJsSitesAsCompetitors;
        }

        /// <summary>
        /// Pickup success status files and pick the latest file and run calc with that
        /// </summary>
        /// <param name="forDate"></param>
        public void DoCalcPrices(DateTime? forDate)
        {
            if (!forDate.HasValue) forDate = DateTime.Now;
            // Only ones Uploaded today and successfully processed files
            var processedFiles =  _db.GetFileUploads(forDate, 1, 10).ToList(); // 1 = DailyFile, 10 = Success
            if (processedFiles.Any())
            {
                CalcSitePrices(processedFiles);
            }
        }

        /// <summary>
        /// Calculate prices for files Uploaded today and in a Success state. No retrosprctive calc, No future calc
        /// </summary>
        /// <param name="processedFiles"></param>
        private void CalcSitePrices(IEnumerable<FileUpload> processedFiles)
        {
            var priceService = new PriceService();
            var siteService = new SiteService(_db);
            var forDate = DateTime.Now;

            var sites = _db.GetSites().AsQueryable().AsNoTracking();
            var fuels = LookupService.GetFuelTypes().AsQueryable().AsNoTracking().ToList();

            foreach (var processedFile in processedFiles)
            {
                foreach (var site in sites)
                {
                    var tmpSite = site;
                    foreach (var fuel in fuels.ToList())
                    {
                        var calculatedSitePrice = priceService.CalcPrice(site.Id, fuel.Id);
                    }
                }
            }
        }
        /// <summary>
        /// Calculate price of a Fuel for a Given JS Site based on Pricing Rules
        ///  As per flow diagram 30 Nov 2015
        /// Normally we use Prices for Date on which method is RUN, but we can force it to use any other days DP file (simulation testing)
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="fuelId"></param>
        /// <param name="usingPricesforDate">Optional - If not specified, uses todays date</param>
        /// <returns></returns>
        public SitePrice CalcPrice(int siteId, int fuelId, DateTime? usingPricesforDate = null)
        {
            if (!usingPricesforDate.HasValue) usingPricesforDate = DateTime.Now; // Uses dailyPrices of competitors Upload date matching this date

            var site = _db.GetSite(siteId);
            if (site == null) return null;

            // APPLY PRICING RULES: based on drivetime (see Market Comparison sheet) as per meeting 02Dec2015 @ 13:00
            // If 0-5 mins away – match to minimum competitor
            // If 5-10 mins away – add 1p to minimum competitor 
            // If 10-15 mins away – add 2p to minimum competitor
            // If 15-20 mins away – add 3p to the minimum competitor
            // If 20-25 mins away – add 4p to the minimum competitor price

            // 0-5 min
            var cheapestCompetitor = GetCheapestPriceUsingParams(siteId, 0, 5, fuelId,
                usingPricesforDate.Value, 0, _includeJsSitesAsCompetitors);
            // 5-10 min
            if (!cheapestCompetitor.HasValue)
                cheapestCompetitor = GetCheapestPriceUsingParams(siteId, 5, 10, fuelId,
                usingPricesforDate.Value, 1, _includeJsSitesAsCompetitors);
            // 10-15 min
            if (!cheapestCompetitor.HasValue)
                cheapestCompetitor = GetCheapestPriceUsingParams(siteId, 10, 15, fuelId,
                usingPricesforDate.Value, 2, _includeJsSitesAsCompetitors);
            // 15-20 min
            if (!cheapestCompetitor.HasValue)
                cheapestCompetitor = GetCheapestPriceUsingParams(siteId, 15, 20, fuelId,
                usingPricesforDate.Value, 3, _includeJsSitesAsCompetitors);
            // 20-25 min
            if (!cheapestCompetitor.HasValue)
                cheapestCompetitor = GetCheapestPriceUsingParams(siteId, 20, 25, fuelId,
                usingPricesforDate.Value, 4, _includeJsSitesAsCompetitors);

            if (!cheapestCompetitor.HasValue) return null;

            var competitor = cheapestCompetitor.Value.Key;
            var markup = cheapestCompetitor.Value.Value;

            var retval = new SitePrice
            {
                SiteId = siteId,
                JsSite = site,
                FuelTypeId = fuelId,
                DateOfPrice = competitor.DailyPrice.DateOfPrice,
                UploadId = competitor.DailyPrice.DailyUploadId, // If we can provide traceability to calc file, then why not
                SuggestedPrice = competitor.DailyPrice.ModalPrice + markup * 10, // since modalPrice is held in pence*10 (Catalist format)
                DateOfCalc = DateTime.Now.Date // Only date component
            };
            var updatedPrice = _db.AddOrUpdateSitePriceRecord(retval); // This is done in caller.
            return updatedPrice;
        }

        /// <summary>
        /// 1. Find competitors within drivetime criteria
        /// 2. If none found returns null
        /// 3. Else Find 
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="driveTimeFrom"></param>
        /// <param name="driveTimeTo"></param>
        /// <param name="fuelId"></param>
        /// <param name="usingPricesForDate"></param>
        /// <param name="markup"></param>
        /// <param name="includeJsSiteAsComp"></param>
        /// <returns></returns>
        private KeyValuePair<CheapestCompetitor, int>? GetCheapestPriceUsingParams(
            int siteId, int driveTimeFrom, int driveTimeTo, int fuelId, 
            DateTime usingPricesForDate, int markup, bool includeJsSiteAsComp = false)
        {
            var competitorsXtoYmiles = _db.GetCompetitors(siteId, driveTimeFrom, driveTimeTo, includeJsSiteAsComp).ToList(); // Only Non-JS competitors (2nd arg false)
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
                CompetitorWithDriveTime = competitor, 
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