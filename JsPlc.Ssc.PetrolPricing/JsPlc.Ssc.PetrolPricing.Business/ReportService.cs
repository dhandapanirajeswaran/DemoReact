using JsPlc.Ssc.PetrolPricing.Models.ViewModels;

namespace JsPlc.Ssc.PetrolPricing.Business
{
    public class ReportService : BaseService
    {
        public CompetitorSiteReportViewModel GetCompetitorSites(int siteId)
        {
            return _db.GetCompetitorSiteReport(siteId);
        }
    }
}
