using JsPlc.Ssc.PetrolPricing.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JsPlc.Ssc.PetrolPricing.Models.ViewModels
{

    public class LastSitePricesFuelPriceViewModel
    {
        public int SiteId { get; set; }
        public FuelTypeItem FuelType { get; set; }
        public int ModalPrice { get; set; } = 0;
        public DateTime? LastPriceDate { get; set; }
    }

    public class LastSitePriceSiteViewModel
    {
        public int SiteId { get; set; }
        public string SiteName { get; set; }
        public bool IsSainsburysSite { get; set; }
        public bool IsActive { get; set; }
        public string CatNo { get; set; }
        public string PfsNo { get; set; }

        public List<LastSitePricesFuelPriceViewModel> FuelPrices { get; set; } = new List<LastSitePricesFuelPriceViewModel>();
    }

    public class LastSitePricesViewModel
    {
        public IEnumerable<LastSitePriceSiteViewModel> Sites { get; set; } = new List<LastSitePriceSiteViewModel>();
    }
}
