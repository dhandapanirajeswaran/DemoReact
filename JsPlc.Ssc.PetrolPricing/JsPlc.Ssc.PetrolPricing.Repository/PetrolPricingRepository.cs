using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Migrations;
using System.Data.Entity.SqlServer;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using System.Security.Permissions;
using System.Threading.Tasks;
using System.Transactions;
using EntityFramework.Utilities;
using JsPlc.Ssc.PetrolPricing.Models;

using System.Data.Entity.Validation;
using System.Diagnostics;
using JsPlc.Ssc.PetrolPricing.Models.Common;
using JsPlc.Ssc.PetrolPricing.Models.ViewModels;
using MoreLinq;
using EntityState = System.Data.Entity.EntityState;

namespace JsPlc.Ssc.PetrolPricing.Repository
{
    public class PetrolPricingRepository : IPetrolPricingRepositoryLookup, IPetrolPricingRepository, IDisposable
    {
        private readonly RepositoryContext _context;

        public PetrolPricingRepository()
        {
        }

        public PetrolPricingRepository(RepositoryContext context)
        {
            _context = context;
        }

        public IEnumerable<AppConfigSettings> GetAppConfigSettings()
        {
            return _context.AppConfigSettings.ToList();
        }

        public IEnumerable<Site> GetJsSites()
        {
            return _context.Sites
                //.Include(s => s.Emails)
                .Where(s => s.IsSainsburysSite)
                .AsNoTracking()
                .OrderBy(q => q.Id).ToArray();
        }

        public IEnumerable<Site> GetSites()
        {
            return _context.Sites
                .Include(s => s.Emails)
                .OrderBy(q => q.Id);
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

        public IEnumerable<Site> GetSitesWithCompetitors()
        {
            var retval = _context.Sites
                .Include(x => x.Competitors)
                .Where(x => x.IsActive)
                .OrderBy(q => q.Id).ToList();
            return retval;
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

            int daysBetweenFromAndTo = Convert.ToInt32((toPriceDate.Value - fromPriceDate.Value).TotalDays);
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
                _context.Entry(site).State = EntityState.Detached;
                var prices = getPricesForSite(site.Id);
                site.Prices = new List<SitePrice>();
                site.Prices = prices;
                sites.Add(site);
            }
            return sites;
        }

        public IEnumerable<SitePriceViewModel> GetSitesWithPrices(DateTime forDate, int siteId = 0, int pageNo = 1,
            int pageSize = Constants.PricePageSize)
        {
            var retval = CallSitePriceSproc(forDate, siteId, pageNo, pageSize);

            return retval;
        }

        public IEnumerable<SitePriceViewModel> GetCompetitorsWithPrices(DateTime forDate, int siteId = 0, int pageNo = 1,
            int pageSize = Constants.PricePageSize)
        {
            var retval = CallCompetitorsWithPriceSproc(forDate, siteId, pageNo, pageSize);

            return retval;
        }

