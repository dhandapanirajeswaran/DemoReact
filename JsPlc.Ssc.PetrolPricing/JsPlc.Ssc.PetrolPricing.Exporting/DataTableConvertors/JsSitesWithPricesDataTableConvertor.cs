using JsPlc.Ssc.PetrolPricing.Core;
using JsPlc.Ssc.PetrolPricing.Models.Enums;
using JsPlc.Ssc.PetrolPricing.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JsPlc.Ssc.PetrolPricing.Exporting.DataTableConvertors
{
    internal static class JsSitesWithPricesDataTableConvertor
    {
        private static List<FuelTypeEnum> _fuelTypesInOrder = new List<FuelTypeEnum>
            {
                FuelTypeEnum.Unleaded,
                FuelTypeEnum.SuperUnleaded,
                FuelTypeEnum.Diesel
            };

        public static DataTable ToDataTable(DateTime forDate, IEnumerable<SitePriceViewModel> sitesViewModelsWithPrices, List<int> jsSitesPfsNums)
        {
            var dt = new DataTable("Sites");
            AddColumnNames(dt);

            var header = BuildHeader(forDate, dt);
            dt.Rows.Add(header);

            foreach (var pfsNum in jsSitesPfsNums)
            {
                var siteList = sitesViewModelsWithPrices.Where(x => x.PfsNo == pfsNum).ToList();
                if (siteList.Count == 0) continue;
                var siteVM = siteList[0];
                DataRow dr = BuildDataRow(_fuelTypesInOrder, dt, siteVM);

                dt.Rows.Add(dr);
            }
            return dt;
        }

        private static void AddColumnNames(DataTable dt)
        {
            dt.Columns.Add("-");
            dt.Columns.Add("-  ");
            dt.Columns.Add("-   ");
            dt.Columns.Add("UnLeaded ");
            dt.Columns.Add("Diff");
            dt.Columns.Add("Super Unleaded ");
            dt.Columns.Add("Diff ");
            dt.Columns.Add("Diesel ");
            dt.Columns.Add("Diff  ");
        }

        private static DataRow BuildHeader(DateTime forDate, DataTable dt)
        {
            DataRow dr = dt.NewRow();
            dr[0] = forDate.ToString("dd/MM/yyyy");
            return dr;
        }

        private static DataRow BuildDataRow(List<FuelTypeEnum> fuelTypesInOrder, DataTable dt, SitePriceViewModel siteVM)
        {
            var dr = dt.NewRow();
            var columnIndex = 0;
            dr[columnIndex++] = siteVM.PfsNo.ToString().PadLeft(4, '0');
            dr[columnIndex++] = siteVM.PfsNo;
            dr[columnIndex++] = siteVM.StoreName.Replace(Const.SAINSBURYS, "");

            foreach (var fuelType in fuelTypesInOrder)
            {
                FuelPriceViewModel fuelPrice = siteVM.FuelPrices != null
                    ? siteVM.FuelPrices.FirstOrDefault(x => x.FuelTypeId == (int)fuelType)
                    : null;

                var priceString = "";
                var diffString = "";

                if (fuelPrice != null && fuelPrice.AutoPrice.HasValue)
                {
                    var autoPrice = fuelPrice.AutoPrice.Value;
                    var overridePrice = fuelPrice.OverridePrice.HasValue
                        ? fuelPrice.OverridePrice.Value
                        : 0;
                    var todayPrice = fuelPrice.TodayPrice.HasValue
                        ? fuelPrice.TodayPrice.Value
                        : 0;

                    var overrideOrAutoPrice = overridePrice > 0
                        ? overridePrice
                        : autoPrice;

                    priceString = FormatPriceDivideBy10(overrideOrAutoPrice);
                    var diff = 0;

                    if (overridePrice > 0 && todayPrice > 0)
                    {
                        diff = overridePrice - todayPrice;
                    }
                    else if (autoPrice > 0 && todayPrice > 0)
                    {
                        diff = autoPrice - todayPrice;
                    }

                    if (diff != 0)
                    {
                        if (diff > 0)
                            diffString = "+" + FormatPriceDivideBy10(diff) + " ppl";
                        else
                            diffString = "-" + FormatPriceDivideBy10(Math.Abs(diff)) + " ppl";
                    }
                }

                dr[columnIndex++] = priceString;
                dr[columnIndex++] = diffString;
            }
            return dr;
        }

        private static string FormatPriceDivideBy10(int? value)
        {
            return value.HasValue
                ? ((double)value / 10.0).ToString()
                : "";
        }
    }
}