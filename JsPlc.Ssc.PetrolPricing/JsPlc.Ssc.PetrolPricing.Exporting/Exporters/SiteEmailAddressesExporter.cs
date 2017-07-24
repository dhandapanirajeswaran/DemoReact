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
using JsPlc.Ssc.PetrolPricing.Models;

namespace JsPlc.Ssc.PetrolPricing.Exporting.Exporters
{
    public class SiteEmailAddressesExporter : BaseExporter
    {

        public ClosedXML.Excel.XLWorkbook ToExcelWorkbook(
            IEnumerable<SiteEmailAddressViewModel> emailAddresess
            )
        {
            var dataTable = SiteEmailAddressesTableConvertor.ToDataTable(emailAddresess);
            var dataTables = new List<DataTable>();
            dataTables.Add(dataTable);

            var workbook = base.DataTableToExcel(dataTables, null, ExportExcelFileType.SiteEmailAddreses);
            return workbook;
        }
    }
}
