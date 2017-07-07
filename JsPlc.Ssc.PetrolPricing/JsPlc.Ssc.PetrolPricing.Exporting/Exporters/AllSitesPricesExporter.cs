using JsPlc.Ssc.PetrolPricing.Core;
using JsPlc.Ssc.PetrolPricing.Exporting.DataTableConvertors;
using JsPlc.Ssc.PetrolPricing.Exporting.Interfaces;
using JsPlc.Ssc.PetrolPricing.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JsPlc.Ssc.PetrolPricing.Exporting.Exporters
{
    public class AllSitesPricesExporter : BaseExporter
    {
        public ClosedXML.Excel.XLWorkbook ToExcelWorkbook(
            IEnumerable<SitePriceViewModel> sitesViewModelsWithPrices,
            DateTime forDate,
            IGetCompetitorsWithPrices getCompetitorsWithPrices
            )
        {
            Dictionary<int, int> dicgroupRows = new Dictionary<int, int>();

            var datatable = AllSitesPricesDataTableConvertor.ToDataTable(forDate, sitesViewModelsWithPrices, ref dicgroupRows, getCompetitorsWithPrices);

            var datatables = new List<DataTable> { datatable };

            var workbook = base.DataTableToExcel(datatables, dicgroupRows, ExportExcelFileType.ExportAllSites);
            return workbook;
        }
    }
}
