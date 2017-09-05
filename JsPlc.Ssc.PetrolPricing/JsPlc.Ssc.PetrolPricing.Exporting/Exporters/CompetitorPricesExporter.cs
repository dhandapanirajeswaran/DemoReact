using ClosedXML.Excel;
using JsPlc.Ssc.PetrolPricing.Core;
using JsPlc.Ssc.PetrolPricing.Exporting.DataTableConvertors;
using JsPlc.Ssc.PetrolPricing.Exporting.Formatting;
using JsPlc.Ssc.PetrolPricing.Exporting.Interfaces;
using JsPlc.Ssc.PetrolPricing.Models.Enums;
using JsPlc.Ssc.PetrolPricing.Models.ViewModels;
using JsPlc.Ssc.PetrolPricing.Models.ViewModels.SystemSettings;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JsPlc.Ssc.PetrolPricing.Core.Diagnostics;

namespace JsPlc.Ssc.PetrolPricing.Exporting.Exporters
{
    public class DriveTimeLookup
    {
        private int _maxDriveTime;
        private double[] _markups;

        public static DriveTimeLookup Create(int maxDriveTime, IEnumerable<DriveTimeMarkupViewModel> markups)
        {
            var lookup = new DriveTimeLookup();
            lookup.Populate(maxDriveTime, markups);
            return lookup;
        }

        public void Populate(int maxDriveTime, IEnumerable<DriveTimeMarkupViewModel> markups)
        {
            _maxDriveTime = maxDriveTime;

            _markups = new double[maxDriveTime + 1];

            var sortedDriveTimes = markups.OrderBy(x => x.DriveTime).ToArray();
            var index = 0;

            double markup = 0;
            for (var wholeDriveTime = 0; wholeDriveTime <= _maxDriveTime; wholeDriveTime++)
            {
                if (index < sortedDriveTimes.Length)
                {
                    while (index < sortedDriveTimes.Length && wholeDriveTime > sortedDriveTimes[index].MaxDriveTime)
                    {
                        index++;
                    }
                    if (index < sortedDriveTimes.Length)
                        markup = sortedDriveTimes[index].Markup;
                }
                _markups[wholeDriveTime] = markup;
            }
        }

        public double GetMarkup(float? driveTime)
        {
            if (driveTime == null)
                return 0;
            int wholeDriveTime = (int)driveTime;
            wholeDriveTime = Math.Max(wholeDriveTime, 0);
            wholeDriveTime = Math.Min(wholeDriveTime, _maxDriveTime);
            return _markups[wholeDriveTime];
        }
    }

    public class CompetitorPricesExporter : BaseExporter
    {
        private const int MaxDriveTime = 25;

        private int _row = 1;

        private const string NAString = "---";

        public ClosedXML.Excel.XLWorkbook ToExcelWorkbook(
            ExportCompetitorPricesViewModel compPrices,
            DateTime forDate
            )
        {
            var workbook = ModelToExcel(compPrices);
            return workbook;
        }

        private enum Columns
        {
            None,
            Sainsburys_StoreNo,
            Sainsburys_SiteName,
            Sainsburys_SiteTown,
            Sainsburys_Unleaded,
            Sainsburys_Diesel,
            Sainsburys_SuperUnleaded,
            Sainsburys_PriceMatchStrategy,
            Sainsburys_Markup,

            // Competitor prices

            Competitor_IsActive,
            Competitor_IsSiteExcluded,
            Competitor_IsBrandExcluded,
            Competitor_Brand,
            Competitor_StoreName,
            Competitor_CatNo,
            Competitor_DriveTime,
            Competitor_Distance,
            Competitor_Unleaded,
            Competitor_Diesel,
            Competitor_SuperUnleaded,
            Competitor_UnleadedDriveTimeMarkup,
            Competitor_DieselDriveTimeMarkup,
            Competitor_SuperUnleadedDriveTimeMarkup,
            Competitor_UnleadedIncMarkup,
            Competitor_DieselIncMarkup,
            Competitor_SuperUnleadedIncMarkup
        }

