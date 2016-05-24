using JsPlc.Ssc.PetrolPricing.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace JsPlc.Ssc.PetrolPricing.Business
{
    public interface IPPUserService
    {
        IEnumerable<PPUser> GetPPUsers();
        IEnumerable<PPUser> AddUser(PPUser user);
        IEnumerable<PPUser> DeleteUser(PPUser user);
      
    }
}
