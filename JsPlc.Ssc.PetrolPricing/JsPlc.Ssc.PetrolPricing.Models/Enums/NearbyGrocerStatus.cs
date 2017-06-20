using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JsPlc.Ssc.PetrolPricing.Models.Enums
{
    [Flags]
    public enum NearbyGrocerStatuses : byte
    {
        None = 0,
        HasNearbyGrocers = 0x01,
        AllGrocersHavePriceData = 0x02
    }
}
