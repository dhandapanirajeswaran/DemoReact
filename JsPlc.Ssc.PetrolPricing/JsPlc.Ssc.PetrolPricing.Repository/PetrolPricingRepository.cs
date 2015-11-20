using System;
using System.Collections.Generic;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using JsPlc.Ssc.PetrolPricing.Models;

namespace JsPlc.Ssc.PetrolPricing.Repository
{
    public class PetrolPricingRepository : IPetrolPricingRepositoryLookup, IPetrolPricingRepository, IDisposable
    {
        private readonly RepositoryContext _db;

        public PetrolPricingRepository() { }

        public PetrolPricingRepository(RepositoryContext context) { _db = context; }

        public IEnumerable<Site> GetSites()
        {
           return _db.Sites.OrderBy(q=>q.Id);
        }

        public Site GetSite(int id)
        {
            return _db.Sites.FirstOrDefault(q => q.Id == id);
        }

        // New File Upload
        public int NewUpload(FileUpload upload)
        {
            if (upload.Status == null)
                upload.Status = GetProcessStatuses().First();
            if (upload.UploadType == null)
            {
                upload.UploadType = GetUploadTypes().FirstOrDefault(x => x.Id == upload.UploadTypeId);
            }

            var result = _db.FileUploads.Add(upload);
            _db.SaveChanges();

            return result.Id;
        }

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

        /// <summary>
        /// Get List of FileUploads, use null params for full list
        /// </summary>
        /// <param name="date">Nullable date to get Files for a specific date</param>
        /// <param name="uploadType">Optional uploadType to filter if needed</param>
        /// <returns></returns>
        public IEnumerable<FileUpload> GetFileUploads(DateTime? date, UploadType uploadType)
        {
            IQueryable<FileUpload> files = _db.FileUploads;

            if (date != null)
            {
                files = files.Where(x => x.UploadDateTime == date);
            }
            if (uploadType != null)
            {
                files = files.Where(x => x.UploadType == uploadType);
            }

            return files;
        }

        ///  Do we have any FileUploads for specified Date and UploadType
        public bool AnyFileUploadForDate(DateTime date, UploadType uploadType)
        {
            var anyFilesForDate = GetFileUploads(date, uploadType);
            return anyFilesForDate.Any();
        }

        public void Dispose()
        {
            _db.Dispose();
        }
    }
}
