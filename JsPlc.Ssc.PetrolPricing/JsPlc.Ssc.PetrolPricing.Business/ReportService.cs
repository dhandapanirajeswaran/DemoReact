using JsPlc.Ssc.PetrolPricing.Models.ViewModels;
using System;

namespace JsPlc.Ssc.PetrolPricing.Business
{
    public class ReportService : BaseService
    {
        public CompetitorSiteReportViewModel GetCompetitorSites(int siteId)
        {
            return _db.GetCompetitorSiteReport(siteId);
        }

        public PricePointReportViewModel GetPricePoints(DateTime when, int fuelTypeId)
        {
            return _db.GetPricePoints(when, fuelTypeId);
        }
    }
}
