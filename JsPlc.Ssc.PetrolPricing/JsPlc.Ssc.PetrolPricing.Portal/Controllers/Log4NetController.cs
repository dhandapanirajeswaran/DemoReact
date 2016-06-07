using JsPlc.Ssc.PetrolPricing.Portal.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace JsPlc.Ssc.PetrolPricing.Portal.Controllers
{
    [Authorize]
    public class Log4NetController : Controller
    {
        // GET: Log4Net
        public ActionResult Index()
        {
            var log4netFilePath = Server.MapPath(@"~/Logs/log.txt");
            var viewModel = ReadLogFile(log4netFilePath);

            return View(viewModel);
        }

        private Log4NetViewModel ReadLogFile(string logFilePath)
        {
            var viewModel = new Log4NetViewModel()
            {
                FileName = logFilePath
            };

            try
            {
                if (System.IO.File.Exists(logFilePath) == false)
                {
                    viewModel.Message = "Unable to find log file";
                    return viewModel;
                }
                viewModel.LastModified = new FileInfo(logFilePath).LastWriteTime;

                using (var fileStream = new FileStream(logFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                using (var textReader = new StreamReader(fileStream))
                {
                    viewModel.FileDump = textReader.ReadToEnd();
                }
                viewModel.Message = "File read successful";
                return viewModel;
            }
            catch (Exception ex)
            {
                viewModel.Message = String.Format("Unable to read file: {0} - Exception: {1}", logFilePath, ex.ToString());
                return viewModel;
            }
        }
    }
}