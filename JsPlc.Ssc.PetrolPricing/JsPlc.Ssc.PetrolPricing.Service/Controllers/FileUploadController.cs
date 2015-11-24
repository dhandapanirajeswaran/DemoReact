using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Results;
using JsPlc.Ssc.PetrolPricing.Business;
using JsPlc.Ssc.PetrolPricing.Models;
using JsPlc.Ssc.PetrolPricing.Repository;
using Newtonsoft.Json;

namespace JsPlc.Ssc.PetrolPricing.Service.Controllers
{
    public class FileUploadController : BaseController
    {

        public FileUploadController() { }

        public FileUploadController(IPetrolPricingRepository repository) : base(repository) { }

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
                using (var fs = new FileService())
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
        /// List of File Uploads
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("api/FileUploads")]
        public async Task<IHttpActionResult> List()
        {
            using (var fs = new FileService())
            {
                return Ok(fs.GetFileUploads().ToList());
            }
        }

        [HttpPut]
        public async Task<IHttpActionResult> Put(FileUpload fileUpload)
        {
            if (fileUpload == null)
            {
                return BadRequest("Invalid passed data: fileUpload");
            }
            try
            {
                using (var fs = new FileService())
                {
                    fs.UpdateUpload(fileUpload); // Update the status of an uploaded file (processing, success, failed etc)
                    return Ok(fileUpload);
                }
            }
            catch (Exception ex)
            {
                return new ExceptionResult(ex, this);
            }
        }

        [HttpGet]
        [Route("api/ExistingDailyUploads/{uploadDateTime}")] // yyyymmdd
        public async Task<IHttpActionResult> ExistingDailyUploads([FromUri] string uploadDateTime)
        {
            try
            {
                using (var fs = new FileService())
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
    }
}
