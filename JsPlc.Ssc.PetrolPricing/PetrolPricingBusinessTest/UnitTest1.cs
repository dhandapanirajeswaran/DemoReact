using System;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using JsPlc.Ssc.PetrolPricing.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using JsPlc.Ssc.PetrolPricing.Business;
using JsPlc.Ssc.PetrolPricing.Repository;


namespace PetrolPricingBusinessTest
{
    [TestClass]
    public class UnitTest1
    {
        private const string dbConnString =
            "Data Source=.;Initial Catalog=PetrolPricingRepository;Integrated Security=True;MultipleActiveResultSets=true";

        private SiteService _siteService;

        [TestInitialize]
        public void TestInit()
        {
            DbConnection dbConnection = SqlClientFactory.Instance.CreateConnection();
            dbConnection.ConnectionString = dbConnString;

            var repositoryContext = new RepositoryContext(dbConnection);

            _siteService = new SiteService(new PetrolPricingRepository(repositoryContext));
        }

        [TestMethod]
        public void GetSiteWithCompetitors()
        {
            var sites = _siteService.GetCompetitors(1, 0, 5, true);
            sites = _siteService.GetCompetitors(2, 5, 10, true);
        }

        [TestCleanup]
        public void Cleanup()
        {
            _siteService.Dispose();
            _siteService = null;
        }
    }
}
