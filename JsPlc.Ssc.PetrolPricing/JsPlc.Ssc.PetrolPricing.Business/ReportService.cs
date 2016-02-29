using System.IO;
using System.Runtime.Caching;
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


        /// <summary>
        /// Cached for 1min (see 1.0 value in expiration policy)
        /// </summary>
        /// <param name="when"></param>
        /// <param name="fuelTypeId"></param>
        /// <returns></returns>
        public PricePointReportViewModel GetReportPricePoints(DateTime when, int fuelTypeId)
        {
            var cacheKey = MakeCacheKey("[{0}], [{1}-{2}]", "PricePointsReport", when.ToString("R"), fuelTypeId);
            PricePointReportViewModel retval = null;

            var reportCached = PetrolPricingCache.CacheObj.Get(cacheKey);
            if (reportCached != null)
            {
                retval = reportCached as PricePointReportViewModel;
                return retval;
            }
            var report = _db.GetReportPricePoints(when, fuelTypeId);
            var cacheItem = new CacheItem(cacheKey, report);
            PetrolPricingCache.CacheObj.Add(cacheItem, PetrolPricingCache.ReportsCacheExpirationPolicy(1.0));
            retval = report;

            return retval;
        }

        public NationalAverageReportViewModel GetReportNationalAverage(DateTime when)
        {
            return _db.GetReportNationalAverage(when);
        }

        public NationalAverageReportViewModel GetReportNationalAverage2(DateTime when)
        {
            return _db.GetReportNationalAverage2(when);
        }

        public CompetitorsPriceRangeByCompanyViewModel GetReportCompetitorsPriceRangeByCompany(DateTime when, string companyName, string brandName)
        {
            return _db.GetReportCompetitorsPriceRangeByCompany(when, companyName, brandName);
        }

        /// <summary>
        /// Cached for 30 seconds (see 0.5 value in expiration policy)
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <param name="fuelTypeId"></param>
        /// <returns></returns>
        public PriceMovementReportViewModel GetReportPriceMovement(string brandName, DateTime from, DateTime to, int fuelTypeId)
        {
            var cacheKey = MakeCacheKey("[{0}], [{1}-{2}-{3}-{4}]", "PriceMovementReport", @from.ToString("R"), @to.ToString("R"), fuelTypeId, brandName);
            PriceMovementReportViewModel retval;

            var reportCached = PetrolPricingCache.CacheObj.Get(cacheKey);
            if (reportCached != null)
            {
                retval = reportCached as PriceMovementReportViewModel;
                return retval;
            }
            var report = _db.GetReportPriceMovement(brandName, from, to, fuelTypeId);
            var cacheItem = new CacheItem(cacheKey, report);
            PetrolPricingCache.CacheObj.Add(cacheItem, PetrolPricingCache.ReportsCacheExpirationPolicy(0.5));
            retval = report;

            return retval;
        }

        private static string MakeCacheKey(string format, params object[] cacheKeyStrings)
        {
            return String.Format(format, cacheKeyStrings);
        }

        /// <summary>
        /// Gets the compliance report for a pump on a given date
        /// </summary>
        /// <param name="forDate"></param>
        /// <returns></returns>
        public ComplianceReportViewModel GetReportCompliance(DateTime forDate)
        {
            var report = _db.GetReportCompliance(forDate);
            return report;
        }
    }
}