        private XLWorkbook ModelToExcel(ExportCompetitorPricesViewModel compPrices)
        {
            using (var wb = new ClosedXML.Excel.XLWorkbook())
            {
                var ws = wb.Worksheets.Add("Competitor Prices");

                AddHeaderRows(ws);

                foreach (var jssite in compPrices.SainsburysSitePrices)
                {
                    var pricesForComp = compPrices.CompetitorPrices.Where(x => x.JsSiteId == jssite.SiteId).OrderBy(x => x.DriveTime).ToList();
                    AddSainburysSite(ws, jssite);
                    AddCompetitorPrices(ws, jssite, pricesForComp, compPrices.DriveTimeMarkups);
                }

                // Auto fit widths
                ws.Columns((int)Columns.Sainsburys_StoreNo, (int)Columns.Competitor_SuperUnleadedIncMarkup).AdjustToContents();

                // Freeze top 1 row
                ws.SheetView.FreezeRows(1);

                return wb;
            }
        }

        private void AddHeaderRows(IXLWorksheet ws)
        {
            WriteSainsburysHeaderCell(ws, Columns.Sainsburys_StoreNo, "Store No");
            WriteSainsburysHeaderCell(ws, Columns.Sainsburys_SiteName, "Site Name");
            WriteSainsburysHeaderCell(ws, Columns.Sainsburys_SiteTown, "Site Town");
            WriteSainsburysHeaderCell(ws, Columns.Sainsburys_Unleaded, "Unleaded");
            WriteSainsburysHeaderCell(ws, Columns.Sainsburys_Diesel, "Diesel");
            WriteSainsburysHeaderCell(ws, Columns.Sainsburys_SuperUnleaded, "Super");
            WriteSainsburysHeaderCell(ws, Columns.Sainsburys_PriceMatchStrategy, "Strategy");
            WriteSainsburysHeaderCell(ws, Columns.Sainsburys_Markup, "Markup");

            // Competitor prices

            WriteCompetitorHeaderCell(ws, Columns.Competitor_IsActive, "Is Active");
            WriteCompetitorHeaderCell(ws, Columns.Competitor_IsSiteExcluded, "Site Excluded");
            WriteCompetitorHeaderCell(ws, Columns.Competitor_IsBrandExcluded, "Brand Excluded");
            WriteCompetitorHeaderCell(ws, Columns.Competitor_Brand, "Brand");
            WriteCompetitorHeaderCell(ws, Columns.Competitor_StoreName, "Store Name");
            WriteCompetitorHeaderCell(ws, Columns.Competitor_CatNo, "Cat No");
            WriteCompetitorHeaderCell(ws, Columns.Competitor_DriveTime, "Drive Time");
            WriteCompetitorHeaderCell(ws, Columns.Competitor_Distance, "Distance");
            WriteCompetitorHeaderCell(ws, Columns.Competitor_Unleaded, "Unleaded");
            WriteCompetitorHeaderCell(ws, Columns.Competitor_Diesel, "Diesel");
            WriteCompetitorHeaderCell(ws, Columns.Competitor_SuperUnleaded, "Super Unleaded");
            WriteCompetitorHeaderCell(ws, Columns.Competitor_UnleadedDriveTimeMarkup, "Unleaded Markup");
            WriteCompetitorHeaderCell(ws, Columns.Competitor_DieselDriveTimeMarkup, "Diesel Markup");
            WriteCompetitorHeaderCell(ws, Columns.Competitor_SuperUnleadedDriveTimeMarkup, "Super Markup");
            WriteCompetitorHeaderCell(ws, Columns.Competitor_UnleadedIncMarkup, "Unleaded Inc");
            WriteCompetitorHeaderCell(ws, Columns.Competitor_DieselIncMarkup, "Diesel Inc");
            WriteCompetitorHeaderCell(ws, Columns.Competitor_SuperUnleadedIncMarkup, "Super Inc");

            NewRow();
        }

