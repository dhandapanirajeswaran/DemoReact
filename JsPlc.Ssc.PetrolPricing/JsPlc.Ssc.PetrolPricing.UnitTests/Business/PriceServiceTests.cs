using JsPlc.Ssc.PetrolPricing.Business;
using JsPlc.Ssc.PetrolPricing.Repository;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace JsPlc.Ssc.PetrolPricing.UnitTests.Business
{
    [TestFixture]
    public class PriceServiceTests : TestBase 
    {
        Mock<IPetrolPricingRepository> _mockRepository;
        Mock<ISettingsService> _mockSettingsService;
        Mock<ILookupService> _mockLookupSerivce;

        Models.Site _site;
        CalcTaskData _calcTaskData;

        [SetUp]
        public void SetUp()
        {
            #region Setup Repository
            _mockRepository = new Mock<IPetrolPricingRepository>();

            //GetCompetitor call
            _mockRepository
                .Setup(r => r.GetCompetitor(It.IsAny<int>(), It.IsAny<int>()))
                .Returns(DummySiteToCompetitors.Skip(1).FirstOrDefault());
            
            #endregion

            _mockSettingsService = new Mock<ISettingsService>();
            _mockLookupSerivce = new Mock<ILookupService>();

            _calcTaskData = new CalcTaskData
            {
                ForDate = DateTime.Today,
                FileUpload = new Models.FileUpload { 
                    Id = 1
                }
            };
            
            _site = new Models.Site {
                CatNo = 1
            };
        }

        [TestCase(1, 0, 4.99f, 2, 1007, 0)]
        [TestCase(2, 10, 14.99f, 9, 1008, 2)]
        public void When_CalcPrice_Method_Called_Valid_Suggested_Price_Should_Be_Found_And_Recorded(
            int fuelTypeId, 
            float driveTimeFrom, 
            float driveTimeTo,
            int competitorId,
            int suggestedPrice,
            int markup)
        {
            //Arrange
            var testSiteToCompetitors = DummySiteToCompetitors.Where(c => c.DriveTime >= driveTimeFrom && c.DriveTime <= driveTimeTo);

            //GetCompetitors call
            _mockRepository
                .Setup(r => r.GetCompetitors(It.IsAny<Models.Site>(), driveTimeFrom, driveTimeTo, false))
                .Returns(testSiteToCompetitors);
            
            //AnyDailyPricesForFuelOnDate call
            _mockRepository
                .Setup(r => r.AnyDailyPricesForFuelOnDate(fuelTypeId, DateTime.Today, It.IsAny<int>()))
                .Returns(true);

            //GetDailyPricesForFuelByCompetitors call
            var testCatNos = testSiteToCompetitors.Select(c => c.Competitor.CatNo).ToArray();
            _mockRepository
                .Setup(r => r.GetDailyPricesForFuelByCompetitors(It.IsAny<IEnumerable<int>>(), fuelTypeId, DateTime.Today))
                .Returns(DummyDailyPrices.Where(dp => dp.FuelTypeId == fuelTypeId && testCatNos.Contains(dp.CatNo)));

            var desiredSitePrice = new Models.SitePrice { 
                DateOfCalc = DateTime.Today,
                FuelTypeId = fuelTypeId,
                CompetitorId = competitorId,
                SuggestedPrice = suggestedPrice,
                Markup = markup
            };

            PriceService sut = new PriceService(_mockRepository.Object, _mockSettingsService.Object, _mockLookupSerivce.Object);

            //Act
            sut.CalcPrice(_mockRepository.Object, _site, fuelTypeId, _calcTaskData);

            //Assert
            _mockRepository
                .Verify(h => h.AddOrUpdateSitePriceRecord(It.Is<Models.SitePrice>(
                    arg => 
                        desiredSitePrice.SuggestedPrice == arg.SuggestedPrice
                        && desiredSitePrice.CompetitorId == arg.CompetitorId
                        && desiredSitePrice.DateOfCalc == DateTime.Today
                        && desiredSitePrice.FuelTypeId == fuelTypeId)), Times.Once());
        }

        
    }
}
