using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JsPlc.Ssc.PetrolPricing.WinService.Models
{
    // NOTE: This is an exact copy of the JsPlc.Ssc.PetrolPricing.Models.ViewModels.StatusViewModel class
    // duplicate due to issues with 32/64bit referencing

    public class StatusViewModel
    {
        public string ErrorMessage = "";
        public string SuccessMessage = "";
    }
}