        private void WriteSainsburysHeaderCell(IXLWorksheet ws, Columns column, string text)
        {
            var cell = ws.Cell(_row, (int)column);
            cell.Value = text;
            ExcelStyleFormatters.ColumnHeaderOrange(cell);
        }

        private void WriteCompetitorHeaderCell(IXLWorksheet ws, Columns column, string text)
        {
            var cell = ws.Cell(_row, (int)column);
            cell.Value = text;
            ExcelStyleFormatters.ColumnHeader(cell);
        }

        private void NewRow()
        {
            _row++;
        }

        private void SetColumn(IXLWorksheet ws, Columns column, object value, Action<IXLCell> formatter)
        {
            var cell = ws.Cell(_row, (int)column);
            cell.Value = value;
            formatter(cell);
        }

        private void DullCell(IXLWorksheet ws, Columns column)
        {
            var cell = ws.Cell(_row, (int)column);
            ExcelStyleFormatters.DullGrey(cell);
        }

        private double ToActualPrice(int modalPrice)
        {
            return ((double)modalPrice) / 10;
        }

        private string FormatPrice(int? price)
        {
            return price.HasValue && price.Value > 0
                ? ToActualPrice(price.Value).ToString("0.0")
                : NAString;
        }

        private string FormatPrice(int price)
        {
            return price == 0
                ? NAString
                : ToActualPrice(price).ToString("0.0");
        }

        private string FormatYesNo(bool value)
        {
            return value
                ? "Yes"
                : "No";
        }

        private string FormatMarkup(double value)
        {
            return value < 0
                ? "-" + Math.Abs(value).ToString("0.0")
                : "+" + Math.Abs(value).ToString("0.0");
        }


        private int GetOverrideOrTodayPrice(FuelPriceViewModel fuelPrice)
        {
            if (fuelPrice == null)
                return 0;
            if (fuelPrice.OverridePrice.HasValue && fuelPrice.OverridePrice.Value > 0)
                return fuelPrice.OverridePrice.Value;
            return fuelPrice.TodayPrice.HasValue
                ? fuelPrice.TodayPrice.Value
                : 0;
        }

        private void AddSainburysSite(IXLWorksheet ws, SitePriceViewModel jssite)
        {
            var unleadedPrice = jssite.FuelPrices.FirstOrDefault(x => x.FuelTypeId == (int)FuelTypeItem.Unleaded);
            var dieselPrice = jssite.FuelPrices.FirstOrDefault(x => x.FuelTypeId == (int)FuelTypeItem.Diesel);
            var superUnleadedPrice = jssite.FuelPrices.FirstOrDefault(x => x.FuelTypeId == (int)FuelTypeItem.Super_Unleaded);

            var unleaded = GetOverrideOrTodayPrice(unleadedPrice);
            var diesel = GetOverrideOrTodayPrice(dieselPrice);
            var superUnleaded = GetOverrideOrTodayPrice(superUnleadedPrice);

            var strategy = "Standard";
            var markup = unleadedPrice == null ? 0.0 : unleadedPrice.CompetitorPriceOffset;
            switch (jssite.PriceMatchType)
            {
                case PriceMatchType.None:
                case PriceMatchType.StandardPrice:
                    strategy = "Standard Price";
                    break;

                case PriceMatchType.TrailPrice:
                    strategy = "Trial Price";
                    break;

                case PriceMatchType.MatchCompetitorPrice:
                    strategy = "Match Competitor";
                    break;
            }

            SetColumn(ws, Columns.Sainsburys_StoreNo, jssite.StoreNo, ExcelStyleFormatters.GeneralIntegerFormatter);
            SetColumn(ws, Columns.Sainsburys_SiteName, jssite.StoreName, ExcelStyleFormatters.GeneralTextLeftAlignedFormatter);
            SetColumn(ws, Columns.Sainsburys_SiteTown, jssite.Town, ExcelStyleFormatters.GeneralTextLeftAlignedFormatter);
            SetColumn(ws, Columns.Sainsburys_Unleaded, FormatPrice(unleaded), ExcelStyleFormatters.GeneralPrice1DPFormatter);
            SetColumn(ws, Columns.Sainsburys_Diesel, FormatPrice(diesel), ExcelStyleFormatters.GeneralPrice1DPFormatter);
            SetColumn(ws, Columns.Sainsburys_SuperUnleaded, FormatPrice(superUnleaded), ExcelStyleFormatters.GeneralPrice1DPFormatter);
            SetColumn(ws, Columns.Sainsburys_PriceMatchStrategy, strategy, ExcelStyleFormatters.GeneralTextLeftAlignedFormatter);
            SetColumn(ws, Columns.Sainsburys_Markup, markup, ExcelStyleFormatters.GeneralPrice1DPFormatter);

            StyleSainsburysRowBordersAndColor(ws, Columns.Sainsburys_StoreNo, Columns.Competitor_SuperUnleadedIncMarkup);

            NewRow();
        }

