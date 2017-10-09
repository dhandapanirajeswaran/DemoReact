using JsPlc.Ssc.PetrolPricing.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JsPlc.Ssc.PetrolPricing.Repository
{
    /// <summary>
    /// Handle the mapping of groups of Brand names (such as TESCO EXPRESS and TESCO EXTRA into TESCO)
    /// </summary>
    public class BrandGroupMapper
    {
        private Dictionary<string, string> _mappings;
        public BrandGroupMapper()
        {
            _mappings = CreateMappings();
        }

        public string Map(string brandName)
        {
            if (String.IsNullOrEmpty(brandName))
                return "";

            if (_mappings.ContainsKey(brandName))
                return _mappings[brandName];
            return brandName;
        }

        internal List<RankedBrandName> UniqueBrandGroups(IEnumerable<RankedBrandName> rankedBrandNames)
        {
            var uniqueGroups = new List<RankedBrandName>();
            foreach (var brand in rankedBrandNames)
            {
                var brandGroup = Map(brand.BrandName);
                if (!uniqueGroups.Any(x => x.BrandName == brandGroup))
                    uniqueGroups.Add(brand);
            }
            return uniqueGroups;
        }

        private Dictionary<string, string> CreateMappings()
        {
            var map = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);
            map.Add(Const.TESCOEXPRESS, Const.TESCO);
            map.Add(Const.TESCOEXTRA, Const.TESCO);
            return map;
        }
    }
}
