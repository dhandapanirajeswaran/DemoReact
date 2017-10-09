using JsPlc.Ssc.PetrolPricing.Core;
using JsPlc.Ssc.PetrolPricing.Models;
using JsPlc.Ssc.PetrolPricing.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JsPlc.Ssc.PetrolPricing.Repository
{
    internal class RankedBrandName
    {
        public string BrandName { get; private set; }
        public ReportRowType ReportRowType { get; private set; }

        public RankedBrandName(string brandName, ReportRowType reportRowType)
        {
            this.BrandName = brandName;
            this.ReportRowType = reportRowType;
        }
    }

    public class BrandNameRanker
    {
        private List<RankedBrandName> _sortedAndRanked = new List<RankedBrandName>();

        private List<string> _brands = new List<string>();
        private List<string> _grocers = new List<string>();

        public BrandNameRanker AddBrands(IEnumerable<string> brands)
        {
            var combined = new List<string>();
            combined.AddRange(_brands);
            combined.AddRange(brands);

            _brands = combined
                .Distinct()
                .OrderBy(x => x)
                .ToList();

            return this;
        }

        public BrandNameRanker AddGrocerBrands(IEnumerable<GrocerBrandName> grocerBrands)
        {
            var combined = new List<string>();
            combined.AddRange(_grocers);
            combined.AddRange(grocerBrands.Select(x => x.BrandName).ToList());

            var sortedUniqueGrocers = combined
                .Distinct()
                .OrderBy(x => x)
                .ToList();

            // ensure Sainsburys is first item
            if (sortedUniqueGrocers.Contains(Const.SAINSBURYS))
            {
                sortedUniqueGrocers.Remove(Const.SAINSBURYS);
                sortedUniqueGrocers.Insert(0, Const.SAINSBURYS);
            }
            _grocers = sortedUniqueGrocers;

            return this;
        }

        public BrandNameRanker RankGrocers(ReportRowType reportRowType)
        {
            foreach (var grocer in _grocers)
                AddRankedItem(grocer, reportRowType);

            return this;
        }

        public BrandNameRanker RankItem(ReportRowType reportRowType, string brandName)
        {
            AddRankedItem(brandName, reportRowType);

            return this;
        }

        public BrandNameRanker RankBrands(ReportRowType reportRowType)
        {
            foreach (var brand in _brands)
            {
                if (!_grocers.Any(x => x == brand)) // ignore Grocer brands
                    AddRankedItem(brand, reportRowType);
            }

            return this;
        }

        internal List<RankedBrandName> Build()
        {
            return _sortedAndRanked;
        }

        #region private methods

        private void AddRankedItem(string brandName, ReportRowType reportRowType)
        {
            var item = new RankedBrandName(
                brandName: brandName,
                reportRowType: reportRowType);

            _sortedAndRanked.Add(item);
        }

        #endregion private methods

    }
}