using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JsPlc.Ssc.PetrolPricing.Models.ViewModels.Diagnostics
{
    public class DiagnosticsSchedulerStatusViewModel
    {
        public bool IsRunning { get; set; } = false;
        public DateTime? LastStarted { get; set; }
        public DateTime? LastStopped { get; set; }
        public DateTime? LastPolled { get; set; }
        public DateTime? LastErrored { get; set; }
        public string LastErrorMessage { get; set; } = "";
    }
}