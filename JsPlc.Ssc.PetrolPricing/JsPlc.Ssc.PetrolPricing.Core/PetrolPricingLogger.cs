using JsPlc.Ssc.PetrolPricing.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using log4net;

namespace JsPlc.Ssc.PetrolPricing.Core
{
    public class PetrolPricingLogger : ILogger
    {
        private static ILog logger = LogManager.GetLogger(typeof(PetrolPricingLogger));

        public void Error(Exception ex)
        {
            var err = String.Format("PetrolPricing Error at {1}", DateTime.Now.ToString());
            logger.Error(err, ex);
        }

        public void Information(string message)
        {
            var info = String.Format("{0} at {1}", message, DateTime.Now.ToString());
            logger.Info(info);
        }
    }
}
