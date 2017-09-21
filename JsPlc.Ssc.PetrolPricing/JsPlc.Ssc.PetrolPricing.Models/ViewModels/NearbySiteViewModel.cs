using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JsPlc.Ssc.PetrolPricing.Models.ViewModels
{
    public class NearbySiteViewModel
    {
        public int CompetitorSiteId { get; set; }
        public string Brand { get; set; }
        public string SiteName { get; set; }
        public string Town { get; set; }
        public string PostCode { get; set; }
        public bool IsActive { get; set; }
        public double Distance { get; set; }
        public double DriveTime { get; set; }
        public int UnleadedDriveTimeMarkup { get; set; }
        public int DieselDriveTimeMarkup { get; set; }
        public int SuperUnleadedDriveTimeMarkup { get; set; }
    }
}
