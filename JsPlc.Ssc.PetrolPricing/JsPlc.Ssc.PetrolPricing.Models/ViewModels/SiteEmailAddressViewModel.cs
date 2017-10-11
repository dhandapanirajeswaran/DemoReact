using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JsPlc.Ssc.PetrolPricing.Models.ViewModels
{
    public class SiteEmailAddressViewModel
    {
        public int SiteId { get; set; }
        public bool IsSiteActive { get; set; }
        public int StoreNo { get; set; }
        public int CatNo { get; set; }
        public int PfsNo { get; set; }
        public string StoreName { get; set; }
        public string EmailAddress { get; set; }
    }

    public class SiteEmailImportViewModel
    {
        public int StoreNo { get; set; }
        public string StoreName { get; set; }
        public string EmailAddress { get; set; }
    }


    public class SiteEmailImportResultViewModel
    {
        public StatusViewModel Status = new StatusViewModel();
        public ImportSiteEmailSettings Settings = new ImportSiteEmailSettings();
        public List<SiteEmailImportRowStatusViewModel> Row = new List<SiteEmailImportRowStatusViewModel>();
    }

    public class SiteEmailImportRowStatusViewModel
    {
        public int RowNumber { get; set; }
        public string StoreNo { get; set; }
        public string StoreName { get; set; }
        public string EmailAddress { get; set; }
        public string CatNo { get; set; }
        public string PfsNo { get; set; }
        public bool IsSuccess { get; set; }
        public string Message { get; set; }
    }

}