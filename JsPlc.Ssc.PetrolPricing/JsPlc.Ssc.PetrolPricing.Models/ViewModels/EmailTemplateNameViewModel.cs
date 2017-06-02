using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JsPlc.Ssc.PetrolPricing.Models.ViewModels
{
    public class EmailTemplateNameViewModel
    {
        public int EmailTemplateId { get; set; }
        public string TemplateName { get; set; }
        public bool IsDefault { get; set; }
        public string SubjectLine { get; set; }
    }
}
