using JsPlc.Ssc.PetrolPricing.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace JsPlc.Ssc.PetrolPricing.Business
{
    public interface IFileService
    {
        Task<FileUpload> NewUpload(FileUpload fileUpload);

        bool ExistsUpload(string storedFileName);

        Task<IEnumerable<FileUpload>> ExistingDailyUploads(DateTime uploadDateTime);

        Task<IEnumerable<FileUpload>> GetFileUploads(DateTime? date, int? uploadTypeId, int? statusId);

        FileUpload GetFileUpload(int id);

        Task<FileUpload> ProcessDailyPrice(List<FileUpload> listOfFiles);

        Task<FileUpload> ProcessQuarterlyFileNew(List<FileUpload> uploadedFiles);
    }
}
