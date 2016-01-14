using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Runtime.Caching;
using System.Web;
using JsPlc.Ssc.PetrolPricing.Models;
using JsPlc.Ssc.PetrolPricing.Repository;

namespace JsPlc.Ssc.PetrolPricing.Business
{
    public class BaseService : IDisposable
    {
        protected readonly IPetrolPricingRepository _db;

        public BaseService()
        {
            _db = new PetrolPricingRepository(new RepositoryContext());
        }

        public BaseService(IPetrolPricingRepository repository)
        {
            _db = repository;
        }

        public void Dispose()
        {
            _db.Dispose();
        }
    }
    public static class PetrolPricingCache
    {
        public static ObjectCache CacheObj = MemoryCache.Default;

        public static CacheItemPolicy ReportsCacheExpirationPolicy(double slidingTimeMin)
        {
            var slidingExpirationPolicy = new CacheItemPolicy()
            {
                SlidingExpiration = TimeSpan.FromMinutes(slidingTimeMin)
            };
            return slidingExpirationPolicy;
        }

    }
}