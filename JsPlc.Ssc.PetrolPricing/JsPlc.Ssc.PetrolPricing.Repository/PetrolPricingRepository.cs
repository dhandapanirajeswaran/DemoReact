using JsPlc.Ssc.PetrolPricing.Core;
using JsPlc.Ssc.PetrolPricing.Models;
using JsPlc.Ssc.PetrolPricing.Models.Common;
using JsPlc.Ssc.PetrolPricing.Models.Enums;
using JsPlc.Ssc.PetrolPricing.Models.ViewModels;
using MoreLinq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Validation;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using EntityState = System.Data.Entity.EntityState;

namespace JsPlc.Ssc.PetrolPricing.Repository
{
    public class PetrolPricingRepository : IPetrolPricingRepository
    {
        private const string SainsburysCompanyName = "J SAINSBURY PLC";

        private enum ReportTypes { Default, NormalisedMax }

        private readonly RepositoryContext _context;

        private List<string> LstOfBandsToRemoveInNA2;

        public PetrolPricingRepository(RepositoryContext context)
        {
            _context = context;
            AddListOfBandsToRemoveInNA2();
        }

        private void AddListOfBandsToRemoveInNA2()
        {
            LstOfBandsToRemoveInNA2 = new List<string> {  Const.WCF,
                Const.UNBRANDED,
                Const.TORQ,
                Const.TOPAZ,
                Const.THAMES,
                Const.TEXACO,
                Const.STAR,
                Const.SPAR,
                Const.SOLO,
                Const.RIX,
                Const.PRAX,
                Const.POWER,
                Const.PACE,
                Const.OIL4WALES,
                Const.NOW,
                Const.NISA,
                Const.MURCO,
                Const.MAXOL,
                Const.JET,
                Const.IMPERIAL,
                Const.HARVESTENERGY,
                Const.GULF,
                Const.GO,
                Const.GLEANER,
                Const.GB,
                Const.EMO,
                Const.DRAGON,
                Const.COSTCUTTER,
                Const.COOP,
                Const.CARLTON,
                Const.CALLOW,
                Const.BWOC,
                Const.BATA,
                Const.TESCOEXPRESS,
                Const.TESCOEXTRA,
                Const.COOKE,
                Const.HELTOR,
                Const.LOCALFUELS
        
            };

        }

        private static object cachedAppSettingsLock = new Object();

        public IEnumerable<PPUser> GetPPUsers()
        {
            return _context.PPUsers.ToArray();
        }
        public IEnumerable<PPUser> AddPPUser(PPUser ppuser)
        {
            var result = _context.PPUsers.Add(ppuser);
            _context.SaveChanges();

            return this.GetPPUsers(); // return full object back
        }



        public IEnumerable<PPUser> DeletePPUser(PPUser ppuser)
        {

            _context.PPUsers.Remove(ppuser);
            _context.SaveChanges();

            return this.GetPPUsers();

        }

        public IEnumerable<Site> GetJsSites()
        {
            return _context.Sites
                .Where(s => s.IsSainsburysSite)
                .AsNoTracking()
                .OrderBy(q => q.SiteName)
                .ToArray();
            //.OrderBy(q => q.Id).ToArray();
        }

        public IEnumerable<Site> GetSites()
        {
            return _context.Sites
                .Include(s => s.Emails)
                .OrderBy(q => q.Id)
                .AsNoTracking();
        }

        public Dictionary<string, int> GetCompanies()
        {
            return GetSites()
                .Where(s => string.IsNullOrWhiteSpace(s.Company) == false)
                .GroupBy(g => g.Company)
                .Select(s => new { Count = s.Count(), CompanyName = s.Key })
                .OrderByDescending(x => x.Count)
                .ToDictionary(k => k.CompanyName, v => v.Count);
        }

        // Not safe to use without date clause subsetting Prices, else we might get a ton of price data
        public IQueryable<Site> GetSitesIncludePrices(DateTime? forDate = null)
        {
            if (!forDate.HasValue) forDate = DateTime.Now;
            return _context.Sites
                .Include(x => x.Emails)
                .Include(x => x.Prices)
                .Where(x => x.IsActive)
                .Where(x => x.Prices.All(p => p.DateOfPrice.Equals(forDate)));
        }

        private static object cachedCompetitorsLock = new Object();

        public Dictionary<int, Site> GetSitesWithCompetitors()
        {
            Dictionary<int, Site> cachedCompetitors = PetrolPricingRepositoryMemoryCache.CacheObj.Get("GetSitesWithCompetitors") as Dictionary<int, Site>;

            if (cachedCompetitors == null)
            {
                lock (cachedCompetitorsLock)
                {
                    cachedCompetitors = PetrolPricingRepositoryMemoryCache.CacheObj.Get("GetSitesWithCompetitors") as Dictionary<int, Site>;

                    if (cachedCompetitors == null)
                    {
                        cachedCompetitors = _context.Sites
                .Include(x => x.Competitors)
                .Where(x => x.IsActive)
                .OrderBy(q => q.Id).ToDictionary(k => k.Id, v => v);

                        PetrolPricingRepositoryMemoryCache.CacheObj.Add("GetSitesWithCompetitors", cachedCompetitors, PetrolPricingRepositoryMemoryCache.ReportsCacheExpirationPolicy(20));
                    }
                }
            }

            return cachedCompetitors;
        }

        // Useful for emailing
        public IEnumerable<Site> GetSitesWithEmailsAndPrices(DateTime? fromPriceDate = null,
            DateTime? toPriceDate = null)
        {
            if (!fromPriceDate.HasValue) fromPriceDate = DateTime.Now.AddDays(-3);
            if (!toPriceDate.HasValue) toPriceDate = DateTime.Now.AddDays(3);

            var sites = new List<Site>();

            var retval = _context.Sites
                .Include(s => s.Emails)
                .Where(x => x.IsActive && x.IsSainsburysSite)
                .OrderBy(q => q.Id).ToList();

            int daysBetweenFromAndTo =
                Convert.ToInt32((toPriceDate.Value - fromPriceDate.Value).TotalDays);

            var rangedDatePrices = _context.SitePrices.Select(x => new
            {
                DiffFromDays = DbFunctions.DiffDays(fromPriceDate.Value, x.DateOfPrice),
                DiffToDays = DbFunctions.DiffDays(x.DateOfPrice, toPriceDate.Value),
                FromDate = fromPriceDate.Value,
                DateOfCalc = x.DateOfPrice,
                ToDate = toPriceDate.Value,
                Price = x
            }).Where(x => x.DiffFromDays >= 0 && x.DiffToDays <= daysBetweenFromAndTo);

            IQueryable<SitePrice> pricesForAllSitesBetweenDates = from f in rangedDatePrices
                                                                  select f.Price;

            var getPricesForSite = new Func<int, List<SitePrice>>(i =>
                pricesForAllSitesBetweenDates.Where(p => p.SiteId == i).ToList());

            //List<SitePrice> pricesForSite = getPricesForSite(2); // sample
            foreach (var site in retval)
            {
                var emails = new List<string>();
                site.Emails.ForEach(x => emails.Add("ramaraju.vittanala@sainsburys.co.uk"));  //TODO: RAM replace your email with    site.Emails.ForEach(x => emails.Add(x.EmailAddress));  
                _context.Entry(site).State = EntityState.Detached;

                var prices = getPricesForSite(site.Id);
                site.Prices = new List<SitePrice>();
                site.Prices = prices;

                emails.ForEach(x => site.Emails.Add(new SiteEmail
                {
                    EmailAddress = "ramaraju.vittanala@sainsburys.co.uk"   //TODO: RAM replace your email with    site.Emails.ForEach(x => emails.Add(x.EmailAddress));  
                }));

                sites.Add(site);
            }
            return sites;
        }

        public IEnumerable<Site> GetBrandWithDailyPricesAsPrices(string brandName, DateTime? fromPriceDate = null,
            DateTime? toPriceDate = null)
        {
            if (!fromPriceDate.HasValue) fromPriceDate = DateTime.Now.AddDays(-3);
            if (!toPriceDate.HasValue) toPriceDate = DateTime.Now.AddDays(3);

            var sites = new List<Site>();
            var sites_tmp = _context.Sites;
            var retval =brandName=="All"?   sites_tmp.Where(x => x.IsActive) .OrderBy(q => q.SiteName).ToList() : sites_tmp
                .Where(x => x.IsActive && x.Brand == brandName)
                .OrderBy(q => q.SiteName).ToList();

            int daysBetweenFromAndTo =
                Convert.ToInt32((toPriceDate.Value - fromPriceDate.Value).TotalDays);

            var fileUploads = _context.FileUploads
                .Where(x => DbFunctions.TruncateTime(x.UploadDateTime) >= fromPriceDate.Value && DbFunctions.TruncateTime(x.UploadDateTime) <= toPriceDate.Value)
                .Select(x => x.Id).ToArray();

            var rangedDatePrices = _context.DailyPrices.Include(x => x.DailyUpload)
                .Where(x => fileUploads.Contains(x.DailyUploadId.Value)).ToList();

            var getPricesForSite = new Func<int, List<DailyPrice>>(i =>
                rangedDatePrices.Where(p => p.CatNo == i).ToList());



            Parallel.ForEach(retval, site =>
            {
                try
                {
                    if (site.CatNo.HasValue == false)
                        return;

                    site.Prices = new List<SitePrice>();
                    var siteprices = getPricesForSite(site.CatNo.Value);
                    site.Prices = transformToPrices(siteprices, site);

                    sites.Add(site);
                }
                catch (Exception ce)
                {
                    string str = ce.InnerException.Message;
                }
                //_context.Entry(site).State = EntityState.Detached;
            });


            return sites;
        }

        public IEnumerable<SitePriceViewModel> GetSitesWithPrices(DateTime forDate, string storeName = "", int catNo = 0, int storeNo = 0, string storeTown = "", int siteId = 0, int pageNo = 1,
            int pageSize = Constants.PricePageSize)
        {
            Task<IEnumerable<SitePriceViewModel>> task = Task<IEnumerable<SitePriceViewModel>>.Factory.StartNew(() =>
            {

                return CallSitePriceSproc(forDate, storeName, catNo, storeNo, storeTown, siteId, pageNo, pageSize);
                ;

            });
            return task.Result;
        }

        public IEnumerable<SitePriceViewModel> GetCompetitorsWithPrices(DateTime forDate, int siteId = 0, int pageNo = 1,
            int pageSize = Constants.PricePageSize)
        {
            Task<IEnumerable<SitePriceViewModel>> task = Task<IEnumerable<SitePriceViewModel>>.Factory.StartNew(() =>
            {
                var key = "CallCompetitorsWithPriceSproc-" + forDate.ToString() + "-" + Convert.ToString(siteId) + "-" +
                          Convert.ToString(pageNo) + "-" + Convert.ToString(pageSize);
                var cachedCompetitorsWithPrices =
                    PetrolPricingRepositoryMemoryCache.CacheObj.Get(key) as IEnumerable<SitePriceViewModel>;


                if (cachedCompetitorsWithPrices == null)
                {
                    lock (cachedCompetitorsLock)
                    {
                        cachedCompetitorsWithPrices =
                            PetrolPricingRepositoryMemoryCache.CacheObj.Get(key) as IEnumerable<SitePriceViewModel>;

                        if (cachedCompetitorsWithPrices == null)
                        {
                            var cachedCompetitors = CallCompetitorsWithPriceSproc(forDate, siteId, pageNo, pageSize);

                            PetrolPricingRepositoryMemoryCache.CacheObj.Add(key, cachedCompetitors,
                                PetrolPricingRepositoryMemoryCache.ReportsCacheExpirationPolicy(20));
                            return cachedCompetitors;
                        }
                    }
                }


                return cachedCompetitorsWithPrices;
            });

            return task.Result;



        }

        public SitePriceViewModel GetASiteWithPrices(int siteId, DateTime forDate, string storeName)
        {
            Task<IEnumerable<SitePriceViewModel>> task = Task<IEnumerable<SitePriceViewModel>>.Factory.StartNew(() =>
            {
                return CallSitePriceSproc(forDate, string.Empty, 0, 0, string.Empty, siteId, 1, 1);
            });

            var listSitePlusPrice = task.Result;
            //sitePlusPrice = listSitePlusPrice.Where(x => x.SiteId == siteId);
            SitePriceViewModel sitePlusPrice = new SitePriceViewModel();
            foreach (SitePriceViewModel xxx in listSitePlusPrice)
            {
                if (xxx.SiteId == siteId)
                {
                    sitePlusPrice = xxx;
                }
            }

            return sitePlusPrice;
        }

