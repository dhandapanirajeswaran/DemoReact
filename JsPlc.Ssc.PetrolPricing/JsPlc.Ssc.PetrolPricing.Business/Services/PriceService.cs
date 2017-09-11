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
using JsPlc.Ssc.PetrolPricing.Core;
using JsPlc.Ssc.PetrolPricing.Core.Interfaces;
using JsPlc.Ssc.PetrolPricing.Models;
using JsPlc.Ssc.PetrolPricing.Models.Common;
using JsPlc.Ssc.PetrolPricing.Models.ViewModels;
using JsPlc.Ssc.PetrolPricing.Repository;
using JsPlc.Ssc.PetrolPricing.Models.Enums;
using System.Text;
using JsPlc.Ssc.PetrolPricing.Core.Diagnostics;
using JsPlc.Ssc.PetrolPricing.Models.ViewModels.SystemSettings;

namespace JsPlc.Ssc.PetrolPricing.Business
{
	public class PriceService : IPriceService
	{
        // ignore Competitors beyond this Drive Time limit
        const int MaximumCompetitorSearchDriveTime = 25;


		protected readonly IPetrolPricingRepository _db;
		protected readonly ILookupService _lookupService;
		protected readonly IFactory _factory;
		protected readonly IAppSettings _appSettings;
        protected readonly ISiteService _siteService;
        protected readonly ISystemSettingsService _systemSettingsService;
        protected readonly ILogger _logger;

		public PriceService(IPetrolPricingRepository db,
			IAppSettings appSettings,
			ILookupService lookupSerivce,
			IFactory factory,
            ISiteService siteService,
            ISystemSettingsService systemSettingsService
            )
		{
			_db = db;
			_lookupService = lookupSerivce;
			_factory = factory;
			_appSettings = appSettings;
            _siteService = siteService;
            _systemSettingsService = systemSettingsService;
		    _logger = new PetrolPricingLogger();
		}

		private readonly int[] _fuelSelectionArray = new[] { 1, 2, 6, }; // superunl, unl, diesel,  

		/// <summary>
		/// 
		/// </summary>
		/// <param name="forDate"></param>
		/// <returns></returns>
		public void DoCalcDailyPrices(DateTime? forDate)
		{
			if (false == forDate.HasValue)
				forDate = DateTime.Now;

			// Pick file with Status Success or CalcFailed & Update status to Calculating
			var calcRunningFile = _db.GetDailyFileWithCalcRunningForDate(forDate.Value);
			if (calcRunningFile != null)
				throw new ApplicationException(
					"Calculation already running, please wait until that completes. UploadId:" + calcRunningFile.Id);

			var dpFile = _db.GetDailyFileAvailableForCalc(forDate.Value);
			if (dpFile == null)
				throw new ApplicationException(
					"No file available for calc, please provide a new Daily Price upload.");

			try
			{
				_db.UpdateImportProcessStatus(11, dpFile); //Calculating 11

				calculatePrices(new PriceCalculationTaskData
				{
					ForDate = forDate.Value,
					FileUpload = dpFile,
                    SystemSettings = _systemSettingsService.GetSystemSettings()
				});

				_db.UpdateImportProcessStatus(10, dpFile); //Success 10
			}
			catch (Exception ex)
			{
                _logger.Error(ex);
				_db.UpdateImportProcessStatus(12, dpFile); //CalcFailed 12
				_db.LogImportError(dpFile, string.Format("Exception: {0}", ex.ToString()), 0);
			}
		}

        public void DoCalcDailyPricesForSite(int siteId, DateTime forDate)
        {
            var db = _factory.Create<IPetrolPricingRepository>(CreationMethod.ServiceLocator, null);
            var site = db.GetSite(siteId);

            var dpFile = _db.GetDailyFileAvailableForCalc(forDate);

            var priceService = new PriceService(db, _appSettings, _lookupService, _factory, _siteService, _systemSettingsService);
            priceService.PriceSiteFuels(db, site, new PriceCalculationTaskData
            {
                ForDate = forDate,
                FileUpload = dpFile
            });
        }

        public void DoCalcDailyPricesForSitesBatch(DateTime forDate, string siteIds)
        {
            var db = _factory.Create<IPetrolPricingRepository>(CreationMethod.ServiceLocator, null);
            var dpFile = _db.GetDailyFileAvailableForCalc(forDate);

            var priceService = new PriceService(db, _appSettings, _lookupService, _factory, _siteService, _systemSettingsService);

            var calcTaskData = new PriceCalculationTaskData
            {
                ForDate = forDate,
                FileUpload = dpFile
            };

            priceService.PriceSiteFuelsBatch(db, siteIds, calcTaskData);
        }

