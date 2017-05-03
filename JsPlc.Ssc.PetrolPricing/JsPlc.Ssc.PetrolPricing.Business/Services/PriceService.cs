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

namespace JsPlc.Ssc.PetrolPricing.Business
{
	public class PriceService : IPriceService
	{
		protected readonly IPetrolPricingRepository _db;
		protected readonly ILookupService _lookupService;
		protected readonly IFactory _factory;
		protected readonly IAppSettings _appSettings;
        protected readonly ILogger _logger;

		public PriceService(IPetrolPricingRepository db,
			IAppSettings appSettings,
			ILookupService lookupSerivce,
			IFactory factory
            )
		{
			_db = db;
			_lookupService = lookupSerivce;
			_factory = factory;
			_appSettings = appSettings;
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
					FileUpload = dpFile
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
            var fuels =
                   _lookupService.GetFuelTypes()
                       .Where(x => _fuelSelectionArray.Contains(x.Id))
                       .AsQueryable()
                       .AsNoTracking()
                       .ToList(); // Limit calc iterations to known fuels


            var db = _factory.Create<IPetrolPricingRepository>(CreationMethod.ServiceLocator, null);
	        var site = db.GetSite(siteId);

            var dpFile = _db.GetDailyFileAvailableForCalc(forDate);
        
        

            foreach (var fuel in fuels.ToList())
            {
                var priceService = new PriceService(db, _appSettings, _lookupService, _factory);

                priceService.CalcPrice(db, site, fuel.Id, new PriceCalculationTaskData
                {
                    ForDate = forDate,
                    FileUpload = dpFile
                });
            }
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

			var usingPricesforDate = calcTaskData.ForDate; // Uses dailyPrices of competitors Upload date matching this date
            int minPriceFound = int.MaxValue;
         
			var cheapestPrice = new SitePrice
			{
				SiteId = site.Id,
				FuelTypeId = fuelId,
				DateOfCalc = usingPricesforDate.Date, // Only date component
				DateOfPrice = usingPricesforDate.Date.AddDays(-1),
				SuggestedPrice = 0,
                UploadId = calcTaskData.FileUpload.Id,
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

			if (site.TrailPriceCompetitorId.HasValue)
			{
				var foundCompetitorPrices = getCompetitorPriceUsingParams(db, site, fuelId, usingPricesforDate);

				if (foundCompetitorPrices != null)
				{
					cheapestPrice.IsTrailPrice = true;
					cheapestCompetitor = new KeyValuePair<CheapestCompetitor, int>(foundCompetitorPrices, 0);
                    minPriceFound = foundCompetitorPrices.DailyPrice.ModalPrice;
				}
				else
				{
                    cheapestPrice.IsTrailPrice = true;
                    minPriceFound = 0;
				}
			}
         
			//when inheritin competitor price, if price wasn't found to this fuel type - find normal suggested price
            if (!cheapestPrice.IsTrailPrice)
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
					var currentCompetitor = getCheapestPriceUsingParams(db, site, nextMin, nextMin + 4.99f, fuelId,
					usingPricesforDate, (int)f);
                    
                    
					if (currentCompetitor.HasValue)
						allCompetitors.Add(currentCompetitor.Value);
				}
				
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

			


		    if (cheapestPrice.IsTrailPrice)
		    {
                var markup = site.CompetitorPriceOffsetNew;
                cheapestPrice.DateOfPrice = calcTaskData.ForDate;
                cheapestPrice.UploadId = calcTaskData.FileUpload.Id; // If we can provide traceability to calc file, then why not
                cheapestPrice.SuggestedPrice = minPriceFound + (int)markup * 10; // since modalPrice is held in pence*10 (Catalist format)
		        cheapestPrice.CompetitorId = site.TrailPriceCompetitorId;
                cheapestPrice.Markup = (int)markup;
		    }
		    else
		    {
                if (cheapestCompetitor == null)
                {
                    db.AddOrUpdateSitePriceRecord(cheapestPrice);
                    return;
                }
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
			

			db.AddOrUpdateSitePriceRecord(cheapestPrice);
		}

		public async Task<int> SaveOverridePricesAsync(List<SitePrice> pricesToSave)
		{
			return await _db.SaveOverridePricesAsync(pricesToSave);
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

			if (!pricesForFuelByCompetitors.Any())
				return null;

			// Sort asc and pick first (i.e. cheapest)
			var cheapestPrice = pricesForFuelByCompetitors.OrderBy(x => x.ModalPrice).First();
			var competitor = competitors.Where(x => x.Competitor.CatNo.HasValue).First(x => x.Competitor.CatNo == cheapestPrice.CatNo);

			return new CheapestCompetitor
			{
				CompetitorWithDriveTime = competitor,
				DailyPrice = cheapestPrice
			};
		}

		private void calculatePrices(PriceCalculationTaskData calcTaskData)
		{
		    try
		    {
		        var forDate = calcTaskData.ForDate;

		        var sites = _db.GetJsSites().Where(x => x.IsActive).AsQueryable().AsNoTracking();

		        var fuels =
		            _lookupService.GetFuelTypes()
		                .Where(x => _fuelSelectionArray.Contains(x.Id))
		                .AsQueryable()
		                .AsNoTracking()
		                .ToList(); // Limit calc iterations to known fuels

		       /* Parallel.ForEach(sites, (site) =>
		        {
		            var db = _factory.Create<IPetrolPricingRepository>(CreationMethod.ServiceLocator, null);

		            foreach (var fuel in fuels.ToList())
		            {
		                var priceService = new PriceService(db, _appSettings, _lookupService, _factory);

		                priceService.CalcPrice(db, site, fuel.Id, calcTaskData);
		            }
		        });*/

                foreach (var site in sites)
		        {
                    var db = _factory.Create<IPetrolPricingRepository>(CreationMethod.ServiceLocator, null);

                    foreach (var fuel in fuels.ToList())
                    {
                        var priceService = new PriceService(db, _appSettings, _lookupService, _factory);

                        priceService.CalcPrice(db, site, fuel.Id, calcTaskData);
                    }
		            
		        }

		        createMissingSuperUnleadedFromUnleaded(forDate); // for performance, run for all sites
		    }
		    catch (Exception ce)
		    {
                _logger.Error(ce);
		        int j = 0;
		    }
		}

		#endregion
	}

	public class PriceCalculationTaskData
	{
		public DateTime ForDate { get; set; }
		public FileUpload FileUpload { get; set; }
	}

}