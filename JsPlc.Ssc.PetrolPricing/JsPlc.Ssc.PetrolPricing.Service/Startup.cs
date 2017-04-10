using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Owin;
using Owin;
using JsPlc.Ssc.PetrolPricing.Repository.Dapper;
using System.Web.Configuration;

[assembly: OwinStartup(typeof(JsPlc.Ssc.PetrolPricing.Service.Startup))]

namespace JsPlc.Ssc.PetrolPricing.Service
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
            ConfigureDapper();
        }

        private void ConfigureDapper()
        {
            var connectionString = WebConfigurationManager.ConnectionStrings["PetrolPricingRepository"].ConnectionString; ;
            DapperHelper.DatabaseConnectionString = connectionString;
        }
    }
}
