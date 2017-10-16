using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JsPlc.Ssc.PetrolPricing.Models.ViewModels.Diagnostics
{
    public class DiagnosticsPingViewModel
    {
        public string Url { get; set; } = "";
        public string ReturnCode { get; set; } = "";
        public string ResponseText { get; set; } = "";
    }
}
