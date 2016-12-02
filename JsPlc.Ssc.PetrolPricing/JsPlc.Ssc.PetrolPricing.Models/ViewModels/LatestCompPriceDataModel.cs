using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JsPlc.Ssc.PetrolPricing.Models.ViewModels
{
    public class LatestCompPriceDataModel
    {
        //Types are set as in excel to avoid parsing on import
        public int Id { get; set; }
        public int UploadId { get; set; }
        public int CatNo { get; set; }
        public String SiteName { get; set; }
        public String UnleadedPrice { get; set; }
        public String DieselPrice { get; set; }
        
    }
}
