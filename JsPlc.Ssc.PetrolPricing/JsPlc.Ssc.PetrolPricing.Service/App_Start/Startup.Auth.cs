using JsPlc.Ssc.PetrolPricing.IoC;
using Owin;
using System.Web.Http;
using System;
using System.Net.Http.Formatting;
using System.Data.Entity;
using JsPlc.Ssc.PetrolPricing.Repository;

namespace JsPlc.Ssc.PetrolPricing.Service
{
    public partial class Startup
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="app"></param>
        // For more information on configuring authentication, please visit http://go.microsoft.com/fwlink/?LinkId=301864
        public void ConfigureAuth(IAppBuilder app)
        {
            var config = new HttpConfiguration();
            WebApiConfig(config);
            ConfigureFormatters(config.Formatters);

            UnityConfig.RegisterComponents(config);
            Bootstrapper.SetupAutoMapper();

            app.UseWebApi(config);
        }

        private void ConfigureFormatters(MediaTypeFormatterCollection formatters)
        {
            formatters.JsonFormatter.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
            formatters.Remove(GlobalConfiguration.Configuration.Formatters.XmlFormatter);
        }

        private void WebApiConfig(HttpConfiguration config)
        {
            config.MapHttpAttributeRoutes();

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );
        }
    }
}
