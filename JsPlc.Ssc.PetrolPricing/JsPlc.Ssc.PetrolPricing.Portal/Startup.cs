using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(JsPlc.Ssc.PetrolPricing.Portal.Startup))]
namespace JsPlc.Ssc.PetrolPricing.Portal
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
