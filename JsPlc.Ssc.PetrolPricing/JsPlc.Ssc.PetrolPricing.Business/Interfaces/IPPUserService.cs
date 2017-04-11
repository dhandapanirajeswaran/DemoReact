using JsPlc.Ssc.PetrolPricing.Models;
using JsPlc.Ssc.PetrolPricing.Models.ViewModels.UserPermissions;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace JsPlc.Ssc.PetrolPricing.Business
{
    public interface IPPUserService
    {
        PPUserList GetPPUsers();
        PPUserList AddUser(PPUser user);
        PPUserList DeleteUser(string email);

        PPUserDetails GetPPUserDetails(int id);

        PPUserPermissions GetPermissions(int ppUserId);
        bool UpsertPermissions(int requestingPPUserId, PPUserPermissions permissions);

        UserAccessViewModel GetUserAccess(string userName);

        void SignIn(string email);
    }
}