        private void StyleSainsburysRowBordersAndColor(IXLWorksheet ws, Columns startColumn, Columns endColumn)
        {
            for (var index = (int)startColumn; index <= (int)endColumn; index++)
            {
                var cell = ws.Cell(_row, index);
                cell.Style.Border.SetTopBorder(XLBorderStyleValues.Medium);
                cell.Style.Border.SetTopBorderColor(XLColor.Orange);
                cell.Style.Border.SetBottomBorder(XLBorderStyleValues.Medium);
                cell.Style.Border.SetBottomBorderColor(XLColor.Orange);
                cell.Style.Font.SetFontColor(XLColor.Orange);
                cell.Style.Font.SetFontSize(16.0);
            }
        }

        private void FormatSainsburysColor(IXLWorksheet ws, Columns column)
        {
            var cell = ws.Cell(_row, (int)column);
            cell.Style.Font.SetFontColor(XLColor.Orange);
            cell.Style.Font.Bold = true;
        }

        private void SetFontAndFillColors(IXLWorksheet ws, Columns column, XLColor fontColor, XLColor fillColor)
        {
            var cell = ws.Cell(_row, (int)column);
            cell.Style.Fill.SetBackgroundColor(fillColor);
            cell.Style.Font.SetFontColor(fontColor);
        }

        private void SetAllBorders(IXLWorksheet ws, Columns column, XLColor borderColor, XLBorderStyleValues borderStyle)
        {
            var cell = ws.Cell(_row, (int)column);
            cell.Style.Border.SetTopBorder(borderStyle);
            cell.Style.Border.SetLeftBorder(borderStyle);
            cell.Style.Border.SetRightBorder(borderStyle);
            cell.Style.Border.SetBottomBorder(borderStyle);
            cell.Style.Border.SetTopBorderColor(borderColor);
            cell.Style.Border.SetLeftBorderColor(borderColor);
            cell.Style.Border.SetRightBorderColor(borderColor);
            cell.Style.Border.SetBottomBorderColor(borderColor);
        }

        private void SetBold(IXLWorksheet ws, Columns column, bool bold = true)
        {
            var cell = ws.Cell(_row, (int)column);
            cell.Style.Font.Bold = bold;
        }

        private void SetFillColor(IXLWorksheet ws, Columns colum, XLColor fillColor)
        {
            var cell = ws.Cell(_row, (int)colum);
            cell.Style.Fill.SetBackgroundColor(fillColor);
        }

