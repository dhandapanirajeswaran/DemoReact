using ClosedXML.Excel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JsPlc.Ssc.PetrolPricing.Exporting.Formatting
{
    public static class ExcelStyleFormatters
    {
        public static void GeneralIntegerFormatter(IXLCell cell)
        {
            int tempInteger = 0;
            if (Int32.TryParse(cell.Value.ToString(), out tempInteger))
            {
                cell.Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Right);
                cell.DataType = XLCellValues.Number;
            }
        }

        public static void GeneralTextFormatter(IXLCell cell)
        {
            cell.DataType = XLCellValues.Text;
        }

        public static void GeneralTextRightAlignedFormatter(IXLCell cell)
        {
            cell.DataType = XLCellValues.Text;
            cell.Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Right);
        }

        public static void GeneralTextCenteredFormatter(IXLCell cell)
        {
            cell.DataType = XLCellValues.Text;
            cell.Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
        }

        public static void GeneralTextLeftAlignedFormatter(IXLCell cell)
        {
            cell.DataType = XLCellValues.Text;
            cell.Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Left);
        }

        public static void GeneralRedYesFormatter(IXLCell cell)
        {
            var value = cell.Value.ToString().ToUpper();
            if (value == "1" || value == "TRUE" || value == "YES")
            {
                cell.Value = "Yes";
                cell.DataType = XLCellValues.Text;
                cell.Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
                cell.Style.Font.FontColor = XLColor.Red;
            }
            else if (value == "0" || value == "FALSE" || value == "NO")
            {
                cell.Value = "";
                cell.DataType = XLCellValues.Text;
                cell.Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
                cell.Style.Font.FontColor = XLColor.Black;
            }
        }

        public static void GeneralYesNoFormatter(IXLCell cell)
        {
            var value = cell.Value.ToString().ToUpper();
            if (value == "1" || value == "TRUE" || value == "YES")
            {
                cell.Value = "Yes";
                cell.DataType = XLCellValues.Text;
                cell.Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
                cell.Style.Font.FontColor = XLColor.Red;
            }
            else if (value == "0" || value == "FALSE" || value == "NO")
            {
                cell.Value = "No";
                cell.DataType = XLCellValues.Text;
                cell.Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
                cell.Style.Font.FontColor = XLColor.Black;
            }
        }

        public static void GeneralPriceFormatter(IXLCell cell)
        {
            decimal tempDecimal;
            if (decimal.TryParse(cell.Value.ToString(), out tempDecimal))
            {
                cell.DataType = XLCellValues.Number;
                cell.Style.NumberFormat.Format = "0.00";
            }
            cell.Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Right);
        }

        public static void GeneralNumberFormatter(IXLCell cell)
        {
            decimal tempDecimal;
            if (decimal.TryParse(cell.Value.ToString(), out tempDecimal))
            {
                cell.DataType = XLCellValues.Number;
                cell.Style.NumberFormat.Format = "0.0";
            }
            cell.Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Right);
        }

        public static void GeneralNumber2DPFormatter(IXLCell cell)
        {
            decimal tempDecimal;
            if (decimal.TryParse(cell.Value.ToString(), out tempDecimal))
            {
                cell.DataType = XLCellValues.Number;
                cell.Style.NumberFormat.Format = "0.00";
            }
            cell.Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Right);
        }
    }
}
