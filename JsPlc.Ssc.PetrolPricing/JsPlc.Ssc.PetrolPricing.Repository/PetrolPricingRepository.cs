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
using JsPlc.Ssc.PetrolPricing.Core.Interfaces;
using EntityState = System.Data.Entity.EntityState;

using JsPlc.Ssc.PetrolPricing.Core.Diagnostics;
using JsPlc.Ssc.PetrolPricing.Repository.Dapper;
using Dapper;
using JsPlc.Ssc.PetrolPricing.Repository.Debugging;
using JsPlc.Ssc.PetrolPricing.Core.Settings;
using JsPlc.Ssc.PetrolPricing.Models.ViewModels.UserPermissions;
using JsPlc.Ssc.PetrolPricing.Models.ViewModels.Diagnostics;
using System.IO;
using JsPlc.Ssc.PetrolPricing.Models.ViewModels.SelfTest;

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
                Const.TESCOEXPRESS,
                Const.TESCOEXTRA,
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
            int pageSize = Constants.PricePageSize)
        {

            try
            {
                _logger.Debug("Started CallSitePriceSproc");

                var useNewCode = CoreSettings.RepositorySettings.SitePrices.UseStoredProcedure;
                var shouldCompareOldvsNew = CoreSettings.RepositorySettings.SitePrices.ShouldCompareWithOldCode;

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

                var dbList = new List<SitePriceViewModel>();
                foreach (Site site in filteredSainsburysSites)
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
                    sitePriceRow.Notes = site.Notes;
                    sitePriceRow.HasEmails = site.Emails.Any();
                    sitePriceRow.PriceMatchType = (PriceMatchType)site.PriceMatchType;
                    sitePriceRow.Emails = site.Emails.Select(x=>x.EmailAddress).ToList();

                    #region OLD CODE
                    if (useNewCode == false)
                    {
                        var TrialPrice = (int)site.CompetitorPriceOffset;
                        TrialPrice = TrialPrice * 10;

                        AddSitePriceRow(FuelTypeItem.Diesel, site, TrialPrice, forDate, catalistFileExits, sitePriceRow.FuelPrices);

                        AddSitePriceRow(FuelTypeItem.Unleaded, site, TrialPrice, forDate, catalistFileExits, sitePriceRow.FuelPrices);

                        AddSitePriceRow(FuelTypeItem.Super_Unleaded, site, TrialPrice, forDate, catalistFileExits, sitePriceRow.FuelPrices);
                    }
                    #endregion

                    dbList.Add(sitePriceRow);
                }

                //_logger.Debug("Started: GetNearbyGrocerPriceStatus");

                const int driveTime = 5;
                GetNearbyGrocerPriceStatus(forDate, dbList, driveTime);

                //_logger.Debug("Finished: GetNearbyGrocerPriceStatus");

                if (useNewCode)
                {
                    _logger.Debug("Started: AddFuelPricesRowsForSites");
                    AddFuelPricesRowsForSites(forDate, dbList);
                    _logger.Debug("Finished: AddFuelPricesRowsForSites");
                }

                // Apply5PMarkupForSuperUnleadedForNonCompetitorSites(dbList);

                if (shouldCompareOldvsNew)
                {
                    _logger.Debug("Started: DumpNewCodeFuelPrices");

                    DumpNewCodeFuelPrices(CoreSettings.RepositorySettings.SitePrices.CompareOutputFilename, forDate, dbList);
                    _logger.Debug("Finished: DumpNewCodeFuelPrices");
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

        private void GetNearbyGrocerPriceStatus(DateTime forDate, IEnumerable<SitePriceViewModel> sites, int driveTime)
        {
            if (sites == null || !sites.Any())
                return;

            var siteIds = sites.Select(x => x.SiteId.ToString()).Aggregate((x, y) => x + "," + y);

            var statuses = _context.GetNearbyGrocerPriceStatusForSites(forDate, siteIds, driveTime);
            foreach(var status in statuses)
            {
                var site = sites.First(x => x.SiteId == status.SiteId);

                site.HasNearbyCompetitorDieselPrice = status.HasNearbyCompetitorDieselPrice;
                site.HasNearbyCompetitorSuperUnleadedPrice = status.HasNearbyCompetitorSuperUnleadedPrice;
                site.HasNearbyCompetitorUnleadedPrice = status.HasNearbyCompetitorUnleadedPrice;
            }
        }

        /// <summary>
        /// DEBUG - dump original vs new code for Fuel Prices
        /// </summary>
        /// <param name="filename"></param>
        /// <param name="forDate"></param>
        /// <param name="sites"></param>
        private void DumpNewCodeFuelPrices(string filename, DateTime forDate, IEnumerable<SitePriceViewModel> sites) 
        {
            var dump = new System.Text.StringBuilder();

            var showIdentical = true;

            var siteIds = sites.Select(x => x.SiteId.ToString()).Aggregate((x, y) => x + "," + y);

            var calculatedPrices = _context.CalculateFuelPricesForSitesAndDate(forDate, siteIds);
            foreach (var site in sites)
            {
                var fuelPricesForSite = calculatedPrices.Where(x => x.SiteId == site.SiteId);

                var newCode_SuperUnleaded = fuelPricesForSite.FirstOrDefault(x => x.FuelTypeId == (int)FuelTypeItem.Super_Unleaded);
                var newCode_Unleaded = fuelPricesForSite.FirstOrDefault(x => x.FuelTypeId == (int)FuelTypeItem.Unleaded);
                var newCode_Diesel = fuelPricesForSite.FirstOrDefault(x => x.FuelTypeId == (int)FuelTypeItem.Diesel);

                var original_SuperUnleaded = site.FuelPrices.FirstOrDefault(x => x.FuelTypeId == (int)FuelTypeItem.Super_Unleaded);
                var original_Unleaded = site.FuelPrices.FirstOrDefault(x => x.FuelTypeId == (int)FuelTypeItem.Unleaded);
                var original_Diesel = site.FuelPrices.FirstOrDefault(x => x.FuelTypeId == (int)FuelTypeItem.Diesel);

                dump.AppendLine("Site: " + site.SiteId + " - Name: " + site.StoreName + " - StoreNo: " + site.StoreNo);
                dump.AppendLine("\t\t" + DumpSerialiseFuelPrice(FuelTypeItem.Super_Unleaded, original_SuperUnleaded, newCode_SuperUnleaded, showIdentical));
                dump.AppendLine("\t\t" + DumpSerialiseFuelPrice(FuelTypeItem.Unleaded, original_Unleaded, newCode_Unleaded, showIdentical));
                dump.AppendLine("\t\t" + DumpSerialiseFuelPrice(FuelTypeItem.Diesel, original_Diesel, newCode_Diesel, showIdentical));
                dump.AppendLine();

                dump.AppendLine();
            }

            System.IO.File.WriteAllText(filename, dump.ToString());
        }

        private string DumpSerialiseFuelPrice(FuelTypeItem fuelType, FuelPriceViewModel originalfuelPrice, FuelPriceViewModel newfuelPrice, bool showIdentical)
        {
            var propertyIndentation = "\n\t\t\t\t";


            var oldAutoPrice = originalfuelPrice == null ? "NULL" : originalfuelPrice.AutoPrice.ToString();
            var newAutoPrice = newfuelPrice == null ? "NULL" : newfuelPrice.AutoPrice.ToString();

            var oldTodayPrice = originalfuelPrice == null ? "NULL" : originalfuelPrice.TodayPrice.ToString();
            var newTodayPrice = newfuelPrice == null ? "NULL" : newfuelPrice.TodayPrice.ToString();

            var oldYestPrice = originalfuelPrice == null ? "NULL" : originalfuelPrice.YestPrice.ToString();
            var newYestPrice = originalfuelPrice == null ? "NULL" : newfuelPrice.YestPrice.ToString();

            var oldMarkup = originalfuelPrice == null ? "NULL" : originalfuelPrice.Markup.ToString();
            var newMarkup = newfuelPrice == null ? "NULL" : newfuelPrice.Markup.ToString();

            var oldCompetitorName = originalfuelPrice == null ? "NULL" : originalfuelPrice.CompetitorName;
            var newCompetitorName = newfuelPrice == null ? "NULL" : newfuelPrice.CompetitorName;

            var oldIsTrailPrice = originalfuelPrice == null ? "NULL" : originalfuelPrice.IsTrailPrice.ToString();
            var newIsTrailPrice = newfuelPrice == null ? "NULL" : newfuelPrice.IsTrailPrice.ToString();

            var oldDifference = originalfuelPrice == null ? "NULL" : originalfuelPrice.Difference.ToString();
            var newDifference = newfuelPrice == null ? "NULL" : newfuelPrice.Difference.ToString();

            var oldCompetitorPriceOffset = originalfuelPrice == null ? "NULL" : originalfuelPrice.CompetitorPriceOffset.ToString();
            var newCompetitorPriceOffset = newfuelPrice == null ? "NULL" : newfuelPrice.CompetitorPriceOffset.ToString();

            var oldIsBasedOnCompetitor = originalfuelPrice == null ? "NULL" : originalfuelPrice.IsBasedOnCompetitor.ToString();
            var newIsBasedOnCompetitor = newfuelPrice == null ? "NULL" : newfuelPrice.IsBasedOnCompetitor.ToString();

            var result = new System.Text.StringBuilder();
            result.Append("[" + fuelType.ToString().ToUpper() + "] : ");

            DebugAppendComparision(result, propertyIndentation, "AutoPrice", showIdentical, oldAutoPrice, newAutoPrice);
            DebugAppendComparision(result, propertyIndentation, "TodayPrice", showIdentical, oldTodayPrice, newTodayPrice);
            DebugAppendComparision(result, propertyIndentation, "YestPrice", showIdentical, oldYestPrice, newYestPrice);
            DebugAppendComparision(result, propertyIndentation, "Markup", showIdentical, oldMarkup, newMarkup);
            DebugAppendComparision(result, propertyIndentation, "CompetitorName", showIdentical, oldCompetitorName, newCompetitorName);
            DebugAppendComparision(result, propertyIndentation, "IsTrialPrice", showIdentical, oldIsTrailPrice, newIsTrailPrice);
            DebugAppendComparision(result, propertyIndentation, "Difference", showIdentical, oldDifference, newDifference);
            DebugAppendComparision(result, propertyIndentation, "CompetitorPriceOffset", showIdentical, oldCompetitorPriceOffset, newCompetitorPriceOffset);
            DebugAppendComparision(result, propertyIndentation, "IsBasedOnCompetitor", showIdentical, oldIsBasedOnCompetitor, newIsBasedOnCompetitor);

            return result.ToString();
        }

        private void DebugAppendComparision(System.Text.StringBuilder sb, string indentation, string fieldName, bool showIdentical, string oldValue, string newValue)
        {
            var isDifferent = oldValue != newValue;
            var indicator = isDifferent ? "**" : "  ";

            if (showIdentical || isDifferent)
            {
                sb.Append(indentation);
                sb.AppendFormat(" {0} {1}: {2} --VS-- {3}", indicator, fieldName.PadRight(25,' '), oldValue, newValue);
            }
        }

        private void AddFuelPricesRowsForSites(DateTime forDate, List<SitePriceViewModel> sites)
        {
            if (sites == null || !sites.Any())
                return;

            var siteIds = sites.Select(x => x.SiteId.ToString()).Aggregate((x, y) => x + "," + y);

            DiagnosticLog.StartDebug("AddFuelPricesRowsForSites - Calling SP");

            var calculatedPrices = _context.CalculateFuelPricesForSitesAndDate(forDate, siteIds);

            DiagnosticLog.StartDebug("AddFuelPricesRowsForSites - Finished SP");

            foreach (var site in sites)
            {
                var fuelPricesForSite = calculatedPrices.Where(x => x.SiteId == site.SiteId);

                var superUnleaded = fuelPricesForSite.First(x => x.FuelTypeId == (int)FuelTypeItem.Super_Unleaded);
                var unleaded = fuelPricesForSite.First(x => x.FuelTypeId == (int)FuelTypeItem.Unleaded);
                var diesel = fuelPricesForSite.First(x => x.FuelTypeId == (int)FuelTypeItem.Diesel);

                superUnleaded.HasNearbyCompetitorPrice = site.HasNearbyCompetitorUnleadedPrice;
                unleaded.HasNearbyCompetitorPrice = site.HasNearbyCompetitorUnleadedPrice;
                diesel.HasNearbyCompetitorPrice = site.HasNearbyCompetitorDieselPrice;

                site.FuelPrices.Add(superUnleaded);
                site.FuelPrices.Add(unleaded);
                site.FuelPrices.Add(diesel);


                var hasUnleadedAutoPrice = unleaded.AutoPrice.HasValue && unleaded.AutoPrice != 0;
                var hasUnleadedTodayPrice = unleaded.TodayPrice.HasValue && unleaded.TodayPrice != 0;


                superUnleaded.AutoPrice = hasUnleadedAutoPrice ? unleaded.AutoPrice.Value + 50 : (int?)null;
                superUnleaded.TodayPrice = hasUnleadedTodayPrice ? unleaded.TodayPrice.Value + 50 : (int?)null;
                superUnleaded.Difference = hasUnleadedAutoPrice && hasUnleadedTodayPrice
                    ? unleaded.TodayPrice - unleaded.AutoPrice
                    : null;
            }
        }

        private void AddSitePriceRow(FuelTypeItem fuelType, Site site, int trialPrice, DateTime forDate,
            bool doesCatalistFileExist, List<FuelPriceViewModel> list)
        {
            int fuelMarkup = 0;
            var orgFuelTypeID = fuelType;

            if (fuelType == FuelTypeItem.Super_Unleaded)
            {
                fuelType = FuelTypeItem.Unleaded;
                fuelMarkup = 50;
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

        private int GetMarkupForFuel(FuelTypeItem fuelType)
        {
            return fuelType == FuelTypeItem.Super_Unleaded
                ? 50
                : 0;
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

        /// <summary>
        /// Apply a 5P markup for Super unleaded for non-competing sites
        /// </summary>
        /// <param name="dbList"></param>
        private static void Apply5PMarkupForSuperUnleadedForNonCompetitorSites(List<SitePriceViewModel> dbList)
        {
            foreach (var site in dbList)
            {
                var unleaded = site.FuelPrices.FirstOrDefault(x => x.FuelTypeId == (int) FuelTypeItem.Unleaded);
                //if (unleaded != null && unleaded.IsBasedOnCompetitor == false)
                if (unleaded != null)
                {
                    var superunleaded =
                        site.FuelPrices.FirstOrDefault(x => x.FuelTypeId == (int) FuelTypeItem.Super_Unleaded);
                    //if (superunleaded != null && superunleaded.IsBasedOnCompetitor == false && unleaded.AutoPrice.HasValue && unleaded.AutoPrice.Value > 0)
                    if (superunleaded != null && unleaded.AutoPrice.HasValue && unleaded.AutoPrice.Value > 0 &&
                        unleaded.TodayPrice.HasValue && unleaded.TodayPrice.Value > 0)
                    {
                        superunleaded.Markup = 5;
                        superunleaded.AutoPrice = unleaded.AutoPrice.Value + 50;
                        superunleaded.AutoPrice = (superunleaded.AutoPrice/10)*10 + 9;
                        superunleaded.TodayPrice = unleaded.TodayPrice.Value + 50;
                        superunleaded.TodayPrice = (superunleaded.TodayPrice/10)*10 + 9;
                    }
                }
            }
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

            var useNewCode = CoreSettings.RepositorySettings.CompetitorPrices.UseStoredProcedure;
            var shouldCompareData = CoreSettings.RepositorySettings.CompetitorPrices.ShouldCompareWithOldCode;
            var debugOutputFile = CoreSettings.RepositorySettings.CompetitorPrices.CompareOutputFilename;

            if (useNewCode)
                return _context.GetCompetitorsWithPriceView(forDate, siteId);

            //
            // OLD CODE
            //
            try
            {
                var lastSiteId = -1;
                SitePriceViewModel sitePriceRow = null;

                var dbList = new List<SitePriceViewModel>();

                var listOfbrands = GetExcludeBrands();
                var allSites = GetSites();
                Site Jssite = allSites.Where(x => x.Id == siteId).FirstOrDefault();
                List<SiteToCompetitor> competitors = _context.SiteToCompetitors.Where(x => x.SiteId == siteId && x.DriveTime < 25 && x.IsExcluded == 0).ToList();

                var forDateNextDay = forDate.Date.AddDays(1);

                var yesterday = forDate.AddDays(-1);
                var yesterdayNextDay = yesterday.Date.AddDays(1);

                ///////////////////////////////////

                var fileUpload_LatestCompPriceData_today = _context.FileUploads.Where(
                                x =>
                                    x.UploadDateTime >= forDate && x.UploadDateTime < forDateNextDay
                                    && x.UploadTypeId == (int)FileUploadTypes.LatestCompPriceData 
                                    && x.Status.Id == 10)
                                    .OrderByDescending(x => x.Id)
                                    .FirstOrDefault();

                List<LatestCompPrice> latestCompPrices_today = null;
                if (fileUpload_LatestCompPriceData_today != null)
                {
                    latestCompPrices_today = _context.LatestCompPrices.Where(x => x.UploadId == fileUpload_LatestCompPriceData_today.Id)
                        .ToList();
                }


                ///////////////////////////////////

                var fileUpload_DailyPriceData_today = _context.FileUploads.Where(
                                x =>
                                    x.UploadDateTime >= forDate && x.UploadDateTime < forDateNextDay
                                    && x.UploadTypeId == (int)FileUploadTypes.DailyPriceData 
                                    && x.Status.Id == 10)
                                    .OrderByDescending(x => x.Id)
                                    .FirstOrDefault();
                
                List<DailyPrice> dailypriceList_today = null;
                if (fileUpload_DailyPriceData_today != null)
                {
                    dailypriceList_today = _context.DailyPrices.Include(x => x.DailyUpload)
                        .Where(
                            x =>
                                    x.DailyUploadId.Value == fileUpload_DailyPriceData_today.Id)
                                    .ToList();
                }

                ///////////////////////////////////

                var fileUpload_LatestCompPriceData_yday = _context.FileUploads.Where(
                           x =>
                               x.UploadDateTime >= yesterday && x.UploadDateTime < yesterdayNextDay
                               && x.UploadTypeId == (int)FileUploadTypes.LatestCompPriceData 
                               && x.Status.Id == 10)
                               .OrderByDescending(x => x.Id)
                               .FirstOrDefault();

                List<LatestCompPrice> latestCompPrices_yday = null;
                if (fileUpload_LatestCompPriceData_today != null)
                {
                    latestCompPrices_yday = _context.LatestCompPrices.Where(x => x.UploadId == fileUpload_LatestCompPriceData_yday.Id)
                        .ToList();
                }

                ///////////////////////////////////

                var fileUpload_DailyPriceData_yday = _context.FileUploads.Where(
                                x =>
                                    x.UploadDateTime >= yesterday && x.UploadDateTime < yesterdayNextDay
                                    && x.UploadTypeId == (int)FileUploadTypes.DailyPriceData 
                                    && x.Status.Id == 10)
                                    .OrderByDescending(x => x.Id)
                                    .FirstOrDefault();

                List<DailyPrice> dailypriceList_yday = null;
                if (fileUpload_DailyPriceData_today != null)
                {
                    dailypriceList_yday = _context.DailyPrices.Include(x => x.DailyUpload)
                        .Where(x =>x.DailyUploadId.Value == fileUpload_DailyPriceData_yday.Id)
                        .ToList();
                }

                ///////////////////////////////////

                var fuelTypes = new List<FuelTypeItem>()
                {
                    FuelTypeItem.Unleaded,
                    FuelTypeItem.Diesel,
                    FuelTypeItem.Super_Unleaded
                };


                foreach (var comp in competitors)
                {
                    Site Compsite = allSites.Where(x => x.Id == comp.CompetitorId).FirstOrDefault();

                    var result = listOfbrands.Contains(Compsite.Brand);

                    if (result == true || Compsite.IsActive==false) continue;
                    sitePriceRow = new SitePriceViewModel();

                    sitePriceRow.SiteId = comp.CompetitorId; // CompetitorId
                    sitePriceRow.JsSiteId = comp.SiteId;
                    sitePriceRow.CatNo = Compsite.CatNo;
                    sitePriceRow.StoreName = Compsite.SiteName;
                    sitePriceRow.Brand = Compsite.Brand;
                    sitePriceRow.Address = Compsite.Address;

                    sitePriceRow.DriveTime = comp.DriveTime;
                    sitePriceRow.Distance = comp.Distance;

                    sitePriceRow.Notes = Compsite.Notes;

                    sitePriceRow.FuelPrices = sitePriceRow.FuelPrices ?? new List<FuelPriceViewModel>();
                    int nOffSet = comp.CompetitorId == Jssite.TrailPriceCompetitorId ? (int)Jssite.CompetitorPriceOffsetNew : 0;

                    foreach (var fuelType in fuelTypes)
                    {
                        AddCompetitorFuelPrice(sitePriceRow.FuelPrices, Jssite, Compsite, fuelType, nOffSet, forDate, latestCompPrices_today, dailypriceList_today, latestCompPrices_yday, dailypriceList_yday);
                    }

                    dbList.Add(sitePriceRow);
                }

                if (shouldCompareData)
                {
                    var newCodeData = _context.GetCompetitorsWithPriceView(forDate, siteId);

                    var debugLog = new DebugCompareCompetitorPrices();
                    debugLog.Compare(dbList, newCodeData);

                    debugLog.WriteToFile(debugOutputFile);
                }

                return dbList;
            }
            catch (Exception ce)
            {
                _logger.Error(ce);
                return null;
            }
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

        private void FillGetCompetitorsWithPriceView(SitePriceViewModel arg1, SqlMapper.GridReader arg2)
        {
            throw new NotImplementedException();
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

        public void AddOrUpdateLatestPrice(RepositoryContext newDbContext,LatestPriceDataModel latestPriceDataModel, FileUpload fileDetails,int fuelTypeId,int fuelPrice)
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
            dbRecord.ModalPrice = fuelPrice * 10;
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
                    var brandAvg = new NationalAverageReportBrandViewModel();
                    fuelRow.Brands.Add(brandAvg);
                    brandAvg.BrandName = brand;

                    var brandCatsNos = competitorSites.Where(x => brand == Const.UK ? true : x.Brand == brand).Where(x => x.CatNo.HasValue).Select(x => x.CatNo.Value).ToList();
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
                forDate = forDate.Date; // remove time so its Date only

                var fuelTypesList = new[] { 2, 6, 1 }; // Unl, Diesel, Super
                var nextday = forDate.AddDays(1);
             //   var prevday = forDate;

                //var fileUpload_LatestCompPriceData_PrevDay = _context.FileUploads.Where(
                //               x =>
                //                   x.UploadDateTime.Month == prevday.Month &&
                //                   x.UploadDateTime.Day == prevday.Day &&
                //                   x.UploadDateTime.Year == prevday.Year && x.UploadTypeId == (int)FileUploadTypes.DailyPriceData && x.Status.Id == 10)
                //                   .OrderByDescending(x => x.Id).ToList();

                //var fileUploadId_LatestCompPriceData_PrevDay = fileUpload_LatestCompPriceData_PrevDay.Count > 0 ? fileUpload_LatestCompPriceData_PrevDay[0].Id : 0;

                var fileUpload_LatestCompPriceData_PrevDay = _context.FileUploads.OrderByDescending(x => x.Id)
                    .FirstOrDefault(x =>
                        x.StatusId == 10
                        && x.UploadTypeId == (int)FileUploadTypes.DailyPriceData
                    );

                var fileUploadId_LatestCompPriceData_PrevDay = fileUpload_LatestCompPriceData_PrevDay == null ? 0 : fileUpload_LatestCompPriceData_PrevDay.Id;

                var dailyPrices = new List<DailyPrice>();
                dailyPrices = GetDailyPricesForDate(nextday);

                var siteVMLlist = GetSitesWithPrices(nextday.AddDays(-1));

                if (dailyPrices.Count == 0 || siteVMLlist.Count() == 0) return null;

                var sites = GetJsSites();

                var reportFuels = GetFuelTypes().Where(x => fuelTypesList.Contains(x.Id)).ToList();

               
                foreach (var site in sites)
                {
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
                            int overridePrice = fuelPrice.OverridePrice.HasValue? fuelPrice.OverridePrice.Value:0;
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

                        dataItem.Diff = (dataItem.CatPriceValue - dataItem.ExpectedPriceValue) / 10;
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
                    UserDiagnosticsAccess = new UserDiagnosticsAccess(diagnosticsPermissions)
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
                throw new Exception("File does not exist");

            var fullPath = System.IO.Path.Combine(fileUploadPath, fileUpload.StoredFileName);
            var fileBytes = System.IO.File.ReadAllBytes(fullPath);

            return new FileDownloadViewModel()
            {
                FileName = fileUpload.OriginalFileName,
                FileBytes = fileBytes
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

        public SystemSettings GetSystemSettings()
        {
            return _context.SystemSettings.FirstOrDefault();
        }

        public void UpdateSystemSettings(SystemSettings systemSettings)
        {
            _context.SaveChanges();
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

            var model = new QuarterlySiteAnalysisReportViewModel()
            {
                NewSiteCount = 0,
                DeletedSiteCount = 0,
                ExistingSiteCount = 0,
                TotalSiteCount = 0,
                ChangeOwnershipCount = 0,
                Rows = _context.GetQuarterlySiteAnalysisReportRows(leftId, rightId)
            };

            // gather statistics
            if (model.Rows.Any())
            {
                model.NewSiteCount = model.Rows.Count(x => x.WasSiteAdded);
                model.DeletedSiteCount = model.Rows.Count(x => x.WasSiteDeleted);
                model.ExistingSiteCount = model.Rows.Count(x => !x.WasSiteAdded && !x.WasSiteDeleted);
                model.TotalSiteCount = model.Rows.Count();
                model.ChangeOwnershipCount = model.Rows.Count(x => x.HasOwnershipChanged);
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


        #endregion private methods

    }
}