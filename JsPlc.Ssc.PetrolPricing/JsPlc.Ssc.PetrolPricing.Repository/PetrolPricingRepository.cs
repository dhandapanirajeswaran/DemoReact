using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Migrations;
using System.Data.Entity.SqlServer;
using System.Data.SqlClient;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using System.Security.Permissions;
using JsPlc.Ssc.PetrolPricing.Models;

using System.Data.Entity.Validation;
using System.Diagnostics;
using JsPlc.Ssc.PetrolPricing.Models.ViewModels;

namespace JsPlc.Ssc.PetrolPricing.Repository
{
    public class PetrolPricingRepository : IPetrolPricingRepositoryLookup, IPetrolPricingRepository, IDisposable
    {
        private readonly RepositoryContext _context;

        public PetrolPricingRepository() { }

        public PetrolPricingRepository(RepositoryContext context) { _context = context; }

        public IEnumerable<AppConfigSettings> GetAppConfigSettings()
        {
            return _context.AppConfigSettings.ToList();
        }

        public IEnumerable<Site> GetSites()
        {
            return _context.Sites.Include(s => s.Emails).OrderBy(q => q.Id);
        }

        public IQueryable<Site> GetSitesIncludePrices(DateTime? forDate = null)
        {
            if (!forDate.HasValue) forDate = DateTime.Now;
            return _context.Sites
                .Include(x => x.Emails)
                .Include(x => x.Prices)
                .Where(x => x.IsActive)
                .Where(x => x.Prices.All(p => p.DateOfCalc.Equals(forDate)));
        }

        public IEnumerable<Site> GetSitesWithPricesAndCompetitors()
        {
            return _context.Sites
                .Include(s => s.Emails)
                .Include(x => x.Competitors)
                .Include(x => x.Prices)
                .Where(x => x.IsActive)
                .OrderBy(q => q.Id);
        }

        public IEnumerable<SitePriceViewModel> GetSitesWithPrices(DateTime forDate, int siteId = 0, int pageNo = 1, int pageSize = Constants.PricePageSize)
        {
            // for 
            var retval = CallSitePriceSproc(forDate, siteId, pageNo, pageSize);

            return retval;
        }

        public SitePriceViewModel GetASiteWithPrices(int siteId, DateTime forDate)
        {
            var listSitePlusPrice = CallSitePriceSproc(forDate, siteId, 1, 1);

            //sitePlusPrice = listSitePlusPrice.Where(x => x.SiteId == siteId);
            SitePriceViewModel sitePlusPrice = new SitePriceViewModel();
            foreach(SitePriceViewModel xxx in listSitePlusPrice)
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
        private IEnumerable<SitePriceViewModel> CallSitePriceSproc(DateTime forDate, int siteId = 0, int pageNo = 1, int pageSize = Constants.PricePageSize)
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
                Value = (pageNo-1) * pageSize
            };
            var takeRecsParam = new SqlParameter("@takeRecs", SqlDbType.Int)
            {
                Value = pageSize
            };
            // any other params here

            var sqlParams = new List<SqlParameter>
            {
                siteIdParam, forDateParam, skipRecsParam, takeRecsParam
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
                        var loopSiteId = (int)pgRow["siteId"];
                        if (loopSiteId != lastSiteId)
                        {
                            sitePriceRow = new SitePriceViewModel();
                            dbList.Add(sitePriceRow);
                            lastSiteId = loopSiteId;
                        }
                        if (sitePriceRow == null) continue;

                        sitePriceRow.SiteId = (int) pgRow["SiteId"];
                        sitePriceRow.CatNo = Convert.IsDBNull(pgRow["CatNo"]) ? null : (int?)pgRow["CatNo"]; // ToNullable<int> or ToNullable<double>
                        sitePriceRow.StoreName = (string)pgRow["SiteName"];
                        sitePriceRow.Address = (string)pgRow["Address"];
                        // any other fields for UI extract here

                        sitePriceRow.FuelPrices = sitePriceRow.FuelPrices ?? new List<FuelPriceViewModel>();
                        if (!Convert.IsDBNull(pgRow["FuelTypeId"]))
                        {
                            sitePriceRow.FuelPrices.Add(new FuelPriceViewModel
                            {
                                FuelTypeId = (int)pgRow["FuelTypeId"],
                                Price = (int?)pgRow["SuggestedPrice"],
                                OverridePrice = (int?)pgRow["OverriddenPrice"],
                                YesterdaysPrice = Convert.IsDBNull(pgRow["OverriddenPriceYest"]) ?
                                    (int?)pgRow["SuggestedPriceYest"]
                                    : (int?)pgRow["OverriddenPriceYest"]
                            });
                        }
                    }
                    return dbList;
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
        public IEnumerable<DailyPrice> GetDailyPricesForFuelByCompetitors(IEnumerable<int> competitorCatNos, int fuelId, DateTime usingPricesforDate)
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

