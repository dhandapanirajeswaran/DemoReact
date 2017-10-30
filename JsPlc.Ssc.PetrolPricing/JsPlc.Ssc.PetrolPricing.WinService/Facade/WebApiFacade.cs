using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Http.Headers;
using JsPlc.Ssc.PetrolPricing.WinService.Logging;
using JsPlc.Ssc.PetrolPricing.WinService.Interfaces;
using JsPlc.Ssc.PetrolPricing.WinService.Models;

namespace JsPlc.Ssc.PetrolPricing.WinService.Facade
{
    internal class WebApiFacade
    {
        private HttpClient _client;

        private IEventLog _logger;
        private IAppSettings _settings;

        public WebApiFacade(IEventLog logger, IAppSettings settings)
        {
            _logger = logger;
            _settings = settings;

            _client = new HttpClient();
            _client.BaseAddress = new Uri(_settings.ServicesBaseUrl);
            _client.DefaultRequestHeaders.Accept.Clear();
            _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        public void ExecuteWinServiceSchedule()
        {
            using (var log = _logger.Context("ExecuteWinServiceSchedule()"))
            {
                const string apiUrl = "api/ExecuteWinServiceSchedule";

                log.Debug("ApiUrl: " + apiUrl);

                HttpResponseMessage response = _client.GetAsync(apiUrl).Result;
                log.Info("IsSuccessStatusCode: " + response.IsSuccessStatusCode);
                if (response.IsSuccessStatusCode)
                {
                    log.Debug("Success");
                    var status = response.Content.ReadAsAsync<StatusViewModel>().Result;
                    if (status == null)
                        log.Error("WebAPI response model is null");
                    else if (!String.IsNullOrEmpty(status.ErrorMessage))
                        log.Error(String.Format("ErrorMessage: {0}", status.ErrorMessage));
                    else if (!String.IsNullOrEmpty(status.SuccessMessage))
                        log.Info(String.Format("SuccessMessage: {0}", status.SuccessMessage));
                }
                else
                    log.Error(String.Format("{0} ({1})", (int)response.StatusCode, response.ReasonPhrase));
            }
        }

        #region private methods

        private T CallAndCatchAsyncGet<T>(string methodName, string apiUrl)
        {
            try
            {
                var response = _client.GetAsync(apiUrl).Result;
                if (response.IsSuccessStatusCode)
                    return response.Content.ReadAsAsync<T>().Result;
                else
                    return default(T);
            }
            catch (Exception ex)
            {
                _logger.Exception("CallAndCatchAsyncGet() APIUrl: " + apiUrl, ex);
                throw new Exception("Exception in " + methodName + System.Environment.NewLine + ex.Message, ex);
            }
        }
        #endregion
    }
}
