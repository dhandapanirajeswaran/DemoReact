using JsPlc.Ssc.PetrolPricing.Models.ViewModels;
using System;

namespace JsPlc.Ssc.PetrolPricing.Business
{
    public class ReportService : BaseService
    {
        public CompetitorSiteReportViewModel GetReportCompetitorSites(int siteId)
        {
            return _db.GetReportCompetitorSite(siteId);
        }

        public PricePointReportViewModel GetReportPricePoints(DateTime when, int fuelTypeId)
        {
            return _db.GetReportPricePoints(when, fuelTypeId);
        }

        public NationalAverageReportViewModel GetReportNationalAverage(DateTime when)
        {
            return _db.GetReportNationalAverage(when);
        }

        public PriceMovementReportViewModel GetReportPriceMovement(DateTime from, DateTime to, int fuelTypeId)
        {
            return _db.GetReportPriceMovement(from, to, fuelTypeId);
       }
    }
}
