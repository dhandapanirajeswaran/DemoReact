using System;
using System.IO;
using System.Collections.Generic;
using System.Configuration;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Text;
using System.Web;
using System.Web.Security;
using System.Threading.Tasks;
using JsPlc.Ssc.PetrolPricing.Models;
using JsPlc.Ssc.PetrolPricing.Models.Enums;
using JsPlc.Ssc.PetrolPricing.Models.ViewModels;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

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
            return (response.IsSuccessStatusCode) ? result : null;
        }

        // Get a list of brands
        public IEnumerable<string> GetBrands()
        {
            var response = _client.Value.GetAsync("api/brands").Result;

            var result = response.Content.ReadAsAsync<IEnumerable<string>>().Result;
            return (response.IsSuccessStatusCode) ? result : null;
        }

        // Get a list of companies
        public Dictionary<string, int> GetCompanies()
        {
            var response = _client.Value.GetAsync("api/companies").Result;

            var result = response.Content.ReadAsAsync<Dictionary<string, int>>().Result;
            return (response.IsSuccessStatusCode) ? result : null;
        }

        // Get a Site
        public SiteViewModel GetSite(int siteId)
        {
            var response = _client.Value.GetAsync("api/sites/" + siteId).Result;

            var result = response.Content.ReadAsAsync<SiteViewModel>().Result;
            return (response.IsSuccessStatusCode) ? result : null;
        }

		public FacadeResponse<SiteViewModel> NewSite(SiteViewModel site)
        {
            const string apiUrl = "api/Sites/";

            var response = _client.Value.PostAsync(apiUrl, site, new JsonMediaTypeFormatter()).Result;
            var result = response.Content.ReadAsAsync<SiteViewModel>().Result;

			FacadeResponse<SiteViewModel> resultViewModel = new FacadeResponse<SiteViewModel>();

			if (response.IsSuccessStatusCode)
			{
				resultViewModel.ViewModel = result;
			}
			else
			{
				JObject joResponse = JObject.Parse(response.Content.ReadAsStringAsync().Result);
				resultViewModel.ErrorMessage = joResponse["Message"].ToString();
			}

			return resultViewModel;
        }

		public FacadeResponse<SiteViewModel> EditSite(SiteViewModel site)
        {
            //TODO
            const string apiUrl = "api/Sites/";

            var response = _client.Value.PutAsync(apiUrl, site, new JsonMediaTypeFormatter()).Result;
            var result = response.Content.ReadAsAsync<SiteViewModel>().Result;

			FacadeResponse<SiteViewModel> resultViewModel = new FacadeResponse<SiteViewModel>();

			if (response.IsSuccessStatusCode)
			{
				resultViewModel.ViewModel = result;
			}
			else
			{
				JObject joResponse = JObject.Parse(response.Content.ReadAsStringAsync().Result);
				resultViewModel.ErrorMessage = joResponse["Message"].ToString();
			}

			return resultViewModel;
        }

        public async Task<List<EmailSendLog>> EmailUpdatedPricesSites(int siteId = 0, DateTime? forDate = null, string apiName = "emailSites")
        {
            string filters = (forDate.HasValue) ? "endTradeDate=" + forDate.Value.ToString("yyyy-MM-dd") + "&" : "";
            filters = filters + "siteId=" + siteId + "&";
            var apiUrl = String.IsNullOrEmpty(filters) ? String.Format("api/{0}/", apiName) : String.Format("api/{0}/?{1}", apiName, filters);

            var response = await _client.Value.GetAsync(apiUrl);
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadAsAsync<List<EmailSendLog>>();
                return result;
            }
            else
            {
                var result = await response.Content.ReadAsStringAsync(); // Reads the HttpResponseMsg as Json
                throw new ApplicationException(result); // json error structure
            }
            // Dont wrap the whole call in try catch as we can handle exceptions in Controllers
        }

        public async Task<List<EmailSendLog>> GetEmailSendLog(int siteId = 0, DateTime? forDate = null, string apiName = "getEmailSendLog")
        {
            string filters = (forDate.HasValue) ? "forDate=" + forDate.Value.ToString("yyyy-MM-dd") + "&" : "";
            filters = filters + "siteId=" + siteId + "&";
            var apiUrl = String.IsNullOrEmpty(filters) ? String.Format("api/{0}/", apiName) : String.Format("api/{0}/?{1}", apiName, filters);

            var response = await _client.Value.GetAsync(apiUrl);
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadAsAsync<List<EmailSendLog>>();
                return result;
            }
            else
            {
                var result = await response.Content.ReadAsStringAsync(); // Reads the HttpResponseMsg as Json
                throw new ApplicationException(result); // json error structure
            }
            // Dont wrap the whole call in try catch as we can handle exceptions in Controllers
        }

        /// <summary>
        /// List of SitePriceViewModel for Site Pricing View
        /// </summary>
        /// <returns></returns>
        public IEnumerable<SitePriceViewModel> GetSitePrices(DateTime? forDate = null,
            string storeName = "",
            int catNo = 0,
            int storeNo = 0,
            string storeTown = "",
            int siteId = 0, int pageNo = 1,
            int pageSize = Constants.PricePageSize, 
            string apiName = "SitePrices")
        {
            // Optional params(defaults) - forDate (Date of Calc/Viewing today), siteId (0 for all sites), pageNo(1), PageSize(20)
            string filters = (forDate.HasValue) ? "forDate=" + forDate.Value.ToString("yyyy-MM-dd") + "&" : "";
            filters = filters + "storeName=" + storeName + "&";
            filters = filters + "storeTown=" + storeTown + "&";
            filters = filters + "catNo=" + catNo + "&";
            filters = filters + "storeNo=" + storeNo + "&";
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
            return GetSitePrices(forDate, string.Empty, 0, 0, string.Empty, siteId, pageNo, Constants.PricePageSize, apiName: "CompetitorPrices");
        }

        // Get list of file uploads
        public async Task<IEnumerable<FileUpload>> GetFileUploads(int? typeId, int? statusId) // 1 = Daily, 2 = Qtryly
        {
            var filters = typeId.HasValue ? "uploadTypeId=" + typeId.Value + "&" : "";
            filters = statusId.HasValue ? filters + "statusId=" + statusId.Value + "&" : "";

            var apiUrl = String.IsNullOrEmpty(filters) ? "api/fileuploads/" : "api/fileuploads/?" + filters;

            var response = await _client.Value.GetAsync(apiUrl);

            var result = response.Content.ReadAsAsync<IEnumerable<FileUpload>>().Result;

            return (response.IsSuccessStatusCode) ? result : null;
        }

        public async Task<FileUpload> GetFileUpload(int id)
        {
            var apiUrl = "api/fileuploads/" + id;

            var response = await _client.Value.GetAsync(apiUrl);

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


        public Object SaveFile(HttpPostedFileBase _uploadedFile, string fileName) // 1 = Daily, 2 = Qtryly
        {
           
            string apiUrl = string.Format("api/SaveFile?file={0}", fileName);

            MemoryStream target = new MemoryStream();
            _uploadedFile.InputStream.CopyTo(target);
            byte[] data = target.ToArray();

            var response = _client.Value.PostAsync(apiUrl, data, new JsonMediaTypeFormatter()).Result;

            var result = response.Content.ReadAsAsync<Object>().Result;

            return (response.IsSuccessStatusCode) ? result : null;
        }


        public async Task<IEnumerable<FileUpload>> ExistingDailyUploads(DateTime uploadDatetime)
        {
            string apiUrl = "api/ExistingDailyUploads/" + uploadDatetime.ToString("yyyy-MM-dd");

            var response = await _client.Value.GetAsync(apiUrl);

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
            var apiUrl = "api/FuelTypes/";

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


        public async Task<string> ReInitDb(string option = "")
        {
            var apiUrl = "api/ReInitDb?buildOptions=" + option;

            var response = await _client.Value.GetAsync(apiUrl);

            var result = await response.Content.ReadAsStringAsync();

            return result; 
        }

        /// <summary>
        /// Save site Price overrides back to backend
        /// </summary>
        /// <param name="siteOverridePriceViewModel"></param>
        public async Task<List<OverridePricePostViewModel>> SaveOverridePricesAsync(
            List<OverridePricePostViewModel> siteOverridePriceViewModel)
        {
            //Done PUT to Api
            var jsonData = JsonConvert.SerializeObject(siteOverridePriceViewModel);
            var response = await RunAsync(jsonData, HttpMethod.Put, "SaveOverridePrices");
            if (!response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadAsStringAsync();
                throw new ApplicationException(result); // json error structure
            }
            // TODO IDEALLY var result = await response.Content.ReadAsAsync<List<OverridePricePostViewModel>>();
            await Task.FromResult(0);
            return siteOverridePriceViewModel;
        }

        // TRULY Async static method (calls /api/{serviceUri})
        public async Task<HttpResponseMessage> RunAsync(string jsonData, HttpMethod method, string serviceUri)
        {
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                var serviceUrl = String.Format("{0}api/{1}", ConfigurationManager.AppSettings["ServicesBaseUrl"],
                    serviceUri);

                var request = new HttpRequestMessage(method, serviceUrl)
                {
                    Content = new StringContent(jsonData,
                        Encoding.UTF8,
                        "application/json")
                };

                var response = await client.SendAsync(request);

                return response;
            }
        }
        public void Dispose()
        {
            _client = null;
        }

        public CompetitorSiteReportViewModel GetCompetitorSites(int siteId)
        {
            try
            {
                var response = _client.Value.GetAsync("api/GetCompetitorSites/" + siteId).Result;
                var result = response.Content.ReadAsAsync<CompetitorSiteReportViewModel>().Result;
                return (response.IsSuccessStatusCode) ? result : null;
            }
            catch (Exception ex)
            {
                throw new Exception("Exception in Reports Generation. Contact support team." + System.Environment.NewLine + ex.Message, ex);
            }
            return null;
        }

        public PricePointReportViewModel GetPricePoints(DateTime when, int fuelTypeId)
        {
            try
            {
                var url = string.Format("api/GetPricePoints/{0}/{1}", when.ToString("ddMMMyyyy"), fuelTypeId);
                var response = _client.Value.GetAsync(url).Result;
                var result = response.Content.ReadAsAsync<PricePointReportViewModel>().Result;
                return (response.IsSuccessStatusCode) ? result : null;
            }
            catch (Exception ex)
            {
                throw new Exception("Exception in Reports Generation. Contact support team." + System.Environment.NewLine + ex.Message, ex);
            }
            return null;
        }

        public ComplianceReportViewModel GetReportCompliance(DateTime when)
        {
            try
            {
                var url = string.Format("api/GetComplianceReport/{0}", when.ToString("ddMMMyyyy"));
                var response = _client.Value.GetAsync(url).Result;
                var result = response.Content.ReadAsAsync<ComplianceReportViewModel>().Result;
                return (response.IsSuccessStatusCode) ? result : null;
            }
            catch (Exception ex)
            {
                throw new Exception("Exception in Reports Generation. Contact support team." + System.Environment.NewLine + ex.Message, ex);
            }
            return null;
        }

        public NationalAverageReportViewModel GetNationalAverage(DateTime when)
        {
            try
            {
                var url = string.Format("api/GetNationalAverage/{0}", when.ToString("ddMMMyyyy"));
                var response = _client.Value.GetAsync(url).Result;
                var result = response.Content.ReadAsAsync<NationalAverageReportViewModel>().Result;
                return (response.IsSuccessStatusCode) ? result : null;
            }
            catch (Exception ex)
            {
                throw new Exception("Exception in Reports Generation. Contact support team." + System.Environment.NewLine + ex.Message, ex);
            }
            return null;
        }

        public NationalAverageReportViewModel GetNationalAverage2(DateTime when)
        {
            try
            {
                var url = string.Format("api/GetNationalAverage2/{0}", when.ToString("ddMMMyyyy"));
                var response = _client.Value.GetAsync(url).Result;
                var result = response.Content.ReadAsAsync<NationalAverageReportViewModel>().Result;
                return (response.IsSuccessStatusCode) ? result : null;
            }
            catch (Exception ex)
            {
                throw new Exception("Exception in Reports Generation. Contact support team." + System.Environment.NewLine + ex.Message, ex);
            }
            return null;
        }

        public CompetitorsPriceRangeByCompanyViewModel GetCompetitorsPriceRangeByCompany(DateTime when, string companyName, string brandName)
        {
            try
            {
                var url = string.Format("api/GetCompetitorsPriceRangeByCompany/{0}/{1}/{2}", when.ToString("ddMMMyyyy"), companyName, brandName);
                var response = _client.Value.GetAsync(url).Result;
                var result = response.Content.ReadAsAsync<CompetitorsPriceRangeByCompanyViewModel>().Result;
                return (response.IsSuccessStatusCode) ? result : null;
            }
            catch (Exception ex)
            {
                throw new Exception("Exception in Reports Generation. Contact support team." + System.Environment.NewLine + ex.Message, ex);               
            }
            return null;
        }

        public PriceMovementReportViewModel GetPriceMovement(string brandName, DateTime fromDate, DateTime toDate, int fuelTypeId)
        {
            try
            {
                var url = string.Format("api/GetPriceMovement/{0}/{1}/{2}/{3}", fromDate.ToString("ddMMMyyyy"), toDate.ToString("ddMMMyyyy"), fuelTypeId, brandName);

                var response = _client.Value.GetAsync(url).Result;
                var result = response.Content.ReadAsAsync<PriceMovementReportViewModel>().Result;
                return (response.IsSuccessStatusCode) ? result : null;
            }
            catch (Exception ex)
            {
                throw new Exception("Exception in Reports Generation. Contact support team." + System.Environment.NewLine + ex.Message, ex);
            }
            return null;
        }

		public string CleanupIntegrationTestsData(string testUserName)
		{
            try
            {
                var url = string.Format("api/CleanupIntegrationTestsData?testUserName={0}", testUserName);

                var response = _client.Value.GetAsync(url).Result;
                var result = response.Content.ReadAsAsync<string>().Result;
                return (response.IsSuccessStatusCode) ? result : null;
            }
            catch (Exception ex)
            {
                throw new Exception("Exception in Reports Generation. Contact support team." + System.Environment.NewLine + ex.Message, ex);
            }
            return null;
		}

        public void RegisterUser(string email)
        {
            var apiUrl = string.Format("api/user?email={0}", email);

            var response = _client.Value.PostAsync(apiUrl, new { }, new JsonMediaTypeFormatter()).Result;

           SaveAuthenticationInfo(email);
            
        }

        private void SaveAuthenticationInfo(string email)
        {
            var timeout = int.Parse(ConfigurationManager.AppSettings["SessionTimeout"]);
            HttpCookie faCookie = new HttpCookie(FormsAuthentication.FormsCookieName, "");
            faCookie.Expires = DateTime.Now.AddMinutes(timeout);
            HttpContext.Current.Response.Cookies.Add(faCookie);

            HttpCookie faCookiePath = new HttpCookie(FormsAuthentication.FormsCookiePath, "");
            faCookiePath.Expires = DateTime.Now.AddMinutes((timeout / 2));
            HttpContext.Current.Response.Cookies.Add(faCookiePath);
        }




    }
}
