﻿using JsPlc.Ssc.PetrolPricing.Business;
using JsPlc.Ssc.PetrolPricing.Business.Interfaces;
using JsPlc.Ssc.PetrolPricing.Business.Services;
using JsPlc.Ssc.PetrolPricing.Core;
using JsPlc.Ssc.PetrolPricing.IoC;
using JsPlc.Ssc.PetrolPricing.Repository;
using Microsoft.Practices.Unity;
using System.Web.Http;
using Unity.WebApi;

namespace JsPlc.Ssc.PetrolPricing.Service
{
    public static class UnityConfig
    {
        public static void RegisterComponents(HttpConfiguration config)
        {
			var container = new UnityContainer();
            
            // register all your components with the container here
            // it is NOT necessary to register your controllers
            
            // e.g. container.RegisterType<ITestService, TestService>();

            Bootstrapper.ConfigureIoC(container);

            container.RegisterType<RepositoryContext>(new TransientLifetimeManager());
            container.RegisterType<ApplicationDbContext>(new TransientLifetimeManager());
            container.RegisterType(typeof(ISmtpClient), typeof(SmtpClientWrapper), new TransientLifetimeManager());
            
			container.RegisterType(typeof(IPetrolPricingRepository), typeof(PetrolPricingRepository), new TransientLifetimeManager());
            container.RegisterType(typeof(IAccountRepository), typeof(AccountRepository), new TransientLifetimeManager());

            container.RegisterType<ILookupService, LookupService>();
            container.RegisterType<IEmailService, EmailService>();
            container.RegisterType<IFileService, FileService>();
            container.RegisterType<IPriceService, PriceService>();
            container.RegisterType<IReportService, ReportService>();
            container.RegisterType<ISiteService, SiteService>();
            container.RegisterType<IDiagnosticsService, DiagnosticsService>();
            container.RegisterType<IFactory, Factory>();
			container.RegisterType<IAppSettings, AppSettings>();
			container.RegisterType<IDataFileReader, DataFileReader>();
            container.RegisterType<IAccountService, AccountService>();
            container.RegisterType<IPPUserService, PPUserService>();

            config.DependencyResolver = new UnityDependencyResolver(container);
        }
    }
}