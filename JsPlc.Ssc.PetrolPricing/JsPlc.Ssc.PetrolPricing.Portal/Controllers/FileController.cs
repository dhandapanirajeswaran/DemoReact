using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace JsPlc.Ssc.PetrolPricing.Portal.Controllers
{
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

            return View();
        }

        [HttpPost]
        public ActionResult Upload(HttpPostedFileBase file)
        {
            ViewBag.Message = "Upload Daily/Quarterly file";
            ViewBag.ErrorMessage = "";

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
                    string savedFileName = String.Format("{1}{2}{3} {4}{5}{6}hrs - {0}", fileName, DateTime.Now.Year,
                        DateTime.Now.Month, DateTime.Now.Day,
                        DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second);

                    var path = Path.Combine(Server.MapPath("~/App_Data/uploads"), savedFileName); // save file as 20151120 123045hrs - Daily pricing file - catalist.txt
                    file.SaveAs(path);

                    // call service to record the upload
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