        public SitePriceViewModel GetASiteWithPrices(int siteId, DateTime forDate)
        {
            var listSitePlusPrice = CallSitePriceSproc(forDate, siteId, 1, 1);

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
        private IEnumerable<SitePriceViewModel> CallSitePriceSproc(DateTime forDate, int siteId = 0, int pageNo = 1,
            int pageSize = Constants.PricePageSize)
        {
            //@siteId int,
            //@forDate DateTime,
            //@skipRecs int,
            //@takeRecs int

            var siteIdParam = new SqlParameter("@siteId", SqlDbType.Int)
            {
                Value = siteId
            };
            var forDateParam = new SqlParameter("@forDate", SqlDbType.DateTime)
            {
                Value = forDate
            };
            var skipRecsParam = new SqlParameter("@skipRecs", SqlDbType.Int)
            {
                Value = (pageNo - 1)*pageSize
            };
            var takeRecsParam = new SqlParameter("@takeRecs", SqlDbType.Int)
            {
                Value = pageSize
            };
            // any other params here

            var sqlParams = new List<SqlParameter>
            {
                siteIdParam,
                forDateParam,
                skipRecsParam,
                takeRecsParam
            };
            const string spName = "dbo.spGetSitePrices";
            // Test in SQL:     Exec dbo.spGetSitePrices 0, '2015-11-30'
            // Output is sorted by siteId so all Fuels for a site appear together

            using (var connection = new SqlConnection(_context.Database.Connection.ConnectionString))
            {
                using (var command = new SqlCommand(spName, connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddRange(sqlParams.ToArray());

                    connection.Open();

                    var reader = command.ExecuteReader();
                    var pgTable = new DataTable();
                    pgTable.Load(reader);

                    var lastSiteId = -1;
                    SitePriceViewModel sitePriceRow = null;

                    var dbList = new List<SitePriceViewModel>();

                    foreach (DataRow pgRow in pgTable.Rows)
                    {
                        var loopSiteId = (int) pgRow["siteId"];
                        if (loopSiteId != lastSiteId)
                        {
                            sitePriceRow = new SitePriceViewModel();
                            dbList.Add(sitePriceRow);
                            lastSiteId = loopSiteId;
                        }
                        if (sitePriceRow == null) continue;

                        sitePriceRow.SiteId = (int) pgRow["SiteId"];
                        sitePriceRow.CatNo = Convert.IsDBNull(pgRow["CatNo"]) ? null : (int?) pgRow["CatNo"];
                            // ToNullable<int> or ToNullable<double>
                        sitePriceRow.StoreName = (string) pgRow["SiteName"];
                        sitePriceRow.Address = (string) pgRow["Address"];
                        // any other fields for UI extract here

                        sitePriceRow.FuelPrices = sitePriceRow.FuelPrices ?? new List<FuelPriceViewModel>();
                        if (!Convert.IsDBNull(pgRow["FuelTypeId"]))
                        {
                            var AutoPrice = pgRow["SuggestedPrice"].ToString().ToNullable<int>();
                            var OverridePrice = pgRow["OverriddenPrice"].ToString().ToNullable<int>();
                            var OverriddenPriceToday = pgRow["OverriddenPriceToday"].ToString().ToNullable<int>();
                            var SuggestedPriceToday = pgRow["SuggestedPriceToday"].ToString().ToNullable<int>();
                            var TodayPrice = (OverriddenPriceToday.HasValue && OverriddenPriceToday.Value != 0)
                                ? OverriddenPriceToday.Value
                                : (SuggestedPriceToday.HasValue) ? SuggestedPriceToday.Value : 0;
                            sitePriceRow.FuelPrices.Add(new FuelPriceViewModel
                            {
                                FuelTypeId = (int) pgRow["FuelTypeId"],
                                // Tomorrow's prices
                                AutoPrice = (!AutoPrice.HasValue) ? 0 : AutoPrice.Value,
                                OverridePrice = (!OverridePrice.HasValue) ? 0 : OverridePrice.Value,

                                // Today's prices (whatever was calculated yesterday OR last)
                                TodayPrice = TodayPrice
                            });
                        }
                    }
                    return dbList;
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
            // TODO wireup params from sproc to a new DTO
            var siteIdParam = new SqlParameter("@siteId", SqlDbType.Int)
            {
                Value = siteId
            };
            var forDateParam = new SqlParameter("@forDate", SqlDbType.DateTime)
            {
                Value = forDate
            };

            // NOTE: Below paging params are for JSSite(s), not for competitors (we get all competitors for the specified sites resultset)
            var skipRecsParam = new SqlParameter("@skipRecs", SqlDbType.Int)
            {
                Value = (pageNo - 1)*pageSize
            };
            var takeRecsParam = new SqlParameter("@takeRecs", SqlDbType.Int)
            {
                Value = pageSize
            };
            // any other params here

            var sqlParams = new List<SqlParameter>
            {
                siteIdParam,
                forDateParam,
                skipRecsParam,
                takeRecsParam
            };
            const string spName = "dbo.spGetCompetitorPrices";
            // Test in SQL:     Exec dbo.[spGetCompetitorPrices] 0, '2015-11-30', 
            // Output is sorted by siteId so all Fuels for a site appear together

            using (var connection = new SqlConnection(_context.Database.Connection.ConnectionString))
            {
                using (var command = new SqlCommand(spName, connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddRange(sqlParams.ToArray());

                    connection.Open();

                    var reader = command.ExecuteReader();
                    var pgTable = new DataTable();
                    pgTable.Load(reader);

                    var lastSiteId = -1;
                    SitePriceViewModel sitePriceRow = null;

                    var dbList = new List<SitePriceViewModel>();

                    foreach (DataRow pgRow in pgTable.Rows)
                    {
                        var loopSiteId = (int) pgRow["siteId"];
                        if (loopSiteId != lastSiteId)
                        {
                            sitePriceRow = new SitePriceViewModel();
                            dbList.Add(sitePriceRow);
                            lastSiteId = loopSiteId;
                        }
                        if (sitePriceRow == null) continue;

                        sitePriceRow.SiteId = (int) pgRow["SiteId"]; // CompetitorId
                        sitePriceRow.JsSiteId = (int) pgRow["JsSiteId"];
                        sitePriceRow.CatNo = Convert.IsDBNull(pgRow["CatNo"]) ? null : (int?) pgRow["CatNo"];
                            // ToNullable<int> or ToNullable<double>
                        sitePriceRow.StoreName = (string) pgRow["SiteName"];
                        sitePriceRow.Brand = (string) pgRow["Brand"];
                        sitePriceRow.Address = (string) pgRow["Address"];

                        sitePriceRow.DriveTime = pgRow["DriveTime"].ToString().ToNullable<float>();
                        sitePriceRow.Distance = pgRow["Distance"].ToString().ToNullable<float>();
                        // any other fields for UI extract here

                        sitePriceRow.FuelPrices = sitePriceRow.FuelPrices ?? new List<FuelPriceViewModel>();
                        if (!Convert.IsDBNull(pgRow["FuelTypeId"]))
                        {
                            //var AutoPrice = pgRow["SuggestedPrice"].ToString().ToNullable<int>();
                            //var OverridePrice = pgRow["OverriddenPrice"].ToString().ToNullable<int>();
                            var TodayPrice = pgRow["ModalPrice"].ToString().ToNullable<int>();
                            var YestPrice = pgRow["ModalPriceYest"].ToString().ToNullable<int>();
                            sitePriceRow.FuelPrices.Add(new FuelPriceViewModel
                            {
                                FuelTypeId = (int) pgRow["FuelTypeId"],
                                //// Tomorrow's prices
                                //AutoPrice = (!AutoPrice.HasValue) ? 0 : AutoPrice.Value,
                                //OverridePrice = (!OverridePrice.HasValue) ? 0 : OverridePrice.Value,

                                // Today's prices (whatever was calculated yesterday OR last)
                                TodayPrice = TodayPrice,

                                // Today's prices (whatever was calculated yesterday OR last)
                                YestPrice = YestPrice
                            });
                        }
                    }
                    return dbList;
                        // .OrderBy(x => x.JsSiteId).ThenBy(x => x.SiteId).ThenBy(x => x.DriveTime); // TODO ordering doesnt work, hmmm...
                }
            }
        }

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
            // If multiple uploads, needs to be handled here, but we assume one for now.
            IEnumerable<DailyPrice> dailyPrices = _context.DailyPrices.Include(x => x.DailyUpload);

            return dailyPrices.Where(x => competitorCatNos.Contains(x.CatNo) &&
                                          x.FuelTypeId == fuelId &&
                                          x.DailyUpload.UploadDateTime.Date.Equals(usingPricesforDate.Date)).ToList();
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
            //_db.Sites.AddOrUpdate(site);
            //_db.SaveChanges();

            _context.Entry(site).State = EntityState.Modified;

            try
            {
                _context.Sites.Attach(site);
                UpdateSiteEmails(site);
                _context.Entry(site).State = EntityState.Modified;
                _context.SaveChanges();

                return true;
            }
            catch
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

        public bool NewQuarterlyRecords(List<CatalistQuarterly> siteCatalistData, FileUpload fileDetails,
            int startingLineNumber)
        {
            int addingEntryLineNo = startingLineNumber;

            using (var newDbContext = new RepositoryContext(new SqlConnection(_context.Database.Connection.ConnectionString)))
            {
                using (var tx = newDbContext.Database.BeginTransaction())
                {
                    newDbContext.Configuration.AutoDetectChangesEnabled = false;
                    try
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

        public bool NewDailyPrices(List<DailyPrice> dailyPriceList, FileUpload fileDetails, int startingLineNumber)
        {
            int addingEntryLineNo = startingLineNumber;

            using (
                var newDbContext =
                    new RepositoryContext(new SqlConnection(_context.Database.Connection.ConnectionString)))
            {
                using (var tx = newDbContext.Database.BeginTransaction())
                {
                    newDbContext.Configuration.AutoDetectChangesEnabled = false;
                    try
                    {
                        foreach (DailyPrice dailyPrice in dailyPriceList)
                        {
                            if (dailyPrice.DailyUpload != null) dailyPrice.DailyUploadId = dailyPrice.DailyUpload.Id;
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

        //public bool UpdateCatalistQuarterlyData(List<CatalistQuarterly> siteCatalistData, FileUpload fileDetails,
        //    bool isSainsburys)
        //{
        //    int addingEntryLineNo = 0;
        //    int savetrigger = 0;

        //    using (
        //        var newDbContext =
        //            new RepositoryContext(new SqlConnection(_context.Database.Connection.ConnectionString)))
        //    {
        //        using (var tx = newDbContext.Database.BeginTransaction()) // TODO, refactor this massive transaction.. 
        //        {
        //            newDbContext.Configuration.AutoDetectChangesEnabled = false;

        //            try
        //            {
        //                foreach (CatalistQuarterly CA in siteCatalistData)
        //                {
        //                    //if (addingEntryLineNo > 1000)
        //                    //{
        //                    //    newDbContext.SaveChanges();
        //                    //    //tx.Commit();
        //                    //}
        //                    Site site = new Site();

        //                    site.CatNo = Convert.ToInt32(CA.CatNo);

        //                    var result = newDbContext.Sites.SingleOrDefault(x => x.CatNo == site.CatNo);

        //                    if (result != null)
        //                    {
        //                        newDbContext.Entry(result).State = EntityState.Modified;

        //                        result.CatNo = Convert.ToInt32(CA.CatNo);
        //                        result.SiteName = CA.SiteName;
        //                        result.Town = CA.Town;
        //                        result.Brand = CA.Brand;
        //                        result.Address = CA.Address;
        //                        result.Suburb = CA.Suburb;
        //                        result.PostCode = CA.Postcode;
        //                        result.Company = CA.CompanyName;
        //                        result.Ownership = CA.Ownership;

        //                        if (CA.Brand == "SAINSBURYS")
        //                        {
        //                            result.IsSainsburysSite = true;
        //                        }
        //                    }
        //                    else
        //                    {
        //                        site.CatNo = Convert.ToInt32(CA.CatNo);
        //                        site.SiteName = CA.SiteName;
        //                        site.Town = CA.Town;
        //                        site.Brand = CA.Brand;
        //                        site.Address = CA.Address;
        //                        site.Suburb = CA.Suburb;
        //                        site.PostCode = CA.Postcode;
        //                        site.Ownership = CA.Ownership;
        //                        site.Company = CA.CompanyName;
        //                        site.IsActive = true;

        //                        if (CA.Brand == "SAINSBURYS")
        //                        {
        //                            site.IsSainsburysSite = true;
        //                        }

        //                        newDbContext.Sites.Add(site);
        //                    }

        //                    addingEntryLineNo += 1;
        //                }

        //                newDbContext.SaveChanges();

        //                tx.Commit();
        //                return true;
        //            }
        //            catch (DbUpdateException e)
        //            {

        //                tx.Rollback();

        //                foreach (var dbUpdateException in e.Entries)
        //                {
        //                    var dailyPrice = dbUpdateException.Entity as DailyPrice ?? new DailyPrice();
        //                    LogImportError(fileDetails, String.Format("Failed to save price:{0},{1},{2},{3},{4}",
        //                        dailyPrice.CatNo, dailyPrice.FuelTypeId, dailyPrice.AllStarMerchantNo,
        //                        dailyPrice.DateOfPrice, dailyPrice.ModalPrice)
        //                        , addingEntryLineNo);
        //                    dbUpdateException.State = EntityState.Unchanged;
        //                }

        //                return false;
        //            }
        //            catch (DbEntityValidationException dbEx)
        //            {
        //                tx.Rollback();
        //                foreach (var validationErrors in dbEx.EntityValidationErrors)
        //                {
        //                    foreach (var validationError in validationErrors.ValidationErrors)
        //                    {
        //                        LogImportError(fileDetails,
        //                            "DbEntityValidationException occured:" + validationError.ErrorMessage +
        //                            "," + validationError.PropertyName, addingEntryLineNo);

        //                        Trace.TraceInformation("Property: {0} Error: {1}", validationError.PropertyName,
        //                            validationError.ErrorMessage);
        //                    }
        //                }
        //                return false;
        //            }
        //        }
        //    }
        //    return true;
        //}

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

        /// <summary>
        /// Delete all QuarterlyUploadStaging records prior to starting Import of QuarterlyUploadStaging
        /// </summary>
        public Task<bool> DeleteRecordsForQuarterlyUploadStaging()
        {
            using (var db = new RepositoryContext(new SqlConnection(_context.Database.Connection.ConnectionString)))
            {
                var deleteCmd = String.Format("Truncate table QuarterlyUploadStaging"); // automatically reseeds to 1
                db.Database.ExecuteSqlCommand(deleteCmd);
            }
            return Task.FromResult(true);
        }

        public Task<bool> ImportQuarterlyUploadStaging(int uploadId)
        {
            using (var db = new RepositoryContext(new SqlConnection(_context.Database.Connection.ConnectionString)))
            {
                var sprocCmd = String.Format("Exec dbo.spImportQuarterlyRecords {0}", uploadId);
                db.Database.ExecuteSqlCommand(sprocCmd); // if exception let it bubble up
            }
            return Task.FromResult(true);
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
            var db = new RepositoryContext(new SqlConnection(_context.Database.Connection.ConnectionString));
            var deleteCmd = String.Format("Delete from DailyPrice Where DailyUploadId in " +
                                          "  (Select Id from FileUpload Where DateDiff(d, UploadDateTime, '{0}') = 0 and Id <> {1})",
                ofdate.ToString("yyyy-MM-dd"), uploadId);
            db.Database.ExecuteSqlCommand(deleteCmd);
        }

        public bool AnyDailyPricesForFuelOnDate(int fuelId, DateTime usingPricesforDate)
        {
            var list1 =
                _context.DailyPrices.Include(x => x.DailyUpload).Where(x => x.FuelTypeId.Equals(fuelId)).ToList();
            var list2 = list1.Any(x => x.DailyUpload.UploadDateTime.Date.Equals(usingPricesforDate.Date));
            return list1.Any() && list2;
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
            // Inline sql as date comparison fails in Linq
            var uploadIds = _context.Database.SqlQuery<int>(
                String.Format(
                    "Select distinct fu.Id from FileUpload fu, DailyPrice dp Where fu.Id = dp.DailyUploadId " +
                    " and fu.UploadTypeId = 1" +
                    " and DateDiff(d, UploadDateTime, '{0}') = 0 and fu.StatusId in (10, 16, 17)",
                    forDate.ToString("yyyy-MM-dd"))).ToArray();

            var fileUploads = _context.FileUploads.Where(x => uploadIds.Contains(x.Id));
            return fileUploads.Any() ? fileUploads.FirstOrDefault() : null;
        }

        /// <summary>
        /// Mark file status = Aborted for any 
        /// imports exceeding 1 min or 
        /// calcs exceeeding 5 min
        /// </summary>
        public void FailHangedFileUploadOrCalcs(int importTimeoutMilliSec, int calcTimeoutMilliSec)
        {
            _context.Database.ExecuteSqlCommand("Update FileUpload Set StatusId = 16 Where " +
                                                "StatusId = 5 and DateDiff(SECOND, UploadDateTime, GetDate()) >= " +
                                                importTimeoutMilliSec/1000);
            _context.Database.ExecuteSqlCommand("Update FileUpload Set StatusId = 17 Where " +
                                                "StatusId = 11 and DateDiff(SECOND, UploadDateTime, GetDate()) >= " +
                                                calcTimeoutMilliSec/1000);
        }

        public FileUpload GetDailyFileWithCalcRunningForDate(DateTime forDate)
        {
            var calcUploads = _context.FileUploads.Where(x => x.StatusId == 11 && x.StatusId == 1);
            if (!calcUploads.Any()) return null;

            var calcFileRunningForDate =
                calcUploads.ToList().FirstOrDefault(x => x.UploadDateTime.Date.Equals(forDate.Date));
            return calcFileRunningForDate; // could be null
        }

        public SitePrice AddOrUpdateSitePriceRecord(SitePrice calculatedSitePrice)
        {
            var priceRecords = _context.SitePrices.AsNoTracking().Where(
                x => x.SiteId == calculatedSitePrice.SiteId
                     && x.FuelTypeId == calculatedSitePrice.FuelTypeId).ToList();
            var existingPriceRecord =
                priceRecords.FirstOrDefault(x => x.DateOfCalc.Date.Equals(calculatedSitePrice.DateOfCalc.Date));

            if (existingPriceRecord == null)
            {
                calculatedSitePrice.JsSite = null;
                //_context.Entry(calculatedSitePrice).State = EntityState.Detached;
                //_context.SitePrices.Attach(calculatedSitePrice);
                _context.Entry(calculatedSitePrice).State = EntityState.Added;
                _context.SaveChanges();
                return calculatedSitePrice;
            }
            //_context.AttachAndModify(new SitePrice { Id = existingPriceRecord.Id }).Set(x => x.SuggestedPrice, calculatedSitePrice.SuggestedPrice);
            var db = new RepositoryContext(new SqlConnection(_context.Database.Connection.ConnectionString));
            {
                db.Entry(existingPriceRecord).State = EntityState.Modified;
                existingPriceRecord.SuggestedPrice = calculatedSitePrice.SuggestedPrice;
                existingPriceRecord.DateOfCalc = calculatedSitePrice.DateOfCalc;
                existingPriceRecord.DateOfPrice = calculatedSitePrice.DateOfPrice;
                existingPriceRecord.UploadId = calculatedSitePrice.UploadId;
                db.SaveChanges();
                return existingPriceRecord;
            }
        }

        public async Task<int> SaveOverridePricesAsync(List<SitePrice> prices, DateTime? forDate = null)
        {
            if (!forDate.HasValue) forDate = DateTime.Now;
            using (var db = new RepositoryContext(new SqlConnection(_context.Database.Connection.ConnectionString)))
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
                    if (entry == null)
                    {
                        throw new ApplicationException(
                            String.Format("Price not found in DB for siteId={0}, fuelId={1}", p1.SiteId, p1.FuelTypeId));
                    }
                    entry.OverriddenPrice = p.OverriddenPrice;
                    db.Entry(entry).State = EntityState.Modified;
                    //db.Entry(entry).Property(x => x.OverriddenPrice).IsModified = true;
                }
                int rowsAffected = await db.SaveChangesAsync();
                return rowsAffected;
            }
        }

    /// <summary>
        /// Generic method to Log an import error for the running import of FileUpload for both Daily and Quarterly files
        /// </summary>
        /// <param name="fileDetails"></param>
        /// <param name="errorMessage"></param>
        /// <param name="lineNumber"></param>
        public void LogImportError(FileUpload fileDetails, string errorMessage = "", int? lineNumber = 0)
        {
            var db = new RepositoryContext(new SqlConnection(_context.Database.Connection.ConnectionString));
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

        /// <summary>
        /// Updated the FileUpload status as specified by param StatusId
        /// </summary>
        /// <param name="fileUpload">The fileUpload object whose status is to be updated</param>
        /// <param name="statusId">Status to set for the uploaded file</param>
        public void UpdateImportProcessStatus(int statusId, FileUpload fileUpload)
        {
            var db = new RepositoryContext(new SqlConnection(_context.Database.Connection.ConnectionString));
            var fu = db.FileUploads.AsNoTracking().FirstOrDefault(x => x.Id == fileUpload.Id);
            if (fu == null) return;
            _context.Database.ExecuteSqlCommand("Update FileUpload Set StatusId = " + statusId + " where Id = " +
                                                fileUpload.Id);
            //fu.StatusId = statusId;
            //db.Entry(fu).State = EntityState.Modified;
            //db.SaveChanges();
        }

        public SiteToCompetitor LookupSiteAndCompetitor(int siteCatNo, int competitorCatNo)
        {
            return _context.SiteToCompetitors.FirstOrDefault(x => 
                    x.Site.CatNo.HasValue && x.Site.CatNo.Value == siteCatNo 
                &&  x.Competitor.CatNo.HasValue && x.Competitor.CatNo.Value == competitorCatNo);
        }
        /// <summary>
        /// Get competitors based on drivetime criteria
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="driveTimeFrom"></param>
        /// <param name="driveTimeTo"></param>
        /// <param name="includeSainsburysAsCompetitors"></param>
        /// <returns></returns>
        public IEnumerable<SiteToCompetitor> GetCompetitors(int siteId, int driveTimeFrom, int driveTimeTo, bool includeSainsburysAsCompetitors = true)
        {
            var site = GetSite(siteId);

            IEnumerable<SiteToCompetitor> siteCompetitors = GetSitesWithCompetitors().Where(x => x.Id == site.Id)
                .SelectMany(x => x.Competitors).Where(x => x.DriveTime >= driveTimeFrom && x.DriveTime <= driveTimeTo)
                .ToList();

            if (!includeSainsburysAsCompetitors)
            {
                siteCompetitors = siteCompetitors.Where(x => !x.Competitor.IsSainsburysSite);
            }
            return siteCompetitors;
        }

        // New File Upload
        public FileUpload NewUpload(FileUpload upload)
        {
            if (upload.Status == null)
                upload.Status = GetProcessStatuses().First();
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
        public Task<List<FileUpload>> GetFileUploads(DateTime? date, int? uploadTypeId, int? statusId)
        {
            IEnumerable<FileUpload> files = GetFileUploads();

            if (date != null)
            {
                files = files.Where(x => x.UploadDateTime.Date.Equals(date.Value.Date));
                //SqlFunctions.DateDiff("day", x.UploadDateTime, date.Value)
            }
            if (uploadTypeId.HasValue)
            {
                files = files.Where(x => x.UploadTypeId == uploadTypeId.Value);
            }
            if (statusId.HasValue)
            {
                files = files.Where(x => x.StatusId == statusId.Value);
            }
            var retval = files.OrderByDescending(x => x.UploadDateTime);
            return Task.FromResult(retval.ToList());
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
        public async Task<bool> AnyFileUploadForDate(DateTime date, UploadType uploadType)
        {
            var anyFilesForDate = await GetFileUploads(date, uploadType.Id, null);
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

        public void Dispose()
        {
            _context.Dispose();
        }

        public CompetitorSiteReportViewModel GetCompetitorSiteReport(int siteId)
        {
            var result = new CompetitorSiteReportViewModel();
            return result;
        }
    }
}
