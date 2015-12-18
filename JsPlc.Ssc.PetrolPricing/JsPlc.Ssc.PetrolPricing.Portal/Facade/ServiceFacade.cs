using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.UI;
using JsPlc.Ssc.PetrolPricing.Models;
//using JsPlc.Ssc.PetrolPricing.Portal.Helpers.Extensions;
using JsPlc.Ssc.PetrolPricing.Models.Enums;
using JsPlc.Ssc.PetrolPricing.Models.ViewModels;
using Newtonsoft.Json;

namespace JsPlc.Ssc.PetrolPricing.Portal.Facade
{
    public class ServiceFacade : IDisposable
    {
        private Lazy<HttpClient> _client;

        public ServiceFacade()
        {
            _client = new Lazy<HttpClient>();
            _client.Value.BaseAddress = new Uri(ConfigurationManager.AppSettings["ServicesBaseUrl"] + "");
            
            _client.Value.DefaultRequestHeaders.Accept.Clear();
            _client.Value.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        // Get a list of sites
        public IEnumerable<Site> GetSites()
        {
            var response = _client.Value.GetAsync("api/sites/").Result;

            var result = response.Content.ReadAsAsync<IEnumerable<Site>>().Result;
            return (response.IsSuccessStatusCode) ? result : null ;
        }

        // Get a Site
        public Site GetSite(int siteId)
        {
            var response = _client.Value.GetAsync("api/sites/" + siteId).Result;

            var result = response.Content.ReadAsAsync<Site>().Result;
            return (response.IsSuccessStatusCode) ? result : null;
        }

        public Site NewSite(Site site) 
        {
            const string apiUrl = "api/Sites/";
            
            var response = _client.Value.PostAsync(apiUrl, site, new JsonMediaTypeFormatter()).Result;
            var result = response.Content.ReadAsAsync<Site>().Result;

            return (response.IsSuccessStatusCode) ? result : null;
        }

        public Site EditSite(Site site)
        {
           //TODO
           const string apiUrl = "api/Sites/";

           var response = _client.Value.PutAsync(apiUrl, site, new JsonMediaTypeFormatter()).Result;
           var result = response.Content.ReadAsAsync<Site>().Result;

           return (response.IsSuccessStatusCode) ? result : null;
        }

        //TODO feed in real data from price grid
        public void EmailUpdatedPricesToSite()
        {

            var response = _client.Value.GetAsync("api/emailSite?siteId=1&endTradeDate=11/12/2015").Result;
           
        }

        //TODO feed in real data from price grid
        public void EmailUpdatedPricesToAllSite(int siteId, DateTime dateOfUpdate)
        {
            
        }


        /// <summary>
        /// List of SitePriceViewModel for Site Pricing View
        /// </summary>
        /// <returns></returns>
        public IEnumerable<SitePriceViewModel> GetSitePrices(DateTime? forDate=null, int siteId=0, int pageNo=1,
                int pageSize = Constants.PricePageSize, string apiName = "SitePrices")
        {
            // Optional params(defaults) - forDate (Date of Calc/Viewing today), siteId (0 for all sites), pageNo(1), PageSize(20)
            string filters = (forDate.HasValue) ? "forDate=" + forDate.Value.ToString("yyyy-MM-dd") + "&" : "";
            filters = filters + "siteId=" + siteId + "&";
            filters = filters + "pageNo=" + pageNo + "&";
            filters = filters + "pageSize=" + pageSize + "&";

            var apiUrl = String.IsNullOrEmpty(filters) ? String.Format("api/{0}/", apiName) : String.Format("api/{0}/?{1}", apiName, filters);
            var response = _client.Value.GetAsync(apiUrl).Result;

            // TODO if response.Content.Headers.ToString().Contains("Content-Type: application/json") do ReadAsync

            var result = response.Content.ReadAsAsync<IEnumerable<SitePriceViewModel>>().Result;
            return (response.IsSuccessStatusCode) ? result : null;
        }

        /// <summary>
        /// Gets competitors prices for Pricing screen
        /// </summary>
        /// <param name="forDate">optional, default to today</param>
        /// <param name="siteId">optional siteId, defaults to all sites</param>
        /// <param name="pageNo"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public IEnumerable<SitePriceViewModel> GetCompetitorsWithPrices(DateTime? forDate = null, int siteId = 0, int pageNo = 1,
            int pageSize = Constants.PricePageSize)
        {
            return GetSitePrices(forDate, siteId, pageNo, Constants.PricePageSize, apiName: "CompetitorPrices");
        }

