using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web;
using System.Web.Http.Results;
using System.IO;
using JsPlc.Ssc.PetrolPricing.Business;
using JsPlc.Ssc.PetrolPricing.Models;
using JsPlc.Ssc.PetrolPricing.Models.Common;
using Newtonsoft.Json;
using JsPlc.Ssc.PetrolPricing.Core;

namespace JsPlc.Ssc.PetrolPricing.Service.Controllers
{
   
    public class FileUploadController : ApiController
    {
        IFileService _fileService;
        IAppSettings _appSettings;
        ISystemSettingsService _systemSettingsService;

        public FileUploadController(IFileService fileService,IAppSettings appSettings, ISystemSettingsService systemSettingsService)
        {
            _fileService = fileService;
            _appSettings = appSettings;
            _systemSettingsService = systemSettingsService;
        }

        [HttpPost]
        [Route("api/SaveHoldFile")]
        public async Task<IHttpActionResult> SaveHoldFile()
        {
            try
            {
                var queryString = Request.GetQueryNameValuePairs().ToDictionary(x => x.Key, x => x.Value);

                var heldFile = queryString["heldfile"];
                var destFile = queryString["destFile"];

                if (File.Exists(heldFile))
                {
                    File.Move(heldFile, destFile);
                    return Ok("SUCCESS");
                }
                else
                    return Ok("FAIL");


            }
            catch (Exception ex)
            {
                return new ExceptionResult(ex, this);
            }
        }

        [HttpPost]
        [Route("api/SaveFile")]
        public async Task<IHttpActionResult> Post()
        {
            try
            {
                var queryString = Request.GetQueryNameValuePairs().ToDictionary(x => x.Key, x => x.Value);

                var uploadPath = _appSettings.UploadPath + "\\" + queryString["file"];

                var result = Request.Content.ReadAsStringAsync().Result.Replace("\"", string.Empty);
                byte[] bytearray = Convert.FromBase64String(result);


                using (FileStream writeStream = new FileStream(uploadPath, FileMode.Create, FileAccess.Write))
                {
                    writeStream.Write(bytearray, 0, bytearray.Length);
                    writeStream.Close();
                }

                return Ok("SUCCESS");
            }
            catch (Exception ex)
            {
                return new ExceptionResult(ex, this);
            }
        }

        [HttpPost] // Create new upload
        public async Task<IHttpActionResult> Post(FileUpload fileUpload)
        {
            if (fileUpload == null)
            {
                return BadRequest("Invalid passed data: fileUpload");
            }

			if (fileUpload.UploadTypeId == (int)FileUploadTypes.DailyPriceData
				&& fileUpload.OriginalFileName.ToLowerInvariant().EndsWith(".xlsx"))
			{
				return BadRequest("Invalid file format for Daily Price Data file. Expected format is CSV in a simple text file.");
			}

            try
            {
                if (_fileService.ExistsUpload(fileUpload.StoredFileName)) // avoid creating records for same filename (user RePosting by mistake)
                {
                    return BadRequest("File with that name already exists. Please try again.");
                }

                fileUpload.FileExists = true;
                var fu = _fileService.NewUpload(fileUpload);

                return Ok(fu);
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

            var list = await Task.Run(() => _fileService.GetFileUploads(dtParam, uploadTypeId, statusId));

            return Ok(list.ToList());

        }

        /// <summary>
        /// List of File Uploads
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("api/FileUploads/{id}")]
        public async Task<IHttpActionResult> Get(int id)
        {
            return Ok(_fileService.GetFileUpload(id));
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

                DateTime dt = DateTime.Parse(uploadDateTime);
                var exists = await _fileService.ExistingDailyUploads(dt);
                return Ok(exists.ToList()); //  T or F

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
        public async Task<IHttpActionResult> ProcessUploadedFile(DateTime? forDate = null, int fileId = 0, int fileTypeId = 0)
        {
            try
            {
                var uploadedFiles = await Task.Run(() => _fileService.GetFileUploads(forDate, fileTypeId, 1));
                //Status = 1 "uploaded" files only
                switch (fileTypeId)
                {
                    case 1:
                        return Ok(_fileService.ProcessDailyPrice(uploadedFiles.ToList()));
                    case 2:
                        return Ok(_fileService.ProcessQuarterlyFileNew(uploadedFiles.ToList()));
                    default:
                        throw new ApplicationException("Invalid file type specified");
                }
            }
            catch (Exception ex)
            {
                return new ExceptionResult(ex, this);
            }
        }

        /// <summary>
        /// Test - Process a quarterly file - import and calc
        /// </summary>
        /// <returns></returns>
        [HttpGet] // Process files in upload list 
        [Route("api/ProcessQuarterlyFile")]
        public async Task<IHttpActionResult> ProcessQuarterlyFile()
        {
            //FOR TESTING
            List<FileUpload> aFileList = new List<FileUpload>();
            FileUpload aFile = new FileUpload();
            aFile.Id = 1;
            aFile.StoredFileName = "C:/Temp/20151126 163800hrs - Catalist quarterly data.xlsx";
            aFileList.Add(aFile);
            //END TEST


            return Ok(_fileService.ProcessQuarterlyFileNew(aFileList));
        }

		[HttpGet] 
		[Route("api/CleanupIntegrationTestsData")]
		public async Task<IHttpActionResult> CleanupIntegrationTestsData(string testUserName)
		{
			try
			{
				_fileService.CleanupIntegrationTestsData(testUserName);
				return Ok(string.Format("{0} user's data deleted", testUserName));
			}
			catch (Exception ex)
			{
				return new ExceptionResult(ex, this);
			}
		}


        /// <summary>
        /// Test - Process a quarterly file - import and calc
        /// </summary>
        /// <returns></returns>
        [HttpGet] // Process files in upload list 
        [Route("api/CalcDailyPrices")]
        public async Task<IHttpActionResult> CalcDailyPrices(int siteId)
        {
            return Ok(_fileService.CalcDailyPrices(siteId));
        }

        [HttpGet]
        [Route("api/File/Download")]
        public async Task<IHttpActionResult> FileDownload([FromUri] int fileUploadId)
        {
            return Ok(_fileService.GetFileDownload(fileUploadId));
        }

        [HttpGet]
        [Route("api/File/DataCleanse")]
        public async Task<IHttpActionResult> DataCleanseFileUploads()
        {
            var systemSettings = _systemSettingsService.GetSettings();

            var daysAgo = systemSettings.DataCleanseFilesAfterDays;

            return Ok(_fileService.DataCleanseFileUploads(daysAgo));
        }

        [HttpGet]
        [Route("api/GetFileUploadInformation/{fileUploadId}")]
        public async Task<IHttpActionResult> GetFileUploadInformation([FromUri]int fileUploadId)
        {
            var result = _fileService.GetFileUploadInformation(fileUploadId);
            return Ok(result);
        }
    }
}
