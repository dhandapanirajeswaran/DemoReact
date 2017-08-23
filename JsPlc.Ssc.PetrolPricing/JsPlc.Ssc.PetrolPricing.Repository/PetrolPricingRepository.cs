﻿using JsPlc.Ssc.PetrolPricing.Core;
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
using JsPlc.Ssc.PetrolPricing.Core.Interfaces;
using EntityState = System.Data.Entity.EntityState;

using JsPlc.Ssc.PetrolPricing.Core.Diagnostics;
using JsPlc.Ssc.PetrolPricing.Core.ExtensionMethods;
using JsPlc.Ssc.PetrolPricing.Repository.Dapper;
using Dapper;
using JsPlc.Ssc.PetrolPricing.Repository.Debugging;
using JsPlc.Ssc.PetrolPricing.Core.Settings;
using JsPlc.Ssc.PetrolPricing.Models.ViewModels.UserPermissions;
using JsPlc.Ssc.PetrolPricing.Models.ViewModels.Diagnostics;
using System.IO;
using JsPlc.Ssc.PetrolPricing.Models.ViewModels.SelfTest;
using JsPlc.Ssc.PetrolPricing.Models.ViewModels.SystemSettings;
using JsPlc.Ssc.PetrolPricing.Models.ViewModels.Schedule;
using JsPlc.Ssc.PetrolPricing.Models.WindowsService;

namespace JsPlc.Ssc.PetrolPricing.Repository
{
    public class PetrolPricingRepository : IPetrolPricingRepository
    {
        private const string SainsburysCompanyName = "J SAINSBURY PLC";

        private enum ReportTypes
        {
            Default,
            NormalisedMax
        }

        private static List<string> Grocers = new List<string>()
        {
            Const.SAINSBURYS,
            Const.ASDA,
            Const.TESCO,
            Const.MORRISONS
        };

        private readonly RepositoryContext _context;

        private List<string> LstOfBandsToRemoveInNA2;

        private readonly ILogger _logger;


        public PetrolPricingRepository(RepositoryContext context)
        {
            _context = context;
            _logger = new PetrolPricingLogger();
            AddListOfBandsToRemoveInNA2();
        }

        private void AddListOfBandsToRemoveInNA2()
        {
            LstOfBandsToRemoveInNA2 = new List<string>
            {
                Const.WCF,
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
                //Const.TESCOEXPRESS,
                //Const.TESCOEXTRA,
                Const.COOKE,
                Const.HELTOR,
                Const.LOCALFUELS

            };

        }

        private static object cachedAppSettingsLock = new Object();

        public PPUserList GetPPUsers()
        {
            return new PPUserList()
            {
                Users = _context.PPUsers.OrderBy(x => x.FirstName).ToArray(),
                ErrorMessage = "",
                SuccessMessage = ""
            };
        }

        public PPUserList AddPPUser(PPUser ppuser)
        {
            var success = "";
            var failure = "";
            var selectedUserId = 0;

            if (_context.PPUsers.Any(x => x.Email == ppuser.Email))
            {
                failure = String.Format("User with the email \"{0}\" already exists", ppuser.Email);
            }
            else
            {
                try
                {
                    var result = _context.PPUsers.Add(ppuser);
                    _context.SaveChanges();

                    selectedUserId = result.Id;
                    success = String.Format("Added User with email \"{0}\"", ppuser.Email);

                    _context.CreateDefaultUserPermissionsForNewUser(selectedUserId, 0);
                }
                catch (Exception ex)
                {
                    failure = "Unable to add user";
                }
            }
            var userList = this.GetPPUsers();
            userList.SelectedUserId = selectedUserId;
            userList.SuccessMessage = success;
            userList.ErrorMessage = failure;

            return userList;
        }

        public PPUserList DeletePPUser(string email)
        {
            var success = "";
            var failure = "";

            var ppUser = _context.PPUsers.FirstOrDefault(x => x.Email == email);
            if (ppUser != null)
            {
                try
                {
                    _context.PPUsers.Remove(ppUser);
                    _context.SaveChanges();

                    _context.DeleteUserPermissions(ppUser.Id);

                    success = String.Format("Deleted User with email \"{0}\"", ppUser.Email);
                }
                catch (Exception ex)
                {
                    failure = String.Format("Unable to delete user with email \"{0}\"", ppUser.Email);
                }
            }
            else
            {
                failure = String.Format("User with email \"{0}\" does not exist", email);
            }
            PPUserList userList = this.GetPPUsers();
            userList.SuccessMessage = success;
            userList.ErrorMessage = failure;
            return userList;
        }

        public PPUserDetails GetPPUserDetails(string userName)
        {
            var ppUserId = 0;
            if (!String.IsNullOrEmpty(userName)) {
                var ppUser = _context.PPUsers.FirstOrDefault(x => x.Email == userName);
                ppUserId = ppUser.Id;
            }
            return GetPPUserDetails(ppUserId);
        }

        public PPUserDetails GetPPUserDetails(int id)
        {
            var userDetails = new PPUserDetails()
            {
                Status = new GenericStatus(),
                User = _context.PPUsers.FirstOrDefault(x => x.Id == id),
                Permissions = _context.GetUserPermissions(id)
            };
            return userDetails;
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
                .Select(s => new {Count = s.Count(), CompanyName = s.Key})
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
                .Where(x => x.Prices.All(p => p.DateOfCalc.Equals(forDate)));
        }

        private static object cachedCompetitorsLock = new Object();

