using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using JsPlc.Ssc.PetrolPricing.Models;
using JsPlc.Ssc.PetrolPricing.Models.Common;
using JsPlc.Ssc.PetrolPricing.Models.ViewModels;

namespace JsPlc.Ssc.PetrolPricing.Business
{
    public class PriceService : BaseService
    {
        private readonly bool _includeJsSitesAsCompetitors; // false by default (excludes JS sites)
        private int[] _fuelSelectionArray = new[] {1, 2, 5, 6, 7}; // superunl, unl, superdiesel, diesel, lpg

        public PriceService()
        {

        }
        public PriceService(bool includeJsSitesAsCompetitors)
        {
            _includeJsSitesAsCompetitors = includeJsSitesAsCompetitors;
        }

        /// <summary>
        /// FIRE AND FORGET - Pickup latest upload file with success/CalcFailed status and run calc with that
        /// This is purely for use indication. 
        /// The CalcPrice relies on the DailyPrice table not a specific file param.
        /// To protect against multiple runs of Calc simultaneously - 
        ///     Checks if calc is running and Throws exception if already calc is running and tell you the uploadId. 
        /// </summary>
        /// <param name="forDate"></param>
        /// <param name="calcTimeoutMilliSecs">timeout in msecs</param>
        public async Task<bool> DoCalcDailyPricesFireAndForget(DateTime? forDate, int calcTimeoutMilliSecs)
        {
            if (!forDate.HasValue) forDate = DateTime.Now;
            // Only ones Uploaded today and successfully processed files
            //var processedFiles = _db.GetFileUploads(forDate, 1, 10).ToList(); // 1 = DailyFile, 10 = Success
            //if (processedFiles.Any())
            {
                // Pick file with Status Success or CalcFailed & Update status to Calculating
                var dpFile = _db.GetDailyFileAvailableForCalc(forDate.Value);
                var calcRunningFile = _db.GetDailyFileWithCalcRunningForDate(forDate.Value);

                if (calcRunningFile != null)
                    throw new ApplicationException(
                        "Calculation already running, please wait until that completes. UploadId:" + calcRunningFile.Id);
                if (dpFile == null)
                    throw new ApplicationException(
                        "No file available for calc, please provide a new Daily Price upload.");

                try
                {
                    _db.UpdateImportProcessStatus(11, dpFile); //Calculating 6

                    var taskData = new CalcTaskData {ForDate = forDate.Value, FileUpload = dpFile};

                    // ###########################
                    // LONG Running Task - Fire and Forget
                    // ###########################
                    Task t = new Task(() => DoCalcAsync(taskData));
                    t.Start();

                    // Run a waiter for aborting the task after set time (wrong approach.., need a cancellation token instead)
                    Task tWait = new Task(() => t.Wait(calcTimeoutMilliSecs));
                    tWait.Start();

                    Debug.WriteLine("Calculation fired...");
                    Trace.WriteLine("Calculation fired... for fileID:" + dpFile.Id);
                }
                catch (Exception)
                {
                    _db.UpdateImportProcessStatus(12, dpFile); //CalcFailed
                }
            }
            await Task.FromResult(0);
            return true;
        }

        // LONG Running Task (also updates status within it)
        private async Task DoCalcAsync(CalcTaskData calcTaskData)
        {
            try
            {
                bool result = await Task.FromResult(CalcAllSitePrices(calcTaskData.ForDate));
                _db.UpdateImportProcessStatus(result ? 10 : 12, calcTaskData.FileUpload);
                //Success 10 (we intentionally use the same success status since we might wanna kickoff the calc again using same successful staus files)
            }
            catch (Exception)
            {
                _db.UpdateImportProcessStatus(12, calcTaskData.FileUpload); //CalcFailed
            }
        }

        /// <summary>
        /// Calculate prices for files Uploaded today and in a Success state. 
        /// No retrospective calc, No future calc
        /// </summary>
        ///// <param name="processedFiles">This param is not required as the DP has only 1 price SET for a given day</param>
        /// <param name="forDate">Optional - use prices of these dates</param>
        private bool CalcAllSitePrices(DateTime? forDate = null)
        {
            // SIMULATE a long running task (TODO remove delay simulation)
            Task.Delay(1000);

            var priceService = new PriceService();
            if(!forDate.HasValue) forDate = DateTime.Now;

            var sites = _db.GetJsSites().Where(x => x.IsActive).AsQueryable().AsNoTracking();
            var fuels = LookupService.GetFuelTypes().Where(x => _fuelSelectionArray.Contains(x.Id)).AsQueryable().AsNoTracking().ToList(); // Limit calc iterations to known fuels
            var taskArray = new List<Task>();
            foreach (var site in sites)
            {
                var tmpSite = site;
                foreach (var fuel in fuels.ToList())
                {
                    Debug.WriteLine("Calculation started... for site:" + site.Id + " at:" + DateTime.Now.ToString("s"));
                    FuelType fuel1 = fuel;
                    var cheapestPrice = priceService.CalcPrice(tmpSite.Id, fuel1.Id, forDate);
                    //taskArray.Add(t);
                    //var calculatedSitePrice = priceService.CalcPrice(tmpSite.Id, fuel.Id, forDate);
                    // AddOrUpdate doesnt work here, only works within the CalcPrice method, Ughh EF!!
                    //if (calculatedSitePrice != null) { var updatedPrice = _db.AddOrUpdateSitePriceRecord(tmpSite, calculatedSitePrice);} 
                }
            }
            // TODO retry this with await WhenAll()
            //Task.WaitAll(taskArray.ToArray());

            CreateMissingSuperUnleadedFromUnleaded(forDate.Value); // for performance, run for all sites
            return true;
        }

