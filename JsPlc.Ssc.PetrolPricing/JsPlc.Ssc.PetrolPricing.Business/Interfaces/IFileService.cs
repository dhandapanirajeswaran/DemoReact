using JsPlc.Ssc.PetrolPricing.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace JsPlc.Ssc.PetrolPricing.Business
{
    public interface IFileService
    {
        FileUpload NewUpload(FileUpload fileUpload);

        bool ExistsUpload(string storedFileName);

        Task<IEnumerable<FileUpload>> ExistingDailyUploads(DateTime uploadDateTime);

        IEnumerable<FileUpload> GetFileUploads(DateTime? date, int? uploadTypeId, int? statusId);

        FileUpload GetFileUpload(int id);

        FileUpload ProcessDailyPrice(List<FileUpload> listOfFiles);

        FileUpload ProcessQuarterlyFileNew(List<FileUpload> uploadedFiles);

		void CleanupIntegrationTestsData(string testUserName = "Integration tests");
    }
}
