using JsPlc.Ssc.PetrolPricing.Models;
using JsPlc.Ssc.PetrolPricing.Models.ViewModels;
using JsPlc.Ssc.PetrolPricing.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JsPlc.Ssc.PetrolPricing.Business
{
    public interface IPriceService
    {
        bool DoCalcDailyPrices(DateTime? forDate);

        void CalcPrice(IPetrolPricingRepository db, Site site, int fuelId, CalcTaskData calcTaskData);

        Task<int> SaveOverridePricesAsync(List<SitePrice> pricesToSave);
    }
}
