using JsPlc.Ssc.PetrolPricing.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JsPlc.Ssc.PetrolPricing.Models.ViewModels
{
    public class HistoricalPriceViewModel
    {
        public DateTime PriceDate { get; set; }
        public int SiteId { get; set; } = 0;
        public int FuelTypeId { get; set; } = 0;
        public int TodayPrice { get; set; } = 0;
        public string PriceSource { get; set; } = "";
        public PriceReasonFlags PriceReasonFlags { get; set; } = PriceReasonFlags.None;
    }
}