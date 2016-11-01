using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JsPlc.Ssc.PetrolPricing.Models.ViewModels
{
    public class LatestPriceDataModel
    {
        //Types are set as in excel to avoid parsing on import
        public int Id { get; set; }
        public int UploadId { get; set; }
        public int PfsNo { get; set; }
        public String SiteName { get; set; }
        public int StoreNo { get; set; }
        public String UnleadedPrice { get; set; }
        public String SuperUnleadedPrice { get; set; }
        public String DieselPrice { get; set; }
        
    }
}