        /// <summary>
        /// Calls spGetSitePrices to get site prices view
        /// </summary>
        /// <param name="forDate">Default to today's date</param>
        /// <param name="siteId">[Optional] Specific siteId, defaults to 0 for all sites</param>
        /// <param name="pageNo">[Optional] Defaults to Page 1, or specify a page no.</param>
        /// <param name="pageSize">[Optional] Defaults to Pagesize as per constant, or specify a page size</param>
        /// <returns>List of SitePriceViewModel (with overridable price as an input price by user)</returns>
        private IEnumerable<SitePriceViewModel> CallSitePriceSproc(
            DateTime forDate, string storeName = "", int catNo = 0, int storeNo = 0, string storeTown = "",
            int siteId = 0, int pageNo = 1,
            int pageSize = Constants.PricePageSize)
        {

            try
            {
                var sainsburysSites = _context.Sites.Where(x => x.IsSainsburysSite == true && x.IsActive == true).OrderBy(x=>x.SiteName).ToList();
                if (!String.IsNullOrEmpty(storeName)) sainsburysSites = sainsburysSites.Where(x => x.SiteName.ToLower().Contains(storeName.ToLower())).ToList();
                if (catNo != 0) sainsburysSites = sainsburysSites.Where(x => x.CatNo == catNo).ToList();
                if (storeNo != 0) sainsburysSites = sainsburysSites.Where(x => x.StoreNo == storeNo).ToList();
                if (siteId != 0) sainsburysSites = sainsburysSites.Where(x => x.Id == siteId).ToList();
                if (!String.IsNullOrEmpty(storeTown)) sainsburysSites = sainsburysSites.Where(x => x.Town.ToLower().Contains(storeTown.ToLower())).ToList();
                SitePriceViewModel sitePriceRow = null;

                var fileUploadedObj = _context.FileUploads.Where(x => x.UploadDateTime.Day == forDate.Day && x.UploadDateTime.Month == forDate.Month && x.UploadDateTime.Year == forDate.Year && x.UploadTypeId==1).SingleOrDefault();
                var dbList = new List<SitePriceViewModel>();
                foreach (Site site in sainsburysSites)
                {
                    sitePriceRow = new SitePriceViewModel();
                    sitePriceRow.SiteId = site.Id;
                    sitePriceRow.CatNo = site.CatNo;
                    sitePriceRow.StoreName = site.SiteName;
                    sitePriceRow.Address = site.Address;
                    sitePriceRow.Town = site.Town;
                    sitePriceRow.PfsNo = site.PfsNo;
                    sitePriceRow.StoreNo = site.StoreNo;
                    sitePriceRow.FuelPrices = new List<FuelPriceViewModel>();

                

                    var TrialPrice = (int) site.CompetitorPriceOffset; 
                    TrialPrice = TrialPrice*10;
                    AddSitePriceRow((int)FuelTypeItem.Diesel,  site, TrialPrice, forDate, fileUploadedObj != null,
                   sitePriceRow.FuelPrices);

                    AddSitePriceRow((int)FuelTypeItem.Unleaded, site, TrialPrice, forDate, fileUploadedObj != null,
                    sitePriceRow.FuelPrices);

                    AddSitePriceRow((int)FuelTypeItem.Super_Unleaded, site, TrialPrice, forDate,  fileUploadedObj != null,
                        sitePriceRow.FuelPrices);
                   /* var siteToCompetitorObjs =
                            transactionContext.SiteToCompetitors.Where(x => x.SiteId == siteID);
                    foreach (var siteToC in siteToCompetitorObjs)
                    {
                        transactionContext.SiteToCompetitors.Remove(siteToC);
                    }*/
                  
                    dbList.Add(sitePriceRow);
                }
               // Apply5PMarkupForSuperUnleadedForNonCompetitorSites(dbList);
             
                return dbList;

            }
            catch (Exception ce)
            {
                return null;
            }
        }

        private void AddSitePriceRow(int fuelType, Site site, int trialPrice, DateTime forDate, bool bIsCatalistFileExits, List<FuelPriceViewModel> list)
        {
            var orgFuelTypeID = fuelType;
            if (fuelType == (int) FuelTypeItem.Super_Unleaded) fuelType = (int) FuelTypeItem.Unleaded;

            var dailypriceData =
                    _context.Set<DailyPrice>()
                        .Where(
                            x =>
                                x.ModalPrice > 0 && x.CatNo == site.CatNo.Value &&
                                (x.FuelTypeId == fuelType))
                        .OrderBy(item => item.Id)
                        .ToList();
            var sitePriceData =
                _context.Set<SitePrice>()
                    .Where(
                        x =>
                          (x.SuggestedPrice > 0 || x.OverriddenPrice > 0) && x.SiteId == site.Id &&
                            (x.FuelTypeId == fuelType))
                    .OrderBy(item => item.Id)
                    .ToList();

        
            var siteData = (from dp in dailypriceData
                           from sp in sitePriceData
                           where dp.FuelTypeId == fuelType && sp.FuelTypeId == fuelType
                           select
                           new
                           {
                               dp.FuelTypeId,
                               sp.SuggestedPrice,
                               dp.ModalPrice,
                               sp.OverriddenPrice,
                               sp.Markup,
                               sp.IsTrailPrice,
                               sp.CompetitorId,
                               sp.DateOfPrice
                           }).ToList();





            if (siteData.Count > 0)
            {
                var autoPrice = orgFuelTypeID == (int) FuelTypeItem.Super_Unleaded
                    ? siteData[0].SuggestedPrice + 50
                    : siteData[0].SuggestedPrice;
                autoPrice += trialPrice;
                autoPrice = (autoPrice/10)*10 + 9;

                var overridePrice = 0;


                var dieselPriceOverride =
                    _context.Set<SitePrice>()
                        .Where(
                            x =>
                                x.OverriddenPrice > 0 && x.SiteId == site.Id && x.FuelTypeId == orgFuelTypeID &&
                                x.DateOfPrice.Day == forDate.Day && x.DateOfPrice.Month == forDate.Month &&
                                x.DateOfPrice.Year == forDate.Year)
                        .OrderBy(item => item.Id).ToList();

                if (dieselPriceOverride != null && dieselPriceOverride.Count > 0)
                {
                    overridePrice = dieselPriceOverride[0].OverriddenPrice;
                    overridePrice += trialPrice;
                    overridePrice = (overridePrice/10)*10 + 9;
                }

                //today Price Calculation
                var todayPriceFromCalculation = GetTodayPrice(fuelType, site, forDate);

                var todayPrice = orgFuelTypeID == (int) FuelTypeItem.Super_Unleaded
                    ? todayPriceFromCalculation + 50
                    : todayPriceFromCalculation;
                todayPrice = (todayPrice/10)*10 + 9;

                var markUp = siteData[0].Markup;
                var isTrailPrice = siteData[0].IsTrailPrice;
                var competitorId = siteData[0].CompetitorId.HasValue ? siteData[0].CompetitorId.Value : 0;
                var competitorName = "Unknown";
                if (competitorId > 0)
                {
                    var competitorSite = GetSite(competitorId);
                    competitorName = string.Format("{0}/{1}", competitorSite.Brand, competitorSite.SiteName);
                }
                list.Add(new FuelPriceViewModel
                {
                    FuelTypeId = orgFuelTypeID,
                    AutoPrice = bIsCatalistFileExits ? autoPrice : 0,
                    OverridePrice = overridePrice,

                    TodayPrice = todayPrice,
                    Markup = markUp,
                    CompetitorName = competitorName,
                    IsTrailPrice = isTrailPrice,
                    CompetitorPriceOffset = site.CompetitorPriceOffset
                    //  IsBasedOnCompetitor = trailPriceCompetitorId.HasValue
                });


            }
            else
            {
                list.Add(new FuelPriceViewModel
                {
                    FuelTypeId = orgFuelTypeID,
                    AutoPrice = 0,
                    OverridePrice = 0,

                    TodayPrice = 0,
                    Markup = 0,
                    CompetitorName = "Unknown",
                    IsTrailPrice = false,
                    CompetitorPriceOffset = site.CompetitorPriceOffset
                    //  IsBasedOnCompetitor = trailPriceCompetitorId.HasValue
                });

            }


        }


        private int GetTodayPrice(int fuelType, Site site, DateTime forDate)
        {
            //Latest Price DateTime
            var latestPrice =
                _context.LatestPrices.Where(x => x.PfsNo == site.PfsNo.Value && x.StoreNo == site.StoreNo.Value && x.FuelTypeId == fuelType).ToList();


            var overridepriceIfAny = _context.Set<SitePrice>()
                    .Where(
                        x =>
                          ( x.OverriddenPrice > 0) && x.SiteId == site.Id &&
                            (x.FuelTypeId == fuelType))
                    .OrderBy(item => item.Id)
                    .ToList();

            var todayPriceSortByDate = _context.Set<DailyPrice>()
                    .Where(
                        x =>
                          (x.ModalPrice > 0 ) && x.CatNo == site.CatNo.Value &&
                            (x.FuelTypeId == fuelType))
                    .OrderBy(item => item.Id)
                    .ToList();
            FileUpload fileUpload = null;
            if (latestPrice.Count > 0)
            {
                var fileUploadList = _context.FileUploads.ToList().Where(x => x.Id == latestPrice[0].UploadId).ToList();
                fileUpload=fileUploadList.Count >0 ? fileUploadList[0]:
                null;
            }

            if (todayPriceSortByDate.Count == 0 && overridepriceIfAny.Count == 0 && latestPrice.Count > 0)
            {
                return latestPrice[0].ModalPrice;
            }
            else if (todayPriceSortByDate.Count == 0 && overridepriceIfAny.Count > 0 && latestPrice.Count == 0)
            {
                return overridepriceIfAny[0].OverriddenPrice;
            }
            else if (todayPriceSortByDate.Count > 0 && overridepriceIfAny.Count == 0 && latestPrice.Count == 0)
            {
                return todayPriceSortByDate[0].ModalPrice;
            }
            else if (todayPriceSortByDate.Count > 0 && overridepriceIfAny.Count > 0 && latestPrice.Count == 0)
            {
                return todayPriceSortByDate[0].DateOfPrice > overridepriceIfAny[0].DateOfPrice ? todayPriceSortByDate[0].ModalPrice : overridepriceIfAny[0].OverriddenPrice;
            }
            else if (todayPriceSortByDate.Count == 0 && overridepriceIfAny.Count > 0 && latestPrice.Count > 0)
            {
                return  fileUpload.UploadDateTime > overridepriceIfAny[0].DateOfPrice ? latestPrice[0].ModalPrice : overridepriceIfAny[0].OverriddenPrice;
            }
            else if (todayPriceSortByDate.Count > 0 && overridepriceIfAny.Count == 0 && latestPrice.Count > 0)
            {
                return  todayPriceSortByDate[0].DateOfPrice > fileUpload.UploadDateTime ? todayPriceSortByDate[0].ModalPrice : latestPrice[0].ModalPrice;
            }
            else if (todayPriceSortByDate.Count > 0 && overridepriceIfAny.Count > 0 && latestPrice.Count > 0)
            {
                if (todayPriceSortByDate[0].DateOfPrice > fileUpload.UploadDateTime)
                {
                    if (todayPriceSortByDate[0].DateOfPrice > overridepriceIfAny[0].DateOfPrice)
                    {
                        return todayPriceSortByDate[0].ModalPrice;
                    }
                    else if (fileUpload.UploadDateTime > overridepriceIfAny[0].DateOfPrice)
                    {
                        return  latestPrice[0].ModalPrice;
                    }
                    else
                    {
                        return  overridepriceIfAny[0].OverriddenPrice;
                    }
                }
                else
                {
                    if (fileUpload.UploadDateTime > overridepriceIfAny[0].DateOfPrice)
                    {
                        return latestPrice[0].ModalPrice;
                    }
                    else
                    {
                        return  overridepriceIfAny[0].OverriddenPrice;
                    }
                }

            }
            return 0;
        }
        /// <summary>
        /// Apply a 5P markup for Super unleaded for non-competing sites
        /// </summary>
        /// <param name="dbList"></param>
        private static void Apply5PMarkupForSuperUnleadedForNonCompetitorSites(List<SitePriceViewModel> dbList)
        {
            foreach (var site in dbList)
            {
                var unleaded = site.FuelPrices.FirstOrDefault(x => x.FuelTypeId == (int)FuelTypeItem.Unleaded);
                //if (unleaded != null && unleaded.IsBasedOnCompetitor == false)
                if (unleaded != null)
                {
                    var superunleaded = site.FuelPrices.FirstOrDefault(x => x.FuelTypeId == (int)FuelTypeItem.Super_Unleaded);
                    //if (superunleaded != null && superunleaded.IsBasedOnCompetitor == false && unleaded.AutoPrice.HasValue && unleaded.AutoPrice.Value > 0)
                    if (superunleaded != null && unleaded.AutoPrice.HasValue && unleaded.AutoPrice.Value > 0 && unleaded.TodayPrice.HasValue && unleaded.TodayPrice.Value > 0)
                    {
                        superunleaded.Markup = 5;
                        superunleaded.AutoPrice = unleaded.AutoPrice.Value + 50;                        
                        superunleaded.AutoPrice = (superunleaded.AutoPrice / 10) * 10 + 9;
                        superunleaded.TodayPrice = unleaded.TodayPrice.Value + 50;
                        superunleaded.TodayPrice = (superunleaded.TodayPrice / 10) * 10 + 9;
                    }
                }
            }
        }

