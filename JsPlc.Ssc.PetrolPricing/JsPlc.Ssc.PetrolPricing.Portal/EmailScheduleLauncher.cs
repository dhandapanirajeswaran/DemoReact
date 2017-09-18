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
        private static int _emailPollInterval = 0;
        private static string _pollUrl = null;
        private static DateTime? _emailPollLastDateTime = null;
        private static string _emailPollLastStatus = "";

        public static Dictionary<string, string> GetDiagnosticValues()
        {
            var diagnostics = new Dictionary<string, string>()
            {
                {"WebSiteBaseUrl", _websiteBaseUrl },
                {"EmailPollInterval", _emailPollInterval.ToString() },
                {"EmailPollUrl", _pollUrl },
                {"EmailPollDateTime", _emailPollLastDateTime.ToString() },
                {"EmailPollLastStatus", _emailPollLastStatus}
            };
            return diagnostics;
        }

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
            _emailPollInterval = pollInterval;
            _pollUrl = String.Concat(_websiteBaseUrl, "/PublicApi/PollEmailSchedule");

            var interval = TimeSpan.FromMinutes(pollInterval);
            var serviceFacade = new ServiceFacade(logger);
            var runner = new Action(() => EmailSchedulerPollAction());
            SimpleScheduler.Start(interval, runner);
        }

        private static void EmailSchedulerPollAction()
        {
            _emailPollLastDateTime = DateTime.Now;

            if (String.IsNullOrEmpty(_pollUrl))
            {
                _emailPollLastStatus = "No PollUrl";
                return;
            }
            try
            {
                WebRequest request = WebRequest.Create(_pollUrl);
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                _emailPollLastStatus = response.StatusCode.ToString();
            }
            catch (Exception ex)
            {
                _emailPollLastStatus = ex.ToString();
            }
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