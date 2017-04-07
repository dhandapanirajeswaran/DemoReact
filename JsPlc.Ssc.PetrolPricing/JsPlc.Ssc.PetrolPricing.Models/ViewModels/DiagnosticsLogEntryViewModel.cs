using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JsPlc.Ssc.PetrolPricing.Models.ViewModels
{
    public class DiagnosticsLogEntryViewModel
    {
        public DateTime Created = DateTime.Now;
        public string Level = "";
        public string Message = "";
        public string Exception = "";
    }
}
