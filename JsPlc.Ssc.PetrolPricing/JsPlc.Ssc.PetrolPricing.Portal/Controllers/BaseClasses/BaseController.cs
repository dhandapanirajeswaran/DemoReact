using ClosedXML.Excel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.Owin.Security;
using Microsoft.Owin;
using JsPlc.Ssc.PetrolPricing.Portal.Facade;
using JsPlc.Ssc.PetrolPricing.Core.Interfaces;
using JsPlc.Ssc.PetrolPricing.Core;
using JsPlc.Ssc.PetrolPricing.Models.ViewModels.UserPermissions;
using JsPlc.Ssc.PetrolPricing.Models.ViewModels;
using JsPlc.Ssc.PetrolPricing.Portal.Helper.Extensions;
using System.Net.Http;
using System.Net;

namespace JsPlc.Ssc.PetrolPricing.Portal.Controllers.BaseClasses
{
    public class BaseController : Controller
    {
        private readonly ServiceFacade _serviceFacade;
        private readonly ILogger _logger;

        public BaseController()
        {
            _logger = new PetrolPricingLogger();
            _serviceFacade = new ServiceFacade(_logger);
        }

        protected bool IsUserAuthenticated
        {
            get { return Request.IsAuthenticated; }
        }

        protected string UserName
        {
            get { return GetCurrentUserName(); }
        }

        protected UserAccessViewModel GetUserAccessModel()
        {
            if (!IsUserAuthenticated)
                return new UserAccessViewModel();

            const string requestCacheKey = "UserAccess";

            var model = System.Web.HttpContext.Current.Items[requestCacheKey] as UserAccessViewModel;
            if (model == null)
            {
                model = _serviceFacade.GetUserAccessModel(UserName);
                System.Web.HttpContext.Current.Items[requestCacheKey] = model;
            }

            return model;
        }

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

        protected JsonResult StandardJsonResultMessage(object payload)
        {
            return new HttpResponseMessage(HttpStatusCode.OK)
                .ToJsonResult(payload, null, "ApiSuccess");
        }

        #region private methods

        private string GetCurrentUserName()
        {
            var userName = "";

            if (IsUserAuthenticated)
            {
                IOwinContext context = Request.GetOwinContext();
                if (context != null && context.Authentication != null && context.Authentication.User != null)
                    userName = (context.Authentication.User.Identity.Name + "#").Split('#')[0];
            }
            return userName;
        }

        #endregion
    }
}