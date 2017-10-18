using JsPlc.Ssc.PetrolPricing.WinService.Logging;

namespace JsPlc.Ssc.PetrolPricing.WinService.Interfaces
{
    public interface IPetrolPricingTaskScheduler
    {
        void Start(IEventLog log, IAppSettings settings);

        void Stop(IEventLog log, IAppSettings settings);
    }
}