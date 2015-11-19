using System.Data.Entity;
using JsPlc.Ssc.PetrolPricing.Models;


namespace JsPlc.Ssc.PetrolPricing.Repository
{
    public interface IRepositoryContext
    {
        IDbSet<Site> Sites { get; }

    }
}
