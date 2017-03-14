using JsPlc.Ssc.PetrolPricing.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace JsPlc.Ssc.PetrolPricing.Business
{
    public interface IPPUserService
    {
        PPUserList GetPPUsers();
        PPUserList AddUser(PPUser user);
        PPUserList DeleteUser(string email);
      
    }
}
