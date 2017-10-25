using ClosedXML.Excel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JsPlc.Ssc.PetrolPricing.Exporting.Helpers
{
    public static class WorksheetHelper
    {
        public static IXLWorksheet CreateFromDataTable(XLWorkbook wb, DataTable dt)
        {
            var worksheetName = String.Format("Table{0}",
                 wb.Worksheets.Count + 1
                 );

            var ws = wb.Worksheets.Add(worksheetName);

            // add column headings
            for (var columnIndex = 0; columnIndex < dt.Columns.Count; columnIndex++)
            {
                var cell = ws.Cell(1, columnIndex + 1);

                cell.Value = "'" + dt.Columns[columnIndex].ColumnName;

                switch (dt.Columns[columnIndex].DataType.ToString().ToUpperInvariant())
                {
                    case "INT":
                    case "FLOAT":
                    case "DOUBLE":
                        cell.DataType = XLCellValues.Number;
                        break;
                    case "TIMESPAN":
                        cell.DataType = XLCellValues.TimeSpan;
                        break;

                    case "DATETIME":
                        cell.DataType = XLCellValues.DateTime;
                        break;

                    default:
                        cell.DataType = XLCellValues.Text;
                        break;
                }
            }

            // add rows
            for (var rowIndex = 0; rowIndex < dt.Rows.Count; rowIndex++)
            {
                var row = dt.Rows[rowIndex];
                for (var columnIndex = 0; columnIndex < dt.Columns.Count; columnIndex++)
                {
                    var cell = ws.Cell(2 + rowIndex, columnIndex + 1);
                    cell.Value = "'" + row[columnIndex].ToString();
                    cell.DataType = XLCellValues.Text;
                }
            }
            return ws;
        }
    }
}
