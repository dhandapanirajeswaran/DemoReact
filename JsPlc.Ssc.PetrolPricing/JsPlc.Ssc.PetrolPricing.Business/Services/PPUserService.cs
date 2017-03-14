using JsPlc.Ssc.PetrolPricing.Models;
using JsPlc.Ssc.PetrolPricing.Repository;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

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
        
    }
}
