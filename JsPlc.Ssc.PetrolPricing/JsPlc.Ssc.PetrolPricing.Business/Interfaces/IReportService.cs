using JsPlc.Ssc.PetrolPricing.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JsPlc.Ssc.PetrolPricing.Business
{
    public interface IReportService
    {
        CompetitorSiteReportViewModel GetReportCompetitorSites(int siteId);

        PricePointReportViewModel GetReportPricePoints(DateTime when, int fuelTypeId);

        NationalAverageReportViewModel GetReportNationalAverage(DateTime when);

        NationalAverageReportViewModel GetReportNationalAverage2(DateTime when,bool ViewAllCompetitors = false);

        NationalAverageReportViewModel GetReportcompetitorsPriceRange(DateTime when);        

        CompetitorsPriceRangeByCompanyViewModel GetReportCompetitorsPriceRangeByCompany(DateTime when, string companyName, string brandName);

        PriceMovementReportViewModel GetReportPriceMovement(string brandName, DateTime from, DateTime to, int fuelTypeId,string siteName);

        ComplianceReportViewModel GetReportCompliance(DateTime forDate);

        IEnumerable<SelectItemViewModel> GetQuarterlyFileUploadOptions();

        QuarterlySiteAnalysisReportViewModel GetQuarterlySiteAnalysisReport(int leftId, int rightId);
    }
}
