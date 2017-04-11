using JsPlc.Ssc.PetrolPricing.Models;
using JsPlc.Ssc.PetrolPricing.Repository;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using JsPlc.Ssc.PetrolPricing.Models.ViewModels.UserPermissions;

namespace JsPlc.Ssc.PetrolPricing.Business
{
    public class PPUserService : IPPUserService
    {
        private IPetrolPricingRepository _repository;

        public PPUserService(IPetrolPricingRepository repository)
        {
            _repository = repository;
        }

        public PPUserList GetPPUsers()
        {
            return _repository.GetPPUsers();
        }
        public PPUserList AddUser(PPUser user)
        {
             return _repository.AddPPUser(user);
        }
        public PPUserList DeleteUser(string email)
        {
            return _repository.DeletePPUser(email);
        }

        public PPUserDetails GetPPUserDetails(int id)
        {
            return _repository.GetPPUserDetails(id);
        }

        public PPUserPermissions GetPermissions(int ppUserId)
        {
            return _repository.GetUserPermissions(ppUserId);
        }

        public bool UpsertPermissions(int requestingPPUserId, PPUserPermissions permissions)
        {
            return _repository.UpsertUserPermissions(requestingPPUserId, permissions);
        }

        public UserAccessViewModel GetUserAccess(string userName)
        {
            return _repository.GetUserAccess(userName);
        }

        public void SignIn(string email)
        {
            _repository.SignIn(email);
        }
    }
}
