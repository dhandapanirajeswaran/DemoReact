using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JsPlc.Ssc.PetrolPricing.Models.ViewModels
{
    public class EmailTemplateViewModel
    {
        public int EmailTemplateId { get; set; }
        public bool IsDefault { get; set; }
        public string TemplateName { get; set; }
        public string SubjectLine { get; set; }
        public int PPUserId { get; set; }
        public string PPUserEmail { get; set; }
        public string EmailBody { get; set; }
    }
}