        public void PriceSiteFuels(IPetrolPricingRepository db, Site site, PriceCalculationTaskData calcTaskData)
        {
            var maxGrocerDriveTime = 25;

            _db.ProcessSitePricing(
                site.Id,
                calcTaskData.ForDate,
                calcTaskData.FileUpload.Id,
                maxGrocerDriveTime
                );
        }

        public void PriceSiteFuelsBatch(IPetrolPricingRepository db, string siteIds, PriceCalculationTaskData calcTaskData)
        {
            if (String.IsNullOrEmpty(siteIds))
                return;

            var maxGrocerDriveTime = 25;
                    
            _db.ProcessSitePricingBatch(
                calcTaskData.ForDate,
                calcTaskData.FileUpload.Id,
                maxGrocerDriveTime,
                siteIds
                );
        }

        /// <summary>
        /// Calculate price of a Fuel for a Given JS Site based on Pricing Rules and updates DB
        /// As per flow diagram 30 Nov 2015
        /// Normally we use Prices for Date on which method is RUN, but we can force it to use any other days DP file (simulation testing)
        /// </summary>
        /// <param name="db"></param>
        /// <param name="site"></param>
        /// <param name="fuelId"></param>
        /// <param name="calcTaskData"></param>
        public void CalcPrice(IPetrolPricingRepository db, Site site, int fuelId, PriceCalculationTaskData calcTaskData)
        {
            if (db == null)
                db = _db;

            if (calcTaskData == null)
                throw new ArgumentNullException("calcTaskData can't be null");

            if (site == null)
                throw new ArgumentNullException("site can't be null");

            if (calcTaskData.SystemSettings == null)
                throw new ArgumentException("SystemSettings can't be null");

            var usingPricesforDate = calcTaskData.ForDate; // Uses dailyPrices of competitors Upload date matching this date
            int minPriceFound = int.MaxValue;

            var isSuperUnleaded = fuelId == (int)FuelTypeItem.Super_Unleaded;

            var cheapestPrice = new SitePrice
            {
                SiteId = site.Id,
                FuelTypeId = fuelId,
                DateOfCalc = usingPricesforDate.Date, // Only date component
                DateOfPrice = usingPricesforDate.Date.AddDays(-1),
                SuggestedPrice = 0,
                UploadId = calcTaskData.FileUpload.Id,
                Markup = 0,
                CompetitorId = null
            };

            if (false == site.CatNo.HasValue)
            {
                db.AddOrUpdateSitePriceRecord(cheapestPrice);
                return;
            }

            //if daily price data not found created default record.
            if (false == db.AnyDailyPricesForFuelOnDate(fuelId, usingPricesforDate, calcTaskData.FileUpload.Id)) // TODO chk in caller
            {
                db.AddOrUpdateSitePriceRecord(cheapestPrice);
                return;
            }

            KeyValuePair<CheapestCompetitor, int>? cheapestCompetitor = null;

            //
            // Match Competitor - Price Match Strategy ?
            //
            if (site.TrailPriceCompetitorId.HasValue)
            {
                var foundCompetitorPrices = getCompetitorPriceUsingParams(db, site, fuelId, usingPricesforDate);

                if (foundCompetitorPrices != null)
                {
                    cheapestPrice.IsTrailPrice = true;
                    cheapestCompetitor = new KeyValuePair<CheapestCompetitor, int>(foundCompetitorPrices, 0);
                    if (foundCompetitorPrices.DailyPrice != null)
                    {
                        minPriceFound = foundCompetitorPrices.DailyPrice.ModalPrice;
                    }
                    else
                    {
                        minPriceFound = foundCompetitorPrices.LatestCompPrice.ModalPrice;
                    }
                }
                else
                {
                    cheapestPrice.IsTrailPrice = true;
                    minPriceFound = 0;
                }

                // No cheapest price for Match Competitor - return 0 = N/A
                if (minPriceFound == 0)
                {
                    db.AddOrUpdateSitePriceRecord(cheapestPrice);
                    return;
                }

                cheapestPrice.IsTrailPrice = minPriceFound > 0;
            }

            //when inheritin competitor price, if price wasn't found to this fuel type - find normal suggested price
            if (!cheapestPrice.IsTrailPrice)
            {
                List<KeyValuePair<CheapestCompetitor, int>> allCompetitors = new List<KeyValuePair<CheapestCompetitor, int>>();

                const int DriveTimeMinutesBeyondLastRecord = 30;

                //
                // Get all DriveTimeMarkups from database
                //
                var driveTimeMarkups = _db.GetAllDriveTimeMarkups();

                // narrow down by FuelTypeId
                var driveTimeMarkupsForFuel = driveTimeMarkups.Where(x => x.FuelTypeId == fuelId).OrderBy(x => x.DriveTime).ToArray();

                for (var i = 0; i < driveTimeMarkupsForFuel.Length; i++)
                {
                    var isLastItem = (i + 1) == driveTimeMarkupsForFuel.Length;

                    var driveTimeMarkup = driveTimeMarkupsForFuel[i];

                    float minDriveTime = driveTimeMarkup.DriveTime;
                    float nextDriveTime = (isLastItem ? minDriveTime + DriveTimeMinutesBeyondLastRecord : driveTimeMarkupsForFuel[i + 1].DriveTime);
                    float maxDriveTime = nextDriveTime - 0.0001f; // NOTE: make sure this is more granular than the UI/data accuracy ! (e.g. 9.95)

                    // safety check - ignore if outside the max DriveTime...
                    if (minDriveTime >= MaximumCompetitorSearchDriveTime)
                        continue;

                    if (maxDriveTime >= MaximumCompetitorSearchDriveTime)
                        maxDriveTime = MaximumCompetitorSearchDriveTime; // clip Max limit

                    // reject if drive time range <= 0
                    if (maxDriveTime <= minDriveTime)
                        continue;

                    // include Sainsburys as a Competitor...

                    // NOTE: for Super-Unleaded search for Unleaded then apply a markup amount
                    var competitorFuelTypeId = isSuperUnleaded
                            ? (int)FuelTypeItem.Unleaded
                            : fuelId;

                    var includeJsSiteAsComp = true;
                    var currentCompetitor = getCheapestPriceUsingParams(db, site, minDriveTime, maxDriveTime, competitorFuelTypeId, usingPricesforDate, driveTimeMarkup.Markup, includeJsSiteAsComp);

                    if (currentCompetitor.HasValue)
                    {
                        allCompetitors.Add(currentCompetitor.Value);

                        var bestComp = currentCompetitor.Value.Key;

                        //DiagnosticLog.AddLog("Trace",
                        //    "Found Competitor between " + minDriveTime + " and " + maxDriveTime
                        //    , null
                        //    , String.Format("SiteId: {0}, Fuel: {1}, CompetitorId: {2} - Price Daily: {3} - LatestCompPrice: {4}",
                        //    site.Id,
                        //    fuelId,
                        //    bestComp.CompetitorWithDriveTime.CompetitorId,
                        //    (bestComp.DailyPrice != null ? bestComp.DailyPrice.ModalPrice.ToString() : "??"),
                        //    (bestComp.LatestCompPrice != null ? bestComp.LatestCompPrice.ModalPrice.ToString() : "??")
                        //    )
                        //);

                    }
                }

                //// APPLY PRICING RULES: based on drivetime (see Market Comparison sheet) as per meeting 02Dec2015 @ 13:00
                foreach (var currentCompetitor in allCompetitors)
                {
                    var priceWithMarkup = 0;
                    if (currentCompetitor.Key.DailyPrice != null)
                    {
                        priceWithMarkup = currentCompetitor.Key.DailyPrice.ModalPrice + currentCompetitor.Value * 10;
                    }
                    else
                    {
                        priceWithMarkup = currentCompetitor.Key.LatestCompPrice.ModalPrice + currentCompetitor.Value * 10;
                    }

                    // Add Super-Unleaded markup (when using Unleaded as base price)
                    if (isSuperUnleaded)
                        priceWithMarkup += calcTaskData.SystemSettings.SuperUnleadedMarkupPrice;


                    if (minPriceFound > priceWithMarkup)
                    {
                        cheapestCompetitor = currentCompetitor;
                        minPriceFound = priceWithMarkup;
                    }
                }
            }

            if (cheapestPrice.IsTrailPrice)
            {
                var markup = site.CompetitorPriceOffsetNew;
                cheapestPrice.DateOfPrice = calcTaskData.ForDate;
                cheapestPrice.UploadId = calcTaskData.FileUpload.Id; // If we can provide traceability to calc file, then why not
                cheapestPrice.SuggestedPrice = minPriceFound + (int)markup * 10; // since modalPrice is held in pence*10 (Catalist format)
                cheapestPrice.CompetitorId = site.TrailPriceCompetitorId;
                cheapestPrice.Markup = (int)markup;
            }
            else if (cheapestCompetitor != null)
            {
                var competitor = cheapestCompetitor.Value.Key;
                var markup = site.CompetitorPriceOffset;
                if (minPriceFound == int.MaxValue)
                {
                    minPriceFound = 0;
                    markup = 0;
                }
                cheapestPrice.DateOfPrice = competitor.DailyPrice.DateOfPrice;
                cheapestPrice.UploadId = competitor.DailyPrice.DailyUploadId; // If we can provide traceability to calc file, then why not
                cheapestPrice.SuggestedPrice = minPriceFound + (int)markup * 10; // since modalPrice is held in pence*10 (Catalist format)
                cheapestPrice.CompetitorId = competitor.CompetitorWithDriveTime.CompetitorId;
                cheapestPrice.Markup = (int)markup;
            }

            // get site prices
            //var currentSitePrices = _siteService.GetSitesWithPrices(calcTaskData.ForDate, "", 0, 0, "", site.Id).FirstOrDefault();

            var currentSitePrices = _siteService.GetTodayPricesForCalcPrice(calcTaskData.ForDate, site.Id);

            ApplyGrocerRoundingAndPriceVarianceRules(cheapestPrice, calcTaskData.SystemSettings, calcTaskData.ForDate, site, currentSitePrices, fuelId);

            db.AddOrUpdateSitePriceRecord(cheapestPrice);
        }


