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
        private readonly RepositoryContext _db;

        public PetrolPricingRepository() { }

        public PetrolPricingRepository(RepositoryContext context) { _db = context; }

        public IEnumerable<AppConfigSettings> GetAppConfigSettings()
        {
            return _db.AppConfigSettings.ToList();
        }

        public IEnumerable<Site> GetSites()
        {
            return _db.Sites.Include(s => s.Emails).OrderBy(q => q.Id);
        }

        public IEnumerable<Site> GetSitesWithPricesAndCompetitors()
        {
            return _db.Sites
                .Include(s => s.Emails)
                .Include(x => x.Competitors)
                .Include(x => x.Prices)
                .Where(x => x.IsActive)
                .OrderBy(q => q.Id);
        }

        public IEnumerable<SitePriceViewModel> GetSitesWithPrices(DateTime forDate, int siteNo = 0, int pageNo = 1, int pageSize = Constants.PricePageSize)
        {
            // for 
            var retval = CallSitePriceSproc(forDate, siteNo, pageNo, pageSize);

            return retval;
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
            var skipRecsParam = new SqlParameter("@skipRecs", SqlDbType.DateTime)
            {
                Value = (pageNo-1) * pageSize
            };
            var takeRecsParam = new SqlParameter("@takeRecs", SqlDbType.DateTime)
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
    
            using (var connection = new SqlConnection(_db.Database.Connection.ConnectionString))
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
            IEnumerable<DailyPrice> dailyPrices = _db.DailyPrices.Include(x => x.DailyUpload);

            return dailyPrices.Where(x => competitorCatNos.Contains(x.CatNo) &&
                                       x.FuelTypeId == fuelId &&
                                       x.DailyUpload.UploadDateTime.Date.Equals(usingPricesforDate.Date)).ToList();
        }

        public Site GetSite(int id)
        {
            return _db.Sites.Include(s => s.Emails).FirstOrDefault(q => q.Id == id);
        }

        public Site GetSiteByCatNo(int catNo)
        {
            return _db.Sites.FirstOrDefault(q => q.CatNo.HasValue && q.CatNo.Value == catNo);
        }

        public Site NewSite(Site site)
        {
            var result = _db.Sites.Add(site);
            _db.SaveChanges();

            return result; // return full object back
        }

        public bool UpdateSite(Site site)
        {
            //_db.Sites.AddOrUpdate(site);
            //_db.SaveChanges();

            // TODO Email edits and deletes are not impacting DB yet

            _db.Entry(site).State = EntityState.Modified;

            try
            {
                _db.Sites.Attach(site);
                UpdateSiteEmails(site);
                _db.Entry(site).State = EntityState.Modified;
                _db.SaveChanges();

                return true;
            }
            catch
            {
                return false;
            }

        }

        public bool NewDailyPrices(List<DailyPrice> DailyPriceList, FileUpload FileDetails, int StartingLineNumber)
        {
            //_db.Configuration.AutoDetectChangesEnabled = false;
            //int startingLineNumber = StartingLineNumber

            using (var tx = _db.Database.BeginTransaction())
            {
                try
                {
                    //LogImportError(FileDetails); this works
                    foreach (DailyPrice dailyPrice in DailyPriceList)
                    {
                        //startingLineNumber++;
                        _db.DailyPrices.Add(dailyPrice);
                    }

                    _db.SaveChanges();

                    tx.Commit();
                    return true;
                }
                catch (DbUpdateException e)
                {

                    tx.Rollback();
                    foreach (var dbUpdateException in e.Entries)
                    {
                        LogImportError(FileDetails, "Failed to save", null);
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
                            Trace.TraceInformation("Property: {0} Error: {1}",
                                                    validationError.PropertyName,
                                                    validationError.ErrorMessage);
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
                    _db.Entry(delEmail).State = EntityState.Deleted;
                }
            }
            var siteEmails = site.Emails.ToList();

            foreach (var email in siteEmails)
            {
                if (email.Id == 0) _db.Entry(email).State = EntityState.Added;
                if (email.Id != 0) _db.Entry(email).State = EntityState.Modified;
            }
        }

        public void LogImportError(FileUpload fileDetails, string errorMessage, int? LineNumber)
        {
            using (var db = new RepositoryContext(_db.Database.Connection))
            {
                ImportProcessError importProcessErrors = new ImportProcessError();

                importProcessErrors.UploadId = fileDetails.Id;
                importProcessErrors.ErrorMessage = errorMessage;

                if (LineNumber != null)
                {
                    importProcessErrors.RowOrLineNumber = int.Parse(LineNumber.ToString());
                }

                db.ImportProcessErrors.Add(importProcessErrors);
                db.SaveChanges();
            }
        }

        public void UpdateImportProcessStatus(FileUpload FileUpload, int StatusId)
        {
            FileUpload.StatusId = StatusId;
            _db.Entry(FileUpload).State = EntityState.Modified;
            _db.SaveChanges();
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

            var result = _db.FileUploads.Add(upload);
            _db.SaveChanges();

            return result; // return full object back
        }

        public bool ExistsUpload(string storedFileName)
        {
            return
                _db.FileUploads.Any(
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
            return _db.FileUploads.Include(x => x.UploadType).Include(y => y.Status);
        }

        public FileUpload GetFileUpload(int id)
        {
            return _db.FileUploads.Include(x => x.UploadType).Include(y => y.Status).FirstOrDefault(x => x.Id == id);
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
            return _db.UploadType.ToList();
        }

        public IEnumerable<FuelType> GetFuelTypes()
        {
            return _db.FuelType.ToList();
        }

        public IEnumerable<ImportProcessStatus> GetProcessStatuses()
        {
            return _db.ImportProcessStatus.ToList();
        }

        public void Dispose()
        {
            _db.Dispose();
        }
    }
}
