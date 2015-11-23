using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
//using JsPlc.Ssc.PetrolPricing.Business;
using JsPlc.Ssc.PetrolPricing.Models;
using JsPlc.Ssc.PetrolPricing.Models.ViewModels;
//using JsPlc.Ssc.PetrolPricing.Business;
using JsPlc.Ssc.PetrolPricing.Portal.Facade;

namespace JsPlc.Ssc.PetrolPricing.Portal.Controllers
{
    [Authorize]
    public class FileController : Controller
    {
        readonly ServiceFacade _serviceFacade = new ServiceFacade();

        public ActionResult Index(string msg="")
        {
            // Display list of existing files along with their status
            ViewBag.Message = msg;

            return View();
        }

        public ActionResult Upload()
        {
            ViewBag.Message = "Upload Daily/Quarterly file";
            var model = new UploadViewModel
            {
                //UploadTypes = new LookupService().GetUploadTypes(),
                UploadTypes = GetUploadTypes(),
                UploadDate = DateTime.Now
            };
            if (DoesDailyUploadExists())
            {
                ViewBag.WarningMessage = "Warning: Daily file already exists for today";
            }
            return View(model);
        }

        [HttpPost]
        public ActionResult Upload(HttpPostedFileBase file, int uploadTypes, DateTime? uploadDate)
        {
            var model = new UploadViewModel
            {
                //UploadTypes = new Lookup().GetUploadTypes(),
                UploadTypes = GetUploadTypes(),
                UploadDate = DateTime.Now
            };

            ViewBag.Message = "Upload Daily/Quarterly file";
            ViewBag.ErrorMessage = "";

            const string successMessage = "File uploaded successfully";

            try
            {
                var holdName = "";
                if (DoesDailyUploadExists())
                {
                    // Initiate a two step flow
                    holdName = Guid.NewGuid().ToString();

                    // SaveFile to hold path with holdname
                    // Redirect to confirmation page with holdName prefix (will show existing uploads for user)
                    // Confirmed :-> 
                        // Rename file from hold to .. with removal of guid (use fixed length substring)
                        // RecordUpload
                    // Cancel :-> Cleanup the hold file
                }

                var fileUpload = SaveFile(file, holdName, uploadTypes, uploadDate);
                fileUpload = RecordUpload(fileUpload); // returns the upload record with the created Id

                return RedirectToAction("Index", new {msg = successMessage + ":" + fileUpload.OriginalFileName});
            }
            catch (ApplicationException ex)
            {
                ViewBag.ErrorMessage = ex.Message;
            }
            catch (Exception)
            {
                ViewBag.ErrorMessage = "Sorry, an error occured, please try again.";
            }
            return View(model);
        }

        private bool DoesDailyUploadExists()
        {
            return ExistingDailyUploads().Any();
        }

        // To check for warning where file exists (daily file type check for the date)
        private IEnumerable<FileUpload> ExistingDailyUploads()
        {
            return _serviceFacade.ExistingDailyUploads(DateTime.Now);
        }

        private FileUpload SaveFile(HttpPostedFileBase file, string guid, int uploadType, DateTime? uploadDate)
        {
            var uploadDateTime = uploadDate ?? DateTime.Now;
            //var uploadPath = SettingsService.GetUploadPath(); // throws exception if not found
            var uploadPath = _serviceFacade.GetUploadPath();

            if (file == null || file.ContentLength <= 0)
            {
                throw new ApplicationException("Upload file is empty or no file selected. Please select a file to upload");
            }

            var fileName = Path.GetFileName(file.FileName);

            var originalFileName = fileName;

            var savedFileName = String.Format("{1} {2}hrs - {0}", fileName,
                uploadDateTime.ToString("yyyyMMdd"),
                uploadDateTime.ToString("HHmmss"));

            //var path = Path.Combine(Server.MapPath("~/App_Data/uploads"), savedFileName); // save file as 20151120 123045hrs - Daily pricing file - catalist.txt
            var path = Path.Combine(uploadPath, savedFileName);
            file.SaveAs(path); // PERSIST The File
            
            return new FileUpload
            {
                OriginalFileName = originalFileName,
                StoredFileName = savedFileName,
                UploadedBy = User.Identity.Name,
                StatusId = 1,
                UploadDateTime = uploadDateTime,
                UploadTypeId = uploadType
            };
        }

        private FileUpload RecordUpload(FileUpload fileUpload)
        {
            // call service to record the upload
            var fu = _serviceFacade.NewUpload(fileUpload);
            return fu;
        }

        private static IEnumerable<UploadType> GetUploadTypes()
        {
            using (var svcFacade = new ServiceFacade())
            {
                return svcFacade.GetUploadTypes();
            }
        }
    }
}