            // TODO Email edits and deletes are not impacting DB yet

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

        public bool NewDailyPrices(List<DailyPrice> dailyPriceList, FileUpload fileDetails, int startingLineNumber)
        {
            //_db.Configuration.AutoDetectChangesEnabled = false;
            //int startingLineNumber = StartingLineNumber
            int addingEntryLineNo = startingLineNumber;
            using (var tx = _context.Database.BeginTransaction())
            {
                try
                {
                    //LogImportError(FileDetails); this works
                    foreach (DailyPrice dailyPrice in dailyPriceList)
                    {
                        //startingLineNumber++;
                        _context.DailyPrices.Add(dailyPrice);
                        addingEntryLineNo += 1;
                    }

                    _context.SaveChanges();

                    tx.Commit();
                    return true;
                }
                catch (DbUpdateException e)
                {

                    tx.Rollback();
                    
                    foreach (var dbUpdateException in e.Entries)
                    {
                        // TODO as per dbUpdateException log error for that entry which failed
                        var dailyPrice = dbUpdateException.Entity as DailyPrice ?? new DailyPrice();
                        LogImportError(fileDetails, String.Format("Failed to save price:{0},{1},{2},{3},{4}",
                            dailyPrice.CatNo, dailyPrice.FuelTypeId, dailyPrice.AllStarMerchantNo, dailyPrice.DateOfPrice, dailyPrice.ModalPrice)
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
                            LogImportError(fileDetails, "DbEntityValidationException occured:" + validationError.ErrorMessage +
                                "," + validationError.PropertyName, addingEntryLineNo);
                            
                            Trace.TraceInformation("Property: {0} Error: {1}", validationError.PropertyName, validationError.ErrorMessage);
                        }
                    }
                    return false;
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

        // TODO - yet to be tested
        public SitePrice AddOrUpdateSitePriceRecord(SitePrice calculatedSitePrice)
        {
            // Find the Site Price record for a Given Site, Fuel and Date
            var existingPriceRecord = _context.SitePrices.FirstOrDefault(
                x => x.SiteId == calculatedSitePrice.SiteId 
                    && x.FuelTypeId == calculatedSitePrice.FuelTypeId 
                    && x.DateOfCalc.Date.Equals(calculatedSitePrice.DateOfCalc.Date));

            if (existingPriceRecord == null)
            {
                _context.SitePrices.Add(calculatedSitePrice);
            }
            else
            {
                existingPriceRecord.SuggestedPrice = calculatedSitePrice.SuggestedPrice;
                _context.Entry(existingPriceRecord).State = EntityState.Modified;
            }
            _context.SaveChanges();
            return existingPriceRecord;
        }

        public void LogImportError(FileUpload fileDetails, string errorMessage = "", int? lineNumber = 0)
        {
            var db = new RepositoryContext(_context.Database.Connection);
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

        public void UpdateImportProcessStatus(FileUpload fileUpload, int statusId)
        {
            var db = new RepositoryContext(_context.Database.Connection);
            var fu = db.FileUploads.FirstOrDefault(x => x.Id == fileUpload.Id);
            if (fu == null) return;
            _context.Database.ExecuteSqlCommand("Update FileUpload Set StatusId = " + statusId + " where Id = " +
                                                fileUpload.Id);
            //fu.StatusId = statusId;
            //db.Entry(fu).State = EntityState.Modified;
            //db.SaveChanges();
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

            IEnumerable<SiteToCompetitor> siteCompetitors = GetSitesWithPricesAndCompetitors().Where(x => x.Id == site.Id)
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
        public IEnumerable<FileUpload> GetFileUploads(DateTime? date, int? uploadTypeId, int? statusId)
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
            var retval = files.ToArray();
            return retval;
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

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