        /// <summary>
        /// Calls [spGetCompetitorPrices] to get competitors prices view
        /// </summary>
        /// <param name="forDate">Default to today's date</param>
        /// <param name="siteId">[Optional] Specific siteId, defaults to 0 for all sites</param>
        /// <param name="pageNo">[Optional] Defaults to Page 1, or specify a page no.</param>
        /// <param name="pageSize">[Optional] Defaults to Pagesize as per constant, or specify a page size</param>
        /// <returns>List of SitePriceViewModel (with overridable price as an input price by user)</returns>
        private IEnumerable<SitePriceViewModel> CallCompetitorsWithPriceSproc(DateTime forDate, int siteId = 0,
                 int pageNo = 1, int pageSize = Constants.PricePageSize)
        {
            try
            {
                var siteToCompetitors = _context.SiteToCompetitors.Where(x => x.SiteId == siteId && x.DriveTime<25 && x.IsExcluded==0).OrderBy(x=>x.DriveTime);


                SitePriceViewModel sitePriceRow = null;

                var dbList = new List<SitePriceViewModel>();

                var listOfbrands = GetExcludeBrands();

                Site site = GetSites().Where(x => x.Id == siteId).FirstOrDefault();

                foreach (var competitor in siteToCompetitors)
                {
                    Site competitorSite = GetSites().Where(x => x.Id == competitor.CompetitorId && x.IsActive==true).FirstOrDefault();
                    if (competitorSite==null) continue;
                    var result = listOfbrands.Contains(competitorSite.Brand);

                    if (result == true) continue;
                    sitePriceRow = new SitePriceViewModel();
                    var loopSiteId = competitor.CompetitorId;
                 

                    sitePriceRow.SiteId = competitor.CompetitorId; // CompetitorId
                    sitePriceRow.JsSiteId = competitor.SiteId;
                    sitePriceRow.CatNo = competitorSite.CatNo;
                    // ToNullable<int> or ToNullable<double>
                    sitePriceRow.StoreName = competitorSite.SiteName;
                    sitePriceRow.Brand = competitorSite.Brand;
                    sitePriceRow.Address = competitorSite.Address;

                    sitePriceRow.DriveTime = competitor.DriveTime;
                    sitePriceRow.Distance = competitor.Distance;
                    // any other fields for UI extract here

                    sitePriceRow.FuelPrices = sitePriceRow.FuelPrices ?? new List<FuelPriceViewModel>();
                    int nOffSet = loopSiteId == site.TrailPriceCompetitorId ? (int)site.CompetitorPriceOffsetNew : 0;
                    AddFuelPricesForCompetitors(sitePriceRow.FuelPrices, (int)FuelTypeItem.Unleaded, forDate, nOffSet, competitorSite);
                    AddFuelPricesForCompetitors(sitePriceRow.FuelPrices, (int)FuelTypeItem.Diesel, forDate, nOffSet, competitorSite);
                    AddFuelPricesForCompetitors(sitePriceRow.FuelPrices, (int)FuelTypeItem.Super_Unleaded, forDate, nOffSet, competitorSite);
                    dbList.Add(sitePriceRow);
                }
                return dbList;
            }
            catch (Exception ce)
            {
                return null;
            }
        }

        private void AddFuelPricesForCompetitors(List<FuelPriceViewModel> competitorFuelList, int FuelType, DateTime forDate, int nOffSet, Site site)
        {
            DateTime Yday = forDate.AddDays(-1);

            var dailyPriceToday =
                _context.DailyPrices.Where(
                    x =>
                        x.DateOfPrice.Year == forDate.Year && x.DateOfPrice.Month == forDate.Month &&
                        x.DateOfPrice.Day == forDate.Day && x.FuelTypeId == FuelType && x.CatNo==site.CatNo).OrderBy(x=>x.Id) .ToList();

            var dailyPriceYday =
               _context.DailyPrices.Where(
                   x =>
                       x.DateOfPrice.Year == Yday.Year && x.DateOfPrice.Month == Yday.Month &&
                       x.DateOfPrice.Day == Yday.Day && x.FuelTypeId == FuelType && x.CatNo == site.CatNo).OrderBy(x => x.Id).ToList();

           
                competitorFuelList.Add(new FuelPriceViewModel
                {
                    FuelTypeId = FuelType,

                    // Today's prices (whatever was calculated yesterday OR last)
                    TodayPrice = dailyPriceToday.Count>0 ?  dailyPriceToday[0].ModalPrice + nOffSet :0,

                    // Today's prices (whatever was calculated yesterday OR last)
                    YestPrice = dailyPriceYday.Count>0? dailyPriceYday[0].ModalPrice + nOffSet:0,

                    //Difference between yesterday and today
                    Difference = dailyPriceToday.Count>0 && dailyPriceYday.Count>0 ? dailyPriceToday[0].ModalPrice - dailyPriceYday[0].ModalPrice:0
                });
           


        }

        private static object cachedGetDailyPricesForFuelByCompetitorsLock = new Object();

        /// <summary>
        /// Gets a list of DailyPrices for the list of Competitors for the specified fuel
        /// </summary>
        /// <param name="competitorCatNos"></param>
        /// <param name="fuelId"></param>
        /// <param name="usingPricesforDate"></param>
        /// <returns></returns>
        public IEnumerable<DailyPrice> GetDailyPricesForFuelByCompetitors(IEnumerable<int> competitorCatNos, int fuelId,
            DateTime usingPricesforDate)
        {

            string cacheKey = usingPricesforDate.Ticks.ToString() ;
            Dictionary<string, DailyPrice> dailyPricesCache = PetrolPricingRepositoryMemoryCache.CacheObj.Get(cacheKey) as Dictionary<string, DailyPrice>;

            if (dailyPricesCache == null)
            {
                lock (cachedGetDailyPricesForFuelByCompetitorsLock)
                {
                    dailyPricesCache = PetrolPricingRepositoryMemoryCache.CacheObj.Get(cacheKey) as Dictionary<string, DailyPrice>;

                    if (dailyPricesCache == null)
                    {
                        // If multiple uploads, needs to be handled here, but we assume one for now.
                        dailyPricesCache = _context.DailyPrices.Include(x => x.DailyUpload)
                            .Where(x => DbFunctions.TruncateTime(x.DateOfPrice) == usingPricesforDate.Date)
                            .ToDictionary(k => string.Format("{0}_{1}", k.FuelTypeId, k.CatNo), v => v);

                        PetrolPricingRepositoryMemoryCache.CacheObj.Add(cacheKey, dailyPricesCache, PetrolPricingRepositoryMemoryCache.ReportsCacheExpirationPolicy(20));
                    }
                }
            }

            List<DailyPrice> result = new List<DailyPrice>();

            foreach (var catNo in competitorCatNos)
            {
                var key = string.Format("{0}_{1}", fuelId, catNo);
                if (dailyPricesCache.ContainsKey(key))
                {
                    result.Add(dailyPricesCache[key]);
                }
            }

            return result;
        }

        public Site GetSite(int id)
        {
            return _context.Sites.Include(s => s.Emails).FirstOrDefault(q => q.Id == id);
        }

        public Site GetSiteByCatNo(int catNo)
        {
            return _context.Sites.FirstOrDefault(q => q.CatNo.HasValue && q.CatNo.Value == catNo);
        }

        public Site NewSite(Site site)
        {
            var result = _context.Sites.Add(site);
            _context.SaveChanges();

            return result; // return full object back
        }

        public bool UpdateSite(Site site)
        {
            // _context.SaveChanges();
            _context.Entry(site).State = EntityState.Modified;

            try
            {
                _context.Sites.Attach(site);
                UpdateSiteEmails(site);
                if (site.Competitors != null) UpdateSiteCompetitors(site);
                _context.Entry(site).State = EntityState.Modified;
                int nReturn = _context.SaveChanges();

                return true;
            }
            catch (Exception ce)
            {
                return false;
            }
        }


