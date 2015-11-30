﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Web;
using System.Web.UI;
using JsPlc.Ssc.PetrolPricing.Models;
//using JsPlc.Ssc.PetrolPricing.Portal.Helpers.Extensions;
using JsPlc.Ssc.PetrolPricing.Models.Enums;
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

        public void Dispose()
        {
            _client = null;
        }
 
    }
}
