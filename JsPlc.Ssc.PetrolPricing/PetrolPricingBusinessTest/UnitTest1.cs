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
            "Data Source=(local)\\SQL2014;Database=PetrolPricingRepository;User ID=sa;Password=Password12!;MultipleActiveResultSets=true";

        private SiteService _siteService;
        private RepositoryContext _context;
        private PetrolPricingRepository _repository;

        [TestInitialize]
        public void TestInit()
        {
            DbConnection dbConnection = SqlClientFactory.Instance.CreateConnection();
            dbConnection.ConnectionString = dbConnString;

            _context = new RepositoryContext(dbConnection);
            _repository = new PetrolPricingRepository(_context);

            _siteService = new SiteService();
        }

        [TestMethod]
        public void GetSitesPricesInDateRange()
        {
            var siteWithPricesInDateRange = _repository.GetSitesWithEmailsAndPrices(DateTime.Now, DateTime.Now);
        }

        [TestCleanup]
        public void Cleanup()
        {
            _siteService.Dispose();
            _siteService = null;
        }
    }
}
