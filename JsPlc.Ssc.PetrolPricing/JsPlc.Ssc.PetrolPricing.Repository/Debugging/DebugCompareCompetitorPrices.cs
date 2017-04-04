using JsPlc.Ssc.PetrolPricing.Models.Enums;
using JsPlc.Ssc.PetrolPricing.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JsPlc.Ssc.PetrolPricing.Repository.Debugging
{
    internal class DebugCompareCompetitorPrices
    {
        private DebugFileLogger _logger = new DebugFileLogger();

        public DebugCompareCompetitorPrices()
        {
        }

        public void Compare(IEnumerable<SitePriceViewModel> leftSites, IEnumerable<SitePriceViewModel> rightSites)
        {
            foreach(var leftSite in leftSites)
            {
                var rightSite = rightSites.FirstOrDefault(x => x.SiteId == leftSite.SiteId);

                CompareSite(leftSite, rightSite);

                _logger.BlankLine();
            }
        }

        public void CompareSite(SitePriceViewModel leftSite, SitePriceViewModel rightSite)
        {
            _logger.BlankLine();
            _logger.LogSiteInfo(leftSite);
            _logger.BlankLine();

            _logger.LogFuelPrice(FuelTypeItem.Unleaded, leftSite, rightSite);
            _logger.LogFuelPrice(FuelTypeItem.Diesel, leftSite, rightSite);
            _logger.LogFuelPrice(FuelTypeItem.Super_Unleaded, leftSite, rightSite);
        }

        public void WriteToFile(string filename)
        {
            _logger.WriteToFile(filename);
        }
    }
}
