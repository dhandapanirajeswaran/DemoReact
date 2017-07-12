using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JsPlc.Ssc.PetrolPricing.Models.WindowsService
{
    // NOTE: This is duplicated within the database itself
    public enum WinServiceEventType
    {
        None = 0,
        DailyPriceEmail = 1
    }
}
