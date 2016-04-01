using Microsoft.AspNet.Identity.EntityFramework;
using System;

namespace JsPlc.Ssc.PetrolPricing.Repository
{
    public class AccountRepository : IAccountRepository
    {
        private readonly ApplicationDbContext _context;

        public AccountRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public void RegisterUser(string email)
        {
            try {
                var user = _context.Database.SqlQuery<IdentityUser>("SELECT * FROM AspNetUsers WHERE Email = @p0", email).ToArrayAsync().Result;

                if (user == null || user.Length == 0)
                {
                    _context.Users.Add(
                        new IdentityUser
                        {
                            AccessFailedCount = 0,
                            Email = email,
                            EmailConfirmed = true,
                            Id = Guid.NewGuid().ToString(),
                            LockoutEnabled = false,
                            PhoneNumberConfirmed = false,
                            SecurityStamp = Guid.NewGuid().ToString(),
                            TwoFactorEnabled = false,
                            UserName = email
                        });

                    _context.SaveChanges();
                }
            }
            catch
            {
                //DO ERROR LOGGING
            }
        }
    }
}
