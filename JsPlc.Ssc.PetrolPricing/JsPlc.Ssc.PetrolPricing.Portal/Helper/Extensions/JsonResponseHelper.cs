using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Mvc;

namespace JsPlc.Ssc.PetrolPricing.Portal.Helper.Extensions
{
    public class PetrolPricingJsonData
    {
        public JsonStatusCode JsonStatusCode { get; set; }
        public object JsonObject { get; set; }
        public object ModelErrors { get; set; }
    }

    public class JsonStatusCode
    {
        public HttpStatusCode ApiStatusCode { get; set; }
        public string CustomStatusCode { get; set; } // extent to be enums
    }

    public static class JsonResponseHelper
    {
        public static JsonResult ToJsonResult(this HttpResponseMessage response, object jsonPayload, object modelErrors, string customStatusCode)
        {
            return new JsonResult
            {
                JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                Data = new PetrolPricingJsonData
                {
                    ModelErrors = modelErrors,
                    JsonObject = jsonPayload,
                    JsonStatusCode = new JsonStatusCode
                    {
                        ApiStatusCode = response.StatusCode,
                        CustomStatusCode = customStatusCode
                    }
                }
            };
        }
    }
}