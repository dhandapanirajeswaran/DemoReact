using JsPlc.Ssc.PetrolPricing.Models.ViewModels;
using System;

namespace JsPlc.Ssc.PetrolPricing.Business
{
    public class ReportService : BaseService
    {
        public CompetitorSiteReportViewModel GetReportCompetitorSites(int siteId)
        {
            return _db.GeReporttCompetitorSite(siteId);
        }

        public PricePointReportViewModel GetReportPricePoints(DateTime when, int fuelTypeId)
        {
            return _db.GetReportPricePoints(when, fuelTypeId);
        }

        public NationalAverageReportViewModel GetReportNationalAverage(DateTime when)
        {
            return _db.GetReportNationalAverage(when);
        }

    }
}
