using System;
using System.IO;
using System.Web;
using System.Web.Mvc;
using JsPlc.Ssc.PetrolPricing.Models;
using JsPlc.Ssc.PetrolPricing.Models.ViewModels;
using JsPlc.Ssc.PetrolPricing.Business;

namespace JsPlc.Ssc.PetrolPricing.Portal.Controllers
{
    [Authorize]
    public class FileController : Controller
    {
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
                UploadTypes = new Lookup().GetUploadTypes(),
                UploadDate = DateTime.Now
            };
            
            return View(model);
        }

        [HttpPost]
        public ActionResult Upload(HttpPostedFileBase file, int uploadTypes, DateTime? uploadDate)
        {
            ViewBag.Message = "Upload Daily/Quarterly file";
            ViewBag.ErrorMessage = "";
            var uploadDateTime = uploadDate ?? DateTime.Now;

            const string successMessage = "File uploaded successfully";

            if (file.ContentLength <= 0)
            {
                ViewBag.ErrMessage = "Sorry, an error occured, please try again.";
                return View();
            }

            try
            {
                string originalFileName = "";
                var fileName = Path.GetFileName(file.FileName);
                if (fileName != null)
                {
                    originalFileName = fileName;
                    string savedFileName = String.Format("{1} {2}hrs - {0}", fileName,
                        uploadDateTime.ToString("yyyyMMdd"),
                        uploadDateTime.ToString("HHmmss"));

                    var path = Path.Combine(Server.MapPath("~/App_Data/uploads"), savedFileName); // save file as 20151120 123045hrs - Daily pricing file - catalist.txt
                    file.SaveAs(path);

                    // call service to record the upload
                    using (var filesvc = new FileService())
                    {
                        filesvc.NewUpload(new FileUpload
                        {
                            OriginalFileName = originalFileName,
                            StoredFileName = savedFileName,
                            UploadedBy = User.Identity.Name,
                            StatusId = 1,
                            UploadDateTime = uploadDateTime,
                            UploadTypeId = uploadTypes
                        });
                    }
                }

                return RedirectToAction("Index", new { msg = successMessage + ":" + originalFileName });
            }
            catch (Exception ex)
            {
                ViewBag.ErrMessage = "Sorry, an error occured, please try again.";
                return View();
            }
        }
    }
}