        private void FormatUnleadedCell(IXLWorksheet ws, Columns column, bool isCheapest, bool isCompetitor)
        {
            if (isCheapest)
            {
                SetFontAndFillColors(ws, column, XLColor.Black, XLColor.Yellow);
                if (isCompetitor)
                {
                    SetAllBorders(ws, column, XLColor.Orange, XLBorderStyleValues.Medium);
                    SetBold(ws, column);
                }
            }
            else
            {
                SetFontAndFillColors(ws, column, XLColor.Black, XLColor.PastelGreen);
                if (isCompetitor)
                {
                    SetAllBorders(ws, column, XLColor.Orange, XLBorderStyleValues.Medium);
                    SetBold(ws, column);
                }
            }
        }

        private void FormatDieselCell(IXLWorksheet ws, Columns column, bool isCheapest, bool isCompetitor)
        {
            if (isCheapest)
            {
                SetFontAndFillColors(ws, column, XLColor.Black, XLColor.Yellow);
                if (isCompetitor)
                {
                    SetAllBorders(ws, column, XLColor.Orange, XLBorderStyleValues.Medium);
                    SetBold(ws, column);
                }
            }
            else
            {
                SetFontAndFillColors(ws, column, XLColor.White, XLColor.Black);
                if (isCompetitor)
                {
                    SetAllBorders(ws, column, XLColor.Orange, XLBorderStyleValues.Medium);
                    SetBold(ws, column);
                }
            }
        }

        private void FormatSuperUnleadedCell(IXLWorksheet ws, Columns column, bool isCheapest, bool isCompetitor)
        {
            if (isCheapest)
            {
                SetFontAndFillColors(ws, column, XLColor.Black, XLColor.Yellow);
                if (isCompetitor)
                {
                    SetAllBorders(ws, column, XLColor.Orange, XLBorderStyleValues.Medium);
                    SetBold(ws, column);
                }
            }
            else
            {
                SetFontAndFillColors(ws, column, XLColor.Black, XLColor.White);
                if (isCompetitor)
                {
                    SetAllBorders(ws, column, XLColor.Orange, XLBorderStyleValues.Medium);
                    SetBold(ws, column);
                }
            }
        }

        private int GetTodayPrice(FuelTypeItem fuelType, IEnumerable<FuelPriceViewModel> fuelPrices)
        {
            if (fuelPrices == null)
                return 0;

            var price = fuelPrices.FirstOrDefault(x => x.FuelTypeId == (int)fuelType);
            if (price == null)
                return 0;
            return price.TodayPrice.HasValue
                ? price.TodayPrice.Value
                : 0;
        }

