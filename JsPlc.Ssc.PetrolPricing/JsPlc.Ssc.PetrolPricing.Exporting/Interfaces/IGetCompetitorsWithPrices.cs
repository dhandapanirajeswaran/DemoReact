using JsPlc.Ssc.PetrolPricing.Models;
using JsPlc.Ssc.PetrolPricing.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JsPlc.Ssc.PetrolPricing.Exporting.Interfaces
{
    public interface IGetCompetitorsWithPrices
    {
        IEnumerable<SitePriceViewModel> GetCompetitorsWithPrices(DateTime? forDate = null, int siteId = 0, int pageNo = 1, int pageSize = Constants.PricePageSize);
    }
}