        /// <summary>
        /// Demo 22/12/15 new requirement: Create SitePrices for SuperUnleaded with Markup
        /// </summary>
        /// <param name="forDate"></param>
        /// <param name="markup">SuperUnl normally 5ppl dearer than Unl</param>
        /// <param name="siteId">0 to run for all sites, or set specific SiteId param</param>
        /// <returns></returns>
        public async Task<int> CreateMissingSuperUnleadedFromUnleaded(DateTime forDate, int markup, int siteId = 0)
        {
            //@forDate DateTime,

            var siteIdParam = new SqlParameter("@siteId", SqlDbType.Int)
            {
                Value = siteId
            };
            var forDateParam = new SqlParameter("@forDate", SqlDbType.DateTime)
            {
                Value = forDate
            };
            var markupParam = new SqlParameter("@SuperUnleadedMarkup", SqlDbType.Int)
            {
                Value = markup
            };

            // any other params here

            var sqlParams = new List<SqlParameter>
            {
                siteIdParam,
                forDateParam,
                markupParam
            };
            const string spName = "dbo.spSetSuperUnleadedPricesFromUnleaded";
            // Test in SQL:     Exec dbo.spSetSuperUnleadedPricesFromUnleaded '2015-11-30'
            // No output, just successful execution, Exception on failure

            using (var connection = new SqlConnection(_context.Database.Connection.ConnectionString))
            {
                using (var command = new SqlCommand(spName, connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddRange(sqlParams.ToArray());

                    connection.Open();

                    int rowsAffectedTask = await command.ExecuteNonQueryAsync();
                    return rowsAffectedTask;
                }
            }
        }

        public IEnumerable<QuarterlyUploadStaging> GetQuarterlyRecords()
        {
            return _context.QuarterlyUploadStaging.AsNoTracking();
        }

        public bool NewQuarterlyRecords(List<CatalistQuarterly> siteCatalistData, FileUpload fileDetails,
            int startingLineNumber)
        {
            int addingEntryLineNo = startingLineNumber;
          
            using (var newDbContext = new RepositoryContext())
            {
                using (var tx = newDbContext.Database.BeginTransaction())
                {
                    newDbContext.Configuration.AutoDetectChangesEnabled = false;
                    try
                    {
                        var exitingSiteCatalistData=newDbContext.QuarterlyUploadStaging.ToList();

                        siteCatalistData.RemoveAll(
                            x =>
                                exitingSiteCatalistData.Any(y => y.SainsSiteCatNo == (int) x.SainsCatNo) &&
                                exitingSiteCatalistData.Any(y => y.CatNo == (int) x.CatNo));
                        if (siteCatalistData.Count > 0)
                        {
                            foreach (CatalistQuarterly fileRecord in siteCatalistData)
                            {
                                var dbRecord = new QuarterlyUploadStaging();
                                dbRecord.QuarterlyUploadId = fileDetails.Id;

                                dbRecord.SainsSiteCatNo = (int) fileRecord.SainsCatNo;
                                dbRecord.SainsSiteName = fileRecord.SainsSiteName;
                                dbRecord.SainsSiteTown = fileRecord.SainsSiteTown;

                                dbRecord.Rank = (int) fileRecord.Rank;
                                dbRecord.DriveDist = (float) fileRecord.DriveDistanceMiles;
                                dbRecord.DriveTime = (float) fileRecord.DriveTimeMins;
                                dbRecord.CatNo = (int) fileRecord.CatNo;

                                dbRecord.Brand = fileRecord.Brand;
                                dbRecord.SiteName = fileRecord.SiteName;
                                dbRecord.Addr = fileRecord.Address;
                                dbRecord.Suburb = fileRecord.Suburb;

                                dbRecord.Town = fileRecord.Town;
                                dbRecord.PostCode = fileRecord.Postcode;
                                dbRecord.Company = fileRecord.CompanyName;
                                dbRecord.Ownership = fileRecord.Ownership;

                                newDbContext.QuarterlyUploadStaging.Add(dbRecord);
                                addingEntryLineNo += 1;

                            }

                            newDbContext.SaveChanges();
                            tx.Commit();
                        }
                        return true;
                    }
                    catch (DbUpdateException e)
                    {
                        tx.Rollback();

                        foreach (var dbUpdateException in e.Entries)
                        {
                            var dbRecord = dbUpdateException.Entity as QuarterlyUploadStaging ??
                                           new QuarterlyUploadStaging();
                            LogImportError(fileDetails,
                                String.Format("Failed to save Quarterly record:{0},{1},{2},{3},{4},{5},{6},{7},{8},{9}",
                                    dbRecord.SainsSiteName, dbRecord.SainsSiteTown, dbRecord.SainsSiteCatNo,
                                    dbRecord.Rank, dbRecord.DriveDist, dbRecord.DriveTime, dbRecord.CatNo,
                                    dbRecord.Brand, dbRecord.SiteName,
                                    dbRecord.Addr),
                                startingLineNumber);
                            dbUpdateException.State = EntityState.Unchanged;
                        }

                        return false;
                    }
                    catch (DbEntityValidationException dbEx)
                    {
                        tx.Rollback();
                        foreach (var validationErrors in dbEx.EntityValidationErrors)
                        {
                            foreach (var validationError in validationErrors.ValidationErrors)
                            {
                                LogImportError(fileDetails,
                                    "DbEntityValidationException occured:" + validationError.ErrorMessage +
                                    "," + validationError.PropertyName, addingEntryLineNo);

                                Trace.TraceInformation("Property: {0} Error: {1}", validationError.PropertyName,
                                    validationError.ErrorMessage);
                            }
                        }
                        return false;
                    }
                }
            }
        }


        public bool NewLatestPriceRecords(List<LatestPriceDataModel> LatestSiteData, FileUpload fileDetails,
            int startingLineNumber)
        {
            int addingEntryLineNo = startingLineNumber;

            using (var newDbContext = new RepositoryContext())
            {
                using (var tx = newDbContext.Database.BeginTransaction())
                {
                    newDbContext.Configuration.AutoDetectChangesEnabled = false;
                    try
                    {


                        if (LatestSiteData.Count > 0)
                        {
                            foreach (LatestPriceDataModel LatestPriceDataModel in LatestSiteData)
                            {
                              
                                if (!String.IsNullOrEmpty(LatestPriceDataModel.UnleadedPrice))
                                {
                                    AddOrUpdateLatestPrice(newDbContext, LatestPriceDataModel, fileDetails,
                                        (int)FuelTypeItem.Unleaded, (int)Convert.ToDouble(LatestPriceDataModel.UnleadedPrice));
                                  

                                }
                                if (!String.IsNullOrEmpty(LatestPriceDataModel.SuperUnleadedPrice))
                                {
                                  
                                    AddOrUpdateLatestPrice(newDbContext, LatestPriceDataModel, fileDetails,
                                     (int)FuelTypeItem.Super_Unleaded, (int)Convert.ToDouble(LatestPriceDataModel.SuperUnleadedPrice));
                            

                                }
                                if (!String.IsNullOrEmpty(LatestPriceDataModel.DieselPrice))
                                {
                                     AddOrUpdateLatestPrice(newDbContext, LatestPriceDataModel, fileDetails,
                                   (int)FuelTypeItem.Diesel, (int)Convert.ToDouble(LatestPriceDataModel.DieselPrice));
                                }
                            }

                            newDbContext.SaveChanges();
                            tx.Commit();
                        }
                        return true;
                    }
                    catch (DbUpdateException e)
                    {
                        tx.Rollback();

                        foreach (var dbUpdateException in e.Entries)
                        {
                            var dbRecord = dbUpdateException.Entity as LatestPrice ??
                                           new LatestPrice();
                            LogImportError(fileDetails,
                                String.Format("Failed to save LatestPrice record:{0},{1},{2},{3},{4},{5},{6},{7},{8},{9}",
                                    dbRecord.FuelTypeId, dbRecord.PfsNo, dbRecord.StoreNo,
                                    dbRecord.ModalPrice),
                                startingLineNumber);
                            dbUpdateException.State = EntityState.Unchanged;
                        }

                        return false;
                    }
                    catch (DbEntityValidationException dbEx)
                    {
                        tx.Rollback();
                        foreach (var validationErrors in dbEx.EntityValidationErrors)
                        {
                            foreach (var validationError in validationErrors.ValidationErrors)
                            {
                                LogImportError(fileDetails,
                                    "DbEntityValidationException occured:" + validationError.ErrorMessage +
                                    "," + validationError.PropertyName, addingEntryLineNo);

                                Trace.TraceInformation("Property: {0} Error: {1}", validationError.PropertyName,
                                    validationError.ErrorMessage);
                            }
                        }
                        return false;
                    }
                }
            }
        }

        public void AddOrUpdateLatestPrice(RepositoryContext newDbContext,LatestPriceDataModel latestPriceDataModel, FileUpload fileDetails,int fuelTypeId,int fuelPrice)
        {
            LatestPrice dbRecord = null;
            bool iNewRecord = false;
            dbRecord = newDbContext.LatestPrices.Where(
                x =>
                    x.PfsNo == latestPriceDataModel.PfsNo &&
                    x.StoreNo == latestPriceDataModel.StoreNo &&
                    x.FuelTypeId == fuelTypeId).SingleOrDefault();
            if (dbRecord == null)
            {
                dbRecord = new LatestPrice();
                iNewRecord = true;
            }
            dbRecord.UploadId = fileDetails.Id;
            dbRecord.PfsNo = latestPriceDataModel.PfsNo;
            dbRecord.StoreNo = latestPriceDataModel.StoreNo;
            dbRecord.FuelTypeId = (int)FuelTypeItem.Unleaded;
            dbRecord.ModalPrice = fuelPrice * 10;
            if (iNewRecord) newDbContext.LatestPrices.Add(dbRecord);
            else
            {
                newDbContext.LatestPrices.Attach(dbRecord);
                newDbContext.Entry(dbRecord).State = EntityState.Modified;
            }
        }

        public bool NewDailyPrices(List<DailyPrice> dailyPriceList, FileUpload fileDetails, int startingLineNumber)
        {
            int addingEntryLineNo = startingLineNumber;

            using (
                var newDbContext =
                    new RepositoryContext())
            {
                using (var tx = newDbContext.Database.BeginTransaction())
                {
                    newDbContext.Configuration.AutoDetectChangesEnabled = false;
                    try
                    {
                        foreach (DailyPrice dailyPrice in dailyPriceList)
                        {
                            if (dailyPrice.DailyUpload != null)
                                dailyPrice.DailyUploadId = dailyPrice.DailyUpload.Id;
                            dailyPrice.DailyUpload = null;
                            dailyPrice.FuelType = null;

                            newDbContext.DailyPrices.Add(dailyPrice);

                            addingEntryLineNo += 1;
                        }

                        newDbContext.SaveChanges();

                        tx.Commit();

                        return true;
                    }
                    catch (DbUpdateException e)
                    {
                        tx.Rollback();

                        foreach (var dbUpdateException in e.Entries)
                        {
                            var dailyPrice = dbUpdateException.Entity as DailyPrice ?? new DailyPrice();

                            LogImportError(fileDetails, String.Format("Failed to save price:{0},{1},{2},{3},{4}",
                                dailyPrice.CatNo, dailyPrice.FuelTypeId, dailyPrice.AllStarMerchantNo,
                                dailyPrice.DateOfPrice, dailyPrice.ModalPrice)
                                , startingLineNumber);

                            LogImportError(fileDetails, e.Message, startingLineNumber);

                            LogImportError(fileDetails, e.StackTrace, startingLineNumber);

                            if (e.InnerException != null)
                            {
                                LogImportError(fileDetails, e.InnerException, startingLineNumber);
                            }

                            dbUpdateException.State = EntityState.Unchanged;
                        }

                        return false;
                    }
                    catch (DbEntityValidationException dbEx)
                    {
                        tx.Rollback();
                        foreach (var validationErrors in dbEx.EntityValidationErrors)
                        {
                            foreach (var validationError in validationErrors.ValidationErrors)
                            {
                                LogImportError(fileDetails,
                                    "DbEntityValidationException occured:" + validationError.ErrorMessage +
                                    "," + validationError.PropertyName, addingEntryLineNo);

                                LogImportError(fileDetails, dbEx.Message, startingLineNumber);

                                LogImportError(fileDetails, dbEx.StackTrace, startingLineNumber);

                                if (dbEx.InnerException != null)
                                {
                                    LogImportError(fileDetails, dbEx.InnerException, startingLineNumber);
                                }
                            }
                        }

                        return false;
                    }
                }
            }
        }

        private void UpdateSiteEmails(Site site)
        {
            var siteEmailIds = site.Emails.Select(x => x.Id).ToList();

            var siteOrig = GetSite(site.Id);
            if (siteOrig.Emails.Any())
            {
                var deletedEmails = siteOrig.Emails.Where(x => !siteEmailIds.Contains(x.Id)).ToList();
                foreach (var delEmail in deletedEmails)
                {
                    _context.Entry(delEmail).State = EntityState.Deleted;
                }
            }
            var siteEmails = site.Emails.ToList();

            foreach (var email in siteEmails)
            {
                if (email.Id == 0) _context.Entry(email).State = EntityState.Added;
                if (email.Id != 0) _context.Entry(email).State = EntityState.Modified;
            }
        }


        private void UpdateSiteCompetitors(Site site)
        {
            var Competitors = site.Competitors.ToList();

            /*   var siteOrig = GetSite(site.Id);
               if (siteOrig.Competitors.Any())
               {
                   var deletedCompetitors = siteOrig.Competitors.Where(x => !Competitors.Contains(x)).ToList();
                   foreach (var delCompetitor in deletedCompetitors)
                   {
                       _context.Entry(delCompetitor).State = EntityState.Deleted;
                   }
               }
               */

            foreach (var competitor in Competitors)
            {
                _context.Entry(competitor).State = EntityState.Modified;
                //  if (competitor.Id == 0) _context.Entry(competitor).State = EntityState.Added;
                //  if (competitor.Id != 0) _context.Entry(competitor).State = EntityState.Modified;
            }
        }
        /// <summary>
        /// Delete all QuarterlyUploadStaging records prior to starting Import of QuarterlyUploadStaging
        /// </summary>
        public void TruncateQuarterlyUploadStaging()
        {
            using (var db = new RepositoryContext())
            {
                db.Database.ExecuteSqlCommand("Truncate table QuarterlyUploadStaging");
                db.Database.ExecuteSqlCommand("DBCC CHECKIDENT ('QuarterlyUploadStaging',RESEED, 0)");

                  
            }
        }

        public void TruncateLatestPriceData()
        {
            using (var db = new RepositoryContext())
            {
                db.Database.ExecuteSqlCommand("Truncate table LatestPrice");
                db.Database.ExecuteSqlCommand("DBCC CHECKIDENT ('LatestPrice',RESEED, 0)");

            }
        }

        public void TruncateSiteToCompetitor()
        {
            _context.Database.ExecuteSqlCommand("Truncate table SiteToCompetitor");
        }

        public void UpdateSitesCatNo(List<Site> sitesToUpdateCatNo)
        {
            //update Site's CatNo
            using (var newContext = new RepositoryContext())
            {
                var siteIdsToUpdate = sitesToUpdateCatNo.Select(s => s.Id).ToArray();

                foreach (var site in newContext.Sites.Where(s => siteIdsToUpdate.Contains(s.Id)))
                {
                    var newSiteValues = sitesToUpdateCatNo.First(s => s.Id == site.Id);

                    site.CatNo = newSiteValues.CatNo;
                }
                newContext.SaveChanges();
            }
        }

        public void UpdateSitesPrimaryInformation(List<Site> sitesToUpdateByCatNo)
        {
            //update existing Site details by CatNo
            using (var newContext = new RepositoryContext())
            {
                var siteCatNoToUpdate = sitesToUpdateByCatNo.Select(s => s.CatNo).ToArray();

                foreach (var site in newContext.Sites.Where(s => siteCatNoToUpdate.Contains(s.CatNo)))
                {
                    var newSiteValues = sitesToUpdateByCatNo.Single(s => s.CatNo == site.CatNo);

                    site.SiteName = newSiteValues.SiteName;
                    site.Town = newSiteValues.Town;
                    site.Brand = newSiteValues.Brand;
                    site.Address = newSiteValues.Address;
                    site.Suburb = newSiteValues.Suburb;
                    site.PostCode = newSiteValues.PostCode;
                    site.Company = newSiteValues.Company;
                    site.Ownership = newSiteValues.Ownership;
                }
                newContext.SaveChanges();
            }
        }

        public List<Site> NewSites(List<Site> newSitesToAdd)
        {
            var result = new List<Site>();
            //new Sites
            using (var newContext = new RepositoryContext())
            {
                newContext.Configuration.AutoDetectChangesEnabled = false;

                foreach (var siteToAdd in newSitesToAdd)
                {
                    result.Add(newContext.Sites.Add(siteToAdd));
                }
                newContext.SaveChanges();
            }

            return result;
        }

        public void UpdateSiteToCompetitor(List<SiteToCompetitor> newSiteToCompetitorRecords)
        {
            using (var transactionContext = new RepositoryContext())
            {
                using (var transaction = transactionContext.Database.BeginTransaction())
                {
                    transactionContext.Configuration.AutoDetectChangesEnabled = false;

                    try
                    {
                        var existingSiteToCompetitorRecords = transactionContext.SiteToCompetitors.ToList();
                        var commonrecords = existingSiteToCompetitorRecords.Where(x => newSiteToCompetitorRecords.Any(y => y.SiteId == x.SiteId));
                        var siteIdsToRemove=commonrecords.Select(x => x.SiteId).Distinct().ToList();

                        foreach (var siteID in siteIdsToRemove)
                        {
                            var siteToCompetitorObjs =
                                transactionContext.SiteToCompetitors.Where(x => x.SiteId == siteID);
                            foreach (var siteToC in siteToCompetitorObjs)
                            {
                                transactionContext.SiteToCompetitors.Remove(siteToC);
                            }
                            
                        }
                       
                      //  newSiteToCompetitorRecords.RemoveAll(x =>siteIdsToRemove.Any(y => y == (int)x.SiteId) ); //Remove Common Records

                    
                        if (newSiteToCompetitorRecords.Count > 0)
                        {
                            //add new site to competitor records
                            foreach (var newSiteToCompetitor in newSiteToCompetitorRecords)
                            {
                                transactionContext.SiteToCompetitors.Add(newSiteToCompetitor);
                            }

                            transactionContext.SaveChanges();

                            transaction.Commit();
                        }
                    }
                    catch (Exception ce)
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }

        /// <summary>
        /// Delete all DailyImport records of older uploads of today
        /// ONLY call this on successful import of at least one file of ofDate
        /// Reason - To keep DailyPrice table lean. Otherwise CalcPrice will take a long time to troll through a HUGE table
        /// </summary>
        /// <param name="ofdate"></param>
        /// <param name="uploadId"></param>
        public void DeleteRecordsForOlderImportsOfDate(DateTime ofdate, int uploadId)
        {
            using (var db = new RepositoryContext())
            {
                var deleteCmd = string.Format("Delete from DailyPrice Where DailyUploadId in " +
                                              "  (Select Id from FileUpload Where DateDiff(d, UploadDateTime, '{0}') = 0 and Id <> {1})",
                    ofdate.ToString("yyyy-MM-dd"), uploadId);
                db.Database.ExecuteSqlCommand(deleteCmd);
            }
        }

        public void CleanupIntegrationTestsData(string testUserName = "Integration tests")
        {
            using (var db = new RepositoryContext())
            {
                var testFileUploads = db.FileUploads.Where(fu => fu.UploadedBy == testUserName && fu.UploadTypeId == (int)FileUploadTypes.DailyPriceData).AsNoTracking();

                var testFileUploadIds = testFileUploads.Select(fu => fu.Id).ToArray();

                var deleteCmd = string.Format(
@"DELETE FROM DailyPrice WHERE DailyUploadId IN ({0});
DELETE FROM SitePrice WHERE UploadId IN ({0});
DELETE FROM FileUpload WHERE Id IN ({0});", string.Join(",", testFileUploadIds));
                db.Database.ExecuteSqlCommand(deleteCmd);
            }
        }

        private static object cachedAnyDailyPricesForFuelOnDateLock = new Object();

        public bool AnyDailyPricesForFuelOnDate(int fuelId, DateTime usingPricesforDate, int fileUploadId)
        {
            var cacheKey = fileUploadId.ToString();

            List<int> cachedAnyDailyPricesForFuelOnDate = PetrolPricingRepositoryMemoryCache.CacheObj.Get(cacheKey) as List<int>;

            bool isCacheFound = cachedAnyDailyPricesForFuelOnDate == null
                ? false
                : cachedAnyDailyPricesForFuelOnDate.Count != 0;
            if (!isCacheFound)
            {
                lock (cachedAnyDailyPricesForFuelOnDateLock)
                {
                    cachedAnyDailyPricesForFuelOnDate = PetrolPricingRepositoryMemoryCache.CacheObj.Get(cacheKey) as List<int>;
                    bool isFound = cachedAnyDailyPricesForFuelOnDate == null
                ? false
                : cachedAnyDailyPricesForFuelOnDate.Count != 0;
                    if (!isFound)
                    {
                        cachedAnyDailyPricesForFuelOnDate = _context.DailyPrices
                            .Include(x => x.DailyUpload)
                            .Where(x => DbFunctions.TruncateTime(x.DateOfPrice) == usingPricesforDate.Date)
                            .Select(x => x.FuelTypeId).Distinct().ToList();

                        PetrolPricingRepositoryMemoryCache.CacheObj.Add(cacheKey, cachedAnyDailyPricesForFuelOnDate, PetrolPricingRepositoryMemoryCache.ReportsCacheExpirationPolicy(5));
                    }
                }
            }

            return cachedAnyDailyPricesForFuelOnDate.Any(x => x == fuelId);
        }

        /// <summary>
        /// Gets the FileUpload available for Calc/ReCalc
        /// i.e those which has been imported to DailyPrice either Successfully OR
        /// (ImportAborted or CalcAborted previously to allow rerun)
        /// </summary>
        /// <param name="forDate"></param>
        /// <returns>Returns null if none available</returns>
        public FileUpload GetDailyFileAvailableForCalc(DateTime forDate)
        {
            int[] validForUploadStatuses = new int[] {
                (int)ImportProcessStatuses.Success,
                (int)ImportProcessStatuses.ImportAborted,
                (int)ImportProcessStatuses.CalcAborted
            };

            return _context.FileUploads.Where(x => x.UploadTypeId == 1
                && DbFunctions.TruncateTime(x.UploadDateTime) == forDate.Date
                && validForUploadStatuses.Contains(x.StatusId))
                .OrderByDescending(x => x.UploadDateTime)
                .FirstOrDefault();
        }

        public FileUpload GetDailyFileWithCalcRunningForDate(DateTime forDate)
        {
            return _context.FileUploads.FirstOrDefault(x => (x.StatusId == (int)ImportProcessStatuses.Calculating || x.StatusId == (int)ImportProcessStatuses.Uploaded) && DbFunctions.TruncateTime(x.UploadDateTime) == forDate.Date);
        }

        public void AddOrUpdateSitePriceRecord(SitePrice calculatedSitePrice)
        {
            var existingPriceRecord = _context.SitePrices.AsNoTracking().FirstOrDefault(
                x => x.SiteId == calculatedSitePrice.SiteId
                     && x.FuelTypeId == calculatedSitePrice.FuelTypeId
                     && DbFunctions.TruncateTime(x.DateOfPrice) == calculatedSitePrice.DateOfPrice.Date);

            if (existingPriceRecord == null)
            {
                calculatedSitePrice.JsSite = null;
                _context.Entry(calculatedSitePrice).State = EntityState.Added;
                _context.SaveChanges();
            }
            else
            {
                var stores = _context.Database.ExecuteSqlCommand(
    @"UPDATE [dbo].[SitePrice]
   SET
     [SuggestedPrice] = {0}
	,[DateOfCalc] = {1}
	,[DateOfPrice] = {2}
	,[UploadId] = {3}
    ,[CompetitorId] = {4}
    ,[Markup] = {5}
    ,[IsTrailPrice] = {6}
 WHERE Id = {7}",
                    calculatedSitePrice.SuggestedPrice,
                    calculatedSitePrice.DateOfCalc,
                    calculatedSitePrice.DateOfPrice,
                    calculatedSitePrice.UploadId,
                    calculatedSitePrice.CompetitorId,
                    calculatedSitePrice.Markup,
                    calculatedSitePrice.IsTrailPrice,
                    calculatedSitePrice.Id);
            }
        }

        public async Task<int> SaveOverridePricesAsync(List<SitePrice> prices, DateTime? forDate = null)
        {
            if (!forDate.HasValue) forDate = DateTime.Now;
            int rowsAffected=0;
            using (var db = new RepositoryContext())
            {
                foreach (SitePrice p in prices)
                {
                    //
                    var dbPricesForDate =
                        db.SitePrices.Where(x => DbFunctions.DiffDays(x.DateOfPrice, forDate) == 0)
                            .AsNoTracking()
                            .ToList();

                     rowsAffected = UpdateFuelOverridePrice(forDate, db, p);
                    if (p.FuelTypeId == (int) FuelTypeItem.Unleaded)
                    {
                        p.FuelTypeId = (int) FuelTypeItem.Super_Unleaded;
                        p.OverriddenPrice += 50;
                        rowsAffected=UpdateFuelOverridePrice(forDate, db, p);
                    }
                   
                }
               
                return rowsAffected;
            }
        }

        private int UpdateFuelOverridePrice(DateTime? forDate,RepositoryContext db, SitePrice p)
        {
             var dbPricesForDate =
                        db.SitePrices.Where(x => DbFunctions.DiffDays(x.DateOfPrice, forDate) == 0)
                            .AsNoTracking()
                            .ToList();

                    SitePrice p1 = p; // to prevent closure issue
                  
                    var entry =
                        dbPricesForDate.FirstOrDefault(x => x.SiteId == p1.SiteId && x.FuelTypeId == p1.FuelTypeId);
                    if (entry == null)
                    {
                        entry = new SitePrice
                        {
                            SiteId = p.SiteId,
                            FuelTypeId = p.FuelTypeId,
                            DateOfCalc = forDate.Value,
                            DateOfPrice = forDate.Value,
                            SuggestedPrice = 0,
                            OverriddenPrice = p.OverriddenPrice
                        };
                        db.Entry(entry).State = EntityState.Added;
                        //throw new ApplicationException(
                        //    String.Format("Price not found in DB for siteId={0}, fuelId={1}", p1.SiteId, p1.FuelTypeId));
                    }
                    else
                    {
                        entry.OverriddenPrice = p.OverriddenPrice;
                        db.Entry(entry).State = EntityState.Modified;
                    }
                    //db.Entry(entry).Property(x => x.OverriddenPrice).IsModified = true;
                
                return  db.SaveChanges();
        }

        /// <summary>
        /// Generic method to Log an import error for the running import of FileUpload for both Daily and Quarterly files
        /// </summary>
        /// <param name="fileDetails"></param>
        /// <param name="errorMessage"></param>
        /// <param name="lineNumber"></param>
        public void LogImportError(FileUpload fileDetails, string errorMessage = "", int? lineNumber = 0)
        {
            using (var db = new RepositoryContext())
            {
                ImportProcessError importProcessErrors = new ImportProcessError();

                importProcessErrors.UploadId = fileDetails.Id;
                importProcessErrors.ErrorMessage = errorMessage;

                if (lineNumber != null)
                {
                    importProcessErrors.RowOrLineNumber = int.Parse(lineNumber.ToString());
                }

                db.ImportProcessErrors.Add(importProcessErrors);
                db.SaveChanges();
            }
        }

        public void LogImportError(FileUpload fileDetails, Exception exception, int? lineNumber = 0)
        {
            using (var db = new RepositoryContext())
            {
                ImportProcessError importProcessErrors = new ImportProcessError();

                importProcessErrors.UploadId = fileDetails.Id;
                importProcessErrors.ErrorMessage = string.Format("Message: {0} Stack trace: {1}", exception.Message, exception.StackTrace);

                if (lineNumber != null)
                {
                    importProcessErrors.RowOrLineNumber = int.Parse(lineNumber.ToString());
                }

                db.ImportProcessErrors.Add(importProcessErrors);
                db.SaveChanges();
            }

            if (exception.InnerException != null)
                LogImportError(fileDetails, exception.InnerException, lineNumber);
        }

        /// <summary>
        /// As this is used for Audit, suppress errors as not useful to user..
        /// </summary>
        /// <param name="logItems"></param>
        /// <returns></returns>
        public async Task<List<EmailSendLog>> LogEmailSendLog(List<EmailSendLog> logItems)
        {
            try
            {
                using (var db = new RepositoryContext())
                {
                    foreach (var logItem in logItems)
                    {
                        db.EmailSendLogs.Attach(logItem);
                        db.Entry(logItem).State = EntityState.Added;
                    }
                    int rowsAffected = await db.SaveChangesAsync();
                    return rowsAffected <= 0 ? null : logItems;
                }
            }
            catch (Exception) // User wouldnt want to know about email logging errors. Thats implementation issue.
            {
                return null;
                // possibly log to Application error log..
            }
        }

        /// <summary>
        /// Gets the EmailSendLog entries for selected site (or all sites if siteId =0) forDate, latest first
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="forDate"></param>
        /// <returns></returns>
        public Task<List<EmailSendLog>> GetEmailSendLog(int siteId, DateTime forDate)
        {
            var retval = new List<EmailSendLog>();
            using (var db = new RepositoryContext())
            {
                var list = db.EmailSendLogs;
                if (!list.Any()) return Task.FromResult(retval);

                retval = list.Where(x => DbFunctions.DiffDays(x.SendDate, forDate) == 0)
                    .AsNoTracking()
                    .ToList();
                retval = retval.OrderByDescending(x => x.SendDate).ToList();
            }
            return Task.FromResult(retval);
        }

        /// <summary>
        /// Updated the FileUpload status as specified by param StatusId
        /// </summary>
        /// <param name="fileUpload">The fileUpload object whose status is to be updated</param>
        /// <param name="statusId">Status to set for the uploaded file</param>
        public void UpdateImportProcessStatus(int statusId, FileUpload fileUpload)
        {
            using (var db = new RepositoryContext())
            {
                _context.Database.ExecuteSqlCommand("Update FileUpload Set StatusId = " + statusId + " where Id = " +
                                                    fileUpload.Id);
            }
        }

        public SiteToCompetitor LookupSiteAndCompetitor(int siteCatNo, int competitorCatNo)
        {
            return _context.SiteToCompetitors.FirstOrDefault(x =>
                    x.Site.CatNo.HasValue && x.Site.CatNo.Value == siteCatNo
                && x.Competitor.CatNo.HasValue && x.Competitor.CatNo.Value == competitorCatNo);
        }

        /// <summary>
        /// Get competitors based on drivetime criteria
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="driveTimeFrom"></param>
        /// <param name="driveTimeTo"></param>
        /// <param name="includeSainsburysAsCompetitors"></param>
        /// <returns></returns>
        public IEnumerable<SiteToCompetitor> GetCompetitors(Site site, float driveTimeFrom, float driveTimeTo, bool includeSainsburysAsCompetitors = true)
        {
            IEnumerable<SiteToCompetitor> siteCompetitors = GetSitesWithCompetitors()[site.Id]
                .Competitors.Where(x => x.DriveTime >= driveTimeFrom && x.DriveTime <= driveTimeTo)
                .ToList();

            if (!includeSainsburysAsCompetitors)
            {
                siteCompetitors = siteCompetitors.Where(x => !x.Competitor.IsSainsburysSite);
            }
            return siteCompetitors;
        }

        public SiteToCompetitor GetCompetitor(int siteId, int competitorId)
        {
            return _context.SiteToCompetitors.FirstOrDefault(x => x.CompetitorId == competitorId && x.SiteId == siteId);
        }

        // New File Upload
        public FileUpload NewUpload(FileUpload upload)
        {
            if (upload.Status == null)
            {
                try
                {
                    upload.Status = GetProcessStatuses().First();
                }
                catch (Exception ce)
                {

                }
            }

            if (upload.UploadType == null)
            {
                upload.UploadType = GetUploadTypes().FirstOrDefault(x => x.Id == upload.UploadTypeId);
            }

            var result = _context.FileUploads.Add(upload);

            _context.SaveChanges();

            return result; // return full object back
        }

        public bool ExistsUpload(string storedFileName)
        {
            return
                _context.FileUploads.Any(
                    x => x.StoredFileName.Equals(storedFileName, StringComparison.CurrentCultureIgnoreCase));
        }

        /// <summary>
        /// Get List of FileUploads, use null params for full list
        /// </summary>
        /// <param name="date">Nullable date to get Files for a specific date</param>
        /// <param name="uploadTypeId">Optional uploadType to filter if needed</param>
        /// <param name="statusId">Optional statusType to filter if needed</param>
        /// <returns></returns>
        public List<FileUpload> GetFileUploads(DateTime? date, int? uploadTypeId, int? statusId)
        {
            IEnumerable<FileUpload> files = GetFileUploads();

            if (date.HasValue)
            {
                files = files.Where(x => x.UploadDateTime.Date == date.Value.Date);
            }
            if (uploadTypeId.HasValue)
            {
                files = files.Where(x => x.UploadTypeId == uploadTypeId.Value);
            }
            if (statusId.HasValue)
            {
                files = files.Where(x => x.StatusId == statusId.Value);
            }

            return files.OrderByDescending(x => x.UploadDateTime).ToList();
        }

        public IEnumerable<FileUpload> GetFileUploads()
        {
            return _context.FileUploads.Include(x => x.UploadType).Include(y => y.Status);
        }

        public FileUpload GetFileUpload(int id)
        {
            return _context.FileUploads.Include(x => x.UploadType).Include(y => y.Status).FirstOrDefault(x => x.Id == id);
        }

        ///  Do we have any FileUploads for specified Date and UploadType
        public bool AnyFileUploadForDate(DateTime date, UploadType uploadType)
        {
            var anyFilesForDate = GetFileUploads(date, uploadType.Id, null);
            return anyFilesForDate.Any();
        }

        // ############### Lookup #############
        public IEnumerable<UploadType> GetUploadTypes()
        {
            return _context.UploadType.ToList();
        }

        public IEnumerable<FuelType> GetFuelTypes()
        {
            return _context.FuelType.ToList();
        }

        public IEnumerable<ImportProcessStatus> GetProcessStatuses()
        {
            return _context.ImportProcessStatus.ToList();
        }

        public CompetitorSiteReportViewModel GetReportCompetitorSite(int siteId)
        {
            var result = new CompetitorSiteReportViewModel();

            var sainsburysSites = _context.Sites.Include(x => x.Competitors).Where(x => x.Id == (siteId > 0 ? siteId : x.Id)).ToList();

            //normalised brand competitor report
            Dictionary<string, int> normalisedBrandCompetitors = new Dictionary<string, int>();

            if (sainsburysSites.Any())
            {
                if (sainsburysSites.Count == 1)
                {
                    result.SiteName = sainsburysSites.First().SiteName;
                }

                var brandReportRows = new List<CompetitorBrandTimeViewModel>();

                ReportTypes reportType = siteId == -1
                ? ReportTypes.NormalisedMax
                : ReportTypes.Default;

                // 1) Sainsbury's site
                foreach (var sainsburysSite in sainsburysSites)
                {
                    if (reportType == ReportTypes.Default)
                    {
                        var brandNames = sainsburysSite.Competitors.Select(x => x.Competitor.Brand).Distinct().OrderBy(x => x);

                        foreach (var brandName in brandNames)
                        {
                            var brandCompetitors = sainsburysSite.Competitors.Where(x => x.Competitor.Brand == brandName).ToList();

                            var brandReportRow = brandReportRows.FirstOrDefault(x => x.BrandName == brandName);

                            if (brandReportRow == null)
                            {
                                brandReportRow = new CompetitorBrandTimeViewModel();
                                brandReportRow.BrandName = brandName;
                                brandReportRows.Add(brandReportRow);
                            }
                            brandReportRow.Count0To5 += Count(brandCompetitors, 0, 4.99f);
                            brandReportRow.Count5To10 += Count(brandCompetitors, 5, 9.99f);
                            brandReportRow.Count10To15 += Count(brandCompetitors, 10, 14.99f);
                            brandReportRow.Count15To20 += Count(brandCompetitors, 15, 19.99f);
                            brandReportRow.Count20To25 += Count(brandCompetitors, 20, 24.99f);
                            brandReportRow.Count25To30 += Count(brandCompetitors, 25, 29.99f);
                            brandReportRow.CountMoreThan30 += Count(brandCompetitors, 30, int.MaxValue);
                        }
                    }
                    else
                    {
                        var brandNames = sainsburysSite.Competitors.Select(x => x.Competitor.Brand).Distinct();

                        // 2) all unique brand names e.g. ASDA, TESCO
                        foreach (var brandName in brandNames)
                        {
                            // 3) brands of the competitor from step 2) e.g. ASDA so we get all ASDA here
                            var brandCompetitors = sainsburysSite.Competitors.Where(x => x.Competitor.Brand == brandName).ToList();

                            // 4) have we already counted for this brand
                            var brandReportRow = brandReportRows.FirstOrDefault(x => x.BrandName == brandName);
                            if (brandReportRow == null)
                            {
                                brandReportRow = new CompetitorBrandTimeViewModel();
                                brandReportRow.BrandName = brandName;
                                brandReportRows.Add(brandReportRow);
                            }

                            // 5) let's count
                            brandReportRow.Count0To5 += NormalisedCount(brandCompetitors /* e.g. All ASDAs*/, normalisedBrandCompetitors, 0, 4.99f);
                            brandReportRow.Count5To10 += NormalisedCount(brandCompetitors, normalisedBrandCompetitors, 5, 9.99f);
                            brandReportRow.Count10To15 += NormalisedCount(brandCompetitors, normalisedBrandCompetitors, 10, 14.99f);
                            brandReportRow.Count15To20 += NormalisedCount(brandCompetitors, normalisedBrandCompetitors, 15, 19.99f);
                            brandReportRow.Count20To25 += NormalisedCount(brandCompetitors, normalisedBrandCompetitors, 20, 24.99f);
                            brandReportRow.Count25To30 += NormalisedCount(brandCompetitors, normalisedBrandCompetitors, 25, 29.99f);
                            brandReportRow.CountMoreThan30 += NormalisedCount(brandCompetitors, normalisedBrandCompetitors, 30, int.MaxValue);
                        }
                    }
                }
                result.BrandTimes = brandReportRows;
            }

            return result;
        }

        public PricePointReportViewModel GetReportPricePoints(DateTime when, int fuelTypeId)
        {
            try
            {
                const bool useRefactoredCode = true;

                var result = new PricePointReportViewModel();

                var f = _context.FuelType.FirstOrDefault(x => x.Id == fuelTypeId);
                if (f != null)
                {
                    result.FuelTypeName = f.FuelTypeName;

                    // Ignore this approach.. which uses Date Of Price from DailyPrice, instead see next line..
                    //var dailyPrices = _context.DailyPrices.Where(x => DbFunctions.DiffDays(x.DateOfPrice, when) == 0 && x.FuelTypeId == fuelTypeId).ToList();


                    // Report uses Prices as per date of upload..(not date of Price in DailyPrice)..
                    var dailyPrices = _context.DailyPrices.Where(x => DbFunctions.DiffDays(x.DateOfPrice, when) == 0 && x.FuelTypeId == fuelTypeId).ToList();

                    var distinctPrices = dailyPrices.Select(x => x.ModalPrice).Distinct().OrderBy(x => x).ToList();
                    var distinctCatNos = dailyPrices.Select(x => x.CatNo).Distinct().ToList();
                    var competitorSites = _context.Sites.Where(x => distinctCatNos.Contains(x.CatNo.Value) && !x.IsSainsburysSite).ToList();
                    var distinctBrands = competitorSites.Select(x => x.Brand).Distinct().OrderBy(x => x).ToList();


                    /*   #region original code

                       if (useRefactoredCode == false)
                       {
                           foreach (var distinctPrice in distinctPrices)
                           {
                               var matchingDailyPrices = dailyPrices.Where(x => x.ModalPrice == distinctPrice).ToList();
                               foreach (var dailyPrice in matchingDailyPrices)
                               {
                                   var reportRowItem = result.PricePointReportRows.FirstOrDefault(x => x.Price == distinctPrice);
                                   if (reportRowItem == null)
                                   {
                                       reportRowItem = new PricePointReportRowViewModel
                                       {
                                           Price = distinctPrice
                                       };
                                       result.PricePointReportRows.Add(reportRowItem);
                                   }

                                   foreach (var distinctBrand in distinctBrands)
                                   {
                                       var b = reportRowItem.PricePointBrands.FirstOrDefault(x => x.Name == distinctBrand);
                                       if (b == null)
                                       {
                                           b = new PricePointBrandViewModel
                                           {
                                               Name = distinctBrand
                                           };
                                           reportRowItem.PricePointBrands.Add(b);
                                       }
                                       b.Count += competitorSites.Count(x => x.Brand == distinctBrand && x.CatNo == dailyPrice.CatNo);                               
                                   }
                               }
                           }
                       }
                       #endregion original code*/

                    #region new code

                    if (useRefactoredCode)
                    {
                        distinctBrands.Insert(0, "Summary");
                        var priceColumnIndexes = new Dictionary<int, int>();
                        var brandRowIndexes = new Dictionary<string, int>();
                        var tableCells = new int?[distinctBrands.Count(), distinctPrices.Count()];

                        // build row and column index lookup tables
                        var rowCounter = 0;
                        foreach (var brand in distinctBrands)
                            brandRowIndexes[brand] = rowCounter++;

                        var columnCounter = 0;
                        foreach (var price in distinctPrices)
                            priceColumnIndexes[price] = columnCounter++;

                        // loop and count each brand, price combination
                        foreach (var daily in dailyPrices)
                        {
                            var price = daily.ModalPrice;
                            var competitor = competitorSites.FirstOrDefault(x => x.CatNo.Value == daily.CatNo);

                            if (competitor == null)
                                continue;

                            var brand = competitor.Brand;

                            var rowIndex = brandRowIndexes[brand];
                            var columnIndex = priceColumnIndexes[price];

                            var cell = tableCells[rowIndex, columnIndex];

                            if (cell.HasValue)
                            {
                                tableCells[rowIndex, columnIndex]++;
                                var rowIndex2 = brandRowIndexes["Summary"];
                                tableCells[rowIndex2, columnIndex] = tableCells[rowIndex2, columnIndex].HasValue ? tableCells[rowIndex2, columnIndex].Value + 1 : tableCells[rowIndex, columnIndex];

                            }
                            else
                            {
                                tableCells[rowIndex, columnIndex] = 1;
                                rowIndex = brandRowIndexes["Summary"];
                                tableCells[rowIndex, columnIndex] = tableCells[rowIndex, columnIndex].HasValue ? tableCells[rowIndex, columnIndex].Value + 1 : 1;
                            }


                        }

                        // construct the view model
                        rowCounter = 0;
                        foreach (var brand in distinctBrands)
                        {
                            var reportRowItem = new PricePointReportRowViewModel
                            {
                                Brand = brand
                            };
                            result.PricePointReportRows.Add(reportRowItem);

                            columnCounter = 0;

                            foreach (var price in distinctPrices)
                            {
                                var count = tableCells[rowCounter, columnCounter];

                                var reportColumnItem = new PricePointPriceViewModel
                                {
                                    Price = price,
                                    Count = count.HasValue ? count.Value : 0
                                };
                                reportRowItem.PricePointPrices.Add(reportColumnItem);
                                columnCounter++;
                            }
                            if (distinctPrices.Count ==0)
                            {
                                var reportColumnItem = new PricePointPriceViewModel
                                {
                                    Price = 0.0,
                                    Count = 2
                                };
                                reportRowItem.PricePointPrices.Add(reportColumnItem);
                            }
                            rowCounter++;
                        }
                    }
                    #endregion new code
                }

                result.PricePointReportRows = result.PricePointReportRows.Where(x => x.PricePointPrices.Any(b => b.Count > 0)).ToList();

                return result;
            }
            catch(Exception ce)
            {
                return null;
            }
        }

        public NationalAverageReportViewModel GetReportNationalAverage(DateTime when)
        {
            var result = new NationalAverageReportViewModel();

            var fuelTypeIds = new List<int> { (int)FuelTypeItem.Unleaded,(int)FuelTypeItem.Diesel };

            // Report uses Prices as per date of upload..(not date of Price in DailyPrice)..
            var dailyPrices = _context.DailyPrices.Where(x => DbFunctions.DiffDays(x.DateOfPrice, when) == 0 && fuelTypeIds.Contains(x.FuelTypeId)).ToList();

            var fuels = _context.FuelType.ToList();

            var distinctCatNos = dailyPrices.Select(x => x.CatNo).Distinct().ToList();
            var competitorSites = _context.Sites.Where(x => distinctCatNos.Contains(x.CatNo.Value)).ToList();

            //calculating by brands
            var distinctBrands = competitorSites.Select(x => x.Brand).Distinct().OrderBy(x => x).ToList();

            if (distinctBrands.Count > 0)
            {
                distinctBrands.Remove(Const.SAINSBURYS);
                distinctBrands.Insert(0, Const.SAINSBURYS);
                distinctBrands.Remove(Const.ASDA);
                distinctBrands.Insert(1, Const.ASDA);
                distinctBrands.Remove(Const.TESCO);
                distinctBrands.Insert(2, Const.TESCO);
                distinctBrands.Remove(Const.MORRISONS);
                distinctBrands.Insert(3, Const.MORRISONS);

            }
          

            foreach (var fuelType in fuelTypeIds)
            {
                var f = fuels.FirstOrDefault(x => x.Id == fuelType);
                if (f == null) continue;

                var fuelRow = new NationalAverageReportFuelViewModel();
                result.Fuels.Add(fuelRow);
                fuelRow.FuelName = f.FuelTypeName;
                int nTotalIndependentsMin = 0;
                int nTotalIndependentsMax = 0;
                int nTotalIndependentsAvg = 0;
                int nCount = 0;
                if (distinctBrands.Count > 0)
                {
                    foreach (var brand in distinctBrands)
                    {
                        var brandAvg = new NationalAverageReportBrandViewModel();
                        fuelRow.Brands.Add(brandAvg);
                        brandAvg.BrandName = brand;

                        var brandCatsNos = competitorSites.Where(x => x.Brand == brand).Where(x => x.CatNo.HasValue).Select(x => x.CatNo.Value).ToList();
                        var pricesList = dailyPrices.Where(x => x.FuelTypeId == fuelType && brandCatsNos.Contains(x.CatNo)).ToList();

                        if (pricesList.Any())
                        {
                            brandAvg.Min = (int)pricesList.Min(x => x.ModalPrice);
                            brandAvg.Average = (int)pricesList.Average(x => x.ModalPrice);
                            brandAvg.Max = (int)pricesList.Max(x => x.ModalPrice);
                            if (!brand.Equals(Const.Sainsburys, StringComparison.InvariantCultureIgnoreCase)
                                || !brand.Equals(Const.ASDA, StringComparison.InvariantCultureIgnoreCase)
                                || !brand.Equals(Const.TESCO, StringComparison.InvariantCultureIgnoreCase)
                                || !brand.Equals(Const.MORRISONS, StringComparison.InvariantCultureIgnoreCase))
                            {
                                nTotalIndependentsMin = nTotalIndependentsMin > brandAvg.Min ? brandAvg.Min : nTotalIndependentsMin;
                                nTotalIndependentsMax = nTotalIndependentsMax < brandAvg.Max ? brandAvg.Max : nTotalIndependentsMax;
                                nTotalIndependentsAvg += brandAvg.Average;
                                nCount++;
                            }
                        }

                        if (brand.Equals(Const.Sainsburys, StringComparison.InvariantCultureIgnoreCase))
                        {
                            fuelRow.SainsburysPrice = brandAvg.Average;
                        }
                    }
                    var brandIndependent = new NationalAverageReportBrandViewModel();
                    fuelRow.Brands.Insert(4, brandIndependent);
                    brandIndependent.BrandName = "Total Independents";
                    brandIndependent.Min = nTotalIndependentsMin;
                    brandIndependent.Max = nTotalIndependentsMax;
                    brandIndependent.Average = nTotalIndependentsAvg / nCount;
                    var temp1 = new NationalAverageReportBrandViewModel();
                    fuelRow.Brands.Insert(5, temp1);

                    var temp2 = new NationalAverageReportBrandViewModel();
                    temp2.BrandName = "All other independent brands";
                    fuelRow.Brands.Insert(6, temp2);
                }

            }

            return result;
        }

        public NationalAverageReportViewModel GetReportcompetitorsPriceRange(DateTime when)
        {
            var result = new NationalAverageReportViewModel();

            var fuelTypeIds = new List<int> { (int)FuelTypeItem.Diesel, (int)FuelTypeItem.Unleaded };

            // Report uses Prices as per date of upload..(not date of Price in DailyPrice)..
            var dailyPrices = _context.DailyPrices.Where(x => DbFunctions.DiffDays(x.DateOfPrice, when) == 0 && fuelTypeIds.Contains(x.FuelTypeId)).ToList();

            var fuels = _context.FuelType.ToList();

            var distinctCatNos = dailyPrices.Select(x => x.CatNo).Distinct().ToList();
            var competitorSites = _context.Sites.Where(x => distinctCatNos.Contains(x.CatNo.Value)).ToList();

            //calculating by brands
            var distinctBrands = competitorSites.Select(x => x.Brand).Distinct().OrderBy(x => x).ToList();

            if (distinctBrands.Count > 0)
            {
                distinctBrands.Remove(Const.SAINSBURYS);
                distinctBrands.Insert(0, Const.SAINSBURYS);
            }

            foreach (var fuelType in fuelTypeIds)
            {
                var f = fuels.FirstOrDefault(x => x.Id == fuelType);
                if (f == null) continue;

                var fuelRow = new NationalAverageReportFuelViewModel();
                result.Fuels.Add(fuelRow);
                fuelRow.FuelName = f.FuelTypeName;

                foreach (var brand in distinctBrands)
                {
                    var brandAvg = new NationalAverageReportBrandViewModel();
                    fuelRow.Brands.Add(brandAvg);
                    brandAvg.BrandName = brand;

                    var brandCatsNos = competitorSites.Where(x => x.Brand == brand).Where(x => x.CatNo.HasValue).Select(x => x.CatNo.Value).ToList();
                    var pricesList = dailyPrices.Where(x => x.FuelTypeId == fuelType && brandCatsNos.Contains(x.CatNo)).ToList();

                    if (pricesList.Any())
                    {
                        brandAvg.Min = (int)pricesList.Min(x => x.ModalPrice);
                        brandAvg.Average = (int)pricesList.Average(x => x.ModalPrice);
                        brandAvg.Max = (int)pricesList.Max(x => x.ModalPrice);
                    }

                    if (brand.Equals(Const.Sainsburys, StringComparison.InvariantCultureIgnoreCase))
                    {
                        fuelRow.SainsburysPrice = brandAvg.Average;
                    }
                }
            }

            return result;
        }

        public NationalAverageReportViewModel GetReportNationalAverage2(DateTime when, bool ViewAllCompetitors)
        {
            var result = new NationalAverageReportViewModel();

            var fuelTypeIds = new List<int> { (int)FuelTypeItem.Unleaded, (int)FuelTypeItem.Diesel };

            // Report uses Prices as per date of upload..(not date of Price in DailyPrice)..
            var dailyPrices = _context.DailyPrices.Where(x => DbFunctions.DiffDays(x.DateOfPrice, when) == 0 && fuelTypeIds.Contains(x.FuelTypeId)).ToList();

            var fuels = _context.FuelType.ToList();

            var distinctCatNos = dailyPrices.Select(x => x.CatNo).Distinct().ToList();
            var competitorSites = _context.Sites.Where(x => distinctCatNos.Contains(x.CatNo.Value)).ToList();

            //calculating by brands
            var distinctBrands = competitorSites.Select(x => x.Brand).Distinct().OrderBy(x => x).ToList();

            if (distinctBrands.Count > 0)
            {
                distinctBrands.Remove(Const.SAINSBURYS);
                distinctBrands.Insert(0, Const.SAINSBURYS);
                if (!ViewAllCompetitors)
                {
                    foreach (var band in LstOfBandsToRemoveInNA2)
                        distinctBrands.Remove(band);


                    distinctBrands.Remove(Const.TESCO);
                    distinctBrands.Insert(1, Const.TESCO);
                    distinctBrands.Remove(Const.MORRISONS);
                    distinctBrands.Insert(2, Const.MORRISONS);
                    distinctBrands.Remove(Const.ASDA);
                    distinctBrands.Insert(3, Const.ASDA);
                    distinctBrands.Remove(Const.UK);
                    distinctBrands.Insert(4, Const.UK);
                    distinctBrands.Remove(Const.SHELL);
                    distinctBrands.Insert(5, Const.SHELL);
                    distinctBrands.Remove(Const.ESSO);
                    distinctBrands.Insert(6, Const.ESSO);
                    distinctBrands.Remove(Const.TOTAL);
                    distinctBrands.Insert(7, Const.TOTAL);
                    distinctBrands.Remove(Const.BP);
                    distinctBrands.Insert(8, Const.BP);
                }
            }

            foreach (var fuelType in fuelTypeIds)
            {
                var f = fuels.FirstOrDefault(x => x.Id == fuelType);
                if (f == null) continue;

                var fuelRow = new NationalAverageReportFuelViewModel();
                result.Fuels.Add(fuelRow);
                fuelRow.FuelName = f.FuelTypeName;

                foreach (var brand in distinctBrands)
                {
                    var brandAvg = new NationalAverageReportBrandViewModel();
                    fuelRow.Brands.Add(brandAvg);
                    brandAvg.BrandName = brand;

                    var brandCatsNos = competitorSites.Where(x => x.Brand == brand).Where(x => x.CatNo.HasValue).Select(x => x.CatNo.Value).ToList();
                    var pricesList = dailyPrices.Where(x => x.FuelTypeId == fuelType && brandCatsNos.Contains(x.CatNo)).ToList();

                    if (pricesList.Any())
                    {
                        brandAvg.Min = (int)pricesList.Min(x => x.ModalPrice);
                        brandAvg.Average = (int)pricesList.Average(x => x.ModalPrice);
                        brandAvg.Max = (int)pricesList.Max(x => x.ModalPrice);
                    }

                    if (brand.Equals(Const.Sainsburys, StringComparison.InvariantCultureIgnoreCase))
                    {
                        fuelRow.SainsburysPrice = brandAvg.Average;
                    }
                }
            }

            return result;
        }


        public CompetitorsPriceRangeByCompanyViewModel GetReportCompetitorsPriceRangeByCompany(DateTime when, string companyName, string brandName)
        {
            var result = new CompetitorsPriceRangeByCompanyViewModel();
            result.Date = when;
            // Report uses Prices as per date of upload..(not date of Price in DailyPrice)..
            var dailyPrices = _context.DailyPrices.Where(x => DbFunctions.DiffDays(x.DateOfPrice, when) == 0 && result.FuelTypeIds.Contains(x.FuelTypeId)).ToList();

            var fuels = _context.FuelType.ToList();

            var distinctCatNos = dailyPrices.Select(x => x.CatNo).Distinct().ToList();

            var companiesAndBrandsForReportDate = _context.Sites.Where(x => distinctCatNos.Contains(x.CatNo.Value)).ToList();

            var companyNamesQuery = companiesAndBrandsForReportDate.Select(x => x.Company);

            if (string.IsNullOrWhiteSpace(companyName) || companyName.Equals("All"))
            {
                var availableCompanyNames = GetCompanies().Where(v => v.Value > 1).Select(k => k.Key).ToArray();

                companyNamesQuery = companyNamesQuery.Where(cn => availableCompanyNames.Contains(cn));
            }
            else
            {
                companyNamesQuery = companyNamesQuery.Where(cn => cn.Equals(companyName));
            }

            var distinctCompaniesForReportDate = companyNamesQuery.Distinct().OrderBy(x => x).ToList();

            distinctCompaniesForReportDate.Remove(SainsburysCompanyName.ToUpper());
            distinctCompaniesForReportDate.Insert(0, SainsburysCompanyName.ToUpper());

            foreach (var company in distinctCompaniesForReportDate)
            {
                var newCompanyReportRow = new CompetitorsPriceRangeByCompanyCompanyViewModel
                {
                    CompanyName = company
                };

                var companyBrands = companiesAndBrandsForReportDate.Where(x => x.Company.Equals(company) && x.CatNo.HasValue);

                if (false == string.IsNullOrWhiteSpace(brandName) && false == brandName.Equals("All"))
                {
                    companyBrands = companyBrands.Where(x => x.Brand.Equals(brandName) || x.Brand.Equals(Const.SAINSBURYS));
                }

                foreach (var companyBrand in companyBrands.Select(b => b.Brand).Distinct())
                {
                    var newBrandReportRow = new CompetitorsPriceRangeByCompanyBrandViewModel();
                    newBrandReportRow.BrandName = companyBrand;

                    foreach (var fuelTypeId in result.FuelTypeIds)
                    {
                        var companyBrandCatNos = companyBrands.Where(b => b.Brand == companyBrand).Select(x => x.CatNo.Value).ToList();

                        var pricesList = dailyPrices.Where(x => x.FuelTypeId == fuelTypeId && companyBrandCatNos.Contains(x.CatNo)).ToList();

                        if (pricesList.Any())
                        {
                            var newFuel = new CompetitorsPriceRangeByCompanyBrandFuelViewModel();

                            newBrandReportRow.Fuels.Add(newFuel);
                            newFuel.FuelTypeId = fuelTypeId;
                            newFuel.Min = (int)pricesList.Min(x => x.ModalPrice);
                            newFuel.Average = (int)pricesList.Average(x => x.ModalPrice);
                            newFuel.Max = (int)pricesList.Max(x => x.ModalPrice);

                            if (company.Equals(SainsburysCompanyName, StringComparison.InvariantCultureIgnoreCase))
                            {
                                result.SainsburysPrices.Add(fuelTypeId, newFuel.Average);
                            }
                        }
                    }

                    newCompanyReportRow.Brands.Add(newBrandReportRow);
                }

                result.ReportCompanies.Add(newCompanyReportRow);
            }

            return result;
        }

        public PriceMovementReportViewModel GetReportPriceMovement(string brandName, DateTime fromDt, DateTime toDt, int fuelTypeId,string siteName)
        {
            var retval = new PriceMovementReportViewModel();

            Task task = Task.Factory.StartNew(() =>
            {
                var dates = new List<DateTime>();
                for (var d = fromDt; d <= toDt; d = d.AddDays(1))
                {
                    dates.Add(d);
                }
                retval.Dates = dates;

                var sitesWithPrices = brandName == "SAINSBURYS"
                    ? GetSitesWithEmailsAndPrices(fromDt, toDt).ToList()
                    : GetBrandWithDailyPricesAsPrices(brandName, fromDt, toDt).ToList();

                var sortedSitesWithPrices = siteName.Trim() == "empty" ? sitesWithPrices.OrderBy(x => x.SiteName) : sitesWithPrices.Where(x => x.SiteName.ToUpper().Trim().Contains(siteName.ToUpper().Trim())).OrderBy(x => x.SiteName);

                foreach (var s in sortedSitesWithPrices)
                {
                    var dataRow = new PriceMovementReportRows
                    {
                        SiteId = s.Id,
                        SiteName = s.SiteName,
                        DataItems = new List<PriceMovementReportDataItems>()
                    };
                    retval.ReportRows.Add(dataRow);
                    var dataItems = dataRow.DataItems;

                    dataItems.AddRange(dates.Select(d => new PriceMovementReportDataItems
                    {
                        PriceDate = d,
                        PriceValue = GetSitePriceOnDate(s.Prices, d, fuelTypeId)
                    }));
                }
            });
            task.Wait();


            
            return retval;
        }

        /// <summary>
        /// Compliance Report - ComplianceReportContainerViewModel.ComplianceReportViewModel
        /// Approach:
        ///     CatPrice - Look for DailyPrice going forward D+1, D+2 until we find one.. (possibly none found)
        ///     ExpectedPrice - Look for SitePrice going back D-1, D-2 until we find one..
        ///     where D = forDate
        /// </summary>
        /// <param name="forDate"></param>
        /// <returns></returns>
        public ComplianceReportViewModel GetReportCompliance(DateTime forDate)
        {
            var retval = new ComplianceReportViewModel();
            try
            {
                var fuelTypesList = new[] { 2, 6, 1 }; // Unl, Diesel, Super
                var nextday = forDate.AddDays(1);

                //DateTime? sitePriceDateLookingBack = GetLastSitePriceDate(forDate);
                DateTime? catPriceDateLookingForward = GetFirstDailyPriceDate(forDate);
                DateTime? catPriceDateLookingForward_nextday = GetFirstDailyPriceDate(nextday);

                if (catPriceDateLookingForward == null || catPriceDateLookingForward_nextday == null) return null;

                var sites = GetJsSites();

                var reportFuels = GetFuelTypes().Where(x => fuelTypesList.Contains(x.Id)).ToList();

                var sitePrices = CallSitePriceSproc(forDate);
                var sitePrices_nextday = CallSitePriceSproc(nextday);

                var dailyPrices = new List<DailyPrice>();
                if (catPriceDateLookingForward != null)
                {
                    dailyPrices = GetDailyPricesForDate(catPriceDateLookingForward.Value);
                }

                var dailyPrices_nextday = new List<DailyPrice>();
                if (catPriceDateLookingForward_nextday != null)
                {
                    dailyPrices_nextday = GetDailyPricesForDate(catPriceDateLookingForward_nextday.Value);
                }


                if (dailyPrices.Count == 0 || dailyPrices_nextday.Count==0)
                {                          
                    return null;
                }

                foreach (var site in sites)
                {
                    Site site1 = site;
                    var dataRow = new ComplianceReportRow
                    {
                        SiteId = site1.Id,
                        PfsNo = site1.PfsNo.ToString(),
                        StoreNo = site1.StoreNo.ToString(),
                        CatNo = site1.CatNo.ToString(),
                        SiteName = site1.SiteName,
                        DataItems = new List<ComplianceReportDataItem>()
                    };
                    retval.ReportRows.Add(dataRow);
                   
                    var dataItems = dataRow.DataItems;
                 /*   var sitePriceViewModels = sitePrices as SitePriceViewModel[] ?? sitePrices;
                    var sitePriceViewModels_nextday = sitePrices_nextday as SitePriceViewModel[] ?? sitePrices_nextday;
                   
                    var sitePriceViewModel = sitePriceViewModels==null? null: sitePriceViewModels.FirstOrDefault(x => x.SiteId == site1.Id);
                    var sitePriceViewModel_nextday = sitePriceViewModels_nextday == null ? null : sitePriceViewModels_nextday.FirstOrDefault(x => x.SiteId == site1.Id);*/
                 
                    foreach (var fuelId in fuelTypesList) // report order as per array - Unl, Diesel, Super
                    {
                        FuelType fuel = reportFuels.FirstOrDefault(x => x.Id == fuelId);

                        if (fuel == null)
                        {
                           throw new ApplicationException("FuelId:" + fuelId + " not found in database.");
                        }

                        var dataItem = new ComplianceReportDataItem
                        {
                            FuelTypeId = fuel.Id,
                            FuelTypeName = fuel.FuelTypeName
                        };
                        dataItems.Add(dataItem);

                        var dailyPrice_nextday = dailyPrices_nextday.FirstOrDefault(x => x.CatNo.Equals(site1.CatNo) && x.FuelTypeId == fuel.Id);
                        // Find the ExpectedPrice
                        if (dailyPrice_nextday != null)
                        {  
                            dataItem.FoundExpectedPrice = true;
                            dataItem.ExpectedPriceValue = dailyPrice_nextday.ModalPrice;                          
                        }

                        var dailyPrice = dailyPrices.FirstOrDefault(x => x.CatNo.Equals(site1.CatNo) && x.FuelTypeId == fuel.Id);
                        if (dailyPrice != null)
                        {
                            dataItem.CatPriceValue = dailyPrice.ModalPrice;
                            dataItem.FoundCatPrice = true;
                        }
                        if (!dataItem.FoundCatPrice || !dataItem.FoundExpectedPrice) continue;

                        dataItem.Diff = (dataItem.CatPriceValue - dataItem.ExpectedPriceValue) / 10;
                        dataItem.DiffValid = true;
                    }
                                       
                }
                return retval;
            }
            catch (Exception ce)
            {
                return null;
            }
        }


        public bool RemoveExcludeBrand(string strBrandName)
        {
            try
            {
                ExcludeBrands brand = _context.ExcludeBrands.ToList().Find(x => x.BrandName == strBrandName);
               _context.ExcludeBrands.Remove(brand);
                var returnval = _context.SaveChanges();
                return returnval != null;
            }
            catch (Exception ce)
            {
                return false;
            }
        }

        public bool SaveExcludeBrands(List<String> listOfBrands)
        {
            bool isListEmpty = listOfBrands == null ? true : listOfBrands.Count == 0;
            if (isListEmpty)
            {
                foreach (var item in _context.ExcludeBrands.ToList())
                {
                    _context.ExcludeBrands.Remove(item);
                }
            }
            else
            {
                foreach (string brandName in listOfBrands)
                {
                    if (brandName == null) continue;
                    ExcludeBrands excludeBrand = new ExcludeBrands();
                    excludeBrand.BrandName = brandName;
                    _context.ExcludeBrands.Add(excludeBrand);
                }
            }
            int nRet = _context.SaveChanges();



            return nRet > 0;
        }

        public List<String> GetExcludeBrands()
        {
            try
            {
                var returnvalue = from item in _context.ExcludeBrands.ToList() select item.BrandName;
                return returnvalue.ToList();
            }
            catch (Exception ce)
            {
                return null;
            }
        }

        // Move forward from the forDate and find a set of Prices which were recently uploaded..
        private DateTime? GetFirstDailyPriceDate(DateTime forDate)
        {
            using (var db = new RepositoryContext())
            {
                /*var priceDates = db.DailyPrices.Include(x => x.DailyUpload)
                    .Where(x => x.DailyUpload.Status.Id == 10
                    && DbFunctions.DiffDays(forDate, x.DailyUpload.UploadDateTime) == 0)
                    .DistinctBy(x => x.DailyUpload.UploadDateTime).OrderBy(x => x.DailyUpload.UploadDateTime).Take(5); // ascending order*/

                var priceDates = db.DailyPrices.Where(x => x.DailyUpload.Status.Id == 10 && DbFunctions.DiffDays(forDate, x.DateOfPrice) == 0).OrderBy(x => x.DateOfPrice).Take(5);

                // success status
                //.Select(x => x.DailyUpload.UploadDateTime)
                //.Where(x => DbFunctions.DiffDays(x, forDate) >= 1) // UploadDate - forDate >= 1
                if (priceDates.Any())
                {
                    return priceDates.First().DateOfPrice;
                }
                return null;
            }
        }

        /// <summary>
        /// Get DailyPrices for the upload date specified
        /// </summary>
        /// <param name="forUploadDate"></param>
        /// <returns></returns>
        private List<DailyPrice> GetDailyPricesForDate(DateTime forUploadDate)
        {
            List<DailyPrice> retval = new List<DailyPrice>();
            using (var db = new RepositoryContext())
            {
                var prices = db.DailyPrices.Where(x => DbFunctions.DiffDays(forUploadDate, x.DateOfPrice) == 0 && x.DailyUpload.StatusId == 10).OrderByDescending(x => x.Id);
                
                retval.AddRange(prices);
                return retval;
            }
        }

        private static int GetSitePriceForFuel(SitePriceViewModel sitePrice, int fuelId)
        {
            var fuelPrice = sitePrice.FuelPrices.FirstOrDefault(x => x.FuelTypeId == fuelId);
            return (fuelPrice == null)
                ? 0
                : (fuelPrice.TodayPrice.HasValue) ? fuelPrice.TodayPrice.Value : 0;
        }

        private static int GetSitePriceOnDate(IEnumerable<SitePrice> sitePrices, DateTime d, int fuelId)
        {
            var price = sitePrices.FirstOrDefault(x => x.DateOfPrice.Equals(d) && x.FuelTypeId == fuelId);
            return (price == null)
                ? 0
                : (price.OverriddenPrice == 0) ? price.SuggestedPrice : price.OverriddenPrice;
        }

        private static int GetDailyPriceOnDate(IEnumerable<DailyPrice> dailyPrices, DateTime d, int fuelId)
        {
            var price = dailyPrices.FirstOrDefault(x => x.DateOfPrice.Date.Equals(d) && x.FuelTypeId == fuelId);
            return (price == null)
                ? 0
                : price.ModalPrice;
        }

        private static int Count(IEnumerable<SiteToCompetitor> brandCompetitors, float min, float max)
        {
            var result = 0;
            if (brandCompetitors != null)
            {
                result = brandCompetitors.Count(x => x.DriveTime >= min && x.DriveTime <= max);
            }
            return result;
        }

        private int NormalisedCount(
            IEnumerable<SiteToCompetitor> brandCompetitors /*e.g. All ASDAs */,
            Dictionary<string, int> normalisedBrandCompetitors,
            float min, float max)
        {
            int result = 0;

            // e.g. All ASDAs within min - max drive time
            foreach (var brandCompetitor in brandCompetitors.Where(x => x.DriveTime >= min && x.DriveTime <= max))
            {
                var key = string.Format("{0}_{1}_{2}", brandCompetitor.CompetitorId, min, max);

                if (normalisedBrandCompetitors.ContainsKey(key) == false)
                {
                    normalisedBrandCompetitors.Add(key, 1);
                    result++;
                }
            }
            return result;
        }

        private ICollection<SitePrice> transformToPrices(List<DailyPrice> dailyPrices, Site site)
        {
            List<SitePrice> result = new List<SitePrice>();

            Parallel.ForEach(dailyPrices,dp => result.Add(new SitePrice
            {
                SiteId = site.Id,
                FuelTypeId = dp.FuelTypeId,
                FuelType = dp.FuelType,
                DateOfCalc = dp.DailyUpload.UploadDateTime.Date,
                DateOfPrice = dp.DateOfPrice.Date,
                SuggestedPrice = dp.ModalPrice
            }));

            return result;
        }



    }
}