using JsPlc.Ssc.PetrolPricing.Models.Enums;
using JsPlc.Ssc.PetrolPricing.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace JsPlc.Ssc.PetrolPricing.Repository.Debugging
{
    internal class DebugFileLogger
    {
        private StringBuilder _sb = new StringBuilder();

        const string indentSite = "\t";
        const string indentFuelPrice = "\t\t";
        const string indentProperty = "\t\t\t";
        const string indentFuel = "\t\t";

        private static List<string> FuelPricePropertyNames = new List<string>()
        {
            "AutoPrice",
            "TodayPrice",
            "YestPrice",
            "Markup",
            "CompetitorName",
            "IsTrailPrice",
            "Difference",
            "CompetitorPriceOffset",
            "IsBasedOnCompetitor"
        };

        public DebugFileLogger()
        {
        }

        public void Clear()
        {
            _sb.Clear();
        }

        public void LogSiteInfo(SitePriceViewModel site)
        {
            _sb.Append(indentSite);
            _sb.AppendLine("Site: " + site.SiteId + " - Name: " + site.StoreName + " - StoreNo: " + site.StoreNo);
        }

        public void LogFuelPrice(FuelTypeItem fuelType, SitePriceViewModel left, SitePriceViewModel right)
        {
            var leftFuelPrice = left.FuelPrices.FirstOrDefault(x => x.FuelTypeId == (int)fuelType);

            var rightFuelPrice = right.FuelPrices.FirstOrDefault(x => x.FuelTypeId == (int)fuelType);

            DumpFuelPrice(leftFuelPrice, rightFuelPrice, fuelType);
        }

        public void BlankLine()
        {
            _sb.AppendLine();
        }

        public void WriteToFile(string filename)
        {
            File.WriteAllText(filename, _sb.ToString());
        }
        #region private methods
        private void DumpFuelPrice(FuelPriceViewModel leftFuelPrice, FuelPriceViewModel rightFuelPrice, FuelTypeItem fuelType)
        {
            var leftKeyValuePairs = PropertiesAsKeyValuePairs(leftFuelPrice, FuelPricePropertyNames);
            var rightKeyValuePairs = PropertiesAsKeyValuePairs(rightFuelPrice, FuelPricePropertyNames);

            var paddedLength = FuelPricePropertyNames.Max(x => x.Length);

            _sb.AppendLine();

            _sb.Append(indentFuel);
            _sb.AppendFormat("[{0}]", fuelType);
            _sb.AppendLine();

            foreach(var kvp in leftKeyValuePairs)
            {
                var name = kvp.Key;
                var leftValue = kvp.Value;
                var rightValue = rightKeyValuePairs[kvp.Key];
                var different = leftValue == rightValue ? "  " : "**";

                _sb.Append(indentProperty);
                _sb.AppendFormat("{0} {1}: {2} --VS-- {3}", different, name.PadRight(paddedLength, ' '), leftValue, rightValue);
                _sb.AppendLine();
            }
        }

        private Dictionary<string, string> PropertiesAsKeyValuePairs(object data, IEnumerable<String> propertyNames)
        {
            var dict = new Dictionary<string, string>();
            if (data == null)
            {
                foreach (var name in propertyNames)
                    dict.Add(name, "NULL");
            }
            else
            {
                var type = data.GetType();
                foreach (var name in propertyNames)
                {
                    var value = "NULL";

                    if (data != null)
                    {
                        try
                        {
                            var property = type.GetRuntimeProperties().FirstOrDefault(p => string.Equals(p.Name, name, StringComparison.OrdinalIgnoreCase));
                            if (property != null)
                            {
                                var propValue = property.GetValue(data);
                                value = propValue == null ? "NULL" : propValue.ToString();
                            }
                        }
                        catch (Exception ex)
                        {
                            value = ex.ToString();
                        }
                    }
                    dict.Add(name, value);
                }
            }
            return dict;
        }

        #endregion
    }
}
