using JsPlc.Ssc.PetrolPricing.Business;
using JsPlc.Ssc.PetrolPricing.Core.Diagnostics;
using JsPlc.Ssc.PetrolPricing.Core.Interfaces;
using JsPlc.Ssc.PetrolPricing.Models;
using JsPlc.Ssc.PetrolPricing.Models.Enums;
using JsPlc.Ssc.PetrolPricing.Models.ViewModels;
using JsPlc.Ssc.PetrolPricing.Models.ViewModels.Diagnostics;
using JsPlc.Ssc.PetrolPricing.Models.ViewModels.SelfTest;
using JsPlc.Ssc.PetrolPricing.Models.ViewModels.SystemSettings;
using JsPlc.Ssc.PetrolPricing.Models.ViewModels.UserPermissions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using JsPlc.Ssc.PetrolPricing.Exporting.Interfaces;
using JsPlc.Ssc.PetrolPricing.Models.ViewModels.Schedule;

namespace JsPlc.Ssc.PetrolPricing.Portal.Facade
{

    public class GetCompetitorsWithPricesService : IGetCompetitorsWithPrices
    {
        private ServiceFacade _serviceFacade;

        public GetCompetitorsWithPricesService(ServiceFacade serviceFacade)
        {
            _serviceFacade = serviceFacade;
        }

        public IEnumerable<SitePriceViewModel> GetCompetitorsWithPrices(DateTime? forDate = default(DateTime?), int siteId = 0, int pageNo = 1, int pageSize = 2000)
        {
            return _serviceFacade.GetCompetitorsWithPrices(forDate, siteId, pageNo, pageSize);
        }
    }

    public class ServiceFacade : IDisposable
    {
        private Lazy<HttpClient> _client;
        private Lazy<HttpClient> _clientLongTimeout;
        private ILogger _logger;

        private const int LongTimeoutInSeconds = 180;

        public ServiceFacade(ILogger logger)
        {
            _client = new Lazy<HttpClient>();
            _client.Value.BaseAddress = new Uri(ConfigurationManager.AppSettings["ServicesBaseUrl"] + "");

            _client.Value.DefaultRequestHeaders.Accept.Clear();
            _client.Value.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            // create second client with long timeout

            _clientLongTimeout = new Lazy<HttpClient>();
            _clientLongTimeout.Value.BaseAddress = new Uri(ConfigurationManager.AppSettings["ServicesBaseUrl"] + "");
            _clientLongTimeout.Value.DefaultRequestHeaders.Accept.Clear();
            _clientLongTimeout.Value.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            _clientLongTimeout.Value.Timeout = TimeSpan.FromSeconds(LongTimeoutInSeconds);

            _logger = logger;
        }

        // Get a list of sites
        public IEnumerable<Site> GetSites()
        {
            var response = _client.Value.GetAsync("api/sites/").Result;

            var result = response.Content.ReadAsAsync<IEnumerable<Site>>().Result;
            return (response.IsSuccessStatusCode) ? result : null;
        }

        public PPUserList GetPPUsers()
        {
            var response = _client.Value.GetAsync("api/PPUsers/").Result;

            var result = response.Content.ReadAsAsync<PPUserList>().Result;
            return (response.IsSuccessStatusCode) ? result : null;
        }

        public PPUserList AddPPUser(PPUser user)
        {
            var querystring = "api/PPUsers/Add?email=" + user.Email + "&firstname=" + user.FirstName + "&lastname=" + user.LastName;

            var response = _client.Value.PostAsync(querystring, null).Result;

            var result = response.Content.ReadAsAsync<PPUserList>().Result;

            return (response.IsSuccessStatusCode) ? result : null;
        }

