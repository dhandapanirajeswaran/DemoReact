using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using System.Text;
using System.Threading.Tasks;

namespace JsPlc.Ssc.PetrolPricing.Repository
{
    public static class PetrolPricingRepositoryMemoryCache
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
