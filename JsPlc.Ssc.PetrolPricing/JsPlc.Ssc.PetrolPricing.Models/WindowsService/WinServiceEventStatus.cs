using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JsPlc.Ssc.PetrolPricing.Models.WindowsService
{
    public enum WinServiceEventStatus
    {
        None = 0,
        Paused = 1,
        Sleeping = 2,
        Running = 3,
        Success = 4,
        Failed = 5
    }
}