        private void ApplyGrocerRoundingAndPriceVarianceRules(SitePrice cheapestSitePrice, SystemSettings systemSettings, DateTime forDate, Site site, SitePriceViewModel currentSitePrices, int fuelTypeId)
        {
            FuelPriceViewModel siteFuelPrice = null;

            if (currentSitePrices != null && currentSitePrices.FuelPrices != null)
            {
                siteFuelPrice = currentSitePrices.FuelPrices.FirstOrDefault(x => x.FuelTypeId == fuelTypeId);
            }

            if (siteFuelPrice == null)
            {
                siteFuelPrice = new FuelPriceViewModel()
                {
                    FuelTypeId = fuelTypeId,
                    AutoPrice = 0,
                    TodayPrice = 0,
                    YestPrice = 0,
                    OverridePrice = 0
                };
            }

            // get Grocer Status for Fuel
            NearbyGrocerStatuses grocerStatus = NearbyGrocerStatuses.None;

            var allGrocerStatuses = _db.GetNearbyGrocerPriceStatusForSites(forDate, site.Id.ToString(), systemSettings.MaxGrocerDriveTimeMinutes);
            if (allGrocerStatuses != null && allGrocerStatuses.Any())
            {
                var firstGrocerStatus = allGrocerStatuses.First();

                switch (fuelTypeId)
                {
                    case (int)FuelTypeItem.Unleaded:
                        grocerStatus = firstGrocerStatus.Unleaded;
                        break;
                    case (int)FuelTypeItem.Diesel:
                        grocerStatus = firstGrocerStatus.Diesel;
                        break;
                    case (int)FuelTypeItem.Super_Unleaded:
                        grocerStatus = firstGrocerStatus.SuperUnleaded;
                        break;
                }
            }

            // get the 'today' price (Override or AutoPrice) from Yesterday
            var todayprice = siteFuelPrice.OverridePrice.HasValue && siteFuelPrice.OverridePrice.Value > 0
                ? siteFuelPrice.OverridePrice.Value
                : siteFuelPrice.TodayPrice.Value;

            // Nearby Grocers, but Incomplete grocer data ?
            if (grocerStatus.HasFlag(NearbyGrocerStatuses.HasNearbyGrocers) && !grocerStatus.HasFlag(NearbyGrocerStatuses.AllGrocersHavePriceData))
            {
                // are we higher than cheapest price ?
                if (todayprice > 0 && cheapestSitePrice.SuggestedPrice > todayprice)
                {
                    // use today's price
                    cheapestSitePrice.SuggestedPrice = todayprice;
                }
            }

            // handle no Suggested Price (AutoPrice)
            if (cheapestSitePrice.SuggestedPrice == 0 && todayprice > 0)
                cheapestSitePrice.SuggestedPrice = todayprice;

            // apply decimal rounding (if any)
            if (systemSettings.DecimalRounding != -1)
            {
                if (cheapestSitePrice.SuggestedPrice > 0)
                    cheapestSitePrice.SuggestedPrice = ((cheapestSitePrice.SuggestedPrice / 10) * 10) + systemSettings.DecimalRounding;
            }

            // is within Price Variance (less or equal to)
            if (todayprice > 0)
            {
                var diff = cheapestSitePrice.SuggestedPrice - todayprice;
                if (Math.Abs(diff) <= systemSettings.PriceChangeVarianceThreshold)
                {
                    // Use today's price
                    cheapestSitePrice.SuggestedPrice = todayprice;
                }
            }
        }

