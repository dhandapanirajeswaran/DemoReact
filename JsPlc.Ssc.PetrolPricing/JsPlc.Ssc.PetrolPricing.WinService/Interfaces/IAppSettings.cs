using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JsPlc.Ssc.PetrolPricing.WinService.Interfaces
{
    public interface IAppSettings
    {
        string WebApiServiceBaseUrl { get; }
        int RunEmailSecheduleEveryXMinutes { get; }
        bool EnableDebugLog { get; }
        bool EnableTraceLog { get; }
        bool EnableInfoLog { get;}
    }
}
