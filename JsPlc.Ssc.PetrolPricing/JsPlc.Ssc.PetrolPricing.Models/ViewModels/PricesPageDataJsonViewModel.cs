using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JsPlc.Ssc.PetrolPricing.Models.ViewModels
{
    public class DailyPriceDataPageDataViewModel
    {
        public bool IsMissing = true;
        public bool IsOutdated = true;
    }

    public class LatestPriceDataPageDataViewModel
    {
        public bool IsMissing = true;
    }

    public class PriceSnapshotPageDataViewModel
    {
        public bool IsActive = false;
        public bool IsOutdated = true;
    }

    public class PricesPageDataJsonViewModel
    {
        public DailyPriceDataPageDataViewModel DailyPriceData = new DailyPriceDataPageDataViewModel();

        public LatestPriceDataPageDataViewModel LatestPriceData = new LatestPriceDataPageDataViewModel();

        public PriceSnapshotPageDataViewModel PriceSnapshot = new PriceSnapshotPageDataViewModel();
    }
}
