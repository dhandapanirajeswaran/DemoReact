using JsPlc.Ssc.PetrolPricing.Models.Enums;

namespace JsPlc.Ssc.PetrolPricing.Models
{
    public class FuelPriceSetting
    {
        public FuelTypeItem FuelType { get; set; }
        public int Markup { get; set; }
        public int MinPrice { get; set; }
        public int MaxPrice { get; set; }
        public int MinPriceChange { get; set; }
        public int MaxPriceChange { get; set; }
    }
}