        public void AddCompetitorPrices(IXLWorksheet ws, SitePriceViewModel jssite, IEnumerable<SitePriceViewModel> pricesForCompetitors, DriveTimeFuelSettingsViewModel driveTimeMarkups)
        {
            // build DriveTime look for speed
            var unleadedDriveTimeLookup = DriveTimeLookup.Create(MaxDriveTime, driveTimeMarkups.Unleaded);
            var dieselDriveTimeLookup = DriveTimeLookup.Create(MaxDriveTime, driveTimeMarkups.Diesel);
            var superUnleadedDriveTimeLookup = DriveTimeLookup.Create(MaxDriveTime, driveTimeMarkups.SuperUnleaded);

            // cheapest competitors used
            var unleadedCompetitorName = jssite.FuelPrices.First(x => x.FuelTypeId == (int)FuelTypeItem.Unleaded).CompetitorName.ToUpper();
            var dieselCompetitorName = jssite.FuelPrices.First(x => x.FuelTypeId == (int)FuelTypeItem.Diesel).CompetitorName.ToUpper();
            var superUnleadedCompetitorName = jssite.FuelPrices.First(x => x.FuelTypeId == (int)FuelTypeItem.Super_Unleaded).CompetitorName.ToUpper();

            // find cheapest Competitor price for each fuel
            var cheapestUnleadedIncDriveTime = FindCheapestPriceForFuel(FuelTypeItem.Unleaded, pricesForCompetitors, unleadedDriveTimeLookup);
            var cheapestDieselIncDriveTime = FindCheapestPriceForFuel(FuelTypeItem.Diesel, pricesForCompetitors, dieselDriveTimeLookup);
            var cheapestSuperUnleadedIncDriveTime = FindCheapestPriceForFuel(FuelTypeItem.Super_Unleaded, pricesForCompetitors, superUnleadedDriveTimeLookup);

            foreach (var comp in pricesForCompetitors)
            {
                var unleadedModalPrice = GetTodayPrice(FuelTypeItem.Unleaded, comp.FuelPrices);
                var dieselModalPrice = GetTodayPrice(FuelTypeItem.Diesel, comp.FuelPrices);
                var superUnleadedModalPrice = GetTodayPrice(FuelTypeItem.Super_Unleaded, comp.FuelPrices);

                var isIgnored = comp.IsActive == false || comp.IsExcluded || comp.IsExcludedBrand;

                SetColumn(ws, Columns.Competitor_IsActive, FormatYesNo(comp.IsActive), ExcelStyleFormatters.GeneralRedNoFormatter);
                SetColumn(ws, Columns.Competitor_IsSiteExcluded, FormatYesNo(comp.IsExcluded), ExcelStyleFormatters.GeneralRedYesFormatter);
                SetColumn(ws, Columns.Competitor_IsBrandExcluded, FormatYesNo(comp.IsExcludedBrand), ExcelStyleFormatters.GeneralRedYesFormatter);
                SetColumn(ws, Columns.Competitor_Brand, comp.Brand, ExcelStyleFormatters.GeneralTextLeftAlignedFormatter);

                if (!isIgnored && comp.Brand.ToUpper() == "SAINSBURYS")
                    FormatSainsburysColor(ws, Columns.Competitor_Brand);
                else if (comp.IsGrocer)
                    FormatGrocerCell(ws, Columns.Competitor_Brand);

                SetColumn(ws, Columns.Competitor_StoreName, comp.StoreName, ExcelStyleFormatters.GeneralTextLeftAlignedFormatter);
                SetColumn(ws, Columns.Competitor_CatNo, comp.CatNo, ExcelStyleFormatters.GeneralIntegerFormatter);
                SetColumn(ws, Columns.Competitor_DriveTime, comp.DriveTime, ExcelStyleFormatters.GeneralNumber2DPFormatter);
                SetColumn(ws, Columns.Competitor_Distance, comp.Distance, ExcelStyleFormatters.GeneralNumber2DPFormatter);

                if (unleadedModalPrice > 0)
                {
                    var unleadedDriveTimeMarkup = unleadedDriveTimeLookup.GetMarkup(comp.DriveTime);
                    var unleadedPriceIncMarkup = unleadedModalPrice + (int)(unleadedDriveTimeMarkup * 10);
                    var isCheapestUnleaded = unleadedPriceIncMarkup == cheapestUnleadedIncDriveTime;
                    var isUnleadedCompetitor = IsCompetitorName(unleadedCompetitorName, comp, unleadedModalPrice);

                    SetColumn(ws, Columns.Competitor_Unleaded, FormatPrice(unleadedModalPrice), ExcelStyleFormatters.GeneralPrice1DPFormatter);
                    SetColumn(ws, Columns.Competitor_UnleadedDriveTimeMarkup, FormatMarkup(unleadedDriveTimeMarkup), ExcelStyleFormatters.GeneralPrice1DPFormatter);
                    SetColumn(ws, Columns.Competitor_UnleadedIncMarkup, FormatPrice(unleadedPriceIncMarkup), ExcelStyleFormatters.GeneralPrice1DPFormatter);

                    FormatUnleadedCell(ws, Columns.Competitor_Unleaded, isCheapestUnleaded, isUnleadedCompetitor);
                    FormatUnleadedCell(ws, Columns.Competitor_UnleadedDriveTimeMarkup, isCheapestUnleaded, isUnleadedCompetitor);
                    FormatUnleadedCell(ws, Columns.Competitor_UnleadedIncMarkup, isCheapestUnleaded, isUnleadedCompetitor);
                }
                else
                {
                    SetColumn(ws, Columns.Competitor_Unleaded, NAString, ExcelStyleFormatters.GeneralNAFormatter);
                    SetColumn(ws, Columns.Competitor_UnleadedDriveTimeMarkup, NAString, ExcelStyleFormatters.GeneralNAFormatter);
                    SetColumn(ws, Columns.Competitor_UnleadedIncMarkup, NAString, ExcelStyleFormatters.GeneralNAFormatter);
                }

                if (dieselModalPrice > 0)
                {
                    var dieselDriveTimeMarkup = dieselDriveTimeLookup.GetMarkup(comp.DriveTime);
                    var dieselPriceIncMarkup = dieselModalPrice + (int)(dieselDriveTimeMarkup * 10);
                    var isCheapestDiesel = dieselPriceIncMarkup == cheapestDieselIncDriveTime;
                    var isDieselCompetitor = IsCompetitorName(dieselCompetitorName, comp, dieselModalPrice);

                    SetColumn(ws, Columns.Competitor_Diesel, FormatPrice(dieselModalPrice), ExcelStyleFormatters.GeneralPrice1DPFormatter);
                    SetColumn(ws, Columns.Competitor_DieselDriveTimeMarkup, FormatMarkup(dieselDriveTimeMarkup), ExcelStyleFormatters.GeneralPrice1DPFormatter);
                    SetColumn(ws, Columns.Competitor_DieselIncMarkup, FormatPrice(dieselPriceIncMarkup), ExcelStyleFormatters.GeneralPrice1DPFormatter);

                    FormatDieselCell(ws, Columns.Competitor_Diesel, isCheapestDiesel, isDieselCompetitor);
                    FormatDieselCell(ws, Columns.Competitor_DieselDriveTimeMarkup, isCheapestDiesel, isDieselCompetitor);
                    FormatDieselCell(ws, Columns.Competitor_DieselIncMarkup, isCheapestDiesel, isDieselCompetitor);
                }
                else
                {
                    SetColumn(ws, Columns.Competitor_Diesel, NAString, ExcelStyleFormatters.GeneralNAFormatter);
                    SetColumn(ws, Columns.Competitor_DieselDriveTimeMarkup, NAString, ExcelStyleFormatters.GeneralNAFormatter);
                    SetColumn(ws, Columns.Competitor_DieselIncMarkup, NAString, ExcelStyleFormatters.GeneralNAFormatter);
                }

                if (superUnleadedModalPrice > 0)
                {
                    var superUnleadedDriveTimeMarkup = superUnleadedDriveTimeLookup.GetMarkup(comp.DriveTime);
                    var superUnleadedPriceIncMarkup = superUnleadedModalPrice = (int)(superUnleadedDriveTimeMarkup * 10);
                    var isCheapestSuperUnleaded = superUnleadedPriceIncMarkup == cheapestSuperUnleadedIncDriveTime;
                    var isSuperUnleadedCompetitor = IsCompetitorName(superUnleadedCompetitorName, comp, superUnleadedModalPrice);

                    SetColumn(ws, Columns.Competitor_SuperUnleaded, FormatPrice(superUnleadedModalPrice), ExcelStyleFormatters.GeneralPrice1DPFormatter);
                    SetColumn(ws, Columns.Competitor_SuperUnleadedDriveTimeMarkup, FormatMarkup(superUnleadedDriveTimeMarkup), ExcelStyleFormatters.GeneralPrice1DPFormatter);
                    SetColumn(ws, Columns.Competitor_SuperUnleadedIncMarkup, FormatPrice(superUnleadedPriceIncMarkup), ExcelStyleFormatters.GeneralPrice1DPFormatter);

                    FormatSuperUnleadedCell(ws, Columns.Competitor_SuperUnleaded, isCheapestSuperUnleaded, isSuperUnleadedCompetitor);
                    FormatSuperUnleadedCell(ws, Columns.Competitor_SuperUnleadedDriveTimeMarkup, isCheapestSuperUnleaded, isSuperUnleadedCompetitor);
                    FormatSuperUnleadedCell(ws, Columns.Competitor_SuperUnleadedIncMarkup, isCheapestSuperUnleaded, isSuperUnleadedCompetitor);
                }
                else
                {
                    SetColumn(ws, Columns.Competitor_SuperUnleaded, NAString, ExcelStyleFormatters.GeneralNAFormatter);
                    SetColumn(ws, Columns.Competitor_SuperUnleadedDriveTimeMarkup, NAString, ExcelStyleFormatters.GeneralNAFormatter);
                    SetColumn(ws, Columns.Competitor_SuperUnleadedIncMarkup, NAString, ExcelStyleFormatters.GeneralNAFormatter);
                }

                if (isIgnored)
                {
                    DullCell(ws, Columns.Competitor_Brand);
                    DullCell(ws, Columns.Competitor_StoreName);
                    DullCell(ws, Columns.Competitor_CatNo);
                    DullCell(ws, Columns.Competitor_DriveTime);
                    DullCell(ws, Columns.Competitor_Distance);
                    DullCell(ws, Columns.Competitor_Unleaded);
                    DullCell(ws, Columns.Competitor_Diesel);
                    DullCell(ws, Columns.Competitor_SuperUnleaded);
                    DullCell(ws, Columns.Competitor_UnleadedDriveTimeMarkup);
                    DullCell(ws, Columns.Competitor_DieselDriveTimeMarkup);
                    DullCell(ws, Columns.Competitor_SuperUnleadedDriveTimeMarkup);
                    DullCell(ws, Columns.Competitor_UnleadedIncMarkup);
                    DullCell(ws, Columns.Competitor_DieselIncMarkup);
                    DullCell(ws, Columns.Competitor_SuperUnleadedIncMarkup);

                    SetFillColor(ws, Columns.Competitor_IsActive, XLColor.LightGray);
                    SetFillColor(ws, Columns.Competitor_IsBrandExcluded, XLColor.LightGray);
                    SetFillColor(ws, Columns.Competitor_IsSiteExcluded, XLColor.LightGray);
                }

                NewRow();
            }

            NewRow();
        }

