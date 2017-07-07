using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JsPlc.Ssc.PetrolPricing.Models.ViewModels;
using JsPlc.Ssc.PetrolPricing.Exporting.Interfaces;

namespace JsPlc.Ssc.PetrolPricing.Exporting.DataTableConvertors
{
    internal static class AllSitesPricesDataTableConvertor
    {
        internal static DataTable ToDataTable(
            DateTime forDate, 
            IEnumerable<SitePriceViewModel> sitesViewModelsWithPrices, 
            ref Dictionary<int, int> dicgroupRows, 
            IGetCompetitorsWithPrices getCompetitorsWithPrices
            )
        {
            var dt = new DataTable("Site Pricing");
            dt.Columns.Add("StoreNo.");
            dt.Columns.Add("Store Name");
            dt.Columns.Add("Store Town");
            dt.Columns.Add("Cat No.");
            dt.Columns.Add("PFS No.");
            dt.Columns.Add("UnLeaded ");
            dt.Columns.Add("UnLeaded");
            dt.Columns.Add("Diff");
            dt.Columns.Add("Diesel ");
            dt.Columns.Add("Diesel");
            dt.Columns.Add("Diff ");
            dt.Columns.Add("Super Unleaded ");
            dt.Columns.Add("Super Unleaded");
            dt.Columns.Add("Diff  ");
            DataRow dr = dt.NewRow();
            DateTime tomorrow = forDate.AddDays(1);
            DateTime yday = forDate.AddDays(-1);
            DateTime daybyday = yday.AddDays(-1);
            dr[5] = yday.ToString("dd/MM/yyyy");
            dr[6] = tomorrow.ToString("dd/MM/yyyy");
            dr[8] = yday.ToString("dd/MM/yyyy");
            dr[9] = tomorrow.ToString("dd/MM/yyyy");
            dr[11] = yday.ToString("dd/MM/yyyy");
            dr[12] = tomorrow.ToString("dd/MM/yyyy");
            dt.Rows.Add(dr);
            int nRow = 2;
            Dictionary<int, int> dicColtoFType = new Dictionary<int, int>();
            dicColtoFType.Add(2, 5);
            dicColtoFType.Add(6, 8);
            dicColtoFType.Add(1, 11);

            foreach (var siteVM in sitesViewModelsWithPrices)
            {
                dr = dt.NewRow();
                dr[0] = siteVM.StoreNo;
                dr[1] = siteVM.StoreName;
                dr[2] = siteVM.Town;
                dr[3] = siteVM.CatNo;
                dr[4] = siteVM.PfsNo;

                if (siteVM.FuelPrices != null)
                {
                    foreach (var fp in siteVM.FuelPrices)
                    {
                        if (dicColtoFType.ContainsKey(fp.FuelTypeId))
                        {
                            if (System.DBNull.Value == dr[dicColtoFType[fp.FuelTypeId]]) dr[dicColtoFType[fp.FuelTypeId]] = (fp.TodayPrice / 10.0).ToString();
                            if (System.DBNull.Value == dr[dicColtoFType[fp.FuelTypeId] + 1]) dr[dicColtoFType[fp.FuelTypeId] + 1] = (fp.AutoPrice / 10.0).ToString();
                            if (System.DBNull.Value == dr[dicColtoFType[fp.FuelTypeId] + 2])
                            {
                                dr[dicColtoFType[fp.FuelTypeId] + 2] = fp.AutoPrice > 0 && fp.TodayPrice > 0 ? ((fp.AutoPrice - fp.TodayPrice) / 10.0).ToString() : "n/a";
                            }
                        }
                    }
                }
                dt.Rows.Add(dr);
                nRow = nRow + 1;

                //Adding Competitors
                if (siteVM.competitors == null) siteVM.competitors = getCompetitorsWithPrices.GetCompetitorsWithPrices(forDate, siteVM.SiteId, 1, 2000).OrderBy(x => x.DriveTime).ToList();

                if (siteVM.competitors != null)
                {
                    dr = dt.NewRow();
                    dr[1] = "Brand";
                    dr[2] = "Maker";
                    dr[3] = "Drive-Time";
                    dr[4] = "Cat No.";
                    dr[5] = "UnLeaded";
                    dr[6] = "UnLeaded ";
                    dr[7] = "Diff";
                    dr[8] = "Diesel";
                    dr[9] = "Diesel ";
                    dr[10] = "Diff ";
                    dr[11] = "Super Unleaded";
                    dr[12] = "Super Unleaded ";
                    dr[13] = "Diff  ";
                    dt.Rows.Add(dr);
                    dr = dt.NewRow();
                    dr[5] = daybyday.ToString("dd/MM/yyyy");
                    dr[6] = yday.ToString("dd/MM/yyyy");
                    dr[8] = daybyday.ToString("dd/MM/yyyy");
                    dr[9] = yday.ToString("dd/MM/yyyy");
                    dr[11] = daybyday.ToString("dd/MM/yyyy");
                    dr[12] = yday.ToString("dd/MM/yyyy");
                    dt.Rows.Add(dr);
                    foreach (var compitetorVM in siteVM.competitors)
                    {
                        dr = dt.NewRow();
                        dr[1] = compitetorVM.Brand;
                        dr[2] = compitetorVM.StoreName;
                        dr[3] = compitetorVM.DriveTime;
                        dr[4] = compitetorVM.CatNo;
                        if (compitetorVM.FuelPrices != null)
                        {
                            foreach (var fp in compitetorVM.FuelPrices)
                            {
                                if (dicColtoFType.ContainsKey(fp.FuelTypeId))
                                {
                                    if (System.DBNull.Value == dr[dicColtoFType[fp.FuelTypeId]]) dr[dicColtoFType[fp.FuelTypeId]] = (fp.YestPrice / 10.0).ToString();
                                    if (System.DBNull.Value == dr[dicColtoFType[fp.FuelTypeId] + 1]) dr[dicColtoFType[fp.FuelTypeId] + 1] = (fp.TodayPrice / 10.0).ToString();
                                    if (System.DBNull.Value == dr[dicColtoFType[fp.FuelTypeId] + 2])
                                    {
                                        dr[dicColtoFType[fp.FuelTypeId] + 2] = fp.TodayPrice > 0 && fp.YestPrice > 0 ? ((fp.TodayPrice - fp.YestPrice) / 10.0).ToString() : "n/a";
                                    }
                                }
                            }
                        }
                        dt.Rows.Add(dr);

                    }
                    dr = dt.NewRow();
                    dt.Rows.Add(dr);
                    dicgroupRows.Add(nRow, siteVM.competitors.Count + 2);
                }
            }
            return dt;
        }
    }
}
