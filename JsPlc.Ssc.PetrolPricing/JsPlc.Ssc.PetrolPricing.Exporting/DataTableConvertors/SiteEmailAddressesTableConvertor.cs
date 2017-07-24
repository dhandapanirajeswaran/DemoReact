using JsPlc.Ssc.PetrolPricing.Models;
using JsPlc.Ssc.PetrolPricing.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JsPlc.Ssc.PetrolPricing.Exporting.DataTableConvertors
{
    public class SiteEmailAddressesTableConvertor
    {
        public static DataTable ToDataTable(IEnumerable<SiteEmailAddressViewModel> emailAddresses)
        {
            var dataTable = new DataTable("Email Addresses");
            AddColumnNames(dataTable);

            foreach(var siteEmail in emailAddresses)
            {
                DataRow dataRow = BuildDataRow(dataTable, siteEmail);
                dataTable.Rows.Add(dataRow);
            }
            return dataTable;
        }

        private static void AddColumnNames(DataTable dataTable)
        {
            dataTable.Columns.Add("Store No.");
            dataTable.Columns.Add("Store Name");
            dataTable.Columns.Add("Email Address");
            dataTable.Columns.Add("Cat No");
            dataTable.Columns.Add("Pfs No");
        }

        private static DataRow BuildDataRow(DataTable dataTable, SiteEmailAddressViewModel siteEmail)
        {
            DataRow dataRow = dataTable.NewRow();
            var columnIndex = 0;
            dataRow[columnIndex++] = siteEmail.StoreNo;
            dataRow[columnIndex++] = siteEmail.StoreName;
            dataRow[columnIndex++] = siteEmail.EmailAddress;
            dataRow[columnIndex++] = siteEmail.CatNo;
            dataRow[columnIndex++] = siteEmail.PfsNo;
            return dataRow;
        }
    }
}