        public PPUserList DeletePPUser(string email)
        {
            var querystring = "api/PPUsers/Delete?email=" + email;

            var response = _client.Value.PostAsync(querystring, null).Result;

            var result = response.Content.ReadAsAsync<PPUserList>().Result;

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

        public async Task<List<EmailSendLog>> EmailUpdatedPricesSites(int emailTemplateId, string siteIdsList, DateTime? forDate = null, string apiName = "emailSites")
        {
            string filters = (forDate.HasValue) ? "endTradeDate=" + forDate.Value.ToString("yyyy-MM-dd") + "&" : "";
            filters = filters + "emailTemplateId=" + emailTemplateId + "&siteIdsList=" + siteIdsList + "&";
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

            DiagnosticLog.StartDebug("GetSitePrices");

            var apiUrl = String.IsNullOrEmpty(filters) ? String.Format("api/{0}/", apiName) : String.Format("api/{0}/?{1}", apiName, filters);
            var response = _clientLongTimeout.Value.GetAsync(apiUrl).Result;

            // TODO if response.Content.Headers.ToString().Contains("Content-Type: application/json") do ReadAsync
            if (response.IsSuccessStatusCode)
            {
                var result = response.Content.ReadAsAsync<IEnumerable<SitePriceViewModel>>().Result;
                DiagnosticLog.EndDebug("GetSitePrices success");
                return result;
            }
            else
            {
                DiagnosticLog.FailedDebug("GetSitePrices failed - " + response.StatusCode);
                return null;
            }
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
            if (response.IsSuccessStatusCode)
            {
                var result = response.Content.ReadAsAsync<IEnumerable<FileUpload>>().Result;
                return result;
            }
            else
            {
                return null;
            }
        }

        public async Task<FileUpload> GetFileUpload(int id)
        {
            var apiUrl = "api/fileuploads/" + id;

            var response = await _client.Value.GetAsync(apiUrl);
            if (response.IsSuccessStatusCode)
            {
                var result = response.Content.ReadAsAsync<FileUpload>().Result;
                return result;
            }
            else
            {
                return null;
            }
        }

        public FileUpload NewUpload(FileUpload fu) // 1 = Daily, 2 = Qtryly
        {
            const string apiUrl = "api/FileUpload/";

            var response = _clientLongTimeout.Value.PostAsync(apiUrl, fu, new JsonMediaTypeFormatter()).Result;
            if (response.IsSuccessStatusCode)
            {
                var result = response.Content.ReadAsAsync<FileUpload>().Result;
                return result;
            }
            else
            {
                return null;
            }
        }

        public Object SaveHoldFile(string heldFile, string destFile)
        {
            string apiUrl = string.Format("api/SaveHoldFile?heldfile={0}&destFile={1}", heldFile, destFile);

            var response = _client.Value.PostAsync(apiUrl, null).Result;

            var result = response.Content.ReadAsAsync<Object>().Result;

            return (response.IsSuccessStatusCode) ? result : null;
        }

        public Object SaveFile(HttpPostedFileBase _uploadedFile, string fileName) // 1 = Daily, 2 = Qtryly
        {
            string apiUrl = string.Format("api/SaveFile?file={0}", fileName);

            MemoryStream target = new MemoryStream();
            _uploadedFile.InputStream.CopyTo(target);
            byte[] data = target.ToArray();

            var response = _client.Value.PostAsync(apiUrl, data, new JsonMediaTypeFormatter()).Result;
            if (response.IsSuccessStatusCode)
            {
                var result = response.Content.ReadAsAsync<Object>().Result;
                return result;
            }
            else
            {
                return null;
            }
        }

        public string ImportFileEmailFile(HttpPostedFileBase file)
        {
            var apiUrl = String.Format("api/ImportSiteEmailFile");

            MemoryStream target = new MemoryStream();
            file.InputStream.CopyTo(target);
            byte[] data = target.ToArray();

            var response = _client.Value.PostAsync(apiUrl, data, new JsonMediaTypeFormatter()).Result;
            return response.IsSuccessStatusCode
                ? ""
                : "Unable to Import Site Emails";
        }

        public async Task<IEnumerable<FileUpload>> ExistingDailyUploads(DateTime uploadDatetime)
        {
            string apiUrl = "api/ExistingDailyUploads/" + uploadDatetime.ToString("yyyy-MM-dd");

            var response = await _client.Value.GetAsync(apiUrl);
            if (response.IsSuccessStatusCode)
            {
                var result = response.Content.ReadAsAsync<IEnumerable<FileUpload>>().Result;
                return result;
            }
            else
            {
                return null;
            }
        }

        public string GetUploadPath()
        {
            var response = _client.Value.GetAsync("api/settings/" + SettingsKeys.UploadPath).Result;
            if (response.IsSuccessStatusCode)
            {
                var result = response.Content.ReadAsAsync<string>().Result;
                return result;
            }
            else
            {
                return null;
            }
        }

        public IEnumerable<UploadType> GetUploadTypes()
        {
            var apiUrl = "api/UploadTypes/";

            var response = _client.Value.GetAsync(apiUrl).Result;

            if (response.IsSuccessStatusCode)
            {
                var result = response.Content.ReadAsAsync<IEnumerable<UploadType>>().Result;
                return result;
            }
            else
            {
                return null;
            }
        }

        public IEnumerable<FuelType> GetFuelTypes()
        {
            var apiUrl = "api/FuelTypes/";

            var response = _client.Value.GetAsync(apiUrl).Result;

            if (response.IsSuccessStatusCode)
            {
                var result = response.Content.ReadAsAsync<IEnumerable<FuelType>>().Result;
                return result;
            }
            else
            {
                return null;
            }
        }

        public IEnumerable<ImportProcessStatus> GetProcessStatuses()
        {
            var apiUrl = "api/ProcessStatuses/";

            var response = _client.Value.GetAsync(apiUrl).Result;

            if (response.IsSuccessStatusCode)
            {
                var result = response.Content.ReadAsAsync<IEnumerable<ImportProcessStatus>>().Result;
                return result;
            }
            else
            {
                return null;
            }
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
                if (response.IsSuccessStatusCode)
                {
                    var result = response.Content.ReadAsAsync<CompetitorSiteReportViewModel>().Result;
                    return result;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
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
                if (response.IsSuccessStatusCode)
                {
                    var result = response.Content.ReadAsAsync<PricePointReportViewModel>().Result;
                    return result;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
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
                if (response.IsSuccessStatusCode)
                {
                    var result = response.Content.ReadAsAsync<ComplianceReportViewModel>().Result;
                    return result;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
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

                if (response.IsSuccessStatusCode)
                {
                    var result = response.Content.ReadAsAsync<NationalAverageReportViewModel>().Result;
                    return result;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
                throw new Exception("Exception in Reports Generation. Contact support team." + System.Environment.NewLine + ex.Message, ex);
            }
            return null;
        }

        public NationalAverageReportViewModel CompetitorsPriceRangeData(DateTime when)
        {
            try
            {
                var url = string.Format("api/GetReportcompetitorsPriceRange/{0}", when.ToString("ddMMMyyyy"));
                var response = _client.Value.GetAsync(url).Result;

                if (response.IsSuccessStatusCode)
                {
                    var result = response.Content.ReadAsAsync<NationalAverageReportViewModel>().Result;
                    return result;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
                throw new Exception("Exception in Reports Generation. Contact support team." + System.Environment.NewLine + ex.Message, ex);
            }
            return null;
        }

        public NationalAverageReportViewModel GetNationalAverage2(DateTime when, bool bViewAllCompetitors = false)
        {
            try
            {
                var url = string.Format("api/GetNationalAverage2/{0}/{1}", when.ToString("ddMMMyyyy"), bViewAllCompetitors);
                var response = _client.Value.GetAsync(url).Result;

                if (response.IsSuccessStatusCode)
                {
                    var result = response.Content.ReadAsAsync<NationalAverageReportViewModel>().Result;
                    return result;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
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

                if (response.IsSuccessStatusCode)
                {
                    var result = response.Content.ReadAsAsync<CompetitorsPriceRangeByCompanyViewModel>().Result;
                    return result;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
                throw new Exception("Exception in Reports Generation. Contact support team." + System.Environment.NewLine + ex.Message, ex);
            }
            return null;
        }

        public PriceMovementReportViewModel GetPriceMovement(string brandName, DateTime fromDate, DateTime toDate, int fuelTypeId, string siteName)
        {
            try
            {
                var url = string.Format("api/GetPriceMovement/{0}/{1}/{2}/{3}/{4}", fromDate.ToString("ddMMMyyyy"), toDate.ToString("ddMMMyyyy"), fuelTypeId, brandName, siteName == null ? "empty" : siteName);

                var response = _client.Value.GetAsync(url).Result;

                if (response.IsSuccessStatusCode)
                {
                    var result = response.Content.ReadAsAsync<PriceMovementReportViewModel>().Result;
                    return result;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
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
                if (response.IsSuccessStatusCode)
                {
                    var result = response.Content.ReadAsAsync<string>().Result;
                    return result;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
                throw new Exception("Exception in Reports Generation. Contact support team." + System.Environment.NewLine + ex.Message, ex);
            }
            return null;
        }

        public void RegisterUser(string email)
        {
            var usersList = GetPPUsers();
            var user = (from PPUser a in usersList.Users
                        where a.Email.ToLower() == email.ToLower()
                        select a).SingleOrDefault();
            MailAddress address = new MailAddress(email);
            string host = address.Host;
            if ((user != null && host == "sainsburys.co.uk") || (user != null && host == "jsCoventryDev.onmicrosoft.com"))
            {
                var apiUrl = string.Format("api/user?email={0}", email);

                var response = _client.Value.PostAsync(apiUrl, new { }, new JsonMediaTypeFormatter()).Result;
            }
            else
            {
                HttpContext.Current.Response.Redirect("~/Account/LogOff");
            }
        }

        public void SuccessfulSignIn(string email)
        {
            try
            {
                var apiUrl = String.Format("api/signin?email={0}", email);
                var response = _client.Value.PostAsync(apiUrl, new { }, new JsonMediaTypeFormatter()).Result;
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
            }
        }

        public PPUserDetailsViewModel GetPPUser(int ppUserId)
        {
            var querystring = "api/PPUsers/Edit?id=" + ppUserId;

            var response = _client.Value.GetAsync(querystring).Result;
            if (response.IsSuccessStatusCode)
            {
                var result = response.Content.ReadAsAsync<PPUserDetails>().Result;

                var model = new PPUserDetailsViewModel()
                {
                    Status = new StatusViewModel()
                    {
                        ErrorMessage = "",
                        SuccessMessage = ""
                    },
                    User = result.User,
                    Permissions = UserPermissionsFascade.BuildUserPermissionsViewModel(result.Permissions)
                };

                return model;
            }
            return null;
        }

        public IEnumerable<string> GetExcludeBrands()
        {
            var response = _client.Value.GetAsync("api/excludebrands").Result;

            var result = response.Content.ReadAsAsync<IEnumerable<string>>().Result;
            return (response.IsSuccessStatusCode) ? result : null;
        }

        public FacadeResponse<SiteViewModel> UpdateExcludeBrands(SiteViewModel site)
        {
            //TODO
            const string apiUrl = "api/SaveExcludeBrands/";

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

        // Get a list of companies
        public bool CalcDailyPrices(int siteId)
        {
            var url = string.Format("api/CalcDailyPrices?siteId={0}", siteId);

            try
            {
                var response = _client.Value.GetAsync(url).Result;

                var result = response.Content.ReadAsAsync<bool>().Result;
                return (response.IsSuccessStatusCode) ? result : false;
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
                return false;
            }
        }

        public SiteNoteViewModel GetSiteNote(int siteId)
        {
            try
            {
                var response = _client.Value.GetAsync("api/GetSiteNote/" + siteId).Result;
                if (response.IsSuccessStatusCode)
                {
                    var result = response.Content.ReadAsAsync<SiteNoteViewModel>().Result;
                    return result;
                }
                else
                    return null;
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
                throw new Exception("Exception in GetSiteNote" + System.Environment.NewLine + ex.Message, ex);
            }
        }

        public JsonResultViewModel<bool> UpdateSiteNote(SiteNoteUpdateViewModel model)
        {
            try
            {
                const string apiUrl = "api/UpdateSiteNote/";

                var response = _client.Value.PostAsync(apiUrl, model, new JsonMediaTypeFormatter()).Result;

                if (response.IsSuccessStatusCode)
                {
                    var result = response.Content.ReadAsAsync<JsonResultViewModel<bool>>().Result;
                    return result;
                }
                else
                    return null;
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
                throw new Exception("Exception in UpdateSiteNote" + System.Environment.NewLine + ex.Message, ex);
            }
        }

        public JsonResultViewModel<int> DeleteSiteNode(int siteId)
        {
            try
            {
                var apiUrl = "api/DeleteSiteNote/" + siteId;

                var response = _client.Value.PostAsync(apiUrl, siteId, new JsonMediaTypeFormatter()).Result;

                if (response.IsSuccessStatusCode)
                {
                    var result = response.Content.ReadAsAsync<JsonResultViewModel<int>>().Result;
                    return result;
                }
                else
                    return null;
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
                throw new Exception("Exception in DeleteSiteNote" + System.Environment.NewLine + ex.Message, ex);
            }
        }

        public RecentFileUploadSummary GetRecentFileUploadSummary()
        {
            try
            {
                var apiUrl = "api/GetRecentFileUploadSummary";

                var response = _client.Value.GetAsync(apiUrl).Result;
                if (response.IsSuccessStatusCode)
                {
                    var result = response.Content.ReadAsAsync<RecentFileUploadSummary>().Result;
                    return result;
                }
                else
                    return new RecentFileUploadSummary();
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
                throw new Exception("Exception in GetRecentFileUploadSummary" + System.Environment.NewLine + ex.Message, ex);
            }
        }

        public IEnumerable<ContactDetailViewModel> GetContactDetails()
        {
            try
            {
                var apiUrl = "api/GetContactDetails";

                var response = _client.Value.GetAsync(apiUrl).Result;
                if (response.IsSuccessStatusCode)
                {
                    var result = response.Content.ReadAsAsync<IEnumerable<ContactDetailViewModel>>().Result;
                    return result;
                }
                else
                    return new List<ContactDetailViewModel>();
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
                throw new Exception("Exception in GetContactDetails");
            }
        }

        public DiagnosticsViewModel GetDiagnostics(int daysAgo, string logFilePath)
        {
            try
            {
                var apiUrl = String.Format("api/GetDiagnostics?daysAgo={0}&logFilePath={1}", daysAgo, logFilePath);
                var response = _client.Value.GetAsync(apiUrl).Result;
                if (response.IsSuccessStatusCode)
                {
                    var result = response.Content.ReadAsAsync<DiagnosticsViewModel>().Result;
                    PopulateSchedulerStatusModel(result);
                    return result;
                }
                else
                    return new DiagnosticsViewModel();
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
                return new DiagnosticsViewModel()
                {
                    ApiExceptionMessage = ex.ToString()
                };
            }
        }


        public bool UpdateDiagnosticsSettings(DiagnosticsSettingsViewModel settings)
        {
            try
            {
                var apiUrl = String.Format("api/UpdateDiagnosticsSettings");
                var response = _client.Value.PostAsync(apiUrl, settings, new JsonMediaTypeFormatter()).Result;
                var result = response.Content.ReadAsAsync<bool>().Result;
                return result;
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
                return false;
            }
        }

        public bool ClearDiagnosticsLog()
        {
            try
            {
                var apiUrl = String.Format("api/ClearDiagnosticsLog");
                var response = _client.Value.GetAsync(apiUrl).Result;
                var result = response.Content.ReadAsAsync<bool>().Result;
                return (response.IsSuccessStatusCode) ? result : false;
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
                throw new Exception("Exception in ClearDiagnosticsLog" + System.Environment.NewLine + ex.Message, ex);
            }
        }

        public async Task<DiagnosticsErrorLogFileViewModel> GetDiagnosticsErrorLogFile(string logFilePath, string filename)
        {
            try
            {
                var apiUrl = String.Format("api/GetErrorLogFile?logFilePath={0}&filename={1}", logFilePath, filename);
                var response = await _client.Value.GetAsync(apiUrl);
                var result = response.Content.ReadAsAsync<DiagnosticsErrorLogFileViewModel>().Result;
                return (response.IsSuccessStatusCode) ? result : null;
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
                throw new Exception("Exception in GetDiagnosticsErrorLogFile" + System.Environment.NewLine + ex.Message, ex);
            }
        }

        public UserAccessViewModel GetUserAccessModel(string userName)
        {
            if (String.IsNullOrWhiteSpace(userName))
                return new UserAccessViewModel();

            try
            {
                var apiUrl = String.Format("api/PPUsers/Access/?userName={0}", userName);
                var response = _client.Value.GetAsync(apiUrl).Result;
                var result = response.Content.ReadAsAsync<UserAccessViewModel>().Result;
                return response.IsSuccessStatusCode
                    ? result
                    : new UserAccessViewModel();
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
                throw new Exception("Exception in GetUserAccessModel" + System.Environment.NewLine + ex.Message, ex);
            }
        }

        public async Task<FileDownloadViewModel> DownloadFile(int fileUploadId)
        {
            try
            {
                var apiUrl = String.Format("api/File/Download/?fileUploadId={0}", fileUploadId);
                var response = await _client.Value.GetAsync(apiUrl);
                var result = response.Content.ReadAsAsync<FileDownloadViewModel>().Result;
                return response.IsSuccessStatusCode
                    ? result
                    : new FileDownloadViewModel();
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
                throw new Exception("Exception in DownloadFile" + System.Environment.NewLine + ex.Message, ex);
            }
        }

        public async Task<bool> DataCleanseFileUploads()
        {
            try
            {
                var apiUrl = String.Format("api/File/DataCleanse");
                var response = await _client.Value.GetAsync(apiUrl);
                var result = response.Content.ReadAsAsync<bool>().Result;
                return response.IsSuccessStatusCode
                    ? result
                    : false;
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
                throw new Exception("Exception in DataCleanseFileUploads" + System.Environment.NewLine + ex.Message, ex);
            }
        }

        public IEnumerable<SelectItemViewModel> GetQuarterlyFileUploadOptions()
        {
            try
            {
                var apiUrl = String.Format("api/GetQuarterlyFileUploadOptions");
                var response = _client.Value.GetAsync(apiUrl).Result;
                var result = response.Content.ReadAsAsync<IEnumerable<SelectItemViewModel>>().Result;
                return response.IsSuccessStatusCode
                    ? result
                    : new List<SelectItemViewModel>();
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
                throw new Exception("Exception in GetQuarterlyFileUploadOptions" + System.Environment.NewLine + ex.Message, ex);
            }
        }

        public FacadeResponse<QuarterlySiteAnalysisReportViewModel> GetQuarterlySiteAnalysisReport(int leftFileUploadId, int rightFileuploadId)
        {
            try
            {
                var apiUrl = String.Format("api/GetQuarterlySiteAnalysisReport/{0}/{1}", leftFileUploadId, rightFileuploadId);
                var response = _client.Value.GetAsync(apiUrl).Result;
                var result = response.Content.ReadAsAsync<QuarterlySiteAnalysisReportViewModel>().Result;

                var model = new FacadeResponse<QuarterlySiteAnalysisReportViewModel>();

                if (response.IsSuccessStatusCode)
                {
                    model.ViewModel = result;
                }
                else
                {
                    var joResponse = JObject.Parse(response.Content.ReadAsStringAsync().Result);
                    model.ErrorMessage = joResponse["Message"].ToString();
                }
                return model;
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
                throw new Exception("Exception in GetQuarterlySiteAnalysisReport" + System.Environment.NewLine + ex.Message, ex);
            }
        }

        public FileUpload GetFileUploadInformation(int fileUploadId)
        {
            try
            {
                var apiUrl = String.Format("api/GetFileUploadInformation/{0}", fileUploadId);
                var response = _client.Value.GetAsync(apiUrl).Result;
                var result = response.Content.ReadAsAsync<FileUpload>().Result;
                return response.IsSuccessStatusCode
                    ? result
                    : new FileUpload();
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
                throw new Exception("Exception in GetFileUploadInformation" + System.Environment.NewLine + ex.Message, ex);
            }
        }

        public QuarterlySiteAnalysisContainerViewModel GetQuarterlySiteAnalysisContainerViewModel(int leftFileUploadId, int rightFileUploadId)
        {
            try
            {
                var apiUrl = String.Format("api/GetQuarterlySiteAnalysisContainerViewModel/{0}/{1}", leftFileUploadId, rightFileUploadId);
                var response = _client.Value.GetAsync(apiUrl).Result;
                var result = response.Content.ReadAsAsync<QuarterlySiteAnalysisContainerViewModel>().Result;
                return response.IsSuccessStatusCode
                    ? result
                    : new QuarterlySiteAnalysisContainerViewModel();
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
                throw new Exception("Exception in GetQuarterlySiteAnalysisContainerViewModel" + System.Environment.NewLine + ex.Message, ex);
            }
        }

        public bool DeleteAllData()
        {
            try
            {
                var apiUrl = String.Format("api/DeleteAllData");
                var response = _client.Value.GetAsync(apiUrl).Result;
                var result = response.Content.ReadAsAsync<bool>().Result;
                return response.IsSuccessStatusCode
                    ? result
                    : false;
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
                throw new Exception("Exception in DeleteAllData" + System.Environment.NewLine + ex.Message, ex);
            }
        }

        public DataSanityCheckSummaryViewModel GetDataSanityCheckSummary()
        {
            try
            {
                var apiUrl = String.Format("api/GetDataSanityCheckSummary");
                var response = _client.Value.GetAsync(apiUrl).Result;
                var result = response.Content.ReadAsAsync<DataSanityCheckSummaryViewModel>().Result;
                return response.IsSuccessStatusCode
                    ? result
                    : new DataSanityCheckSummaryViewModel();
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
                throw new Exception("Exception in GetDataSanityCheckSummary" + System.Environment.NewLine + ex.Message, ex);
            }
        }

        public SystemSettingsViewModel GetSystemSettings()
        {
            try
            {
                var apiUrl = String.Format("api/GetSystemSettings");
                var response = _client.Value.GetAsync(apiUrl).Result;
                var result = response.Content.ReadAsAsync<SystemSettingsViewModel>().Result;
                return response.IsSuccessStatusCode
                    ? result
                    : new SystemSettingsViewModel();
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
                throw new Exception("Exception in GetSystemSettings" + System.Environment.NewLine + ex.Message, ex);
            }
        }

        public SystemSettingsViewModel UpdateSystemSettings(SystemSettingsViewModel model)
        {
            try
            {
                var apiUrl = String.Format("api/UpdateSystemSettings");
                var response = _client.Value.PostAsync(apiUrl, model, new JsonMediaTypeFormatter()).Result;
                var result = response.Content.ReadAsAsync<SystemSettingsViewModel>().Result;
                if (response.IsSuccessStatusCode)
                {
                    result.Status.SuccessMessage = "Saved System Settings";
                }
                else
                {
                    result = new SystemSettingsViewModel()
                    {
                        Status = new StatusViewModel()
                        {
                            ErrorMessage = "Unable to save System Settings"
                        }
                    };
                }
                return result;
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
                throw new Exception("Exception in UpdateSystemSettings" + System.Environment.NewLine + ex.Message, ex);
            }
        }

        public SitePricingSettings GetSitePricingSettings()
        {
            try
            {
                var apiUrl = "api/GetSitePricingSettings";
                var response = _client.Value.GetAsync(apiUrl).Result;
                var result = response.Content.ReadAsAsync<SitePricingSettings>().Result;
                return response.IsSuccessStatusCode
                    ? result
                    : null;
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
                throw new Exception("Exception in GetSitePricingSettings" + System.Environment.NewLine + ex.Message, ex);
            }
        }

        public async Task<IEnumerable<EmailTemplateNameViewModel>> GetEmailTemplateNames()
        {
            var apiUrl = "api/GetEmailTemplateNames";
            return CallAndCatchAsyncGet<IEnumerable<EmailTemplateNameViewModel>>("GetEmailTemplateNames", apiUrl);
        }

        public async Task<EmailTemplateViewModel> CreateEmailTemplateClone(int ppUserid, int emailTemplateId, string templateName)
        {
            var apiUrl = String.Format("api/CreateEmailTemplateClone/{0}/{1}/{2}", ppUserid, emailTemplateId, templateName);
            return CallAndCatchAsyncGet<EmailTemplateViewModel>("CreateEmailTemplateClone", apiUrl);
        }

        public async Task<EmailTemplateViewModel> GetEmailTemplate(int emailTemplateId)
        {
            var apiUrl = String.Format("api/GetEmailTemplate/{0}", emailTemplateId);
            return CallAndCatchAsyncGet<EmailTemplateViewModel>("GetEmailTemplate", apiUrl);
        }

        public async Task<EmailTemplateViewModel> UpdateEmailTemplate(EmailTemplateViewModel template)
        {
            var apiUrl = "api/UpdateEmailTemplate";
            return CallAndCatchAsyncPost<EmailTemplateViewModel, EmailTemplateViewModel>("UpdateEmailTemplate", apiUrl, template);
        }

        public async Task<bool> DeleteEmailTemplate(int ppUserId, int emailTemplateId)
        {
            var apiUrl = String.Format("api/DeleteEmailTemplate/{0}/{1}", ppUserId, emailTemplateId);
            return CallAndCatchAsyncGet<bool>("DeleteEmailTemplate", apiUrl);
        }

        public DriveTimeFuelSettingsViewModel GetAllDriveTimeMarkups()
        {
            try
            {
                var apiUrl = String.Format("api/GetAllDriveTimeMarkups");
                var allDriveTimeMarkups = CallAndCatchAsyncGet<IEnumerable<DriveTimeMarkupViewModel>>("GetAllDriveTimeMarkups", apiUrl);

                var model = new DriveTimeFuelSettingsViewModel()
                {
                    Unleaded = allDriveTimeMarkups.Where(x => x.FuelTypeId == (int)FuelTypeItem.Unleaded).OrderBy(x => x.DriveTime).ToList(),
                    Diesel = allDriveTimeMarkups.Where(x => x.FuelTypeId == (int)FuelTypeItem.Diesel).OrderBy(x => x.DriveTime).ToList(),
                    SuperUnleaded = allDriveTimeMarkups.Where(x => x.FuelTypeId == (int)FuelTypeItem.Super_Unleaded).OrderBy(x => x.DriveTime).ToList(),
                };

                return model;
            }
            catch (Exception ex)
            {
                return new DriveTimeFuelSettingsViewModel()
                {
                    Status = new StatusViewModel()
                    {
                        ErrorMessage = "Unable to fetch Drive Time Markups"
                    }
                };
            }
        }

        public async Task<StatusViewModel> UpdateDriveTimeMarkups(List<DriveTimeMarkupViewModel> model)
        {
            var apiUrl = String.Format("api/UpdateDriveTimeMarkups");
            return CallAndCatchAsyncPost<StatusViewModel, List<DriveTimeMarkupViewModel>>("UpdateDriveTimeMarkups", apiUrl, model);
        }

        public async Task<StatusViewModel> RecalculateDailyPrices(DateTime when)
        {
            var apiUrl = String.Format("api/RecalculateDailyPrices/{0}", when);
            return CallAndCatchAsyncGet<StatusViewModel>("RecalculateDailyPrices", apiUrl);
        }

        public BrandsCollectionSummaryViewModel GetBrandSettingsSummary()
        {
            var apiUrl = String.Format("api/GetBrandSettingsSummary");
            return CallAndCatchAsyncGet<BrandsCollectionSummaryViewModel>("GetBrandSettingsSummary", apiUrl);
        }

        public BrandsCollectionSettingsViewModel GetBrandSettings()
        {
            var apiUrl = String.Format("api/GetBrandSettings");
            return CallAndCatchAsyncGet<BrandsCollectionSettingsViewModel>("GetBrandSettings", apiUrl);
        }

        public StatusViewModel UpdateBrandSettings(BrandsSettingsUpdateViewModel model)
        {
            var apiUrl = String.Format("api/UpdateBrandSettings");
            return CallAndCatchAsyncPost<StatusViewModel, BrandsSettingsUpdateViewModel>("UpdateBrandSettings", apiUrl, model);
        }

        public PriceSnapshotViewModel GetPriceSnapshotForDay(DateTime day)
        {
            var apiUrl = String.Format("api/GetPriceSnapshotForDay/?day={0}", day.Ticks);
            return CallAndCatchAsyncGet<PriceSnapshotViewModel>("GetPriceSnapshotForDay", apiUrl);
        }

        public StatusViewModel TriggerDailyPriceRecalculation(DateTime day)
        {
            var apiUrl = String.Format("api/TriggerDailyPriceRecalculation/?day={0}", day.Ticks);
            return CallAndCatchAsyncGet<StatusViewModel>("TriggerDailyPriceRecalculation", apiUrl);
        }

        public StatusViewModel RemoveAllSiteEmailAddresses()
        {
            var apiUrl = String.Format("api/RemoveAllSiteEmailAddresses");
            return CallAndCatchAsyncGet<StatusViewModel>("RemoveAllSiteEmailAddresses", apiUrl);
        }

        public IEnumerable<SiteEmailAddressViewModel> GetAllSiteEmailAddresses(int siteId=0)
        {
            var apiUrl = String.Format("api/GetAllSiteEmailAddresses/?siteId={0}", siteId);
            return CallAndCatchAsyncGet<IEnumerable<SiteEmailAddressViewModel>>("GetAllSiteEmailAddresses", apiUrl);
        }

        public StatusViewModel UpsertSiteEmailAddresses(IEnumerable<SiteEmailAddressViewModel> siteEmailAddresses)
        {
            var apiUrl = String.Format("api/UpsertSiteEmailAddresses");
            return CallAndCatchAsyncPost<StatusViewModel, IEnumerable<SiteEmailAddressViewModel>>("UpsertSiteEmailAddresses", apiUrl, siteEmailAddresses);
        }

        public IEnumerable<ScheduleItemViewModel> GetWinServiceScheduledItems()
        {
            var apiUrl = "api/GetWinServiceScheduledItems/";
            var model = CallAndCatchAsyncGet<IEnumerable<ScheduleItemViewModel>>("GetWinServiceScheduledItems", apiUrl);
            return model ?? new List<ScheduleItemViewModel>();
        }

        public IEnumerable<ScheduleEventLogViewModel> GetWinServiceEventLog()
        {
            var apiUrl = "api/GetWinServiceServiceEventLog/";
            var model = CallAndCatchAsyncGet<IEnumerable<ScheduleEventLogViewModel>>("GetWinServiceEventLog", apiUrl);
            return model ?? new List<ScheduleEventLogViewModel>();
        }

        public ScheduleItemViewModel UpsertWinServiceSchedule(ScheduleItemViewModel model)
        {
            var apiUrl = "api/UpsertWinServiceScheduleItem/";
            model = CallAndCatchAsyncPost<ScheduleItemViewModel, ScheduleItemViewModel>("UpsertWinServiceSchedule", apiUrl, model);
            return model ?? new ScheduleItemViewModel();
        }

        public ScheduleItemViewModel WinServiceGetScheduleItem(int winServiceScheduleId)
        {
            var apiUrl = String.Format("api/GetWinServiceScheduleItem/{0}", winServiceScheduleId);
            return CallAndCatchAsyncGet<ScheduleItemViewModel>("LoadEmailScheduleItem", apiUrl);
        }

        public StatusViewModel ExecuteWinServiceSchedule()
        {
            var apiUrl = String.Format("api/ExecuteWinServiceSchedule/");
            return CallAndCatchAsyncGet<StatusViewModel>("ExecuteWinServiceSchedule", apiUrl);
        }

        public StatusViewModel ClearWinServiceEventLog()
        {
            var apiUrl = String.Format("api/ClearWinServiceEventLog/");
            return CallAndCatchAsyncGet<StatusViewModel>("ClearWinServiceEventLog", apiUrl);
        }

        public List<int> GetJsSitesByPfsNum()
        {
            var apiUrl = String.Format("api/GetJsSitesByPfsNum");
            return CallAndCatchAsyncGet<List<int>>("GetJsSitesByPfsNum", apiUrl);
        }

        #region private methods

        private T CallAndCatchAsyncGet<T>(string methodName, string apiUrl)
        {
            try
            {
                var response = _client.Value.GetAsync(apiUrl).Result;
                if (response.IsSuccessStatusCode)
                    return response.Content.ReadAsAsync<T>().Result;
                else
                    return default(T);
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
                throw new Exception("Exception in " + methodName + System.Environment.NewLine + ex.Message, ex);
            }
        }

        private Tdst CallAndCatchAsyncPost<Tdst, Tsrc>(string methodName, string apiUrl, Tsrc data)
        {
            try
            {
                var response = _client.Value.PostAsync(apiUrl, data, new JsonMediaTypeFormatter()).Result;
                if (response.IsSuccessStatusCode)
                    return response.Content.ReadAsAsync<Tdst>().Result;
                else
                    return default(Tdst);
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
                throw new Exception("Exception in " + methodName + System.Environment.NewLine + ex.Message, ex);
            }
        }

        private void PopulateSchedulerStatusModel(DiagnosticsViewModel model)
        {
            if (model == null)
                return;

            var status = SimpleScheduler.GetStatus();

            model.SchedulerStatus = new DiagnosticsSchedulerStatusViewModel()
            {
                IsRunning = status.IsRunning,
                LastStarted = status.LastStarted,
                LastStopped = status.LastStopped,
                LastPolled = status.LastPolled,
                LastErrored = status.LastErrored,
                LastErrorMessage = status.LastErrorMessage
            };
        }

        #endregion private methods
    }
}