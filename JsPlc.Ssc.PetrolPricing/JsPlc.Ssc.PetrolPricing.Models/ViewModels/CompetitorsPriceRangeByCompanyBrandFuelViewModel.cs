using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JsPlc.Ssc.PetrolPricing.Models.ViewModels
{
    public class CompetitorsPriceRangeByCompanyBrandFuelViewModel
    {
        public int Min { get; set; }

        public int Average { get; set; }

        public int Max { get; set; }

        public int FuelTypeId { get; set; }
    }
}
