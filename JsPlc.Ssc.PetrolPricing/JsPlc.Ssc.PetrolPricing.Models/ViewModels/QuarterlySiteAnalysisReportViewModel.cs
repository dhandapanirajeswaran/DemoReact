using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JsPlc.Ssc.PetrolPricing.Models.ViewModels
{
    public class QuarterlySiteAnalysisReportViewModel
    {
        public int NewSiteCount { get; set; }
        public int DeletedSiteCount { get; set; }
        public int ExistingSiteCount { get; set; }
        public int TotalSiteCount { get; set; }
        public int ChangeOwnershipCount { get; set; }
        public int LeftTotalRecordCount { get; set; }
        public int RightTotalRecordCount { get; set; }

        public IEnumerable<QuarterlySiteAnalysisReportRowViewModel> Rows = new List<QuarterlySiteAnalysisReportRowViewModel>();
    }


    public class QuarterlySiteAnalysisReportRowViewModel
    {
        public int CatNo { get; set; }
        public string SiteName { get; set; }
        public bool HasLeftSite { get; set; }
        public bool HasRightSite { get; set; }
        public string LeftOwnership { get; set; }
        public string RightOwnership { get; set; }
        public bool HasOwnershipChanged { get; set; }
        public bool WasSiteAdded { get; set; }
        public bool WasSiteDeleted { get; set; }
    }
}