        public Dictionary<int, Site> GetSitesWithCompetitors()
        {
            Dictionary<int, Site> cachedCompetitors =
                PetrolPricingRepositoryMemoryCache.CacheObj.Get("GetSitesWithCompetitors") as Dictionary<int, Site>;

            if (cachedCompetitors == null)
            {
                lock (cachedCompetitorsLock)
                {
                    cachedCompetitors =
                        PetrolPricingRepositoryMemoryCache.CacheObj.Get("GetSitesWithCompetitors") as
                            Dictionary<int, Site>;

                    if (cachedCompetitors == null)
                    {
                        cachedCompetitors = _context.Sites
                            .Include(x => x.Competitors)
                            .Where(x => x.IsActive)
                            .OrderBy(q => q.Id).ToDictionary(k => k.Id, v => v);

                        PetrolPricingRepositoryMemoryCache.CacheObj.Add("GetSitesWithCompetitors", cachedCompetitors,
                            PetrolPricingRepositoryMemoryCache.ReportsCacheExpirationPolicy(20));
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
                DiffFromDays = DbFunctions.DiffDays(fromPriceDate.Value, x.DateOfCalc),
                DiffToDays = DbFunctions.DiffDays(x.DateOfCalc, toPriceDate.Value),
                FromDate = fromPriceDate.Value,
                DateOfCalc = x.DateOfCalc,
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
                site.Emails.ForEach(x => emails.Add(x.EmailAddress));
                _context.Entry(site).State = EntityState.Detached;

                var prices = getPricesForSite(site.Id);
                site.Prices = new List<SitePrice>();
                site.Prices = prices;

                emails.ForEach(x => site.Emails.Add(new SiteEmail
                {
                    EmailAddress = x
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
            var retval = brandName == "All"
                ? sites_tmp.Where(x => x.IsActive).OrderBy(q => q.SiteName).ToList()
                : sites_tmp
                    .Where(x => x.IsActive && x.Brand == brandName)
                    .OrderBy(q => q.SiteName).ToList();

            int daysBetweenFromAndTo =
                Convert.ToInt32((toPriceDate.Value - fromPriceDate.Value).TotalDays);

            var fileUploads = _context.FileUploads
                .Where(
                    x =>
                        DbFunctions.TruncateTime(x.UploadDateTime) >= fromPriceDate.Value &&
                        DbFunctions.TruncateTime(x.UploadDateTime) <= toPriceDate.Value)
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
                    _logger.Error(ce);
                    string str = ce.InnerException.Message;
                }
                //_context.Entry(site).State = EntityState.Detached;
            });


            return sites;
        }

        public IEnumerable<SitePriceViewModel> GetSitesWithPrices(DateTime forDate, string storeName = "", int catNo = 0,
            int storeNo = 0, string storeTown = "", int siteId = 0, int pageNo = 1,
            int pageSize = Constants.PricePageSize, bool pricesForCalc = false)
        {
            Task<IEnumerable<SitePriceViewModel>> task = Task<IEnumerable<SitePriceViewModel>>.Factory.StartNew(() =>
            {

                return CallSitePriceSproc(forDate, storeName, catNo, storeNo, storeTown, siteId, pageNo, pageSize, pricesForCalc);
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
                var cachedCompetitorsWithPrices = CallCompetitorsWithPriceSproc(forDate, siteId, pageNo, pageSize);
                //   PetrolPricingRepositoryMemoryCache.CacheObj.Get(key) as IEnumerable<SitePriceViewModel>;

                /* if (cachedCompetitorsWithPrices == null)
                {
                    lock (cachedCompetitorsLock)
                    {`
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
                }*/


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
            int pageSize = Constants.PricePageSize,
            bool pricesForCalc = false)
        {

            try
            {
                _logger.Debug("Started CallSitePriceSproc");

                var sainsburysSites =
                    _context.Sites.Where(x => x.IsSainsburysSite == true && x.IsActive == true);

                if (!String.IsNullOrEmpty(storeName))
                    sainsburysSites =
                        sainsburysSites.Where(x => x.SiteName.ToLower().Contains(storeName.Trim().ToLower()));

                if (catNo != 0)
                    sainsburysSites = sainsburysSites.Where(x => x.CatNo == catNo);

                if (storeNo != 0)
                    sainsburysSites = sainsburysSites.Where(x => x.StoreNo == storeNo);

                if (siteId != 0)
                    sainsburysSites = sainsburysSites.Where(x => x.Id == siteId);

                if (!String.IsNullOrEmpty(storeTown))
                    sainsburysSites =
                        sainsburysSites.Where(x => x.Town.ToLower().Contains(storeTown.Trim().ToLower()));

                var filteredSainsburysSites = sainsburysSites.Include(x => x.Emails).OrderBy(x => x.SiteName).ToList();


                SitePriceViewModel sitePriceRow = null;

                var fileUploadedObj =
                    _context.FileUploads.Where(
                            x =>
                                x.UploadDateTime.Day == forDate.Day
                                && x.UploadDateTime.Month == forDate.Month
                                && x.UploadDateTime.Year == forDate.Year
                                && x.UploadTypeId == 1)
                        .OrderByDescending(x => x.Id)
                        .FirstOrDefault();

                var catalistFileExits = fileUploadedObj != null;

                var fuelPriceSettings = GetAllFuelPriceSettings();

                var dbList = new List<SitePriceViewModel>();
                foreach (Site site in filteredSainsburysSites)
                {
                    sitePriceRow = new SitePriceViewModel()
                    {
                        SiteId = site.Id,
                        CatNo = site.CatNo,
                        StoreName = site.SiteName,
                        Address = site.Address,
                        Town = site.Town,
                        PfsNo = site.PfsNo,
                        StoreNo = site.StoreNo,
                        FuelPrices = new List<FuelPriceViewModel>(),
                        Notes = site.Notes,
                        HasEmails = site.Emails.Any(),
                        PriceMatchType = (PriceMatchType)site.PriceMatchType,
                        Emails = site.Emails.Select(x => x.EmailAddress).ToList()
                    };

                    dbList.Add(sitePriceRow);
                }

                var settings = _context.SystemSettings.FirstOrDefault();

                var nearbyGrocerStatuses = GetNearbyGrocerPriceStatus(forDate, dbList, settings.MaxGrocerDriveTimeMinutes);

                var maxCompetitorDriveTime = 25; // 25 mins for ALL competitors (not just Grocers)

                SetSiteCompetitorPriceInformation(forDate, dbList, maxCompetitorDriveTime);

                _logger.Debug("Started: AddFuelPricesRowsForSites");
                AddFuelPricesRowsForSites(forDate, dbList, pricesForCalc);
                _logger.Debug("Finished: AddFuelPricesRowsForSites");

                var systemSettings = _context.SystemSettings.FirstOrDefault();

                // Apply the Grocer, Decimal Rounding and Price Variance rules
                ApplyGrocerRoundingAndPriceVarianceRules(forDate, dbList, nearbyGrocerStatuses, systemSettings);

                // using test (NON-LIVE) email addresses ?
                if (systemSettings.EnableSiteEmails == false)
                {
                    var testEmailAddresses = systemSettings.SiteEmailTestAddresses.Split(';').ToList();

                    foreach(var site in dbList)
                    {
                        site.HasEmails = testEmailAddresses.Any();
                        site.Emails = testEmailAddresses;
                    }
                }

                _logger.Debug("Finished CallSitePriceSproc");

                return dbList;
            }
            catch (Exception ce)
            {
                _logger.Debug("Crashed: CallSitePriceSproc");
                _logger.Error(ce);
                return null;
            }
        }

        private void ApplyGrocerRoundingAndPriceVarianceRules(DateTime forDate,
            List<SitePriceViewModel> dbList,
            IEnumerable<NearbyGrocerPriceSiteStatus> nearbyGrocerStatuses,
            SystemSettings systemSettings
            )
        {
            // historical data ?
            if (forDate.Date != DateTime.Now.Date)
                return;

            foreach (var site in dbList)
            {
                var siteGrocer = nearbyGrocerStatuses.FirstOrDefault(x => x.SiteId == site.SiteId);

                var wasUnleadedSnappedBackToTodayPrice = false;

                foreach (var siteFuel in site.FuelPrices)
                {
                    var grocerStatus = NearbyGrocerStatuses.None;
                    if (siteGrocer != null) {
                        if (siteFuel.FuelTypeId == (int)FuelTypeItem.Unleaded)
                            grocerStatus = siteGrocer.Unleaded;
                        else if (siteFuel.FuelTypeId == (int)FuelTypeItem.Diesel)
                            grocerStatus = siteGrocer.Diesel;
                        else if (siteFuel.FuelTypeId == (int)FuelTypeItem.Super_Unleaded)
                            grocerStatus = siteGrocer.SuperUnleaded;
                    }

                    var todayPrice = siteFuel.TodayPrice.HasValue ? siteFuel.TodayPrice.Value : 0;
                    var autoPrice = siteFuel.AutoPrice.HasValue ? siteFuel.AutoPrice.Value : 0;

                    if (todayPrice == 0 || autoPrice == 0)
                        continue;

                    // Grocers but incomplete data ?
                    if (grocerStatus.HasFlag(NearbyGrocerStatuses.HasNearbyGrocers) && !grocerStatus.HasFlag(NearbyGrocerStatuses.AllGrocersHavePriceData))
                    {
                        // are we higher than cheapest competitor ?
                        if (autoPrice > todayPrice)
                        {
                            // use today's price
                            autoPrice = todayPrice;
                            if (siteFuel.FuelTypeId == (int)FuelTypeItem.Unleaded)
                                wasUnleadedSnappedBackToTodayPrice = true;
                        }
                    }

                    // apply decimal rounding (if any)
                    if (systemSettings.DecimalRounding != -1)
                    {
                        autoPrice = (int)(autoPrice / 10) * 10 + systemSettings.DecimalRounding;
                    }

                    var diff = autoPrice - todayPrice;
                    if (Math.Abs(diff) <= systemSettings.PriceChangeVarianceThreshold)
                    {
                        // within Price Variance, so use today's price
                        autoPrice = todayPrice;
                        if (siteFuel.FuelTypeId == (int)FuelTypeItem.Unleaded)
                            wasUnleadedSnappedBackToTodayPrice = true;
                    }

                    // store auto price and recalculate difference
                    siteFuel.AutoPrice = (int?)autoPrice;
                    siteFuel.Difference = (int?)(autoPrice - todayPrice);
                }

                if (wasUnleadedSnappedBackToTodayPrice)
                {
                    var unleaded = site.FuelPrices.FirstOrDefault(x => x.FuelTypeId == (int)FuelTypeItem.Unleaded);
                    var superUnleaded = site.FuelPrices.FirstOrDefault(x => x.FuelTypeId == (int)FuelTypeItem.Super_Unleaded);

                    if (unleaded != null && superUnleaded != null)
                    {
                        superUnleaded.AutoPrice = (int?)unleaded.AutoPrice.Value + systemSettings.SuperUnleadedMarkupPrice;

                        // get Super-Unleaded TodayPrice (if any)
                        var superTodayPrice = superUnleaded.TodayPrice.HasValue
                            ? superUnleaded.TodayPrice.Value
                            : 0;

                        // recalc diff
                        superUnleaded.Difference = (int?)superUnleaded.AutoPrice - superTodayPrice;
                    }
                }
            }
        }

        private void SetSiteCompetitorPriceInformation(DateTime forDate, List<SitePriceViewModel> sites, int maxGrocerDriveTimeMinutes)
        {
            if (sites == null || !sites.Any())
                return;

            var siteIds = sites.Select(x => x.SiteId.ToString()).Aggregate((x, y) => x + "," + y);

            IEnumerable<SiteCompetitorPriceSummaryRowViewModel> priceSummaries = _context.GetCompetitorInfoForSites(forDate, siteIds, maxGrocerDriveTimeMinutes);

            // calculate percentages
            foreach(var summary in priceSummaries)
            {
                summary.CompetitorPricePercent = summary.CompetitorCount == 0
                    ? 0
                    : summary.CompetitorPriceCount * 100 / summary.CompetitorCount;

                summary.GrocerPricePercent = summary.GrocerCount == 0
                    ? 0
                    : summary.GrocerPriceCount * 100 / summary.GrocerCount;
            }

            // attach each Fuel Summary to each Site
            foreach(var site in sites)
            {
                var fuelsForSite = priceSummaries.Where(x => x.SiteId == site.SiteId);
                var unleaded = fuelsForSite.FirstOrDefault(x => x.FuelTypeId == FuelTypeItem.Unleaded);
                var diesel = fuelsForSite.FirstOrDefault(x => x.FuelTypeId == FuelTypeItem.Diesel);
                var superUnleaded = fuelsForSite.FirstOrDefault(x => x.FuelTypeId == FuelTypeItem.Super_Unleaded);

                // add in set Order of Unleaded, Diesel, Super-unleaded
                site.SiteCompetitorsInfo.PriceSummaries.Add(unleaded);
                site.SiteCompetitorsInfo.PriceSummaries.Add(diesel);
                site.SiteCompetitorsInfo.PriceSummaries.Add(superUnleaded);
            }
        }

        private IEnumerable<NearbyGrocerPriceSiteStatus> GetNearbyGrocerPriceStatus(DateTime forDate, IEnumerable<SitePriceViewModel> sites, int driveTime)
        {
            if (sites == null || !sites.Any())
                return new List<NearbyGrocerPriceSiteStatus>();

            var siteIds = sites.Select(x => x.SiteId.ToString()).Aggregate((x, y) => x + "," + y);

            var allGrocerFuelStatuses = GetNearbyGrocerPriceStatusForSites(forDate, siteIds, driveTime);
            foreach(var status in allGrocerFuelStatuses)
            {
                var site = sites.First(x => x.SiteId == status.SiteId);

                // unleaded
                site.HasNearbyUnleadedGrocers = status.Unleaded.HasFlag(NearbyGrocerStatuses.HasNearbyGrocers);
                site.HasNearbyUnleadedGrocersPriceData = status.Unleaded.HasFlag(NearbyGrocerStatuses.AllGrocersHavePriceData);
                // diesel
                site.HasNearbyDieselGrocers = status.Diesel.HasFlag(NearbyGrocerStatuses.HasNearbyGrocers);
                site.HasNearbyDieselGrocersPriceData = status.Diesel.HasFlag(NearbyGrocerStatuses.AllGrocersHavePriceData);
                // super-unleaded
                site.HasNearbySuperUnleadedGrocers = status.SuperUnleaded.HasFlag(NearbyGrocerStatuses.HasNearbyGrocers);
                site.HasNearbySuperUnleadedGrocersPriceData = status.SuperUnleaded.HasFlag(NearbyGrocerStatuses.AllGrocersHavePriceData);
            }
            return allGrocerFuelStatuses;
        }

        public IEnumerable<NearbyGrocerPriceSiteStatus> GetNearbyGrocerPriceStatusForSites(DateTime forDate, string siteIds, int driveTime)
        {
            return _context.GetNearbyGrocerPriceStatusForSites(forDate, siteIds, driveTime);
        }

        public StatusViewModel RemoveAllSiteEmailAddresses()
        {
            var status = _context.RemoveAllSiteEmailAddresses();
            return new StatusViewModel()
            {
                SuccessMessage = status == 0 ? "Removed all Sainsburys Site Email Addresses" : "",
                ErrorMessage = status != 0 ? "Unable to remove Site Email Addresses" : ""
            };
        }

        public IEnumerable<SiteEmailAddressViewModel> GetSiteEmailAddresses(int siteId=0)
        {
            var emailAddresses = _context.GetSiteEmailAddresses(siteId);
            return emailAddresses;
        }

        public StatusViewModel UpsertSiteEmailAddresses(IEnumerable<SiteEmailImportViewModel> emailAddresses)
        {
            var status = _context.UpsertSiteEmailAddresses(emailAddresses);
            return new StatusViewModel()
            {
                SuccessMessage = status == 0 ? "Updated Site Email Addresses" : "",
                ErrorMessage = status != 0 ? "Unable to update Site Email Addresses" : ""
            };
        }

        public bool UpsertSiteCatNoAndPfsNos(IEnumerable<SiteNumberImportViewModel> siteNumbers)
        {
            return _context.UpsertSiteCatNoAndPfsNos(siteNumbers) == 0;
        }

        public void FixZeroSuggestedSitePricesForDay(DateTime forDate)
        {
            _context.FixZeroSuggestedSitePricesForDay(forDate);
        }

        public IEnumerable<HistoricalPriceViewModel> GetHistoricPricesForSite(int siteId, DateTime startDate, DateTime endDate)
        {
            return _context.GetHistoricPricesForSite(siteId, startDate, endDate);
        }

        public void RebuildBrands()
        {
            _context.RebuildBrands();
        }

        public FileUploadAttemptStatus ValidateUploadAttempt(int uploadType, DateTime uploadDate)
        {
            return _context.ValidateUploadAttempt(uploadType, uploadDate);
        }

        public void RebuildSiteAttributes(int? siteId)
        {
            _context.RebuildSiteAttributes(siteId);
        }

        public SitePriceViewModel GetTodayPricesForCalcPrice(DateTime forDate, int siteId)
        {
            var pricesForCalc = true;
            return GetSitesWithPrices(forDate, "", 0, 0, "", siteId, 1, Constants.PricePageSize, pricesForCalc).FirstOrDefault();

            //return _context.GetTodayPricesForCalcPrice(forDate, siteId);
        }

        private void AddFuelPricesRowsForSites(DateTime forDate, List<SitePriceViewModel> sites, bool pricesForCalc)
        {
            if (sites == null || !sites.Any())
                return;

            List<FuelPriceViewModel> calculatedPrices = new List<FuelPriceViewModel>();

            var siteIds = sites.Select(x => x.SiteId.ToString()).Aggregate((x, y) => x + "," + y);

            if (pricesForCalc)
            {
                DiagnosticLog.StartDebug("AddFuelPricesRowsForSites - Calling SP - pricesForCalc");

                calculatedPrices = _context.GetTodayPricesForCalcPrice(forDate, sites.First().SiteId);

                DiagnosticLog.StartDebug("AddFuelPricesRowsForSites - Finished SP - pricesForCalc");
            }
            else
            {
                DiagnosticLog.StartDebug("AddFuelPricesRowsForSites - Calling SP");

                calculatedPrices = _context.CalculateFuelPricesForSitesAndDate(forDate, siteIds);

                DiagnosticLog.StartDebug("AddFuelPricesRowsForSites - Finished SP");
            }

            foreach (var site in sites)
            {
                var fuelPricesForSite = calculatedPrices.Where(x => x.SiteId == site.SiteId);

                var superUnleaded = fuelPricesForSite.FirstOrDefault(x => x.FuelTypeId == (int)FuelTypeItem.Super_Unleaded);
                var unleaded = fuelPricesForSite.FirstOrDefault(x => x.FuelTypeId == (int)FuelTypeItem.Unleaded);
                var diesel = fuelPricesForSite.FirstOrDefault(x => x.FuelTypeId == (int)FuelTypeItem.Diesel);

                if (unleaded != null)
                {
                    unleaded.HasNearbyCompetitorPrice = site.HasNearbyUnleadedGrocersPriceData;
                    unleaded.HasNearbyCompetitorWithOutPrice = site.HasNearbyUnleadedGrocersPriceData == false;
                } else
                {
                    unleaded = new FuelPriceViewModel()
                    {
                        FuelTypeId = (int)FuelTypeItem.Unleaded,
                        SiteId = site.SiteId
                    };
                }

                if (superUnleaded != null)
                {
                    superUnleaded.HasNearbyCompetitorPrice = site.HasNearbySuperUnleadedGrocersPriceData;
                    superUnleaded.HasNearbyCompetitorWithOutPrice = site.HasNearbySuperUnleadedGrocersPriceData == false;
                } else
                {
                    superUnleaded = new FuelPriceViewModel()
                    {
                        FuelTypeId = (int)FuelTypeItem.Super_Unleaded,
                        SiteId = site.SiteId
                    };
                }

                if (diesel != null)
                {
                    diesel.HasNearbyCompetitorPrice = site.HasNearbyDieselGrocersPriceData;
                    diesel.HasNearbyCompetitorWithOutPrice = site.HasNearbyDieselGrocersPriceData == false;
                } else
                {
                    diesel = new FuelPriceViewModel()
                    {
                        FuelTypeId = (int)FuelTypeItem.Diesel,
                        SiteId = site.SiteId
                    };
                }

                site.FuelPrices.Add(superUnleaded);
                site.FuelPrices.Add(unleaded);
                site.FuelPrices.Add(diesel);
            }
        }

        private FuelPriceSettings GetAllFuelPriceSettings()
        {
            var allFuels = _context.GetAllFuelPriceSettings();
            var settings = new FuelPriceSettings()
            {
                AllFuels = allFuels
            };
            return settings;
        }

        private void AddSitePriceRow(FuelTypeItem fuelType, Site site, int trialPrice, DateTime forDate,
            bool doesCatalistFileExist, List<FuelPriceViewModel> list, FuelPriceSettings fuelPriceSettings)
        {
            int fuelMarkup = 0;
            var orgFuelTypeID = fuelType;

            if (fuelType == FuelTypeItem.Super_Unleaded)
            {
                fuelType = FuelTypeItem.Unleaded;
                fuelMarkup = fuelPriceSettings.SuperUnleaded.Markup;
            }

            var sitePriceData =
                _context.Set<SitePrice>()
                    .Where(
                        x =>
                            (x.SuggestedPrice > 0 || x.OverriddenPrice > 0)
                            && x.SiteId == site.Id
                            && (x.FuelTypeId == (int)fuelType))
                    .OrderByDescending(item => item.Id)
                    .FirstOrDefault();


            if (sitePriceData != null)
            {
                var autoPrice = sitePriceData.SuggestedPrice + fuelMarkup;
                //autoPrice = SetLastDigitTo9(autoPrice);

                var overridePrice = 0;

                var dieselPriceOverride =
                    _context.Set<SitePrice>()
                        .Where(
                            x =>
                                x.OverriddenPrice > 0 
                                && x.SiteId == site.Id 
                                && x.FuelTypeId == (int)orgFuelTypeID 
                                && x.DateOfPrice.Day == forDate.Day 
                                && x.DateOfPrice.Month == forDate.Month 
                                && x.DateOfPrice.Year == forDate.Year)
                        .OrderBy(item => item.Id).FirstOrDefault();

                if (dieselPriceOverride != null)
                {
                    overridePrice = dieselPriceOverride.OverriddenPrice;
                    overridePrice += trialPrice;
                    //overridePrice = SetLastDigitTo9(overridePrice);
                }

                //today Price Calculation
                var todayPriceFromCalculation = GetTodayPrice(fuelType, site, forDate);

                var todayPrice = todayPriceFromCalculation + fuelMarkup;
                //todayPrice = SetLastDigitTo9(todayPrice);

                var competitorId = sitePriceData.CompetitorId.HasValue ? sitePriceData.CompetitorId.Value : 0;
                var competitorName = "Unknown";
                if (competitorId > 0)
                {
                    var competitorSite = GetSite(competitorId);
                    competitorName = string.Format("{0}/{1}", competitorSite.Brand, competitorSite.SiteName);
                }
                list.Add(new FuelPriceViewModel
                {
                    FuelTypeId = (int)orgFuelTypeID,
                    AutoPrice = doesCatalistFileExist ? autoPrice : 0,
                    OverridePrice = overridePrice,

                    TodayPrice = todayPrice,
                    Markup = sitePriceData.Markup,
                    CompetitorName = competitorName,
                    IsTrailPrice = sitePriceData.IsTrailPrice,
                    CompetitorPriceOffset = site.CompetitorPriceOffset
                    //  IsBasedOnCompetitor = trailPriceCompetitorId.HasValue
                });
            }
            else
            {
                list.Add(new FuelPriceViewModel
                {
                    FuelTypeId = (int)orgFuelTypeID,
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

        private int SetLastDigitTo9(int value)
        {
            return (value / 10) * 10 + 9;
        }

        private int GetTodayPrice(FuelTypeItem fuelType, Site site, DateTime forDate)
        {
            //Latest Price DateTime
            var latestPrice =
                _context.LatestPrices.FirstOrDefault(
                        x => x.PfsNo == site.PfsNo.Value
                        && x.StoreNo == site.StoreNo.Value
                        && x.FuelTypeId == (int)fuelType);

            var overridepriceIfAny = _context.Set<SitePrice>()
                .Where(
                    x =>
                        (x.OverriddenPrice > 0)
                        && x.SiteId == site.Id
                        && (x.FuelTypeId == (int)fuelType)
                        )
                .OrderByDescending(item => item.Id)
                .FirstOrDefault();

            var todayPriceSortByDate = _context.Set<DailyPrice>()
                .Where(
                    x =>
                        (x.ModalPrice > 0)
                        && x.CatNo == site.CatNo.Value
                        && (x.FuelTypeId == (int)fuelType))
                .OrderByDescending(item => item.Id)
                .FirstOrDefault();

            FileUpload fileUpload = latestPrice != null
                ? fileUpload = _context.FileUploads.FirstOrDefault(x => x.Id == latestPrice.UploadId)
                : null;

            if (todayPriceSortByDate == null && overridepriceIfAny == null && latestPrice != null)
            {
                return latestPrice.ModalPrice;
            }
            else if (todayPriceSortByDate == null && overridepriceIfAny != null && latestPrice == null)
            {
                return overridepriceIfAny.OverriddenPrice;
            }
            else if (todayPriceSortByDate != null && overridepriceIfAny == null && latestPrice == null)
            {
                return todayPriceSortByDate.ModalPrice;
            }
            else if (todayPriceSortByDate != null && overridepriceIfAny != null && latestPrice == null)
            {
                return todayPriceSortByDate.DateOfPrice > overridepriceIfAny.DateOfPrice
                    ? todayPriceSortByDate.ModalPrice
                    : overridepriceIfAny.OverriddenPrice;
            }
            else if (todayPriceSortByDate == null && overridepriceIfAny != null && latestPrice != null)
            {
                return fileUpload.UploadDateTime > overridepriceIfAny.DateOfPrice
                    ? latestPrice.ModalPrice
                    : overridepriceIfAny.OverriddenPrice;
            }
            else if (todayPriceSortByDate != null && overridepriceIfAny == null && latestPrice != null)
            {
                return todayPriceSortByDate.DateOfPrice > fileUpload.UploadDateTime
                    ? todayPriceSortByDate.ModalPrice
                    : latestPrice.ModalPrice;
            }
            else if (todayPriceSortByDate != null && overridepriceIfAny != null && latestPrice != null)
            {
                if (todayPriceSortByDate.DateOfPrice > fileUpload.UploadDateTime)
                {
                    if (todayPriceSortByDate.DateOfPrice > overridepriceIfAny.DateOfPrice)
                    {
                        return todayPriceSortByDate.ModalPrice;
                    }
                    else if (fileUpload.UploadDateTime > overridepriceIfAny.DateOfPrice)
                    {
                        return latestPrice.ModalPrice;
                    }
                    else
                    {
                        return overridepriceIfAny.OverriddenPrice;
                    }
                }
                else
                {
                    if (fileUpload.UploadDateTime > overridepriceIfAny.DateOfPrice)
                    {
                        return latestPrice.ModalPrice;
                    }
                    else
                    {
                        return overridepriceIfAny.OverriddenPrice;
                    }
                }

            }
            return 0;
        }

        private FileUpload GetLastFileUploadForDateAndUploadType(DateTime forDate, FileUploadTypes uploadType)
        {
            var forDateNextDay = forDate.Date.AddDays(1);

            var fileupload = _context.FileUploads.Where(
                            x =>
                                x.UploadDateTime >= forDate && x.UploadDateTime < forDateNextDay
                                && x.UploadTypeId == (int)uploadType
                                && x.Status.Id == 10)
                                .OrderByDescending(x => x.Id)
                                .FirstOrDefault();

            return fileupload;
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
            return _context.GetCompetitorsWithPriceView(forDate, siteId);
        }

        private void AddCompetitorFuelPrice(
            List<FuelPriceViewModel> lstFuelPriceViewModel, 
            Site JsSite, 
            Site compSite,
            FuelTypeItem fuelType,
            int nOffSet, 
            DateTime forDate, 
            List<LatestCompPrice> latestCompPrices_today, 
            List<DailyPrice> dailypriceList_today,
            List<LatestCompPrice> latestCompPrices_yday, 
            List<DailyPrice> dailypriceList_yday)
        {

            int DailyPrice_today = 0;
            int DailyPrice_yday = 0;
            int LatestPrice_today = 0;
            int LatestPrice_yday = 0;
            if (dailypriceList_today != null)
            {
                var dailyPrice_today = dailypriceList_today.FirstOrDefault(x => x.FuelTypeId == (int)fuelType && x.CatNo == compSite.CatNo.GetValueOrDefault());
                DailyPrice_today = dailyPrice_today != null ? dailyPrice_today.ModalPrice : 0;
            }
            if (dailypriceList_yday != null)
            {
                var dailyPrice_yday = dailypriceList_yday.FirstOrDefault(x => x.FuelTypeId == (int)fuelType && x.CatNo == compSite.CatNo.GetValueOrDefault());
                DailyPrice_yday = dailyPrice_yday != null ? dailyPrice_yday.ModalPrice : 0;
            }

            if (latestCompPrices_today != null)
            {
                var latestPrice_today = latestCompPrices_today.FirstOrDefault(x => x.FuelTypeId == (int)fuelType && x.CatNo == compSite.CatNo.GetValueOrDefault());
                LatestPrice_today = latestPrice_today != null ? latestPrice_today.ModalPrice : 0;
            }

            if (latestCompPrices_yday != null)
            {
                var latestPrice_yday = latestCompPrices_yday.FirstOrDefault(x => x.FuelTypeId == (int)fuelType && x.CatNo == compSite.CatNo.GetValueOrDefault());
                LatestPrice_yday = latestPrice_yday != null ? latestPrice_yday.ModalPrice : 0;
            }

            var todayPrice = LatestPrice_today > 0 ? LatestPrice_today + nOffSet : DailyPrice_today + nOffSet;

            var yesterdayPrice = LatestPrice_yday > 0 ? LatestPrice_yday + nOffSet : DailyPrice_yday + nOffSet;

            lstFuelPriceViewModel.Add(new FuelPriceViewModel
            {
                FuelTypeId = (int)fuelType,

                // Today's prices (whatever was calculated yesterday OR last)
                TodayPrice = todayPrice,

                // Today's prices (whatever was calculated yesterday OR last)
                YestPrice = yesterdayPrice,

                //Difference between yesterday and today
                Difference = todayPrice > 0 && yesterdayPrice > 0 ? todayPrice - yesterdayPrice : 0
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

            //if (dailyPricesCache == null)
            {
                lock (cachedGetDailyPricesForFuelByCompetitorsLock)
                {
                    dailyPricesCache = PetrolPricingRepositoryMemoryCache.CacheObj.Get(cacheKey) as Dictionary<string, DailyPrice>;

                    if (dailyPricesCache == null)
                    {
                        // If multiple uploads, needs to be handled here, but we assume one for now.

                        var fileUpload =
                            _context.FileUploads.Where(
                                x =>
                                    x.UploadDateTime.Month == usingPricesforDate.Month &&
                                    x.UploadDateTime.Day == usingPricesforDate.Day &&
                                    x.UploadDateTime.Year == usingPricesforDate.Year).OrderByDescending(x=>x.Id).ToList();
                        if (fileUpload.Count > 0)
                        {
                            int fileUploadId = fileUpload[0].Id;
                            dailyPricesCache = _context.DailyPrices.Include(x => x.DailyUpload)
                                .Where(
                                    x =>
                                        x.DailyUploadId.Value == fileUploadId)
                                .ToDictionary(k => string.Format("{0}_{1}", k.FuelTypeId, k.CatNo), v => v);

                            PetrolPricingRepositoryMemoryCache.CacheObj.Add(cacheKey, dailyPricesCache,
                                PetrolPricingRepositoryMemoryCache.ReportsCacheExpirationPolicy(20));
                        }
                        else
                        {
                            return new List<DailyPrice>();
                        }
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


        public IEnumerable<LatestCompPrice> GetLatestCompetitorPricesForFuel(IEnumerable<int> competitorCatNos, int fuelId,
           DateTime usingPricesforDate)
        {

            Dictionary<string, LatestCompPrice> latestCompPrices =new Dictionary<string, LatestCompPrice>();

            var fileUpload =
                _context.FileUploads.Where(
                    x =>
                        x.UploadDateTime.Month == usingPricesforDate.Month &&
                        x.UploadDateTime.Day == usingPricesforDate.Day &&
                        x.UploadDateTime.Year == usingPricesforDate.Year && x.UploadTypeId==4).OrderByDescending(x => x.Id).ToList();
            if (fileUpload.Count > 0)
            {
                int fileUploadId = fileUpload[0].Id;
               
                var LatestCP=_context.LatestCompPrices.ToList();
                latestCompPrices = LatestCP.Where(
                        x =>
                            x.UploadId == fileUploadId)
                    .ToDictionary(k => string.Format("{0}_{1}", k.FuelTypeId, k.CatNo), v => v);
                
               
              
            }


            List<LatestCompPrice> result = new List<LatestCompPrice>();

            foreach (var catNo in competitorCatNos)
            {
                var key = string.Format("{0}_{1}", fuelId, catNo);
                if (latestCompPrices.ContainsKey(key))
                {
                    result.Add(latestCompPrices[key]);
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
            // lookup BrandId
            var brand = _context.Brands.FirstOrDefault(x => x.BrandName.ToUpper() == site.Brand.ToUpper());
            site.BrandId = brand == null ? 0 : brand.Id;

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
                // lookup BrandId
                var brand = _context.Brands.FirstOrDefault(x => x.BrandName.ToUpper() == site.Brand.ToUpper());
                site.BrandId = brand == null ? 0 : brand.Id;

                _context.Sites.Attach(site);
                UpdateSiteEmails(site);
                if (site.Competitors != null) UpdateSiteCompetitors(site);
                _context.Entry(site).State = EntityState.Modified;
                int nReturn = _context.SaveChanges();

                // rebuild brands
                _context.RebuildBrands();

                // rebuild site attributes
                _context.RebuildSiteAttributes(site.Id);

                return true;
            }
            catch (Exception ce)
            {
                _logger.Error(ce);
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
                        _logger.Error(e);
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
                        _logger.Error(dbEx);
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
                                        (int)FuelTypeItem.Unleaded, Convert.ToDouble(LatestPriceDataModel.UnleadedPrice));
                                  

                                }
                                if (!String.IsNullOrEmpty(LatestPriceDataModel.SuperUnleadedPrice))
                                {
                                  
                                    AddOrUpdateLatestPrice(newDbContext, LatestPriceDataModel, fileDetails,
                                     (int)FuelTypeItem.Super_Unleaded, Convert.ToDouble(LatestPriceDataModel.SuperUnleadedPrice));
                            

                                }
                                if (!String.IsNullOrEmpty(LatestPriceDataModel.DieselPrice))
                                {
                                     AddOrUpdateLatestPrice(newDbContext, LatestPriceDataModel, fileDetails,
                                   (int)FuelTypeItem.Diesel, Convert.ToDouble(LatestPriceDataModel.DieselPrice));
                                }
                            }

                            newDbContext.SaveChanges();

                            // Run the post file import task
                            newDbContext.RunPostLatestJsFileImportTasks(fileDetails.Id, fileDetails.UploadDateTime);

                            tx.Commit();
                        }
                        return true;
                    }
                    catch (DbUpdateException e)
                    {
                        _logger.Error(e);
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



        public bool NewLatestCompPriceRecords(List<LatestCompPriceDataModel> LatestCompSiteData, FileUpload fileDetails,
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
                        if (LatestCompSiteData.Count > 0)
                        {
                            TruncateLatestCompPrices();
                            foreach (LatestCompPriceDataModel LatestCompPriceDataModel in LatestCompSiteData)
                            {

                                if (!String.IsNullOrEmpty(LatestCompPriceDataModel.UnleadedPrice))
                                {
                                    AddLatestCompPrice(newDbContext, LatestCompPriceDataModel, fileDetails,
                                        (int)FuelTypeItem.Unleaded, Convert.ToDouble(LatestCompPriceDataModel.UnleadedPrice));


                                }

                                if (!String.IsNullOrEmpty(LatestCompPriceDataModel.DieselPrice))
                                {
                                    AddLatestCompPrice(newDbContext, LatestCompPriceDataModel, fileDetails,
                                  (int)FuelTypeItem.Diesel, Convert.ToDouble(LatestCompPriceDataModel.DieselPrice));
                                }
                            }

                            newDbContext.SaveChanges();

                            // Run the post file import tasks
                            newDbContext.RunPostLatestCompetitorsFileImportTasks(fileDetails.Id, fileDetails.UploadDateTime);

                            tx.Commit();
                        }
                        return true;
                    }
                    catch (DbUpdateException e)
                    {
                        _logger.Error(e);
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

        public void AddOrUpdateLatestPrice(RepositoryContext newDbContext,LatestPriceDataModel latestPriceDataModel, FileUpload fileDetails,int fuelTypeId,double fuelPrice)
        {
            LatestPrice dbRecord = null;
            bool iNewRecord = false;
            var latestPriceList = newDbContext.LatestPrices.Where(
                x =>
                    x.PfsNo == latestPriceDataModel.PfsNo &&
                    x.StoreNo == latestPriceDataModel.StoreNo &&
                    x.FuelTypeId == fuelTypeId).ToList();
            dbRecord=latestPriceList.Count>0 ? latestPriceList[0] :
            null;

            if (latestPriceList.Count > 0)
            {
                foreach (var latestprice in latestPriceList)
                {
                    newDbContext.LatestPrices.Remove(latestprice);
                }
           
            }
            dbRecord = new LatestPrice();
            dbRecord.UploadId = fileDetails.Id;
            dbRecord.PfsNo = latestPriceDataModel.PfsNo;
            dbRecord.StoreNo = latestPriceDataModel.StoreNo;
            dbRecord.FuelTypeId = fuelTypeId;
            dbRecord.ModalPrice = (int)(fuelPrice * 10);
            newDbContext.LatestPrices.Add(dbRecord);
            
        }

        public void AddLatestCompPrice(RepositoryContext newDbContext, LatestCompPriceDataModel latestCompPriceDataModel, FileUpload fileDetails, int fuelTypeId, double fuelPrice)
        {
            LatestCompPrice dbRecord = null;
            bool iNewRecord = false;
            dbRecord = new LatestCompPrice();
            dbRecord.UploadId = fileDetails.Id;
            dbRecord.CatNo = latestCompPriceDataModel.CatNo;
            dbRecord.FuelTypeId = fuelTypeId;
            dbRecord.ModalPrice =(int) (fuelPrice * 10);
            newDbContext.LatestCompPrices.Add(dbRecord);

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
                        _logger.Error(e);
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
                        _logger.Error(dbEx);
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

        private void TruncateLatestCompPrices()
        {
            using (var db = new RepositoryContext())
            {
                db.Database.ExecuteSqlCommand("Truncate table LatestCompPrice");
                db.Database.ExecuteSqlCommand("DBCC CHECKIDENT ('LatestCompPrice',RESEED, 0)");

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
                        _logger.Error(ce);
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

            if (cachedAnyDailyPricesForFuelOnDate == null)
            {
                lock (cachedAnyDailyPricesForFuelOnDateLock)
                {
                    cachedAnyDailyPricesForFuelOnDate = PetrolPricingRepositoryMemoryCache.CacheObj.Get(cacheKey) as List<int>;

                    if (cachedAnyDailyPricesForFuelOnDate == null)
                    {
                        cachedAnyDailyPricesForFuelOnDate = _context.DailyPrices
                            .Include(x => x.DailyUpload)
                            .Where(x => DbFunctions.TruncateTime(x.DailyUpload.UploadDateTime) == usingPricesforDate.Date)
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
                .OrderByDescending(x => x.Id)
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
                     && DbFunctions.TruncateTime(x.DateOfCalc) == calculatedSitePrice.DateOfCalc.Date);

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
                    existingPriceRecord.Id);
            }
        }

        public async Task<int> SaveOverridePricesAsync(List<SitePrice> prices, DateTime? forDate = null)
        {
            if (!forDate.HasValue) forDate = DateTime.Now;

            _context.MarkPriceCacheOutdatedForDay(forDate.Value.Date);

            using (var db = new RepositoryContext())
            {
                foreach (SitePrice p in prices)
                {
                    //
                    var dbPricesForDate =
                        db.SitePrices.Where(x => DbFunctions.DiffDays(x.DateOfCalc, forDate) == 0)
                            .AsNoTracking()
                            .ToList();

                    SitePrice p1 = p; // to prevent closure issue
                    var entry =
                        dbPricesForDate.FirstOrDefault(x => x.SiteId == p1.SiteId && x.FuelTypeId == p1.FuelTypeId);

                    // handle -1 for removing Price Override
                    var newOverridePrice = p.OverriddenPrice < 0 ? 0 : p.OverriddenPrice;

                    if (entry == null)
                    {
                        entry = new SitePrice
                        {
                            SiteId = p.SiteId,
                            FuelTypeId = p.FuelTypeId,
                            DateOfCalc = forDate.Value,
                            DateOfPrice = forDate.Value,
                            SuggestedPrice = 0,
                            OverriddenPrice = newOverridePrice
                        };
                        db.Entry(entry).State = EntityState.Added;
                        //throw new ApplicationException(
                        //    String.Format("Price not found in DB for siteId={0}, fuelId={1}", p1.SiteId, p1.FuelTypeId));
                    }
                    else
                    {
                        entry.OverriddenPrice = newOverridePrice;
                        db.Entry(entry).State = EntityState.Modified;
                    }
                    //db.Entry(entry).Property(x => x.OverriddenPrice).IsModified = true;
                }
                int rowsAffected = await db.SaveChangesAsync();
                return rowsAffected;
            }
        }

        private int UpdateFuelOverridePrice(DateTime? forDate,RepositoryContext db, SitePrice p)
        {
             var dbPricesForDate =
                        db.SitePrices.Where(x => DbFunctions.DiffDays(x.DateOfCalc, forDate) == 0)
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

            _context.MarkPriceCacheOutdatedForFileUpload(fileUpload.Id);
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
                    _logger.Error(ce);
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
            // limit to the last N file uploads for performance
            const int MaxNumberOfFileUploads = 50;

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

            return files.OrderByDescending(x => x.UploadDateTime).Take(MaxNumberOfFileUploads).ToList();
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
                            var brandCompetitors = sainsburysSite.Competitors.Where(x => x.Competitor.Brand == brandName).OrderBy(x => x.Competitor.Brand).ToList();

                            string brandNameAfter = (brandName == Const.TESCOEXTRA || brandName == Const.TESCOEXPRESS)
                                ? Const.TESCO
                                : brandName;
                            var brandReportRow = brandReportRows.FirstOrDefault(x => x.BrandName == brandNameAfter);

                            if (brandReportRow == null )
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
                        var brandNames = sainsburysSite.Competitors.Select(x => x.Competitor.Brand).OrderBy(x=>x).Distinct();

                        // 2) all unique brand names e.g. ASDA, TESCO
                        foreach (string brandName in brandNames)
                        {

                            // 3) brands of the competitor from step 2) e.g. ASDA so we get all ASDA here
                            var brandCompetitors = sainsburysSite.Competitors.Where(x => x.Competitor.Brand == brandName).ToList();

                             // 4) have we already counted for this brand
                            string brandNameAfter = (brandName == Const.TESCOEXTRA || brandName == Const.TESCOEXPRESS)
                               ? Const.TESCO
                               : brandName;
                            var brandReportRow = brandReportRows.FirstOrDefault(x => x.BrandName == brandNameAfter);
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
                    var FileUpload_DailyPriceData_today = _context.FileUploads.Where(
               x =>
                   x.UploadDateTime.Month == when.Month &&
                   x.UploadDateTime.Day == when.Day &&
                   x.UploadDateTime.Year == when.Year && x.UploadTypeId == (int)FileUploadTypes.DailyPriceData && x.Status.Id == 10).ToList();

                    var FileUploadId_DailyPriceData_today = FileUpload_DailyPriceData_today.Count > 0 ? FileUpload_DailyPriceData_today[0].Id : 0;

                    var dailyPrices = _context.DailyPrices.Where(x => x.DailyUploadId == FileUploadId_DailyPriceData_today && x.FuelTypeId == fuelTypeId).ToList();

                    var distinctPrices = dailyPrices.Select(x => x.ModalPrice).Distinct().OrderBy(x => x).ToList();
                    var distinctCatNos = dailyPrices.Select(x => x.CatNo).Distinct().ToList();
                    var competitorSites = _context.Sites.Where(x => distinctCatNos.Contains(x.CatNo.Value) ).ToList();
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
                _logger.Error(ce);
                return null;
            }
        }

        public NationalAverageReportViewModel GetReportNationalAverage(DateTime when)
        {
            var result = new NationalAverageReportViewModel();

            var fuelTypeIds = new List<int> { (int)FuelTypeItem.Unleaded,(int)FuelTypeItem.Diesel };

            // Report uses Prices as per date of upload..(not date of Price in DailyPrice)..
            var FileUpload_DailyPriceData_today = _context.FileUploads.Where(
                 x =>
                     x.UploadDateTime.Month == when.Month &&
                     x.UploadDateTime.Day == when.Day &&
                     x.UploadDateTime.Year == when.Year && x.UploadTypeId == (int)FileUploadTypes.DailyPriceData && x.Status.Id == 10).ToList();

            var FileUploadId_DailyPriceData_today = FileUpload_DailyPriceData_today.Count > 0 ? FileUpload_DailyPriceData_today[0].Id : 0;

            var dailyPrices = _context.DailyPrices.Where(x => x.DailyUploadId == FileUploadId_DailyPriceData_today && fuelTypeIds.Contains(x.FuelTypeId)).ToList();

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

                // TESCO EXPRESS and TESCO EXTRA will be combined with TESCO
                distinctBrands.Remove(Const.TESCOEXPRESS);
                distinctBrands.Remove(Const.TESCOEXTRA);
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
                        var isAsda = brand == Const.ASDA;
                        var isMorrisons = brand == Const.MORRISONS;
                        var isSainsburys = brand == Const.SAINSBURYS;
                        var isTesco = brand == Const.TESCO || brand == Const.TESCOEXPRESS || brand == Const.TESCOEXTRA;

                        var isGrocer = isAsda || isMorrisons || isSainsburys || isTesco;

                        var brandAvg = new NationalAverageReportBrandViewModel();
                        fuelRow.Brands.Add(brandAvg);
                        brandAvg.BrandName = brand;

                        List<int> brandCatsNos;

                        if (isTesco)
                        {
                            // combine ALL TESCO, TESCO EXPRESS and TESCO EXTRA
                            brandAvg.BrandName = brand + " (inc EXPRESS and EXTRA)";

                            brandCatsNos = competitorSites.Where(x => x.Brand == Const.TESCO || x.Brand == Const.TESCOEXPRESS || x.Brand == Const.TESCOEXTRA)
                                .Where(x => x.CatNo.HasValue)
                                .Select(x => x.CatNo.Value)
                                .ToList();
                        }
                        else
                        {
                            brandCatsNos = competitorSites.Where(x => x.Brand == brand)
                                .Where(x => x.CatNo.HasValue)
                                .Select(x => x.CatNo.Value)
                                .ToList();
                        }

                        var pricesList = dailyPrices.Where(x => x.FuelTypeId == fuelType && brandCatsNos.Contains(x.CatNo)).ToList();

                        if (pricesList.Any())
                        {
                            brandAvg.Min = (int)pricesList.Min(x => x.ModalPrice);
                            brandAvg.Average = (int)pricesList.Average(x => x.ModalPrice);
                            brandAvg.Max = (int)pricesList.Max(x => x.ModalPrice);
                            if (!isGrocer)
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

            var FileUpload_DailyPriceData_today = _context.FileUploads.Where(
                          x =>
                              x.UploadDateTime.Month == when.Month &&
                              x.UploadDateTime.Day == when.Day &&
                              x.UploadDateTime.Year == when.Year && x.UploadTypeId == (int)FileUploadTypes.DailyPriceData && x.Status.Id == 10).ToList();

            var FileUploadId_DailyPriceData_today = FileUpload_DailyPriceData_today.Count > 0 ? FileUpload_DailyPriceData_today[0].Id : 0;

            if (FileUploadId_DailyPriceData_today == 0) return result;

            // Report uses Prices as per date of upload..(not date of Price in DailyPrice)..
            var dailyPrices = _context.DailyPrices.Where(x => x.DailyUploadId == FileUploadId_DailyPriceData_today && fuelTypeIds.Contains(x.FuelTypeId)).ToList();

            var fuels = _context.FuelType.ToList();

            var distinctCatNos = dailyPrices.Select(x => x.CatNo).Distinct().ToList();
            var competitorSites = _context.Sites.Where(x => distinctCatNos.Contains(x.CatNo.Value)).ToList();

            //calculating by brands
            var distinctBrands = competitorSites.Select(x => x.Brand).Distinct().OrderBy(x => x).ToList();

            distinctBrands.Remove("TESCO EXPRESS");
            distinctBrands.Remove("TESCO EXTRA");

            distinctBrands = SortBrandsWithGrocersAtTop(distinctBrands);

            foreach (var fuelType in fuelTypeIds)
            {
                var f = fuels.FirstOrDefault(x => x.Id == fuelType);
                if (f == null) continue;

                var fuelRow = new NationalAverageReportFuelViewModel();
                result.Fuels.Add(fuelRow);
                fuelRow.FuelName = f.FuelTypeName;

                foreach (var brand in distinctBrands)
                {
                    var isTesco = brand == "TESCO";
                    var brandAvg = new NationalAverageReportBrandViewModel();
                    fuelRow.Brands.Add(brandAvg);
                    if (isTesco)
                        brandAvg.BrandName = "TESCO (inc EXPRESS and EXTRA)";
                    else
                        brandAvg.BrandName = brand;

                    var brandCatsNos = competitorSites.Where(x => x.Brand == brand)
                        .Where(x => x.CatNo.HasValue)
                        .Select(x => x.CatNo.Value)
                        .ToList();

                    if (isTesco)
                    {
                        // combine TESCO, TESCO EXPRESS and TESCO EXTRA
                        brandCatsNos.AddRange(
                            competitorSites
                            .Where(x => x.Brand == "TESCO EXPRESS" || x.Brand == "TESCO EXTRA")
                            .Where(x => x.CatNo.HasValue)
                            .Select(x => x.CatNo.Value)
                            .ToList()
                            );
                    }
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
            var FileUpload_DailyPriceData_today = _context.FileUploads.Where(
                    x =>
                        x.UploadDateTime.Month == when.Month &&
                        x.UploadDateTime.Day == when.Day &&
                        x.UploadDateTime.Year == when.Year && x.UploadTypeId == (int)FileUploadTypes.DailyPriceData && x.Status.Id == 10).ToList();

            var FileUploadId_DailyPriceData_today = FileUpload_DailyPriceData_today.Count > 0 ? FileUpload_DailyPriceData_today[0].Id : 0;

            var dailyPrices = _context.DailyPrices.Where(x => x.DailyUploadId == FileUploadId_DailyPriceData_today && fuelTypeIds.Contains(x.FuelTypeId)).ToList();

            var fuels = _context.FuelType.ToList();

            var distinctCatNos = dailyPrices.Select(x => x.CatNo).Distinct().ToList();
            var competitorSites = _context.Sites.Where(x => distinctCatNos.Contains(x.CatNo.Value)).ToList();

            //calculating by brands
            List<string> distinctBrands=new List<string>();// = competitorSites.Select(x => x.Brand).Distinct().OrderBy(x => x).ToList();

           // if (distinctBrands.Count > 0)
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
                    var isTesco = brand == Const.TESCO || brand == Const.TESCOEXPRESS || brand == Const.TESCOEXTRA;

                    var brandAvg = new NationalAverageReportBrandViewModel();
                    fuelRow.Brands.Add(brandAvg);
                    brandAvg.BrandName = brand;

                    List<int> brandCatsNos;

                    if (isTesco)
                    {
                        brandAvg.BrandName = brand + " (inc EXPRESS and EXTRA)";

                        brandCatsNos = competitorSites.Where(x => x.Brand == Const.TESCO || x.Brand == Const.TESCOEXPRESS || x.Brand == Const.TESCOEXTRA)
                            .Where(x => x.CatNo.HasValue)
                            .Select(x => x.CatNo.Value)
                            .ToList();
                    }
                    else
                    {
                        brandCatsNos = competitorSites.Where(x => brand == Const.UK ? true : x.Brand == brand)
                            .Where(x => x.CatNo.HasValue)
                            .Select(x => x.CatNo.Value)
                            .ToList();
                    }

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
            var FileUpload_DailyPriceData_today = _context.FileUploads.Where(
                      x =>
                          x.UploadDateTime.Month == when.Month &&
                          x.UploadDateTime.Day == when.Day &&
                          x.UploadDateTime.Year == when.Year && x.UploadTypeId == (int)FileUploadTypes.DailyPriceData && x.Status.Id == 10).ToList();

            var FileUploadId_DailyPriceData_today = FileUpload_DailyPriceData_today.Count > 0 ? FileUpload_DailyPriceData_today[0].Id : 0;

            var dailyPrices = _context.DailyPrices.Where(x => x.DailyUploadId == FileUploadId_DailyPriceData_today && result.FuelTypeIds.Contains(x.FuelTypeId)).ToList();

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

                var distinctBrands = companyBrands.Select(b => b.Brand).Distinct();

                distinctBrands = SortBrandsWithGrocersAtTop(distinctBrands.ToList());

                foreach (var companyBrand in distinctBrands)
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
                                if (!result.SainsburysPrices.ContainsKey(fuelTypeId))  result.SainsburysPrices.Add(fuelTypeId, newFuel.Average);
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

                var sortedSitesWithPrices = siteName.Trim() == "empty" 
                ? sitesWithPrices.OrderBy(x => x.SiteName) 
                : sitesWithPrices.Where(x => x.SiteName.ToUpper().Trim().Contains(siteName.ToUpper().Trim())).OrderBy(x => x.SiteName);

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
                        FuelPrices = GetSiteFuelPricesOnDate(s.Prices, d)
                    }));

                    // Fill weekend/Bank holiday gaps
                    FillPriceMovementReportWeekendGaps(dataItems, FuelTypeItem.Unleaded);
                    FillPriceMovementReportWeekendGaps(dataItems, FuelTypeItem.Diesel);
                    FillPriceMovementReportWeekendGaps(dataItems, FuelTypeItem.Super_Unleaded);
                }
            });
            task.Wait();
            
            return retval;
        }

        private void FillPriceMovementReportWeekendGaps(List<PriceMovementReportDataItems> dataItems, FuelTypeItem fuelType)
        {
            if (dataItems == null)
                return;

            var sortedDays = dataItems.OrderBy(x => x.PriceDate);
            var lastPrice = 0;

            foreach(var day in sortedDays)
            {
                var fuelPrice = day.FuelPrices.FirstOrDefault(x => x.FuelTypeId == (int)fuelType);
                if (fuelPrice == null)
                    continue;

                if (fuelPrice.PriceValue != 0)
                    lastPrice = fuelPrice.PriceValue;
                else
                    fuelPrice.PriceValue = lastPrice;
            }
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
                forDate = forDate.Date; // remove time so its Date only
                var today = forDate.Date;
                var tomorrow = forDate.Date.AddDays(1);

                var fuelTypesList = new[] {
                    (int)FuelTypeItem.Unleaded,
                    (int)FuelTypeItem.Diesel,
                    (int)FuelTypeItem.Super_Unleaded
                };

                var fileUpload_LatestCompPriceData_PrevDay = _context.FileUploads.OrderByDescending(x => x.Id)
                    .FirstOrDefault(x =>
                        x.StatusId == 10
                        && x.UploadTypeId == (int)FileUploadTypes.DailyPriceData
                    );

                var fileUploadId_LatestCompPriceData_PrevDay = fileUpload_LatestCompPriceData_PrevDay == null ? 0 : fileUpload_LatestCompPriceData_PrevDay.Id;

                var dailyPrices = new List<DailyPrice>();
                dailyPrices = GetDailyPricesForDate(tomorrow);

                var siteVMLlist = GetSitesWithPrices(today);

                if (dailyPrices.Count == 0 || siteVMLlist.Count() == 0) return null;

                var sites = GetJsSites();

                var reportFuels = GetFuelTypes().Where(x => fuelTypesList.Contains(x.Id)).ToList();

                foreach (var site in sites)
                {
                    // skip InActive sites
                    if (!site.IsActive)
                        continue;

                    var dataRow = new ComplianceReportRow
                    {
                        SiteId = site.Id,
                        PfsNo = site.PfsNo.ToString(),
                        StoreNo = site.StoreNo.ToString(),
                        CatNo = site.CatNo.ToString(),
                        SiteName = site.SiteName,
                        DataItems = new List<ComplianceReportDataItem>()
                    };
                    retval.ReportRows.Add(dataRow);

                    var dataItems = dataRow.DataItems;

                    var siteVM = siteVMLlist.FirstOrDefault(x => x.SiteId == site.Id);

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

                        var fuelPrice = siteVM.FuelPrices.FirstOrDefault(x => x.FuelTypeId == fuelId);
                        if (fuelPrice != null)
                        {
                            dataItem.FoundExpectedPrice = true;
                            int overridePrice = fuelPrice.OverridePrice.HasValue ? fuelPrice.OverridePrice.Value : 0;
                            int todayPrice = fuelPrice.TodayPrice.HasValue ? fuelPrice.TodayPrice.Value : 0;
                            dataItem.ExpectedPriceValue = overridePrice > 0 ? overridePrice : todayPrice;
                        }
                        else
                        {
                            dataItem.FoundExpectedPrice = false;
                            dataItem.ExpectedPriceValue = 0;
                        }

                        var dailyPrice = dailyPrices.FirstOrDefault(x => x.CatNo.Equals(site.CatNo) && x.FuelTypeId == fuel.Id);
                        if (dailyPrice != null)
                        {
                            dataItem.CatPriceValue = dailyPrice.ModalPrice;
                            dataItem.FoundCatPrice = true;
                        }
                        if (!dataItem.FoundCatPrice || !dataItem.FoundExpectedPrice) continue;

                        dataItem.Diff = (double)(dataItem.CatPriceValue - dataItem.ExpectedPriceValue) / 10;
                        dataItem.DiffValid = true;
                    }
                }
                return retval;
            }
            catch (Exception ce)
            {
                _logger.Error(ce);
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
                _logger.Error(ce);
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
                    var brand = _context.Brands.FirstOrDefault(x => x.BrandName.ToUpper() == brandName.ToUpper());
                    ExcludeBrands excludeBrand = new ExcludeBrands();
                    excludeBrand.BrandName = brandName;
                    excludeBrand.BrandId = brand == null ? 0 : brand.Id; // lookup BrandId
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
                _logger.Error(ce);
                return null;
            }
        }

        public SiteNoteViewModel GetSiteNote(int siteId)
        {
            try
            {
                var site = _context.Sites.FirstOrDefault(x => x.Id == siteId);

                if (site == null)
                    throw new ArgumentException("Unable to find SiteNode - id:" + siteId);

                return new SiteNoteViewModel()
                {
                    SiteId = site.Id,
                    SiteName = site.SiteName,
                    Note = site.Notes
                };
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
                return null;
            }
        }

        public JsonResultViewModel<bool> UpdateSiteNote(SiteNoteUpdateViewModel model)
        {
            try
            {
                if (model.SiteId == 0)
                    throw new ArgumentException("SiteNote - Id cannot be 0!");

                var site = _context.Sites.FirstOrDefault(x => x.Id == model.SiteId);

                if (site == null)
                    throw new Exception("Unable to find siteId:" + model.SiteId);

                site.Notes = String.IsNullOrWhiteSpace(model.Note)
                    ? null
                    : model.Note.Trim();

                _context.SaveChanges();

                return new JsonResultViewModel<bool>()
                {
                    Success = true,
                    Message = "Updated Site Note",
                    Data = true
                };
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
                return new JsonResultViewModel<bool>()
                {
                    Success = false,
                    Message = ex.Message,
                    Data = false
                };
            }
        }

        public JsonResultViewModel<int> DeleteSiteNote(int siteId)
        {
            try
            {
                if (siteId == 0)
                    throw new ArgumentException("SiteId - cannot be 0!");

                var site = _context.Sites.FirstOrDefault(x => x.Id == siteId);
                if (site == null)
                    return new JsonResultViewModel<int>()
                    {
                        Success = false,
                        Message = "Site Note not found",
                        Data = siteId
                    };

                site.Notes = null;
                _context.SaveChanges();
                return new JsonResultViewModel<int>()
                {
                    Success = true,
                    Message = "Deleted Site Note",
                    Data = siteId
                };
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
                return new JsonResultViewModel<int>()
                {
                    Success = false,
                    Message = ex.Message,
                    Data = siteId
                };
            }
        }

        public RecentFileUploadSummary GetRecentFileUploadSummary()
        {
            return _context.GetRecentFileUploadSummary();
        }

        public IEnumerable<ContactDetail> GetContactDetails()
        {
            return _context.GetContactDetails();
        }

        public PPUserPermissions GetUserPermissions(int ppUserId)
        {
            if (ppUserId == 0)
                throw new ArgumentException("PPUserId cannot be zero!");

            var permissions = _context.GetUserPermissions(ppUserId);
            return permissions;
        }

        public bool UpsertUserPermissions(int requestingPPUserId, PPUserPermissions permissions)
        {
            return _context.UpserUserPermissions(requestingPPUserId, permissions);
        }

        public IEnumerable<DiagnosticsDatabaseObject> GetDiagnosticsRecentDatabaseObjectChanges(int daysAgo)
        {
            return _context.GetDiagnosticsRecentDatabaseObjectChanges(daysAgo);
        }
        public DiagnosticsDatabaseObjectSummary GetDiagnosticsDatabaseObjectSummary()
        {
            return _context.GetDiagnosticsDatabaseObjectSummary();
        }

        public UserAccessViewModel GetUserAccess(string userName)
        {
            var ppUser = _context.PPUsers.FirstOrDefault(x => x.Email == userName);
            if (ppUser != null)
            {
                var permissions = GetUserPermissions(ppUser.Id);

                var uploadPermissions = (FileUploadsUserPermissions)permissions.FileUploadsUserPermissions;
                var sitePricingPermissions = (SitesPricingUserPermissions)permissions.SitePricingUserPermissions;
                var sitesManagementPermissions = (SitesMaintenanceUserPermissions)permissions.SitesMaintenanceUserPermissions;
                var reportsPermissions = (ReportsUserPermissions)permissions.ReportsUserPermissions;
                var userManagementPermissions = (UsersManagementUserPermissions)permissions.UsersManagementUserPermissions;
                var diagnosticsPermissions = (DiagnosticsUserPermissions)permissions.DiagnosticsUserPermissions;
                var systemSettingsPermissions = (SystemSettingsUserPermissions)permissions.SystemSettingsUserPermissions;

                var model = new UserAccessViewModel()
                {
                    IsUserAuthenticated = true,
                    IsActive = permissions.IsActive,
                    PPUserId = ppUser.Id,
                    UserName = ppUser.Email,
                    UserFileUploadsAccess = new UserFileUploadsAccess(uploadPermissions),
                    UserSitePricingAccess = new UserSitePricingAccess(sitePricingPermissions),
                    UserSitesMaintenanceAccess = new UserSitesMaintenanceAccess(sitesManagementPermissions),
                    UserReportsAccess = new UserReportsAccess(reportsPermissions),
                    UserUserManagementAccess = new UserUserManagementAccess(userManagementPermissions),
                    UserDiagnosticsAccess = new UserDiagnosticsAccess(diagnosticsPermissions),
                    UserSystemSettingsAccess = new UserSystemSettingsAccess(systemSettingsPermissions)
                };
                return model;
            }
            return new UserAccessViewModel();
        }

        public void SignIn(string email)
        {
            if (String.IsNullOrWhiteSpace(email))
                return;

            var ppUser = _context.PPUsers.FirstOrDefault(x => x.IsActive && x.Email == email);
            if (ppUser != null)
            {
                ppUser.LastUsedOn = DateTime.Now;
                _context.SaveChanges();
            }
        }

        public FileDownloadViewModel GetFileDownload(int fileUploadId, string fileUploadPath)
        {
            if (fileUploadId == 0)
                return new FileDownloadViewModel();

            var fileUpload = _context.FileUploads.FirstOrDefault(x => x.Id == fileUploadId);
            if (fileUpload == null)
                throw new Exception("File Upload Record does not exist");

            var fullPath = System.IO.Path.Combine(fileUploadPath, fileUpload.StoredFileName);
            if (!File.Exists(fullPath))
            {
                fileUpload.FileExists = false;
                _context.SaveChanges();
                throw new Exception("Physical File does not exist");
            }

            var fileBytes = System.IO.File.ReadAllBytes(fullPath);

            var timeStampedFilename = String.Format("{0}-{1}{2}",
                fileUpload.UploadDateTime.ToString("yyyyMMdd_HHmm"),
                (FileUploadTypes)fileUpload.UploadTypeId,
                Path.GetExtension(fileUpload.StoredFileName)
                );

            return new FileDownloadViewModel()
            {
                TimeStampedFileName = timeStampedFilename,
                FileName = fileUpload.OriginalFileName,
                FileBytes = fileBytes,
                UploadDateTime = fileUpload.UploadDateTime,
                FileUploadTypeId = fileUpload.UploadTypeId
            };
        }

        public bool DataCleanseFileUploads(int daysAgo, string fileUploadPath)
        {
            var minDateTime = daysAgo == 0 
                ? DateTime.Now.Date.AddDays(1) // Delete ALL files
                : DateTime.Now.Date.AddDays(0 - daysAgo);

            //
            // first delete old FileUpload physical files older than minDateTime
            //

            var oldFileUploads = _context.FileUploads
                .Where(x => x.FileExists && x.UploadDateTime < minDateTime)
                .ToList();

            var ids = new List<int>();

            foreach(var file in oldFileUploads)
            {
                try
                {
                    var fullPath = System.IO.Path.Combine(fileUploadPath, file.StoredFileName);
                    if (System.IO.File.Exists(fullPath))
                    {
                        System.IO.File.Delete(fullPath);
                    }

                    ids.Add(file.Id);
                }
                catch (Exception ex)
                {
                    _logger.Error(ex);
                }
            }

            if (ids.Any())
            {
                var sql = "UPDATE dbo.FileUpload SET FileExists=0 WHERE FileExists=1 AND ID IN("
                    + ids.Select(x => x.ToString()).Aggregate((x, y) => x + "," + y)
                    + ");";

                _context.Database.ExecuteSqlCommand(sql);
            }

            //
            // now delete any other older files which have no associated FileUpload record
            //
            try
            {
                var directoryInfo = new DirectoryInfo(fileUploadPath);
                var oldFiles = directoryInfo.EnumerateFiles().Where(x => x.CreationTime < minDateTime).ToList();
                foreach (var file in oldFiles)
                {
                    File.Delete(file.FullName);
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
            }

            return true;
        }

        public void PurgePriceSnapshots(int daysAgo)
        {
            _context.PurgePriceSnapshots(daysAgo);
        }

        public void PurgeWinScheduleLogs(int daysAgo)
        {
            _context.PurgeWinScheduleLogs(daysAgo);
        }

        public SystemSettings GetSystemSettings()
        {
            return _context.SystemSettings.FirstOrDefault();
        }

        public void UpdateSystemSettings(SystemSettings systemSettings)
        {
            var row = _context.SystemSettings.FirstOrDefault();

            // safety check !
            if (systemSettings.DataCleanseFilesAfterDays >= Const.MinDataCleanseFilesAfterDays)
            {
                row.DataCleanseFilesAfterDays = systemSettings.DataCleanseFilesAfterDays;
            }

            // copy from new record values into single Entity-Framework record row
            row.MinUnleadedPrice = systemSettings.MinUnleadedPrice;
            row.MaxUnleadedPrice = systemSettings.MaxUnleadedPrice;
            row.MinDieselPrice = systemSettings.MinDieselPrice;
            row.MaxDieselPrice = systemSettings.MaxDieselPrice;
            row.MinSuperUnleadedPrice = systemSettings.MinSuperUnleadedPrice;
            row.MaxSuperUnleadedPrice = systemSettings.MaxSuperUnleadedPrice;
            row.MinUnleadedPriceChange = systemSettings.MinUnleadedPriceChange;
            row.MaxUnleadedPriceChange = systemSettings.MaxUnleadedPriceChange;
            row.MinDieselPriceChange = systemSettings.MinDieselPriceChange;
            row.MaxDieselPriceChange = systemSettings.MaxDieselPriceChange;
            row.MinSuperUnleadedPriceChange = systemSettings.MinSuperUnleadedPriceChange;
            row.MaxSuperUnleadedPriceChange = systemSettings.MaxSuperUnleadedPriceChange;
            row.MaxGrocerDriveTimeMinutes = systemSettings.MaxGrocerDriveTimeMinutes;
            row.PriceChangeVarianceThreshold = systemSettings.PriceChangeVarianceThreshold;
            row.SuperUnleadedMarkupPrice = systemSettings.SuperUnleadedMarkupPrice;
            row.DecimalRounding = systemSettings.DecimalRounding;
            row.EnableSiteEmails = systemSettings.EnableSiteEmails;
            row.SiteEmailTestAddresses = systemSettings.SiteEmailTestAddresses;

            _context.SaveChanges();
        }

        public SitePricingSettings GetSitePricingSettings()
        {
            var settings = GetSystemSettings();
            var model = new SitePricingSettings()
            {
                MinUnleadedPrice = settings.MinUnleadedPrice.ToActualPrice(),
                MaxUnleadedPrice = settings.MaxUnleadedPrice.ToActualPrice(),
                MinDieselPrice = settings.MinDieselPrice.ToActualPrice(),
                MaxDieselPrice = settings.MaxDieselPrice.ToActualPrice(),
                MinSuperUnleadedPrice = settings.MinSuperUnleadedPrice.ToActualPrice(),
                MaxSuperUnleadedPrice = settings.MaxSuperUnleadedPrice.ToActualPrice(),
                MinUnleadedPriceChange = settings.MinUnleadedPriceChange.ToActualPrice(),
                MaxUnleadedPriceChange = settings.MaxUnleadedPriceChange.ToActualPrice(),
                MinDieselPriceChange = settings.MinDieselPriceChange.ToActualPrice(),
                MaxDieselPriceChange = settings.MaxDieselPriceChange.ToActualPrice(),
                MinSuperUnleadedPriceChange = settings.MinSuperUnleadedPriceChange.ToActualPrice(),
                MaxSuperUnleadedPriceChange = settings.MaxSuperUnleadedPriceChange.ToActualPrice(),
                MaxGrocerDriveTimeMinutes = settings.MaxGrocerDriveTimeMinutes,
                PriceChangeVarianceThreshold = settings.PriceChangeVarianceThreshold.ToActualPrice(),
                SuperUnleadedMarkupPrice = settings.SuperUnleadedMarkupPrice.ToActualPrice(),
                DecimalRounding = settings.DecimalRounding,
                EnableSiteEmails = settings.EnableSiteEmails,
                SiteEmailTestAddresses = settings.SiteEmailTestAddresses
            };

            return model;
        }

        public void ArchiveQuarterlyUploadStagingData()
        {
            _context.ArchiveQuarterlyUploadStagingData();
        }

        public IEnumerable<SelectItemViewModel> GetQuarterlyFileUploadOptions()
        {
            var options = _context.GetQuarterlyFileUploadOptions();
            return options;
        }

        public QuarterlySiteAnalysisReportViewModel GetQuarterlySiteAnalysisReport(int leftId, int rightId)
        {
            if (leftId == 0 || rightId == 0 || leftId == rightId)
                return new QuarterlySiteAnalysisReportViewModel();

            var report = _context.GetQuarterlySiteAnalysisReportRows(leftId, rightId);

            var model = new QuarterlySiteAnalysisReportViewModel()
            {
                NewSiteCount = 0,
                DeletedSiteCount = 0,
                ExistingSiteCount = 0,
                TotalSiteCount = 0,
                ChangeOwnershipCount = 0,
                Rows = report.Rows
            };

            // gather statistics
            if (model.Rows.Any())
            {
                model.NewSiteCount = model.Rows.Count(x => x.WasSiteAdded);
                model.DeletedSiteCount = model.Rows.Count(x => x.WasSiteDeleted);
                model.ExistingSiteCount = model.Rows.Count(x => !x.WasSiteAdded && !x.WasSiteDeleted);
                model.TotalSiteCount = model.Rows.Count();
                model.ChangeOwnershipCount = model.Rows.Count(x => x.HasOwnershipChanged);
                model.LeftTotalRecordCount = report.Stats.LeftTotalRecordCount;
                model.RightTotalRecordCount = report.Stats.RightTotalRecordCount;
            }

            return model;
        }

        public FileUpload GetFileUploadInformation(int fileUploadId)
        {
            var fileUpload = _context.FileUploads.FirstOrDefault(x => x.Id == fileUploadId);
            return fileUpload != null
                ? fileUpload
                : new FileUpload();
        }

        public bool DeleteAllData(string fileUploadPath)
        {
            // delete ALL FileUpLOAD files
            DataCleanseFileUploads(0, fileUploadPath);

            // delete ALL non static database data
            return _context.DeleteAllData();
        }


        public void SetSitePriceMatchTypeDefaults()
        {
            _context.SetSitePriceMatchTypeDefaults();
        }

        public void RunPostQuarterlyFileUploadTasks()
        {
            _context.RunPostQuarterlyFileUploadTasks();
        }

        public IEnumerable<DiagnosticsRecordCountViewModel> GetDatabaseRecordCounts()
        {
            return _context.GetDatabaseRecordCounts();
        }

        public DataSanityCheckSummaryViewModel GetDataSanityCheckSummary()
        {
            return _context.GetDataSanityCheckSummary();
        }

        #region Email Templates
        public IEnumerable<EmailTemplateName> GetEmailTemplateNames()
        {
            var names = (from item in _context.EmailTemplates
                         select new EmailTemplateName()
                         {
                             EmailTemplateId = item.EmailTemplateId,
                             TemplateName = item.TemplateName,
                             IsDefault = item.IsDefault,
                             SubjectLine = item.SubjectLine
                         }
                        ).ToList();

            return names;
        }

        public EmailTemplate CreateEmailTemplateClone(int ppUserId, int emailTemplateId, string templateName)
        {
            var sourceTemplate = _context.EmailTemplates.FirstOrDefault(x => x.EmailTemplateId == emailTemplateId);

            var template = new EmailTemplate()
            {
                IsDefault = false, // NOTE: There should only be 1 default (aka standard email template)
                TemplateName = templateName,
                SubjectLine = sourceTemplate.SubjectLine,
                PPUserId = ppUserId,
                EmailBody = sourceTemplate.EmailBody
            };
            _context.EmailTemplates.Add(template);
            _context.SaveChanges();
            return template;
        }

        public EmailTemplate GetEmailTemplate(int emailTemplateId)
        {
            if (emailTemplateId == 0)
            {
                var defaultTemplate = _context.EmailTemplates.FirstOrDefault(x => x.IsDefault);
                return defaultTemplate;
            }

            var template = _context.EmailTemplates.FirstOrDefault(x => x.EmailTemplateId == emailTemplateId);
            return template;
        }

        public EmailTemplate UpdateEmailTemplate(EmailTemplate template)
        {
            var existingTemplate = _context.EmailTemplates.FirstOrDefault(x => x.EmailTemplateId == template.EmailTemplateId);
            if (existingTemplate == null)
                return null;

            // cannot update Delete template !
            if (existingTemplate.IsDefault)
                return null;

            existingTemplate.SubjectLine = template.SubjectLine;
            existingTemplate.EmailBody = template.EmailBody;

            _context.SaveChanges();

            return existingTemplate;
        }

        public bool DeleteEmailTemplate(int ppUserId, int emailTemplateId)
        {
            var template = _context.EmailTemplates.FirstOrDefault(x => x.EmailTemplateId == emailTemplateId && x.IsDefault == false);
            if (template == null)
                return false;

            _context.EmailTemplates.Remove(template);
            _context.SaveChanges();
            return true;
        }

        public IEnumerable<DriveTimeMarkup> GetAllDriveTimeMarkups()
        {
            const int driveTimeMinutesPastLastItem = 5;

            // get items sorted by DriveTime
            var items = _context.Set<DriveTimeMarkup>().OrderBy(x => x.DriveTime).ToList();

            // fill calculated properties
            if (items.Any())
            {
                var fuels = new List<FuelTypeItem>()
                {
                    FuelTypeItem.Unleaded,
                    FuelTypeItem.Diesel,
                    FuelTypeItem.Super_Unleaded
                };

                foreach (var fuel in fuels)
                {
                    var fuelItems = items.Where(x => x.FuelTypeId == (int)fuel).ToList();

                    for (var i = 0; i < fuelItems.Count(); i++)
                    {
                        var isLast = (i + 1) == fuelItems.Count();
                        var item = fuelItems[i];
                        int maxDriveTime = isLast
                            ? item.DriveTime + driveTimeMinutesPastLastItem
                            : fuelItems[i + 1].DriveTime - 1;

                        item.IsFirst = i == 0;
                        item.IsLast = isLast;
                        item.MaxDriveTime = maxDriveTime;
                    }
                }
            }

            return items;
        }

        public StatusViewModel UpdateDriveTimeMarkup(IEnumerable<DriveTimeMarkup> driveTimeMarkups)
        {
            var error = "";

            // validation
            if (!driveTimeMarkups.Any())
                error = "No Drive Time markup data. Please add one or more for each fuel";
            else if (!driveTimeMarkups.Any(x => x.FuelTypeId == (int)FuelTypeItem.Unleaded))
                error = "No Drive Time markup defined for Unleaded";
            else if (!driveTimeMarkups.Any(x => x.FuelTypeId == (int)FuelTypeItem.Diesel))
                error = "No Drive Time markup defined for Diesel";
            else if (!driveTimeMarkups.Any(x => x.FuelTypeId == (int)FuelTypeItem.Super_Unleaded))
                error = "No Drive Time markup defined for Super-Unleaded";

            if (error == "")
                error = _context.UpdateDriveTimeMarkups(driveTimeMarkups);

            return new StatusViewModel()
            {
                ErrorMessage = error != "" ? error : "",
                SuccessMessage = error == "" ? "Drive Time Markup data saved" : ""
            };
        }

        public BrandsCollectionSettingsViewModel GetBrandCollectionSettings()
        {
            var model = _context.GetBrandCollectionSettings();
            return model;
        }

        public bool UpdateBrandCollectionSettings(BrandsSettingsUpdateViewModel brandsCollectionSettings)
        {
            return _context.UpdateBrandCollectionSettings(brandsCollectionSettings);
        }

        public BrandsCollectionSummaryViewModel GetBrandCollectionSummary()
        {
            return _context.GetBrandCollectionSummary();
        }


        public void ResumePriceCacheForDay(DateTime day)
        {
            _context.ResumePriceCacheForDay(day);
        }
        public void SuspendPriceCacheForDay(DateTime day)
        {
            _context.SuspendPriceCacheForDay(day);
        }
        public PriceSnapshotViewModel GetPriceSnapshotForDay(DateTime day)
        {
            return _context.GetPriceSnapshotForDay(day);
        }
        public void MarkPriceCacheOutdatedForDay(DateTime day)
        {
            _context.MarkPriceCacheOutdatedForDay(day);
        }
        public int GetFuelDriveTimeMarkForSiteToCompetitor(int fuelTypeId, int siteId, int competitorId)
        {
            return _context.GetFuelDriveTimeMarkForSiteToCompetitor(fuelTypeId, siteId, competitorId);
        }

        #endregion

        #region Windows Service

        public IEnumerable<ScheduleItemViewModel> GetWinServiceScheduledItems()
        {
            return _context.GetWinServiceScheduledItems();
        }
        public IEnumerable<ScheduleEventLogViewModel> GetWinServiceEventLog()
        {
            return _context.GetWinServiceEventLog();
        }
        public ScheduleItemViewModel GetWinServiceScheduleItem(int winServiceSheduleId)
        {
            return _context.GetWinServiceScheduleItem(winServiceSheduleId);
        }

        public ScheduleItemViewModel UpsertWinServiceSchedule(ScheduleItemViewModel model)
        {
            return _context.UpsertWinServiceSchedule(model);
        }

        public void AddWinServiceEventLog(int winServiceScheduleId, WinServiceEventStatus eventStatus, string message, string exception="")
        {
            _context.AddWinServiceEventLog(winServiceScheduleId, eventStatus, message, exception);
        }

        public void ClearWinServiceEventLog()
        {
            _context.ClearWinServiceEventLog();
        }

        public List<int> GetJsSitesByPfsNum()
        {
            var pfsNums = _context.Sites.Where(x => x.IsSainsburysSite && x.PfsNo != null)
                .OrderBy(X => X.SiteName)
                .Select(x => x.PfsNo)
                .Cast<int>()
                .ToList();

            return pfsNums;

            // NOTE: not sure why this was hardcoded !

            //List<int> lstJsList = new List<int>();
            //lstJsList.Add(578);
            //lstJsList.Add(196);
            //lstJsList.Add(1006);
            //lstJsList.Add(200);
            //lstJsList.Add(456);
            //lstJsList.Add(64);
            //lstJsList.Add(746);
            //lstJsList.Add(1100);
            //lstJsList.Add(1054);
            //lstJsList.Add(165);
            //lstJsList.Add(96);
            //lstJsList.Add(586);
            //lstJsList.Add(155);
            //lstJsList.Add(459);
            //lstJsList.Add(210);
            //lstJsList.Add(150);
            //lstJsList.Add(1008);
            //lstJsList.Add(563);
            //lstJsList.Add(477);
            //lstJsList.Add(179);
            //lstJsList.Add(439);
            //lstJsList.Add(137);
            //lstJsList.Add(223);
            //lstJsList.Add(592);
            //lstJsList.Add(127);
            //lstJsList.Add(1835);
            //lstJsList.Add(449);
            //lstJsList.Add(1231);
            //lstJsList.Add(896);
            //lstJsList.Add(86);
            //lstJsList.Add(238);
            //lstJsList.Add(486);
            //lstJsList.Add(159);
            //lstJsList.Add(80);
            //lstJsList.Add(487);
            //lstJsList.Add(591);
            //lstJsList.Add(1001);
            //lstJsList.Add(91);
            //lstJsList.Add(162);
            //lstJsList.Add(476);
            //lstJsList.Add(239);
            //lstJsList.Add(148);
            //lstJsList.Add(785);
            //lstJsList.Add(1106);
            //lstJsList.Add(240);
            //lstJsList.Add(1071);
            //lstJsList.Add(457);
            //lstJsList.Add(134);
            //lstJsList.Add(139);
            //lstJsList.Add(178);
            //lstJsList.Add(185);
            //lstJsList.Add(233);
            //lstJsList.Add(473);
            //lstJsList.Add(138);
            //lstJsList.Add(126);
            //lstJsList.Add(583);
            //lstJsList.Add(97);
            //lstJsList.Add(176);
            //lstJsList.Add(1010);
            //lstJsList.Add(1073);
            //lstJsList.Add(145);
            //lstJsList.Add(221);
            //lstJsList.Add(197);
            //lstJsList.Add(189);
            //lstJsList.Add(174);
            //lstJsList.Add(218);
            //lstJsList.Add(1067);
            //lstJsList.Add(458);
            //lstJsList.Add(577);
            //lstJsList.Add(1549);
            //lstJsList.Add(89);
            //lstJsList.Add(232);
            //lstJsList.Add(492);
            //lstJsList.Add(493);
            //lstJsList.Add(169);
            //lstJsList.Add(82);
            //lstJsList.Add(325);
            //lstJsList.Add(192);
            //lstJsList.Add(481);
            //lstJsList.Add(168);
            //lstJsList.Add(95);
            //lstJsList.Add(77);
            //lstJsList.Add(81);
            //lstJsList.Add(465);
            //lstJsList.Add(584);
            //lstJsList.Add(92);
            //lstJsList.Add(120);
            //lstJsList.Add(461);
            //lstJsList.Add(482);
            //lstJsList.Add(1081);
            //lstJsList.Add(1625);
            //lstJsList.Add(149);
            //lstJsList.Add(432);
            //lstJsList.Add(597);
            //lstJsList.Add(836);
            //lstJsList.Add(99);
            //lstJsList.Add(94);
            //lstJsList.Add(114);
            //lstJsList.Add(464);
            //lstJsList.Add(483);
            //lstJsList.Add(158);
            //lstJsList.Add(234);
            //lstJsList.Add(75);
            //lstJsList.Add(1268);
            //lstJsList.Add(1288);
            //lstJsList.Add(151);
            //lstJsList.Add(520);
            //lstJsList.Add(66);
            //lstJsList.Add(479);
            //lstJsList.Add(180);
            //lstJsList.Add(1015);
            //lstJsList.Add(1114);
            //lstJsList.Add(451);
            //lstJsList.Add(141);
            //lstJsList.Add(1107);
            //lstJsList.Add(1105);
            //lstJsList.Add(587);
            //lstJsList.Add(152);
            //lstJsList.Add(497);
            //lstJsList.Add(431);
            //lstJsList.Add(107);
            //lstJsList.Add(184);
            //lstJsList.Add(68);
            //lstJsList.Add(469);
            //lstJsList.Add(147);
            //lstJsList.Add(491);
            //lstJsList.Add(132);
            //lstJsList.Add(494);
            //lstJsList.Add(173);
            //lstJsList.Add(575);
            //lstJsList.Add(1005);
            //lstJsList.Add(235);
            //lstJsList.Add(1009);
            //lstJsList.Add(111);
            //lstJsList.Add(1011);
            //lstJsList.Add(396);
            //lstJsList.Add(452);
            //lstJsList.Add(131);
            //lstJsList.Add(478);
            //lstJsList.Add(108);
            //lstJsList.Add(213);
            //lstJsList.Add(1269);
            //lstJsList.Add(85);
            //lstJsList.Add(438);
            //lstJsList.Add(498);
            //lstJsList.Add(1038);
            //lstJsList.Add(582);
            //lstJsList.Add(394);
            //lstJsList.Add(175);
            //lstJsList.Add(193);
            //lstJsList.Add(1170);
            //lstJsList.Add(692);
            //lstJsList.Add(488);
            //lstJsList.Add(446);
            //lstJsList.Add(1246);
            //lstJsList.Add(525);
            //lstJsList.Add(110);
            //lstJsList.Add(485);
            //lstJsList.Add(495);
            //lstJsList.Add(1046);
            //lstJsList.Add(480);
            //lstJsList.Add(112);
            //lstJsList.Add(1063);
            //lstJsList.Add(1059);
            //lstJsList.Add(104);
            //lstJsList.Add(76);
            //lstJsList.Add(136);
            //lstJsList.Add(182);
            //lstJsList.Add(190);
            //lstJsList.Add(230);
            //lstJsList.Add(157);
            //lstJsList.Add(594);
            //lstJsList.Add(470);
            //lstJsList.Add(181);
            //lstJsList.Add(161);
            //lstJsList.Add(78);
            //lstJsList.Add(1004);
            //lstJsList.Add(1548);
            //lstJsList.Add(113);
            //lstJsList.Add(1696);
            //lstJsList.Add(177);
            //lstJsList.Add(231);
            //lstJsList.Add(1080);
            //lstJsList.Add(472);
            //lstJsList.Add(466);
            //lstJsList.Add(1000);
            //lstJsList.Add(90);
            //lstJsList.Add(109);
            //lstJsList.Add(1644);
            //lstJsList.Add(1079);
            //lstJsList.Add(187);
            //lstJsList.Add(1061);
            //lstJsList.Add(88);
            //lstJsList.Add(219);
            //lstJsList.Add(468);
            //lstJsList.Add(1168);
            //lstJsList.Add(133);
            //lstJsList.Add(580);
            //lstJsList.Add(1113);
            //lstJsList.Add(877);
            //lstJsList.Add(146);
            //lstJsList.Add(754);
            //lstJsList.Add(216);
            //lstJsList.Add(467);
            //lstJsList.Add(1003);
            //lstJsList.Add(565);
            //lstJsList.Add(209);
            //lstJsList.Add(154);
            //lstJsList.Add(1078);
            //lstJsList.Add(186);
            //lstJsList.Add(79);
            //lstJsList.Add(1324);
            //lstJsList.Add(98);
            //lstJsList.Add(140);
            //lstJsList.Add(211);
            //lstJsList.Add(142);
            //lstJsList.Add(135);
            //lstJsList.Add(198);
            //lstJsList.Add(84);
            //lstJsList.Add(437);
            //lstJsList.Add(199);
            //lstJsList.Add(1295);
            //lstJsList.Add(1244);
            //lstJsList.Add(144);
            //lstJsList.Add(143);
            //lstJsList.Add(45);
            //lstJsList.Add(103);
            //lstJsList.Add(496);
            //lstJsList.Add(1040);
            //lstJsList.Add(156);
            //lstJsList.Add(1610);
            //lstJsList.Add(188);
            //lstJsList.Add(191);
            //lstJsList.Add(195);
            //lstJsList.Add(445);
            //lstJsList.Add(183);
            //lstJsList.Add(194);
            //lstJsList.Add(842);
            //lstJsList.Add(83);
            //lstJsList.Add(471);
            //lstJsList.Add(129);
            //lstJsList.Add(160);
            //lstJsList.Add(1134);
            //lstJsList.Add(1110);
            //lstJsList.Add(1092);
            //lstJsList.Add(1023);
            //lstJsList.Add(1180);
            //lstJsList.Add(1517);
            //lstJsList.Add(1518);
            //lstJsList.Add(1513);
            //lstJsList.Add(1221);
            //lstJsList.Add(1181);
            //lstJsList.Add(1225);
            //lstJsList.Add(1154);
            //lstJsList.Add(1186);
            //lstJsList.Add(1093);
            //lstJsList.Add(1162);
            //lstJsList.Add(1850);
            //lstJsList.Add(1199);
            //lstJsList.Add(1169);
            //lstJsList.Add(1097);
            //lstJsList.Add(1247);
            //lstJsList.Add(1249);
            //lstJsList.Add(1196);
            //lstJsList.Add(1255);
            //lstJsList.Add(1248);
            //lstJsList.Add(1274);
            //lstJsList.Add(1200);
            //lstJsList.Add(1655);
            //lstJsList.Add(1220);
            //lstJsList.Add(1082);
            //lstJsList.Add(1240);
            //lstJsList.Add(1272);
            //lstJsList.Add(1286);
            //lstJsList.Add(1077);
            //lstJsList.Add(1290);
            //lstJsList.Add(1293);
            //lstJsList.Add(1304);
            //lstJsList.Add(1281);
            //lstJsList.Add(1507);
            //lstJsList.Add(1239);
            //lstJsList.Add(1297);
            //lstJsList.Add(1303);
            //lstJsList.Add(1524);
            //lstJsList.Add(1275);
            //lstJsList.Add(1313);
            //lstJsList.Add(1309);
            //lstJsList.Add(1308);
            //lstJsList.Add(1283);
            //lstJsList.Add(1314);
            //lstJsList.Add(1267);
            //lstJsList.Add(1526);
            //lstJsList.Add(1525);
            //lstJsList.Add(1527);
            //lstJsList.Add(1319);
            //lstJsList.Add(1528);
            //lstJsList.Add(1529);
            //lstJsList.Add(1340);
            //lstJsList.Add(1095);
            //lstJsList.Add(1252);
            //lstJsList.Add(1589);
            //lstJsList.Add(1289);
            //lstJsList.Add(1070);
            //lstJsList.Add(1030);
            //lstJsList.Add(1254);
            //lstJsList.Add(1158);
            //lstJsList.Add(1550);
            //lstJsList.Add(1271);

            //return lstJsList;
        }

        #endregion Windows Service

        #region private methods

        // Move forward from the forDate and find a set of Prices which were recently uploaded..
        private DateTime? GetFirstDailyPriceDate(DateTime forDate)
        {
            using (var db = new RepositoryContext())
            {
                var priceDates =
                    db.FileUploads.Where(x => x.Status.Id == 10 && x.UploadType.Id == (int)FileUploadTypes.DailyPriceData && x.UploadDateTime.Month == forDate.Month && x.UploadDateTime.Day == forDate.Day && x.UploadDateTime.Year == forDate.Year)
                        .ToList();

                // success status
                //.Select(x => x.DailyUpload.UploadDateTime)
                //.Where(x => DbFunctions.DiffDays(x, forDate) >= 1) // UploadDate - forDate >= 1
                if (priceDates.Any())
                {
                    return priceDates.First().UploadDateTime;
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

                var fileUpload_DailyPriceData_today = _context.FileUploads.Where(
                              x =>
                                  x.UploadDateTime.Month == forUploadDate.Month &&
                                  x.UploadDateTime.Day == forUploadDate.Day &&
                                  x.UploadDateTime.Year == forUploadDate.Year && x.UploadTypeId == (int)FileUploadTypes.DailyPriceData && x.Status.Id == 10).ToList();

                var fileUploadId_DailyPriceData_today = fileUpload_DailyPriceData_today.Count > 0 ? fileUpload_DailyPriceData_today[0].Id : 0;



                var prices = db.DailyPrices.Include(x => x.DailyUpload)
                    .Where(
                        x =>
                                x.DailyUploadId.Value == fileUploadId_DailyPriceData_today).OrderByDescending(x => x.Id).ToList();
                 
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
            var price = sitePrices.FirstOrDefault(x => x.DateOfCalc.Equals(d) && x.FuelTypeId == fuelId);
            return (price == null)
                ? 0
                : (price.OverriddenPrice == 0) ? price.SuggestedPrice : price.OverriddenPrice;
        }

        private static List<PriceMovementFuelPriceItem> GetSiteFuelPricesOnDate(IEnumerable<SitePrice> sitePrices, DateTime d)
        {
            var fuelPrices = new List<PriceMovementFuelPriceItem>();
            fuelPrices.Add(new PriceMovementFuelPriceItem()
            {
                FuelTypeId = 2,
                PriceValue = GetSitePriceOnDate(sitePrices, d, 2), // Unleaded
            });
            fuelPrices.Add(new PriceMovementFuelPriceItem()
            {
                FuelTypeId = 6,
                PriceValue = GetSitePriceOnDate(sitePrices, d, 6) // Diesel
            });
            fuelPrices.Add(new PriceMovementFuelPriceItem()
            {
                FuelTypeId = 1,
                PriceValue = GetSitePriceOnDate(sitePrices, d, 1), // Super-unleaded,
            });
            return fuelPrices;
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

        private static List<string> SortBrandsWithGrocersAtTop(List<string> distinctBrands)
        {
            if (distinctBrands.Any())
            {
                // move 'grocers' to top of list (FC-164)
                foreach (var grocer in Grocers)
                    distinctBrands.Remove(grocer);

                // add to top list of (keeping in same order as original list)
                for (var index = Grocers.Count() - 1; index >= 0; index--)
                    distinctBrands.Insert(0, Grocers[index]);
            }
            return distinctBrands;
        }

        IEnumerable<SiteEmailAddressViewModel> IPetrolPricingRepository.GetSiteEmailAddresses(int siteId)
        {
            return _context.GetSiteEmailAddresses(siteId);
        }

        #endregion private methods
    }
}