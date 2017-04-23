using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JsPlc.Ssc.PetrolPricing.Models.Enums
{
    public enum PriceMatchType
    {
        None = 0,
        SoloPrice = 1,
        TrailPrice = 2,
        MatchCompetitorPrice = 3
    }
}
