using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JsPlc.Ssc.PetrolPricing.Models.ViewModels
{
    public class EmailLogViewerViewModel
    {
        public StatusViewModel Status = new StatusViewModel();

        public string Id { get; set; }
        public string SiteId { get; set; }
        public bool IsTest { get; set; }
        public string EmailFrom { get; set; }
        public string FixedEmailTo { get; set; }
        public string ListOfEmailTo { get; set; }
        public string EmailSubject { get; set; }
        public string EmailBody { get; set; }
        public DateTime EndTradeDate { get; set; }
        public DateTime SendDate { get; set; }
        public string LoginUser { get; set; }
        public bool IsSuccess { get; set; }
        public bool IsWarning { get; set; }
        public bool IsError { get; set; }
        public string WarningMessage { get; set; }
        public string ErrorMessage { get; set; }

        public string SiteName { get; set; }
    }
}