		public async Task<int> SaveOverridePricesAsync(List<SitePrice> pricesToSave)
		{
            // fix any SuggestedPrice = 0 records... otherwise the Override will be ignored by UI
            _db.FixZeroSuggestedSitePricesForDay(DateTime.Now.Date);

			return await _db.SaveOverridePricesAsync(pricesToSave);
		}

        public async Task<StatusViewModel> RecalculateDailyPrices(DateTime when)
        {
            try
            {
                DoCalcDailyPrices(when);
                return new StatusViewModel()
                {
                    SuccessMessage = "Recalculated Daily Prices"
                };
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
                return new StatusViewModel()
                {
                    SuccessMessage = "Unable to Recalculate Daily Prices"
                };
            }
        }

        public void ResumePriceCacheForDay(DateTime day)
        {
            _db.ResumePriceCacheForDay(day);
        }
        public void SuspendPriceCacheForDay(DateTime day)
        {
            _db.SuspendPriceCacheForDay(day);
        }
        public PriceSnapshotViewModel GetPriceSnapshotForDay(DateTime day)
        {
            return _db.GetPriceSnapshotForDay(day);
        }

        public void TriggerDailyPriceRecalculation(DateTime day)
        {
            // tell DB that cache is outdated
            _db.MarkPriceCacheOutdatedForDay(day);
        }

