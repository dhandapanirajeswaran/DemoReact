using JsPlc.Ssc.PetrolPricing.Portal.Helper;
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
            //            var viewModel = ReadLogFile(log4netFilePath);
            var viewModel = ReadSessionLog();

            return View(viewModel);
        }

        public ActionResult Clear()
        {
            var logger = new Log4NetSessionLogger(this.Session);
            logger.Clear();
            return Redirect("Index");
        }

        private Log4NetViewModel ReadSessionLog()
        {
            var logger = new Log4NetSessionLogger(this.Session);
            return new Log4NetViewModel()
            {
                FileName = "n/a (session)",
                FileDump = logger.GetLogText()
            };
        }
    }
}