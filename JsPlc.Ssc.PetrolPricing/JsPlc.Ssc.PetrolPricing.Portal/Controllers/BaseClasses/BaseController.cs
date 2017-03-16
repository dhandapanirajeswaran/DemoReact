using ClosedXML.Excel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace JsPlc.Ssc.PetrolPricing.Portal.Controllers.BaseClasses
{
    public class BaseController : Controller
    {

        protected ActionResult SendExcelFile(string excelFilename, XLWorkbook wb, string downloadId)
        {
            Response.Clear();
            Response.Buffer = true;
            Response.Charset = "";
            Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            string downloadHeader = String.Format("attachment;filename= {0}", excelFilename);
            Response.AddHeader("content-disposition", downloadHeader);

            SetDownloadCookie(downloadId);

            using (var myMemoryStream = new System.IO.MemoryStream())
            {
                wb.SaveAs(myMemoryStream);
                myMemoryStream.WriteTo(Response.OutputStream);
                Response.Flush();
                Response.End();
                return new EmptyResult();
            }
        }

        protected void SetDownloadCookie(string downloadId)
        {
            if (String.IsNullOrWhiteSpace(downloadId))
                throw new ArgumentException("DownloadId cannot be empty!");

            Response.Cookies.Add(new System.Web.HttpCookie(downloadId, DateTime.Now.Ticks.ToString()));
        }
    }
}