        public IEnumerable<HistoricalPriceViewModel> GetHistoricalPricesForSite(int siteId, DateTime startDate, DateTime endDate)
        {
            return _db.GetHistoricPricesForSite(siteId, startDate, endDate);
        }

        public IEnumerable<PriceFreezeEventViewModel> GetPriceFreezeEvents()
        {
            return _db.GetPriceFreezeEvents();
        }
        public PriceFreezeEventViewModel GetPriceFreezeEvent(int priceFreezeEventId)
        {
            return _db.GetPriceFreezeEvent(priceFreezeEventId);
        }

        public StatusViewModel UpsertPriceFreezeEvent(PriceFreezeEventViewModel model)
        {
            var result = new StatusViewModel();
            try
            {
                result = _db.UpsertPriceFreezeEvent(model);
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
                result.ErrorMessage = "Unable to update Price Freeze Event";
            }
            return result;
        }

        public StatusViewModel DeletePriceFreezeEvent(int priceFreezeEventId)
        {
            var result = new StatusViewModel();
            try
            {
                _db.DeletePriceFreezeEvent(priceFreezeEventId);
                result.SuccessMessage = "Deleted Price Freeze Event";
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
                result.ErrorMessage = "Unable to Delete Price Freeze Event";
            }
            return result;
        }

        public PriceFreezeEventViewModel GetPriceFreezeEventForDate(DateTime date)
        {
            var model = _db.GetPriceFreezeEventForDate(date);
            return model;
        }

