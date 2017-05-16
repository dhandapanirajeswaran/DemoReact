using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JsPlc.Ssc.PetrolPricing.Core.ExtensionMethods
{
    public static class NumberExtensions
    {
        /// <summary>
        /// Convert a 'modal price' (e.g. 1205) into an Actual Price (120.5)
        /// </summary>
        /// <param name="modalPrice"></param>
        /// <returns></returns>
        public static double ToActualPrice(this int modalPrice)
        {
            return Convert.ToDouble(modalPrice) / 10;
        }

        /// <summary>
        /// Convert an Actual price (e.g. 120.5) into a 'modal price' (e.g. 1205)
        /// </summary>
        /// <param name="actualPrice"></param>
        /// <returns></returns>
        public static int ToModalPrice(this double actualPrice)
        {
            return Convert.ToInt32(actualPrice * 10);
        }
    }
}