        // Get list of file uploads
        public IEnumerable<FileUpload> GetFileUploads(int? typeId, int? statusId) // 1 = Daily, 2 = Qtryly
        {
            var filters = typeId.HasValue ? "uploadTypeId=" + typeId.Value  + "&" : "";
            filters = statusId.HasValue ? filters + "statusId=" + statusId.Value + "&": "";

            var apiUrl = String.IsNullOrEmpty(filters) ? "api/fileuploads/" : "api/fileuploads/?" + filters;

            var response = _client.Value.GetAsync(apiUrl).Result;

            var result = response.Content.ReadAsAsync<IEnumerable<FileUpload>>().Result;

            return (response.IsSuccessStatusCode) ? result : null;
        }

        public FileUpload GetFileUpload(int id)
        {
            var apiUrl = "api/fileuploads/" + id;

            var response = _client.Value.GetAsync(apiUrl).Result;

            var result = response.Content.ReadAsAsync<FileUpload>().Result;

            return (response.IsSuccessStatusCode) ? result : null;
        }

        public FileUpload NewUpload(FileUpload fu) // 1 = Daily, 2 = Qtryly
        {
            const string apiUrl = "api/FileUpload/";

            var response = _client.Value.PostAsync(apiUrl, fu, new JsonMediaTypeFormatter()).Result;

            var result = response.Content.ReadAsAsync<FileUpload>().Result;

            return (response.IsSuccessStatusCode) ? result : null;
        }
        public IEnumerable<FileUpload> ExistingDailyUploads(DateTime uploadDatetime)
        {
            string apiUrl = "api/ExistingDailyUploads/" + uploadDatetime.ToString("yyyy-MM-dd");

            var response = _client.Value.GetAsync(apiUrl).Result;

            var result = response.Content.ReadAsAsync<IEnumerable<FileUpload>>().Result;

            return (response.IsSuccessStatusCode) ? result : null;
        }
        public string GetUploadPath()
        {
            var response = _client.Value.GetAsync("api/settings/" + SettingsKeys.UploadPath).Result;

            var result = response.Content.ReadAsAsync<string>().Result;
            return (response.IsSuccessStatusCode) ? result : null;
        }

        public IEnumerable<UploadType> GetUploadTypes()
        {
            var apiUrl = "api/UploadTypes/";

            var response = _client.Value.GetAsync(apiUrl).Result;

            var result = response.Content.ReadAsAsync<IEnumerable<UploadType>>().Result;

            return (response.IsSuccessStatusCode) ? result : null;
        }

        public IEnumerable<FuelType> GetFuelTypes()
        {
            var apiUrl = "api/UploadTypes/";

            var response = _client.Value.GetAsync(apiUrl).Result;

            var result = response.Content.ReadAsAsync<IEnumerable<FuelType>>().Result;

            return (response.IsSuccessStatusCode) ? result : null;
        }

        public IEnumerable<ImportProcessStatus> GetProcessStatuses()
        {
            var apiUrl = "api/ProcessStatuses/";

            var response = _client.Value.GetAsync(apiUrl).Result;

            var result = response.Content.ReadAsAsync<IEnumerable<ImportProcessStatus>>().Result;

            return (response.IsSuccessStatusCode) ? result : null;
        }


        public string ReInitDb(string option = "")
        {
            var apiUrl = "api/ReInitDb?buildOptions=" + option;

            var response = _client.Value.GetAsync(apiUrl).Result;

            var result = response.Content.ReadAsStringAsync().Result;
            
            return result; //(response.IsSuccessStatusCode) ? result : null;
        }
        /// <summary>
        /// Save site Price overrides back to backend
        /// </summary>
        /// <param name="sitePriceViewModel"></param>
        public async Task<List<SitePriceViewModel>> UpdateSitePricesAsync(List<SitePriceViewModel> sitePriceViewModel)
        {
            //TODO PUT to Api
            await Task.FromResult(0);
            return sitePriceViewModel;
        }

        // TRULY Async static method
        public async Task<HttpResponseMessage> RunAsync(string siteViewJson, HttpMethod method)
        {
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                var serviceUrl = String.Format("{0}api/Sites", ConfigurationManager.AppSettings["ServicesBaseUrl"]);

                var request = new HttpRequestMessage(method, serviceUrl)
                {
                    Content = new StringContent(siteViewJson,
                        Encoding.UTF8,
                        "application/json")
                };

                var response = await client.SendAsync(request)
                    .ContinueWith(responseTask =>
                    {
                        Console.WriteLine("Response: {0}", responseTask.Result);
                        return responseTask.Result;
                    });

                return !response.IsSuccessStatusCode ? new HttpResponseMessage { StatusCode = HttpStatusCode.NotImplemented } : response;
            }
        }
        public void Dispose()
        {
            _client = null;
        }


    }
}