        /// <summary>
        /// Demo 22/12/15 new requirement: Create SitePrices for SuperUnleaded with Markup
        /// Creates rows in SitePrice for SuperUnleaded using Unleaded Prices with a markup to SuggestedPrice (OverridePrice for new rows = 0)
        /// </summary>
        /// <param name="forDate"></param>
        /// <param name="markup"></param>
        /// <param name="siteId"></param>
        public void CreateMissingSuperUnleadedFromUnleaded(DateTime forDate, int? markup = null, int siteId =0)
        {
            if (!markup.HasValue)
            {
                markup = SettingsService.GetSuperUnleadedMarkup().ToNullable<int>();
            }
            if (markup == null) markup = 5; // also defaulted in sproc
            _db.CreateMissingSuperUnleadedFromUnleaded(forDate, markup.Value, siteId);
        }

        /// <summary>
        /// Calculate price of a Fuel for a Given JS Site based on Pricing Rules and updates DB
        ///  As per flow diagram 30 Nov 2015
        /// Normally we use Prices for Date on which method is RUN, but we can force it to use any other days DP file (simulation testing)
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="fuelId"></param>
        /// <param name="usingPricesforDate">Optional - If not specified, uses todays date</param>
        /// <returns></returns>
        public SitePrice CalcPrice(int siteId, int fuelId, DateTime? usingPricesforDate = null)
        {
            // TODO For performance - amend this module to use sproc spGetCompetitorPrices set to calc cheapest prices
            // Call sproc only once for all sites, and passing in comps for site to calc cheapest using that
            // This approach will enable parallel execution

            if (!usingPricesforDate.HasValue) usingPricesforDate = DateTime.Now; // Uses dailyPrices of competitors Upload date matching this date

            var site = _db.GetSite(siteId); // TODO get site in caller
            if (site == null || !site.CatNo.HasValue) return null; // TODO chk in caller
            if (!_db.AnyDailyPricesForFuelOnDate(fuelId, usingPricesforDate.Value)) // TODO chk in caller
                return null;
            // APPLY PRICING RULES: based on drivetime (see Market Comparison sheet) as per meeting 02Dec2015 @ 13:00
            // If 0-5 mins away – match to minimum competitor
            // If 5-10 mins away – add 1p to minimum competitor 
            // If 10-15 mins away – add 2p to minimum competitor
            // If 15-20 mins away – add 3p to the minimum competitor
            // If 20-25 mins away – add 4p to the minimum competitor price

            // Method calls

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

            var cheapestPrice = new SitePrice
            {
                SiteId = siteId,
                //JsSite = site,
                FuelTypeId = fuelId,
                DateOfPrice = competitor.DailyPrice.DateOfPrice,
                UploadId = competitor.DailyPrice.DailyUploadId, // If we can provide traceability to calc file, then why not
                SuggestedPrice = competitor.DailyPrice.ModalPrice + markup * 10, // since modalPrice is held in pence*10 (Catalist format)
                DateOfCalc = usingPricesforDate.Value.Date // Only date component
            };
            // TODO for parallel exec - amend this to run in caller
            // Method call
            var retval = _db.AddOrUpdateSitePriceRecord(cheapestPrice); 
            return cheapestPrice;
        }

        /// <summary>
        /// 1. Find competitors within drivetime criteria
        /// 2. If none found returns null
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="driveTimeFrom"></param>
        /// <param name="driveTimeTo"></param>
        /// <param name="fuelId"></param>
        /// <param name="usingPricesForDate"></param>
        /// <param name="markup"></param>
        /// <param name="includeJsSiteAsComp"></param>
        /// <returns>Pair = Competitor, Markup(input param outputted)</returns>
        private KeyValuePair<CheapestCompetitor, int>? GetCheapestPriceUsingParams(
            int siteId, int driveTimeFrom, int driveTimeTo, int fuelId,
            DateTime usingPricesForDate, int markup, bool includeJsSiteAsComp = false)
        {
            // Method call
            var competitorsXtoYmiles = _db.GetCompetitors(siteId, driveTimeFrom, driveTimeTo, includeJsSiteAsComp).ToList(); // Only Non-JS competitors (2nd arg false)
            if (!competitorsXtoYmiles.Any()) return null;

            // Method call
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

            // Method call
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
            // Method call
            return _db.GetDailyPricesForFuelByCompetitors(competitorCatNos, fuelId, usingPricesforDate);
        }

        public async Task<int> SaveOverridePricesAsync(List<OverridePricePostViewModel> pricesToSave)
        {
            int retval = 0;
            List<SitePrice> prices = new List<SitePrice>();
            foreach (var price in pricesToSave)
            {
                var fuelTypeId = price.FuelTypeId.ToNullable<int>();
                var siteId = price.SiteId.ToNullable<int>();
                var overridePrice = 0.0f;
                if (!float.TryParse(price.OverridePrice, out overridePrice))
                {
                    throw new ApplicationException("Invalid Price:" + price.OverridePrice);
                }
                if (fuelTypeId != null && siteId != null && overridePrice >= 0) // Save 0 prices (no override)
                    prices.Add(new SitePrice
                    {
                        SiteId = siteId.Value,
                        FuelTypeId = fuelTypeId.Value,
                        OverriddenPrice = Convert.ToInt32(Math.Truncate(overridePrice*10))
                    });
            }
            retval = await _db.SaveOverridePricesAsync(prices);
            return retval;
        }
    }

    internal class CalcTaskData
    {
        public DateTime ForDate { get; set; }
        public FileUpload FileUpload { get; set; }
    }

}