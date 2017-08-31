using ClosedXML.Excel;
using JsPlc.Ssc.PetrolPricing.Core;
using JsPlc.Ssc.PetrolPricing.Exporting.Formatting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JsPlc.Ssc.PetrolPricing.Exporting.Styling
{
    public class ExcelStyler
    {
        private class ExcelColumnStyler
        {
            private IXLWorksheet _worksheet;
            private int _firstRow;
            private int _lastRow;

            public ExcelColumnStyler(IXLWorksheet worksheet, int firstRow, int lastRow)
            {
                _worksheet = worksheet;
                _firstRow = firstRow;
                _lastRow = lastRow;
            }

            public ExcelColumnStyler FormatColumn(string excelColumn, Action<IXLCell> formatter)
            {
                var columnIndex = ConvertCellLetterToIndex(excelColumn);
                return FormatColumn(columnIndex, formatter);
            }

            public ExcelColumnStyler FormatColumn(int columnIndex, Action<IXLCell> formatter)
            {
                if (columnIndex < 1)
                    throw new ArgumentException("ColumnIndex cannot be less than 1");

                if (_firstRow < 1)
                    throw new ArgumentException("FirstDataRow cannot be less than 1");

                for (var row = _firstRow; row <= _lastRow; row++)
                {
                    var cell = _worksheet.Cell(row, columnIndex);
                    formatter(cell);
                }
                return this;
            }

            public ExcelColumnStyler FormatColumnRange(int firstColumn, int lastColumn, Action<IXLCell> formatter)
            {
                if (firstColumn < 1)
                    throw new ArgumentException("FirstColumn cannot be less than 1");

                for (var columnIndex = firstColumn; columnIndex <= lastColumn; columnIndex++)
                    FormatColumn(columnIndex, formatter);
                return this;
            }

            #region private methods

            /// <summary>
            /// Converts an Excel cell letter into an index (e.g. 'F' --> 6)
            /// </summary>
            /// <param name="excelCellLetter"></param>
            /// <returns></returns>
            private int ConvertCellLetterToIndex(string excelCellLetter)
            {
                var index = 0;
                foreach (char ch in excelCellLetter.ToUpper())
                    index = index * 26 + ((int)ch - 64);
                return index;
            }

            #endregion private methods
        }

        public ExcelStyler()
        {
        }

        public void ApplySiteExport(IXLWorksheet worksheet, ExportExcelFileType exportType, int totalRows)
        {
            switch (exportType)
            {
                case ExportExcelFileType.ExportAllSites:
                    FormatExportAllWorksheet(worksheet, totalRows);
                    break;

                case ExportExcelFileType.ExportJSSites:
                    FormatExportJSSitesWorksheet(worksheet, totalRows);
                    break;

                case ExportExcelFileType.None:
                    break;

                case ExportExcelFileType.SiteEmailAddreses:
                    FormatSiteEmailAddresses(worksheet, totalRows);
                    break;

                default:
                    throw new ArgumentException("Unsupport Export Type: " + exportType);
            }
        }

        public void ApplyReportExport(IXLWorksheet worksheet, ReportExportFileType reportType, int totalColumns, int totalRows)
        {
            switch (reportType)
            {
                case ReportExportFileType.CompetitorSites:
                    FormatReportCompetitorSites(worksheet, totalColumns, totalRows);
                    break;

                case ReportExportFileType.CompetitorsPriceRange:
                    FormatReportCompetitorsPriceRange(worksheet, totalColumns, totalRows);
                    break;

                case ReportExportFileType.CompetitorsPriceRangeByCompany:
                    FormatReportCompetitorsPriceRangeByCompany(worksheet, totalColumns, totalRows);
                    break;

                case ReportExportFileType.NationalAverageReport:
                    FormatReportNationalAverageReport(worksheet, totalColumns, totalRows);
                    break;

                case ReportExportFileType.NationalAverageReport2:
                    FormatReportNationalAverageReport2(worksheet, totalColumns, totalRows);
                    break;

                case ReportExportFileType.PriceMovementReport:
                    FormatReportPriceMovementReport(worksheet, totalColumns, totalRows);
                    break;

                case ReportExportFileType.PricePointsReport:
                    FormatReportPricePointsReport(worksheet, totalColumns, totalRows);
                    break;

                case ReportExportFileType.QuarterlySiteAnalysis:
                    FormatQuarterlySiteAnalysisReport(worksheet, totalColumns, totalRows);
                    break;

                default:
                    throw new ArgumentException("Unsupport ReportType: " + reportType);
            }
        }

        #region private methods

        private void FormatReportCompetitorSites(IXLWorksheet worksheet, int totalColumns, int totalRows)
        {
            var firstDataRow = 2;
            var lastDataRow = 2 + totalRows - 1;

            new ExcelColumnStyler(worksheet, firstDataRow, lastDataRow)
                .FormatColumn("B", ExcelStyleFormatters.GeneralIntegerFormatter)
                .FormatColumn("C", ExcelStyleFormatters.GeneralIntegerFormatter)
                .FormatColumn("D", ExcelStyleFormatters.GeneralIntegerFormatter)
                .FormatColumn("E", ExcelStyleFormatters.GeneralIntegerFormatter)
                .FormatColumn("F", ExcelStyleFormatters.GeneralIntegerFormatter)
                .FormatColumn("G", ExcelStyleFormatters.GeneralIntegerFormatter)
                .FormatColumn("H", ExcelStyleFormatters.GeneralIntegerFormatter);
        }

        private void FormatReportCompetitorsPriceRange(IXLWorksheet worksheet, int totalColumns, int totalRows)
        {
            var firstDataRow = 2;
            var lastDataRow = 2 + totalRows - 1;

            new ExcelColumnStyler(worksheet, firstDataRow, lastDataRow)
                .FormatColumn("B", ExcelStyleFormatters.GeneralPriceFormatter)
                .FormatColumn("C", ExcelStyleFormatters.GeneralPriceFormatter)
                .FormatColumn("D", ExcelStyleFormatters.GeneralNumberFormatter)
                .FormatColumn("E", ExcelStyleFormatters.GeneralNumberFormatter)
                .FormatColumn("F", ExcelStyleFormatters.GeneralTextCenteredFormatter)
                .FormatColumn("G", ExcelStyleFormatters.GeneralTextCenteredFormatter);
        }

        private void FormatReportCompetitorsPriceRangeByCompany(IXLWorksheet worksheet, int totalColumns, int totalRows)
        {
            var firstDataRow = 2;
            var lastDataRow = 2 + totalRows - 1;

            new ExcelColumnStyler(worksheet, firstDataRow, lastDataRow)
                .FormatColumn("C", ExcelStyleFormatters.GeneralPriceFormatter)
                .FormatColumn("D", ExcelStyleFormatters.GeneralPriceFormatter)
                .FormatColumn("E", ExcelStyleFormatters.GeneralNumberFormatter)
                .FormatColumn("F", ExcelStyleFormatters.GeneralNumberFormatter)
                .FormatColumn("G", ExcelStyleFormatters.GeneralTextCenteredFormatter)
                .FormatColumn("H", ExcelStyleFormatters.GeneralTextCenteredFormatter);
        }

        private void FormatReportNationalAverageReport(IXLWorksheet worksheet, int totalColumns, int totalRows)
        {
            var firstDataRow = 2;
            var lastDataRow = 2 + totalRows - 1;

            new ExcelColumnStyler(worksheet, firstDataRow, lastDataRow)
                .FormatColumn("D", ExcelStyleFormatters.GeneralPriceFormatter)
                .FormatColumn("E", ExcelStyleFormatters.GeneralPriceFormatter)
                .FormatColumn("F", ExcelStyleFormatters.GeneralNumberFormatter)
                .FormatColumn("G", ExcelStyleFormatters.GeneralNumberFormatter)
                .FormatColumn("H", ExcelStyleFormatters.GeneralNumberFormatter);
        }

        private void FormatReportNationalAverageReport2(IXLWorksheet worksheet, int totalColumns, int totalRows)
        {
            var firstDataRow = 2;
            var lastDataRow = 2 + totalRows - 1;

            new ExcelColumnStyler(worksheet, firstDataRow, lastDataRow)
                .FormatColumnRange(3, totalColumns, ExcelStyleFormatters.GeneralNumber2DPFormatter);
        }

        private void FormatReportPriceMovementReport(IXLWorksheet worksheet, int totalColumns, int totalRows)
        {
            var firstDataRow = 3;
            var lastDataRow = 2 + totalRows - 1;

            new ExcelColumnStyler(worksheet, firstDataRow, lastDataRow)
                .FormatColumnRange(2, totalColumns, ExcelStyleFormatters.GeneralNumberFormatter);
        }

        private void FormatReportPricePointsReport(IXLWorksheet worksheet, int totalColumns, int totalRows)
        {
            var firstDataRow = 2;
            var lastDataRow = 2 + totalRows - 1;

            new ExcelColumnStyler(worksheet, firstDataRow, lastDataRow)
                .FormatColumnRange(2, totalColumns, ExcelStyleFormatters.GeneralIntegerFormatter);
        }

        private void FormatQuarterlySiteAnalysisReport(IXLWorksheet worksheet, int totalColumns, int totalRows)
        {
            var firstDataRow = 2;
            var lastDataRow = 2 + totalRows - 1;

            new ExcelColumnStyler(worksheet, firstDataRow, lastDataRow)
                .FormatColumn(1, ExcelStyleFormatters.GeneralIntegerFormatter) // CatNo
                .FormatColumn(2, ExcelStyleFormatters.GeneralTextLeftAlignedFormatter) // SiteName
                .FormatColumn(3, ExcelStyleFormatters.GeneralTextLeftAlignedFormatter) // LeftOwnership
                .FormatColumn(4, ExcelStyleFormatters.GeneralTextLeftAlignedFormatter) // RightOwnership
                .FormatColumn(5, ExcelStyleFormatters.GeneralRedYesFormatter) // HasOwnershipChanged
                .FormatColumn(6, ExcelStyleFormatters.GeneralRedYesFormatter) // WasSiteAdded
                .FormatColumn(7, ExcelStyleFormatters.GeneralRedYesFormatter) // WasSiteDeleted
                .FormatColumn(8, ExcelStyleFormatters.GeneralYesNoFormatter); // NoChange
        }

        private void FormatExportAllWorksheet(IXLWorksheet worksheet, int totalRows)
        {
            var firstDataRow = 3;
            var lastDataRow = 3 + totalRows - 1;

            new ExcelColumnStyler(worksheet, firstDataRow, lastDataRow)
                .FormatColumn("A", ExcelStyleFormatters.GeneralIntegerFormatter)
                .FormatColumn("B", ExcelStyleFormatters.GeneralTextFormatter)
                .FormatColumn("C", ExcelStyleFormatters.GeneralTextFormatter)
                .FormatColumn("D", ExcelStyleFormatters.GeneralIntegerFormatter)
                .FormatColumn("E", ExcelStyleFormatters.GeneralIntegerFormatter)
                .FormatColumn("F", ExcelStyleFormatters.GeneralPriceFormatter)
                .FormatColumn("G", ExcelStyleFormatters.GeneralPriceFormatter)
                .FormatColumn("H", ExcelStyleFormatters.GeneralIntegerFormatter)
                .FormatColumn("I", ExcelStyleFormatters.GeneralPriceFormatter)
                .FormatColumn("J", ExcelStyleFormatters.GeneralPriceFormatter)
                .FormatColumn("K", ExcelStyleFormatters.GeneralIntegerFormatter)
                .FormatColumn("L", ExcelStyleFormatters.GeneralPriceFormatter)
                .FormatColumn("M", ExcelStyleFormatters.GeneralPriceFormatter)
                .FormatColumn("N", ExcelStyleFormatters.GeneralIntegerFormatter);
        }

        private void FormatExportJSSitesWorksheet(IXLWorksheet worksheet, int totalRows)
        {
            var firstDataRow = 3;
            var lastDataRow = 3 + totalRows - 1;

            var viewingDate = DateTime.Parse(worksheet.Cell("A2").Value.ToString());
            var mergeMessage = String.Format("NOTE: The prices for {0} were calculated on the {1}",
                viewingDate.AddDays(1).ToString("dd MMM yyyy"),
                viewingDate.ToString("dd MMM yyyy")
                );

            worksheet.Cell("J2").Value = mergeMessage;
            worksheet.Cell("J2").Style.Font.FontColor = XLColor.Red;

            worksheet.Cell("A2").Style.Font.FontColor = XLColor.Red;

            new ExcelColumnStyler(worksheet, firstDataRow, lastDataRow)
                .FormatColumn("A", ExcelStyleFormatters.GeneralIntegerFormatter)
                .FormatColumn("B", ExcelStyleFormatters.GeneralIntegerFormatter)
                .FormatColumn("C", ExcelStyleFormatters.GeneralTextFormatter)
                .FormatColumn("D", ExcelStyleFormatters.GeneralPriceFormatter)
                .FormatColumn("E", ExcelStyleFormatters.GeneralTextRightAlignedFormatter)
                .FormatColumn("F", ExcelStyleFormatters.GeneralPriceFormatter)
                .FormatColumn("G", ExcelStyleFormatters.GeneralTextRightAlignedFormatter)
                .FormatColumn("H", ExcelStyleFormatters.GeneralPriceFormatter)
                .FormatColumn("I", ExcelStyleFormatters.GeneralTextRightAlignedFormatter);
        }
                    
        private void FormatSiteEmailAddresses(IXLWorksheet worksheet, int totalRows)
        {
            var firstDataRow = 2;
            var lastDatarow = 2 + totalRows - 1;

            new ExcelColumnStyler(worksheet, firstDataRow, lastDatarow)
                .FormatColumn("A", ExcelStyleFormatters.GeneralIntegerFormatter)
                .FormatColumn("B", ExcelStyleFormatters.GeneralTextLeftAlignedFormatter)
                .FormatColumn("C", ExcelStyleFormatters.GeneralTextLeftAlignedFormatter);
        }

        #endregion private methods
    }
}
