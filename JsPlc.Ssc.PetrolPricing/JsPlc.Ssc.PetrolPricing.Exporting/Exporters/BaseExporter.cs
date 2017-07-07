using ClosedXML.Excel;
using JsPlc.Ssc.PetrolPricing.Core;
using JsPlc.Ssc.PetrolPricing.Exporting.Styling;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JsPlc.Ssc.PetrolPricing.Exporting.Exporters
{
    public class BaseExporter
    {
        public XLWorkbook DataTableToExcel(List<DataTable> tables, Dictionary<int, int> dicgroupRows, ExportExcelFileType exportType)
        {
            using (var wb = new ClosedXML.Excel.XLWorkbook())
            {
                foreach (var dt in tables)
                {
                    var ws = wb.Worksheets.Add(dt);
                    //int TotalRows = ws.RowCount();

                    int totalRows = dt.Rows.Count; // NOTE: do not use the Worksheet RowCount() it is always 1048576 !


                    for (int i = 2; i < totalRows; i++)
                    {
                        ChangeCellColor(ws.Cell(i, 8));
                        ChangeCellColor(ws.Cell(i, 11));
                        ChangeCellColor(ws.Cell(i, 14));
                    }
                    if (dicgroupRows != null)
                    {
                        int nSiteRow = 3;
                        int nRow = 3;
                        while (dicgroupRows.ContainsKey(nRow))
                        {
                            int nCompitetors = dicgroupRows[nRow];

                            var cellrange = string.Format("A{0}:N{1}", nSiteRow + 1, nSiteRow + nCompitetors);
                            var cellrangesecondRow = "A2:N2";
                            ws.Range(cellrangesecondRow)
                                .Style.Fill.SetBackgroundColor(ClosedXML.Excel.XLColor.LightGray);
                            ws.Range(cellrange).Style.Fill.SetBackgroundColor(ClosedXML.Excel.XLColor.LightGray);
                            ws.Range(cellrange).Style.Border.OutsideBorder = ClosedXML.Excel.XLBorderStyleValues.Thick;
                            ws.Range(cellrange).Style.Border.OutsideBorderColor = ClosedXML.Excel.XLColor.Gray;

                            ws.Rows(nSiteRow + 1, nSiteRow + nCompitetors).Group();
                            ws.Rows(nSiteRow + 1, nSiteRow + nCompitetors).Collapse();
                            nSiteRow += nCompitetors + 2;
                            nRow++;
                        }
                    }

                    // Apply numeric/string and other formatting
                    var excelStyler = new ExcelStyler();
                    excelStyler.ApplySiteExport(ws, exportType, totalRows);

                    // Autofit all columns
                    ws.Columns().AdjustToContents();
                }
                wb.Style.Alignment.Horizontal = ClosedXML.Excel.XLAlignmentHorizontalValues.Center;
                wb.Style.Font.Bold = true;
                return wb;
            }
        }

        private void ChangeCellColor(IXLCell cell)
        {
            int iValue = 0;
            bool bResult = Int32.TryParse(cell.Value.ToString(), out iValue);
            if (Convert.ToString(cell.Value).Trim() == "n/a")
            {
                cell.Style.Font.FontColor = ClosedXML.Excel.XLColor.Gray;
            }
            else if (Convert.ToString(cell.Value).Trim() == "Diff")
            {
                // cell.Style.Font.FontColor = ClosedXML.Excel.XLColor.Gray;
            }
            else if (bResult && iValue > 0)
            {
                cell.Style.Font.FontColor = ClosedXML.Excel.XLColor.Green;
            }
            else if (bResult && iValue < 0)
            {
                cell.Style.Font.FontColor = ClosedXML.Excel.XLColor.Red;
            }
        }
    }
}
