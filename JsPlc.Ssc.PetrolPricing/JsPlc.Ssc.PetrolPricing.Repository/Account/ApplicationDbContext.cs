using Microsoft.AspNet.Identity.EntityFramework;

namespace JsPlc.Ssc.PetrolPricing.Repository
{
    public class ApplicationDbContext : IdentityDbContext<IdentityUser, IdentityRole, string, IdentityUserLogin, IdentityUserRole, IdentityUserClaim>
    {
        public ApplicationDbContext() : base("PetrolPricingLogins")
        {
        }
    }
}
