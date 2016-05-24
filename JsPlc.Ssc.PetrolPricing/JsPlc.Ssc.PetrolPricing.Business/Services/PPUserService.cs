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

        public IEnumerable<PPUser> GetPPUsers()
        {
            return _repository.GetPPUsers();
        }
        public IEnumerable<PPUser> AddUser(PPUser user)
        {
             return _repository.AddPPUser(user);
        }
        public IEnumerable<PPUser> DeleteUser(PPUser user)
        {
            if (user != null)
            {
                return _repository.DeletePPUser(user);
            }
            return null;

        }
        
    }
}
