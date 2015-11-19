using System;
using System.Data.Entity;
using System.Collections.Generic;
using JsPlc.Ssc.PetrolPricing.Models;

namespace JsPlc.Ssc.PetrolPricing.Repository
{
    public class RepositoryInitializer : DropCreateDatabaseIfModelChanges<RepositoryContext>
    {
        protected override void Seed(RepositoryContext context)
        {
            var sites=new List<Site>{
                new Site{Id=1, SiteName = "JS Dummy Site1", Town = "Coventry"},
            };

            sites.ForEach(c => context.Sites.Add(c));
            context.SaveChanges();
            
            base.Seed(context);
        }
    }
}
