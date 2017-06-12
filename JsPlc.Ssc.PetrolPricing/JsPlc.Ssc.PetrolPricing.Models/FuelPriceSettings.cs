using System.Collections.Generic;
using System.Linq;

namespace JsPlc.Ssc.PetrolPricing.Models
{
    public class FuelPriceSettings
    {
        public IEnumerable<FuelPriceSetting> AllFuels { get; set; }

        public FuelPriceSetting Diesel
        {
            get
            {
                return this.AllFuels.First(x => x.FuelType == Enums.FuelTypeItem.Diesel);
            }
        }

        public FuelPriceSetting SuperUnleaded
        {
            get
            {
                return this.AllFuels.First(x => x.FuelType == Enums.FuelTypeItem.Super_Unleaded);
            }
        }

        public FuelPriceSetting Unleaded
        {
            get
            {
                return this.AllFuels.First(x => x.FuelType == Enums.FuelTypeItem.Unleaded);
            }
        }

        public FuelPriceSettings()
        {
            this.AllFuels = new List<FuelPriceSetting>();
        }
    }
}