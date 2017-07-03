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
        void DoCalcDailyPrices(DateTime? forDate);

        void DoCalcDailyPricesForSite(int siteId, DateTime forDate);

        void CalcPrice(IPetrolPricingRepository db, Site site, int fuelId, PriceCalculationTaskData calcTaskData);

        Task<int> SaveOverridePricesAsync(List<SitePrice> pricesToSave);

        Task<StatusViewModel> RecalculateDailyPrices(DateTime when);

        void ResumePriceCacheForDay(DateTime day);
        void SuspendPriceCacheForDay(DateTime day);
        PriceSnapshotViewModel GetPriceSnapshotForDay(DateTime day);
    }
}
