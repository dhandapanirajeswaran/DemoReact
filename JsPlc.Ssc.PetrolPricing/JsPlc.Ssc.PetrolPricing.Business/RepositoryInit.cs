using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using JsPlc.Ssc.PetrolPricing.Models;
using JsPlc.Ssc.PetrolPricing.Models.Persistence;
using JsPlc.Ssc.PetrolPricing.Repository;

namespace JsPlc.Ssc.PetrolPricing.Business
{
    public class RepositoryInit
    {
        public static void InitializeDatabase()
        {
            using (var context = new RepositoryContext())
            {
                var repoInit = new RepositoryInitializer();

                repoInit.InitializeDatabase(context); // calls seed if needed
            }
        }
    }
}