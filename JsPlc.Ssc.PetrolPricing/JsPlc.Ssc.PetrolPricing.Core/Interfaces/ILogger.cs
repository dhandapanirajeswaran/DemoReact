using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JsPlc.Ssc.PetrolPricing.Core.Interfaces
{
    public interface ILogger
    {
        void Error(Exception ex);
        void Information(string message);

        void Debug(string message);
    }
}
