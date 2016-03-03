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
        public void When_CalcPrice_Method_Called_Then_Valid_Suggested_Price_Should_Be_Found_And_Recorded(
            int fuelTypeId, 
            float driveTimeFrom, 
            float driveTimeTo,
            int competitorId,
            int suggestedPrice,
            int markup)
        {
            //Arrange
            setupMocks(fuelTypeId, driveTimeFrom, driveTimeTo);

            PriceService sut = new PriceService(_mockRepository.Object, _mockSettingsService.Object, _mockLookupSerivce.Object);

            //Act
            sut.CalcPrice(_mockRepository.Object, _site, fuelTypeId, _calcTaskData);

            //Assert
            Assert.DoesNotThrow(delegate {
                _mockRepository
                    .Verify(h => h.AddOrUpdateSitePriceRecord(It.Is<Models.SitePrice>(
                        arg =>
                            suggestedPrice == arg.SuggestedPrice
                            && competitorId == arg.CompetitorId
                            && DateTime.Today == arg.DateOfCalc
                            && fuelTypeId == arg.FuelTypeId
                            && markup == arg.Markup
                            && false == arg.IsTrailPrice)), Times.Once());
            });
        }

        [TestCase(1, 0, 4.99f, 2, 1007)]
        [TestCase(1, 10, 14.99f, 2, 1007)]
        [TestCase(2, 10, 14.99f, 9, 988)]
        [TestCase(2, 20, 24.99f, 9, 988)]
        public void When_CalcPrice_Method_Called_Then_Valid_Trail_Price_Should_Be_Found_And_Recorded(
            int fuelTypeId,
            float driveTimeFrom,
            float driveTimeTo,
            int competitorId,
            int suggestedPrice)
        {
            //Arrange
            setupMocks(fuelTypeId, driveTimeFrom, driveTimeTo);

            _site.TrailPriceCompetitorId = competitorId;

            var competitor = DummySiteToCompetitors.First(c => c.CompetitorId == competitorId);

            _mockRepository
                .Setup(r => r.GetCompetitor(_site.Id, _site.TrailPriceCompetitorId.Value))
                .Returns(competitor);

            //GetDailyPricesForFuelByCompetitors override
            _mockRepository
                .Setup(r => r.GetDailyPricesForFuelByCompetitors(It.IsAny<IEnumerable<int>>(), fuelTypeId, DateTime.Today))
                .Returns(DummyDailyPrices.Where(dp => dp.FuelTypeId == fuelTypeId && dp.CatNo == competitor.Competitor.CatNo));

            PriceService sut = new PriceService(_mockRepository.Object, _mockSettingsService.Object, _mockLookupSerivce.Object);

            //Act
            sut.CalcPrice(_mockRepository.Object, _site, fuelTypeId, _calcTaskData);

            //Assert
            Assert.DoesNotThrow(delegate
            {
                _mockRepository
                    .Verify(h => h.AddOrUpdateSitePriceRecord(It.Is<Models.SitePrice>(
                        arg =>
                            suggestedPrice == arg.SuggestedPrice
                            && competitorId == arg.CompetitorId
                            && DateTime.Today == arg.DateOfCalc
                            && fuelTypeId == arg.FuelTypeId
                            && true == arg.IsTrailPrice)), Times.Once());
            });
        }

        [TestCase(1, 0, 4.99f)]
        [TestCase(2, 10, 14.99f)]
        public void When_CalcPrice_Method_Called_And_DailyPrice_Data_Not_Found_Then_Default_Suggested_Price_Should_Be_Recorderd(
            int fuelTypeId,
            float driveTimeFrom,
            float driveTimeTo)
        {
            //Arrange
            setupMocks(fuelTypeId, driveTimeFrom, driveTimeTo);

            //AnyDailyPricesForFuelOnDate call override
            _mockRepository
                .Setup(r => r.AnyDailyPricesForFuelOnDate(fuelTypeId, DateTime.Today, It.IsAny<int>()))
                .Returns(false);

            PriceService sut = new PriceService(_mockRepository.Object, _mockSettingsService.Object, _mockLookupSerivce.Object);

            //Act
            sut.CalcPrice(_mockRepository.Object, _site, fuelTypeId, _calcTaskData);

            //Assert
            Assert.DoesNotThrow(delegate
            {
                _mockRepository
                    .Verify(h => h.AddOrUpdateSitePriceRecord(It.Is<Models.SitePrice>(
                        arg =>
                            0 == arg.SuggestedPrice
                            && null == arg.CompetitorId
                            && DateTime.Today == arg.DateOfCalc
                            && DateTime.Today == arg.DateOfPrice
                            && fuelTypeId == arg.FuelTypeId
                            && 0 == arg.UploadId
                            && _site.Id == arg.SiteId)), Times.Once());
            });
        }

        #region Private methods
        private void setupMocks(int fuelTypeId, float driveTimeFrom, float driveTimeTo)
        {
            var testSiteToCompetitors = DummySiteToCompetitors.Where(c => c.DriveTime >= driveTimeFrom && c.DriveTime <= driveTimeTo);

            //GetCompetitors call
            _mockRepository
                .Setup(r => r.GetCompetitors(_site, driveTimeFrom, driveTimeTo, false))
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
        }
        #endregion
    }
}
