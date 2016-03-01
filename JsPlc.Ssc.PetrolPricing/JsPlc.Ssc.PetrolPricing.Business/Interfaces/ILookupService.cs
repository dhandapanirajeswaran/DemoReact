using JsPlc.Ssc.PetrolPricing.Models;
using System.Collections.Generic;

namespace JsPlc.Ssc.PetrolPricing.Business
{
    public interface ILookupService
    {
        IEnumerable<UploadType> GetUploadTypes();

        IEnumerable<FuelType> GetFuelTypes();

        IEnumerable<ImportProcessStatus> GetProcessStatuses();
    }
}
