using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Results;
using JsPlc.Ssc.PetrolPricing.Business;
using JsPlc.Ssc.PetrolPricing.Models;
using JsPlc.Ssc.PetrolPricing.Models.Common;
using Newtonsoft.Json;

namespace JsPlc.Ssc.PetrolPricing.Service.Controllers
{
    public class FileUploadController : BaseController
    {

        public FileUploadController() { }

        public FileUploadController(FileService fileService) : base(fileService, null, null) { }

        [HttpPost] // Create new upload
        public async Task<IHttpActionResult> Post(FileUpload fileUpload)
        {
            if (fileUpload == null)
            {
                return BadRequest("Invalid passed data: fileUpload");
            }
            //if (!ModelState.IsValid) // Data Annotations throw it off, so disabled this checking
            //{
            //    return BadRequest(ModelState);
            //}
            try
            {
                using (var fs = _fileService)
                {
                    if (fs.ExistsUpload(fileUpload.StoredFileName)) // avoid creating records for same filename (user RePosting by mistake)
                    {
                        return BadRequest("File with that name already exists. Please try again.");
                    }
                    var fu = fs.NewUpload(fileUpload);
                    return Ok(fu);
                }
            }
            catch (Exception ex)
            {
                return new ExceptionResult(ex, this);
            }
        }

        /// <summary>
        /// List of File Uploads filterable with queryString 
        /// ?date=2015-11-15&amp;uploadTypeId=1&amp;statusId=1 (all params optional)
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("api/FileUploads")]
        public async Task<IHttpActionResult> List()
        {
            IEnumerable<KeyValuePair<string, string>> qryparams = Request.GetQueryNameValuePairs();
            var keyValuePairs = qryparams as KeyValuePair<string, string>[] ?? qryparams.ToArray();

            DateTime? dtParam = keyValuePairs.FirstOrDefault(x => x.Key.Equals("date")).Value.TryParseDateTime();
            int? uploadTypeId = keyValuePairs.FirstOrDefault(x => x.Key.Equals("uploadTypeId")).Value.TryParseInt();
            int? statusId = keyValuePairs.FirstOrDefault(x => x.Key.Equals("statusId")).Value.TryParseInt();

            using (var fs = _fileService)
            {
                return Ok(fs.GetFileUploads(dtParam, uploadTypeId, statusId).ToList());
            }
        }

        /// <summary>
        /// List of File Uploads
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("api/FileUploads/{id}")]
        public async Task<IHttpActionResult> Get(int id)
        {
            using (var fs = _fileService)
            {
                return Ok(fs.GetFileUpload(id));
            }
        }

        /// <summary>
        /// List of Uploads for a given date param
        /// </summary>
        /// <param name="uploadDateTime"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("api/ExistingDailyUploads/{uploadDateTime}")] // yyyymmdd
        public async Task<IHttpActionResult> ExistingDailyUploads([FromUri] string uploadDateTime)
        {
            try
            {
                using (var fs = _fileService)
                {
                    DateTime dt = DateTime.Parse(uploadDateTime);
                    var exists = fs.ExistingDailyUploads(dt);
                    return Ok(exists.ToList()); //  T or F
                }
            }
            catch (Exception ex)
            {
                return new ExceptionResult(ex, this);
            }
        }
        
        /// <summary>
        /// Generic method to kickoff Proesssing of any given DP or QT file
        /// Processes uploaded File to:
        /// - import them into the DailyPrices/Sites table
        /// - kickoff calc
        /// </summary>
        /// <param name="forDate">Optional DateTime - so we can process all files of a specific Date</param>
        /// <param name="fileId">Optional FileId - so we can process one specific file</param>
        /// <param name="fileTypeId"></param>
        /// <returns></returns>
        [HttpGet] // Process files in upload list 
        [Route("api/ProcessUploadedFile")]
        public async Task<IHttpActionResult> ProcessUploadedFile(DateTime? forDate=null, int fileId=0, int fileTypeId=0)
        {
            try
            {
                using (var fs = _fileService)
                {
                    var uploadedFiles = fs.GetFileUploads(forDate, fileTypeId, 1).ToList();
                        //Status = 1 "uploaded" files only
                    switch (fileTypeId)
                    {
                        case 1:
                            return Ok(fs.ProcessDailyPrice(uploadedFiles));
                        case 2:
                            return Ok(fs.ProcessQuarterlyFile(uploadedFiles));
                        default:
                            throw new ApplicationException("Invalid file type specified");
                    }
                }
            }
            catch (Exception ex)
            {
                return new ExceptionResult(ex, this);
            }
        }

        [HttpGet] // Process files in upload list 
        [Route("api/ProcessQuarterlyFile")]
        public async Task<IHttpActionResult> ProcessQuarterlyFile()
        {
            using (var fs = _fileService)
            {
                return Ok(fs.ProcessQuarterlyFile());
            }
        
        }

    }
}
