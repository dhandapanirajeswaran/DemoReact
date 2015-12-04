﻿using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Security.Permissions;
using JsPlc.Ssc.PetrolPricing.Models;

using System.Data.Entity.Validation;
using System.Diagnostics;

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

        public bool NewDailyPrices(List<DailyPrice> DailyPriceList)
        {
            //_db.Configuration.AutoDetectChangesEnabled = false;

            try
            {
                foreach (DailyPrice dailyPrice in DailyPriceList)
                {
                    _db.DailyPrices.Add(dailyPrice); 
                }
                
                _db.SaveChanges();

                return true;
            }
            catch(DbUpdateException e)
            {
                //TODO loop over event exceptiob
                return false;
            }
            catch (DbEntityValidationException dbEx)
            {
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
