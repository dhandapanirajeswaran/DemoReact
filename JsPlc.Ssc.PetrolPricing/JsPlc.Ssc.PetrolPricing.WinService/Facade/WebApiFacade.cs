using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Http.Headers;
using JsPlc.Ssc.PetrolPricing.Models.WindowsService;
using JsPlc.Ssc.PetrolPricing.WinService.Logging;
using JsPlc.Ssc.PetrolPricing.Models.ViewModels.Schedule;

namespace JsPlc.Ssc.PetrolPricing.WinService.Facade
{
    internal class WebApiFacade
    {
        private HttpClient _client;

        public WebApiFacade()
        {
            _client = new HttpClient();
            _client.BaseAddress = new Uri(AppSettings.WebApiServiceBaseUrl);
            _client.DefaultRequestHeaders.Accept.Clear();
            _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        public IEnumerable<ScheduleItemViewModel> GetWinServiceScheduledItems()
        {
            DebugLogger.Info("starting: GetWinServiceScheduledItems");


            HttpResponseMessage response = _client.GetAsync("api/GetWinServiceScheduledItems").Result;
            DebugLogger.Info("IsSuccessStatusCode:" + response.IsSuccessStatusCode);
            if (response.IsSuccessStatusCode)
            {
                var dto = response.Content.ReadAsAsync<ScheduleItemViewModel>().Result;
            }
            else
            {
                Console.WriteLine("{0} ({1})", (int)response.StatusCode, response.ReasonPhrase);
            }
            return new List<ScheduleItemViewModel>();


            //var apiUrl = "api/GetWinServiceScheduledItems/";
            //var model = CallAndCatchAsyncGet<IEnumerable<ScheduleItemViewModel>>("GetWinServiceScheduledItems", apiUrl);
            //DebugLogger.Info("finished: GetWinServiceScheduledItems");
            //return model ?? new List<ScheduleItemViewModel>();
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
                DebugLogger.Exception("CallAndCatchAsyncGet() " + apiUrl, ex);
                throw new Exception("Exception in " + methodName + System.Environment.NewLine + ex.Message, ex);
            }
        }
        #endregion
    }
}
