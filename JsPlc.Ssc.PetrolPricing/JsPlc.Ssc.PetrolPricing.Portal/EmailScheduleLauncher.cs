using JsPlc.Ssc.PetrolPricing.Core.Interfaces;
using JsPlc.Ssc.PetrolPricing.Portal.Facade;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;

namespace JsPlc.Ssc.PetrolPricing.Portal
{
    public static class EmailScheduleLauncher
    {
        private static string _websiteBaseUrl = null;

        public static void BeginRequestHook(ILogger logger, int pollInterval)
        {
            // already initialised ?
            if (!String.IsNullOrEmpty(_websiteBaseUrl))
                return;

            // no HttpContext avaiable ?
            if (HttpContext.Current == null || HttpContext.Current.Request == null)
                return;

            var baseUrl = ExtractWebsiteBaseUrl(HttpContext.Current.Request.Url);
            if (String.IsNullOrEmpty(baseUrl))
                return;

            _websiteBaseUrl = baseUrl;

            StartScheduler(logger, pollInterval);
        }

        private static void StartScheduler(ILogger logger, int pollInterval)
        {
            var interval = TimeSpan.FromMinutes(pollInterval);
            var serviceFacade = new ServiceFacade(logger);
            var runner = new Action(() => TriggerSelfWebRequest());
            SimpleScheduler.Start(interval, runner);
        }

        private static void TriggerSelfWebRequest()
        {
            if (String.IsNullOrEmpty(_websiteBaseUrl))
                return;

            var url = String.Concat(_websiteBaseUrl, "/PublicApi/PollEmailSchedule");
            WebRequest request = WebRequest.Create(url);
            request.GetResponse();
        }

        private static string ExtractWebsiteBaseUrl(Uri url)
        {
            var parts = url.ToString().Split('/');
            if (parts.Length > 3)
            {
                if (parts[2].ToLower() == "localhost")
                    return String.Format("{0}//{1}/{2}",
                        parts[0],
                        parts[2],
                        parts[3]
                        );
                else
                    return String.Format("{0}//{1}",
                        parts[0],
                        parts[2]
                        );
            }
            return null;
        }
    }
}