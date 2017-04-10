using System;
using System.Collections.Generic;

namespace JsPlc.Ssc.PetrolPricing.Models.ViewModels
{
    public class DiagnosticsLogEntryViewModel
    {
        public DateTime Created = DateTime.Now;
        public string Level = "";
        public string Message = "";
        public string Exception = "";
        public Dictionary<string, string> Parameters = new Dictionary<string, string>();
    }
}