        private int FindCheapestPriceForFuel(FuelTypeItem fuelType, IEnumerable<SitePriceViewModel> pricesForCompetitors, DriveTimeLookup driveTimeLookup)
        {
            var cheapestPrice = 0;

            foreach(var comp in pricesForCompetitors)
            {
                if (comp.FuelPrices != null)
                {
                    var markupForComp = driveTimeLookup.GetMarkup(comp.DriveTime);

                    foreach(var fuelPrice in comp.FuelPrices)
                    {
                        if (fuelPrice.FuelTypeId == (int)fuelType)
                        {
                            if (fuelPrice.TodayPrice.HasValue && fuelPrice.TodayPrice > 0)
                            {
                                var priceIncDriveTime = fuelPrice.TodayPrice.Value + (int)(10 * markupForComp);
                                if (cheapestPrice == 0 || priceIncDriveTime < cheapestPrice)
                                    cheapestPrice = priceIncDriveTime;
                            }
                        }
                    }
                }
            }
            return cheapestPrice;
        }

        private bool IsCompetitorName(string usedCompetitorName, SitePriceViewModel compSite, int? todayPrice)
        {
            if (String.IsNullOrEmpty(usedCompetitorName) || compSite == null || !todayPrice.HasValue || todayPrice == 0)
                return false;

            return String.Compare(usedCompetitorName, compSite.Brand + "/" + compSite.StoreName, true) == 0;
        }

        private void FormatGrocerCell(IXLWorksheet ws, Columns column)
        {
            var cell = ws.Cell(_row, (int)column);
            cell.Style.Font.Bold = true;
            cell.Style.Font.SetFontColor(XLColor.Blue);
        }
    }
}