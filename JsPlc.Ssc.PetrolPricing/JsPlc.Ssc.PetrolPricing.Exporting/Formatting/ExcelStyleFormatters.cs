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
        public static void ColumnHeader(IXLCell cell)
        {
            cell.Style.Fill.SetBackgroundColor(XLColor.LightGray);
            cell.Style.Font.SetFontColor(XLColor.Black);
            cell.Style.Border.OutsideBorder = XLBorderStyleValues.Thick;
            cell.Style.Border.OutsideBorderColor = XLColor.Gray;
            cell.Style.Font.FontColor = XLColor.Black;
            cell.Style.Font.FontSize = 14.0;
        }

        public static void ColumnHeaderOrange(IXLCell cell)
        {
            cell.Style.Fill.SetBackgroundColor(XLColor.Orange);
            cell.Style.Font.SetFontColor(XLColor.Black);
            cell.Style.Border.OutsideBorder = XLBorderStyleValues.Thick;
            cell.Style.Border.OutsideBorderColor = XLColor.Gray;
            cell.Style.Font.FontColor = XLColor.Black;
            cell.Style.Font.FontSize = 14.0;
        }

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
            if (IsTrueString(value))
            {
                cell.Value = "Yes";
                cell.DataType = XLCellValues.Text;
                cell.Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
                cell.Style.Font.FontColor = XLColor.Red;
            }
            else if (IsFalseString(value))
            {
                cell.Value = "";
                cell.DataType = XLCellValues.Text;
                cell.Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
                cell.Style.Font.FontColor = XLColor.Black;
            }
        }

        public static void GeneralRedNoFormatter(IXLCell cell)
        {
            var value = cell.Value.ToString();
            if (IsTrueString(value))
            {
                cell.Value = "Yes";
                cell.Style.Font.FontColor = XLColor.Black;
            }
            else
            {
                cell.Value = "No";
                cell.Style.Font.FontColor = XLColor.Red;
            }
            cell.DataType = XLCellValues.Text;
            cell.Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
        }

        public static void GeneralYesNoFormatter(IXLCell cell)
        {
            var value = cell.Value.ToString().ToUpper();
            if (IsTrueString(value))
            {
                cell.Value = "Yes";
                cell.Style.Font.FontColor = XLColor.Red;
            }
            else if (IsFalseString(value))
            {
                cell.Value = "No";
                cell.Style.Font.FontColor = XLColor.Black;
            }
            cell.DataType = XLCellValues.Text;
            cell.Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
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

        public static void GeneralPrice1DPFormatter(IXLCell cell)
        {
            decimal tempDecimal;
            if (decimal.TryParse(cell.Value.ToString(), out tempDecimal))
            {
                cell.DataType = XLCellValues.Number;
                cell.Style.NumberFormat.Format = "0.0";
            }
            else
            {
                DullTextColor(cell);
            }
            cell.Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Right);
        }

        public static void GeneralDifferenceNonZeroRed1DPFormatter(IXLCell cell)
        {
            decimal tempDecimal;
            if (decimal.TryParse(cell.Value.ToString(), out tempDecimal))
            {
                cell.DataType = XLCellValues.Number;
                cell.Style.NumberFormat.Format = "0.0";
                if (tempDecimal == 0.0m)

                    cell.Style.Font.FontColor = XLColor.Black;
                else
                    cell.Style.Font.FontColor = XLColor.Red;
            }
            else
            {
                DullTextColor(cell);
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
            else
            {
                DullTextColor(cell);
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
            else
            {
                DullTextColor(cell);
            }
            cell.Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Right);
        }

        public static void GeneralNAFormatter(IXLCell cell)
        {
            DullTextColor(cell);
        }

        public static void DullGrey(IXLCell cell)
        {
            cell.Style.Font.SetFontColor(XLColor.DarkGray);
            cell.Style.Fill.SetBackgroundColor(XLColor.LightGray);
            cell.Style.Font.Italic = true;
        }

        private static void DullTextColor(IXLCell cell)
        {
            cell.Style.Font.SetFontColor(XLColor.LightGray);
            cell.Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
        }

        private static bool IsTrueString(string value)
        {
            if (String.IsNullOrEmpty(value))
                return false;

            var upper = value.ToUpper();
            return upper == "1" || upper == "TRUE" || upper == "YES";
        }

        private static bool IsFalseString(string value)
        {
            if (String.IsNullOrEmpty(value))
                return true;

            var upper = value.ToUpper();
            return upper == "0" || upper == "FALSE" || upper == "NO";
        }
    }
}
