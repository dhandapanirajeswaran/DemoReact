using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JsPlc.Ssc.PetrolPricing.Models.ViewModels.Diagnostics
{
    public class DiagnosticsErrorLogFileInfoViewModel
    {
        public string FileName { get; set; }
        public DateTime DateModified { get; set; }
        public long FileSize { get; set; }
    }
}
