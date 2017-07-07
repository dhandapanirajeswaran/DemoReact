using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ClosedXML;
using JsPlc.Ssc.PetrolPricing.Models.ViewModels;
using JsPlc.Ssc.PetrolPricing.Exporting.DataTableConvertors;
using System.Data;
using JsPlc.Ssc.PetrolPricing.Core;

namespace JsPlc.Ssc.PetrolPricing.Exporting.Exporters
{
    public class JsSitesWithPricesExporter : BaseExporter
    {
        public ClosedXML.Excel.XLWorkbook ToExcelWorkbook(
            DateTime forDate,
            IEnumerable<SitePriceViewModel> sitesViewModelsWithPrices,
            List<int> pfsList
            )
        {
            var dataTable = JsSitesWithPricesDataTableConvertor.ToDataTable(forDate, sitesViewModelsWithPrices, pfsList);

            var dataTables = new List<DataTable>();
            dataTables.Add(dataTable);

            var workbook = base.DataTableToExcel(dataTables, null, ExportExcelFileType.ExportJSSites);
            return workbook;
        }
    }
}