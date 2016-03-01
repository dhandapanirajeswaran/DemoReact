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
using JsPlc.Ssc.PetrolPricing.Repository;

namespace JsPlc.Ssc.PetrolPricing.Business
{
    public class PriceService : IPriceService
    {
        protected readonly IPetrolPricingRepository _db;
        protected readonly ISettingsService _settingsService;
        protected readonly ILookupService _lookupService;

        public PriceService(IPetrolPricingRepository db,
            ISettingsService settingsService,
            ILookupService lookupSerivce)
        {
            _db = db;
            _settingsService = settingsService;
            _lookupService = lookupSerivce;
        }

        private readonly int[] _fuelSelectionArray = new[] { 1, 2, 6, }; // superunl, unl, diesel,  
        // 5, 7 // not used superdiesel, , lpg

        /// <summary>
        /// FIRE AND FORGET - Pickup latest upload file with success/CalcFailed status and run calc with that
        /// This is purely for use indication. 
        /// The CalcPrice relies on the DailyPrice table not a specific file param.
        /// To protect against multiple runs of Calc simultaneously - 
        ///     Checks if calc is running and Throws exception if already calc is running and tell you the uploadId. 
        /// </summary>
        /// <param name="forDate"></param>
        /// <param name="calcTimeoutMilliSecs">timeout in msecs</param>
        public async Task<bool> DoCalcDailyPricesFireAndForget(DateTime? forDate)
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

                    var taskData = new CalcTaskData { ForDate = forDate.Value, FileUpload = dpFile };

                    // ###########################
                    // LONG Running Task - Fire and Forget
                    // ###########################
                    Task t = new Task(() => DoCalcAsync(taskData));
                    t.Start();