        #region Private Methods
        /// <summary>
        /// Demo 22/12/15 new requirement: Create SitePrices for SuperUnleaded with Markup
        /// Creates rows in SitePrice for SuperUnleaded using Unleaded Prices with a markup to SuggestedPrice (OverridePrice for new rows = 0)
        /// </summary>
        /// <param name="forDate"></param>
        /// <param name="markup"></param>
        /// <param name="siteId"></param>
        private void createMissingSuperUnleadedFromUnleaded(DateTime forDate, int? markup = null, int siteId = 0)
		{
			if (!markup.HasValue)
			{
				markup = _appSettings.SuperUnleadedMarkup;
			}
			if (markup == null) 
				markup = 5; // also defaulted in sproc
			
			_db.CreateMissingSuperUnleadedFromUnleaded(forDate, markup.Value, siteId);
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
		private KeyValuePair<CheapestCompetitor, int>? getCheapestPriceUsingParams(
			IPetrolPricingRepository db,
			Site site, float driveTimeFrom, float driveTimeTo, int fuelId,
			DateTime usingPricesForDate, int markup, bool includeJsSiteAsComp = false)
		{
			// Method call
			var competitorsXtoYmiles = db.GetCompetitors(site, driveTimeFrom, driveTimeTo, includeJsSiteAsComp).ToList(); // Only Non-JS competitors (2nd arg false)
			if (!competitorsXtoYmiles.Any()) return null;
			// Method call
			var cheapestCompetitor = getCheapestCompetitor(db, competitorsXtoYmiles, fuelId, usingPricesForDate);

			return (cheapestCompetitor != null)
				? (KeyValuePair<CheapestCompetitor, int>?)
					new KeyValuePair<CheapestCompetitor, int>(cheapestCompetitor, markup)
				: null;
		}

		private CheapestCompetitor getCompetitorPriceUsingParams(IPetrolPricingRepository db, Site site, int fuelId,
			DateTime usingPricesForDate)
		{
			if (site == null)
				throw new ArgumentNullException("site");

			if (site.TrailPriceCompetitorId == null)
				return null;

			// Method call
			var competitor = db.GetCompetitor(site.Id, site.TrailPriceCompetitorId.Value);

			if (competitor == null)
				return null;

			// Method call
			return getCheapestCompetitor(db, new List<SiteToCompetitor> { competitor }, fuelId, usingPricesForDate);
		}

		// Returns the cheapest competitor
		private CheapestCompetitor getCheapestCompetitor(IPetrolPricingRepository db, List<SiteToCompetitor> competitors, int fuelId,
			DateTime usingPricesforDate)
		{

			var competitorCatNos = competitors.Where(x => x.Competitor.CatNo.HasValue)
				.Select(x => x.Competitor.CatNo.Value);

			// Method call
			var pricesForFuelByCompetitors = db.GetDailyPricesForFuelByCompetitors(competitorCatNos, fuelId, usingPricesforDate);

			if (pricesForFuelByCompetitors.Any())
            {
                // Sort asc and pick first (i.e. cheapest)
                var cheapestPrice = pricesForFuelByCompetitors.OrderBy(x => x.ModalPrice).First();
                var competitor = competitors.Where(x => x.Competitor.CatNo.HasValue).First(x => x.Competitor.CatNo == cheapestPrice.CatNo);

                return new CheapestCompetitor
                {
                    CompetitorWithDriveTime = competitor,
                    DailyPrice = cheapestPrice
                };
            }
            else
            {
                var latestCompPrice = db.GetLatestCompetitorPricesForFuel(competitorCatNos, fuelId, usingPricesforDate);
                if (latestCompPrice.Any())
                {
                    // Sort asc and pick first (i.e. cheapest)
                    var cheapestPriceFromCompPrice = latestCompPrice.OrderBy(x => x.ModalPrice).First();
                    var competitor = competitors.Where(x => x.Competitor.CatNo.HasValue).First(x => x.Competitor.CatNo == cheapestPriceFromCompPrice.CatNo);

                    return new CheapestCompetitor
                    {
                        CompetitorWithDriveTime = competitor,
                        LatestCompPrice = cheapestPriceFromCompPrice
                    };
                }
            }

            return null;
		}

		private void calculatePrices(PriceCalculationTaskData calcTaskData)
		{
            const int batchSize = 50;

            try
            {
		        var forDate = calcTaskData.ForDate;

		        var sites = _db.GetJsSites().Where(x => x.IsActive).AsQueryable().AsNoTracking();
                var siteCount = sites.Count();

                var batchIndex = 0;

                while (batchIndex < siteCount)
                {
                    var batchSiteIds = sites.Skip(batchIndex).Take(batchSize).Select(x => x.Id.ToString()).Aggregate((x, y) => x + "," + y);

                    var db = _factory.Create<IPetrolPricingRepository>(CreationMethod.ServiceLocator, null);
                    var priceService = new PriceService(db, _appSettings, _lookupService, _factory, _siteService, _systemSettingsService);
                    priceService.PriceSiteFuelsBatch(db, batchSiteIds, calcTaskData);

                    batchIndex += batchSize;
                }
		    }
		    catch (Exception ce)
		    {
                _logger.Error(ce);
                throw new Exception("Unable to calculate prices");
		    }
		}

		#endregion
	}

	public class PriceCalculationTaskData
	{
		public DateTime ForDate { get; set; }
		public FileUpload FileUpload { get; set; }
        public SystemSettings SystemSettings { get; set; }
    }

}