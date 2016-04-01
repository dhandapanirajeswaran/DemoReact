using JsPlc.Ssc.PetrolPricing.Repository;
using System.Threading.Tasks;

namespace JsPlc.Ssc.PetrolPricing.Business
{
    public class AccountService : IAccountService
    {
        private IAccountRepository _repository;

        public AccountService(IAccountRepository repository)
        {
            _repository = repository;
        }

        public void RegisterUser(string email)
        {
            _repository.RegisterUser(email);
        }
    }
}
