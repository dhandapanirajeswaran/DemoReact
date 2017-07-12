using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JsPlc.Ssc.PetrolPricing.Models.WindowsService
{
    public class WinServiceSettings
    {
        public int PollSettingEveryMinutes { get; set; }
        public DateTime SettingsLastPolledOn { get; set; }
        public DateTime EmailLastRanOn { get; set; }
    }
}
