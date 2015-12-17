using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JsPlc.Ssc.PetrolPricing.Models.ViewModels
{
    public class CatalistQuarterly
    {
        //Types are set as in excel to avoid parsing on import
        public string MasterSiteName { get; set; }
        public string SiteTown { get; set; }
        public double SiteCatNo { get; set; }
        public double Rank { get; set; }
        public double DriveDistanceMiles { get; set; }
        public double DriveTimeMins { get; set; }
        public double CatNo { get; set; }
        public string Brand { get; set; }
        public string SiteName { get; set; }
        public string Address { get; set; }
        public string Suburb { get; set; }
        public string Town { get; set; }
        public string Postcode { get; set; }
        public string CompanyName { get; set; }
        public string Ownership { get; set; }
    }
}
