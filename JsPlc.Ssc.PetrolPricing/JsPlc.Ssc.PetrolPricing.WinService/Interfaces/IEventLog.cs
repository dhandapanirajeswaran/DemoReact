using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JsPlc.Ssc.PetrolPricing.WinService.Interfaces
{
    public interface IEventLog : IDisposable
    {
        IEventLog Context(string name);

        bool EnableTrace { get; set; }
        bool EnableDebug { get; set; }
        bool EnableInfo { get; set; }

        void Info(string message);
        void Error(string message);
        void Exception(string message, Exception ex);
        void Trace(string message);
        void Debug(string message);
    }
}