                    Debug.WriteLine("Calculation fired...");
                    Trace.WriteLine("Calculation fired... for fileID:" + dpFile.Id);
                }
                catch (Exception ex)
                {
                    _db.UpdateImportProcessStatus(12, dpFile); //CalcFailed
                    _db.LogImportError(dpFile, string.Format("Exception: {0}", ex.ToString()), 0);
                }
            }
            await Task.FromResult(0);
            return true;
        }

        /// <summary>
        /// Demo 22/12/15 new requirement: Create SitePrices for SuperUnleaded with Markup
        /// Creates rows in SitePrice for SuperUnleaded using Unleaded Prices with a markup to SuggestedPrice (OverridePrice for new rows = 0)
        /// </summary>
        /// <param name="forDate"></param>
        /// <param name="markup"></param>
        /// <param name="siteId"></param>
        public void CreateMissingSuperUnleadedFromUnleaded(DateTime forDate, int? markup = null, int siteId = 0)
        {
            if (!markup.HasValue)
            {
                markup = _settingsService.GetSuperUnleadedMarkup().ToNullable<int>();
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
        public void CalcPrice(IPetrolPricingRepository db, Site site, int fuelId, CalcTaskData calcTaskData)
        {
            if (db == null)
                db = _db;

            // TODO For performance - amend this module to use sproc spGetCompetitorPrices set to calc cheapest prices
            // Call sproc only once for all sites, and passing in comps for site to calc cheapest using that
            // This approach will enable parallel execution

            var usingPricesforDate = calcTaskData.ForDate; // Uses dailyPrices of competitors Upload date matching this date

            var cheapestPrice = new SitePrice
            {
                SiteId = site.Id,
                //JsSite = site,
                FuelTypeId = fuelId,
                DateOfCalc = usingPricesforDate.Date, // Only date component
                DateOfPrice = usingPricesforDate.Date,
                SuggestedPrice = 0,
                UploadId = 0,
                CompetitorId = null
            };

            if (site == null) return;

            if (!site.CatNo.HasValue)
            {
                db.AddOrUpdateSitePriceRecord(cheapestPrice);
                return;
            }

            if (!db.AnyDailyPricesForFuelOnDate(fuelId, usingPricesforDate, calcTaskData.FileUpload.Id)) // TODO chk in caller
            {
                db.AddOrUpdateSitePriceRecord(cheapestPrice);
                return;
            }

            KeyValuePair<CheapestCompetitor, int>? cheapestCompetitor = null;

            if (site.TrailPriceCompetitorId.HasValue)
            {
                var foundCompetitorPrices = GetCompetitorPriceUsingParams(db, site, site.TrailPriceCompetitorId.Value, fuelId, usingPricesforDate);

                if (foundCompetitorPrices != null)
                {
                    cheapestPrice.IsTrailPrice = true;
                    cheapestCompetitor = new KeyValuePair<CheapestCompetitor, int>(foundCompetitorPrices, 0);
                }
            }

            //when inheritin competitor price, if price wasn't found to this fuel type - find normal suggested price
            if (cheapestCompetitor == null)
            {
                List<KeyValuePair<CheapestCompetitor, int>> allCompetitors = new List<KeyValuePair<CheapestCompetitor, int>>();

                // APPLY PRICING RULES: based on drivetime (see Market Comparison sheet) as per meeting 02Dec2015 @ 13:00
                // 0-4.99 min
                // 5-9.99 mins away – add 1p to minimum competitor 
                // 10-14.99 mins away – add 2p to minimum competitor 
                // 15-19.99 mins away – add 3p to minimum competitor 
                // 20-24.99 mins away – add 4p to minimum competitor 
                // 25-29.99 mins away – add 5p to minimum competitor 

                for (float f = 0; f < 6; f++)
                {
                    float nextMin = f * 5;
                    var currentCompetitor = GetCheapestPriceUsingParams(db, site, nextMin, nextMin + 4.99f, fuelId,
                    usingPricesforDate, (int)f);

                    if (currentCompetitor.HasValue)
                        allCompetitors.Add(currentCompetitor.Value);
                }
                int minPriceFound = int.MaxValue;

                foreach (var currentCompetitor in allCompetitors)
                {
                    var priceWithMarkup = currentCompetitor.Key.DailyPrice.ModalPrice + currentCompetitor.Value * 10;

                    if (minPriceFound > priceWithMarkup)
                    {
                        cheapestCompetitor = currentCompetitor;
                        minPriceFound = priceWithMarkup;
                    }
                }

            }

            if (!cheapestCompetitor.HasValue)
            {
                db.AddOrUpdateSitePriceRecord(cheapestPrice);

                return;
            }

            var competitor = cheapestCompetitor.Value.Key;
            var markup = cheapestCompetitor.Value.Value;

            cheapestPrice.DateOfPrice = competitor.DailyPrice.DateOfPrice;
            cheapestPrice.UploadId = competitor.DailyPrice.DailyUploadId; // If we can provide traceability to calc file, then why not
            cheapestPrice.SuggestedPrice = competitor.DailyPrice.ModalPrice + markup * 10; // since modalPrice is held in pence*10 (Catalist format)
            cheapestPrice.CompetitorId = competitor.CompetitorWithDriveTime.CompetitorId;
            cheapestPrice.Markup = markup;
            // TODO for parallel exec - amend this to run in caller
            // Method call
            db.AddOrUpdateSitePriceRecord(cheapestPrice);


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
                        OverriddenPrice = Convert.ToInt32(Math.Truncate(overridePrice * 10))
                    });
            }
            retval = await _db.SaveOverridePricesAsync(prices);
            return retval;
        }

        #region Private Methods
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
            IPetrolPricingRepository db,
            Site site, float driveTimeFrom, float driveTimeTo, int fuelId,
            DateTime usingPricesForDate, int markup, bool includeJsSiteAsComp = false)
        {
            // Method call
            var competitorsXtoYmiles = db.GetCompetitors(site, driveTimeFrom, driveTimeTo, includeJsSiteAsComp).ToList(); // Only Non-JS competitors (2nd arg false)
            if (!competitorsXtoYmiles.Any()) return null;
            // Method call
            var cheapestCompetitor = GetCheapestCompetitor(db, competitorsXtoYmiles, fuelId, usingPricesForDate);

            return (cheapestCompetitor != null)
                ? (KeyValuePair<CheapestCompetitor, int>?)
                    new KeyValuePair<CheapestCompetitor, int>(cheapestCompetitor, markup)
                : null;
        }

        private CheapestCompetitor GetCompetitorPriceUsingParams(IPetrolPricingRepository db, Site site, int competitorId, int fuelId,
            DateTime usingPricesForDate)
        {
            // Method call
            var competitor = db.GetCompetitor(site.Id, competitorId);

            if (competitor == null)
                return null;

            // Method call
            return GetCheapestCompetitor(db, new List<SiteToCompetitor> { competitor }, fuelId, usingPricesForDate);
        }

        // Returns the cheapest competitor
        private CheapestCompetitor GetCheapestCompetitor(IPetrolPricingRepository db, List<SiteToCompetitor> competitors, int fuelId,
            DateTime usingPricesforDate)
        {

            var competitorCatNos = competitors.Where(x => x.Competitor.CatNo.HasValue)
                .Select(x => x.Competitor.CatNo.Value);

            // Method call
            var dailyPricesForFuelByCompetitors = GetDailyPricesForFuelByCompetitors(db, competitorCatNos, fuelId,
                usingPricesforDate);

            var pricesForFuelByCompetitors = dailyPricesForFuelByCompetitors as IList<DailyPrice> ?? dailyPricesForFuelByCompetitors.ToList();
            if (!pricesForFuelByCompetitors.Any()) return null;

            // Sort asc and pick first (i.e. cheapest)
            var cheapestPrice = pricesForFuelByCompetitors.OrderBy(x => x.ModalPrice).First();
            var competitor = competitors.Where(x => x.Competitor.CatNo.HasValue).First(x => x.Competitor.CatNo == cheapestPrice.CatNo);

            return new CheapestCompetitor
            {
                CompetitorWithDriveTime = competitor,
                DailyPrice = cheapestPrice
            };
        }

        private IEnumerable<DailyPrice> GetDailyPricesForFuelByCompetitors(IPetrolPricingRepository db, IEnumerable<int> competitorCatNos, int fuelId,
            DateTime usingPricesforDate)
        {

            var result = db.GetDailyPricesForFuelByCompetitors(competitorCatNos, fuelId, usingPricesforDate);

            // Method call
            return result;
        }

        // LONG Running Task (also updates status within it)
        private async Task DoCalcAsync(CalcTaskData calcTaskData)
        {
            try
            {
                bool result = await Task.FromResult(CalcAllSitePrices(calcTaskData));
                _db.UpdateImportProcessStatus(result ? 10 : 12, calcTaskData.FileUpload);
                //Success 10 (we intentionally use the same success status since we might wanna kickoff the calc again using same successful staus files)
            }
            catch (Exception ex)
            {
                _db.UpdateImportProcessStatus(12, calcTaskData.FileUpload); //CalcFailed
                _db.LogImportError(calcTaskData.FileUpload, string.Format("Exception: {0}", ex.ToString()), 0);
            }
        }

        /// <summary>
        /// Calculate prices for files Uploaded today and in a Success state. 
        /// No retrospective calc, No future calc
        /// </summary>
        ///// <param name="processedFiles">This param is not required as the DP has only 1 price SET for a given day</param>
        /// <param name="forDate">Optional - use prices of these dates</param>
        private bool CalcAllSitePrices(CalcTaskData calcTaskData)
        {

            var forDate = calcTaskData.ForDate;

            var sites = _db.GetJsSites().Where(x => x.IsActive).AsQueryable().AsNoTracking();
            var fuels = _lookupService.GetFuelTypes().Where(x => _fuelSelectionArray.Contains(x.Id)).AsQueryable().AsNoTracking().ToList(); // Limit calc iterations to known fuels
            var taskArray = new List<Task>();

            Parallel.ForEach(sites, (site) =>
            {
                using (var context = new RepositoryContext())
                {
                    var db = new PetrolPricingRepository(context);

                    foreach (var fuel in fuels.ToList())
                    {
                        var priceService = new PriceService(db, _settingsService, _lookupService);

                        priceService.CalcPrice(db, site, fuel.Id, calcTaskData);
                    }
                }

            });

            CreateMissingSuperUnleadedFromUnleaded(forDate); // for performance, run for all sites

            return true;
        }

        #endregion
    }

    public class CalcTaskData
    {
        public DateTime ForDate { get; set; }
        public FileUpload FileUpload { get; set; }
    }

}