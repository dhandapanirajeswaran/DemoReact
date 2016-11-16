using JsPlc.Ssc.PetrolPricing.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace JsPlc.Ssc.PetrolPricing.Core
{
    public class PetrolPricingLogger : ILogger
    {
     
        public void Error(Exception ex)
        {
             Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
        }

        public void Information(string message)
        {
            Elmah.ErrorSignal.FromCurrentContext().Raise(new Exception(message));
        }
    }
}
