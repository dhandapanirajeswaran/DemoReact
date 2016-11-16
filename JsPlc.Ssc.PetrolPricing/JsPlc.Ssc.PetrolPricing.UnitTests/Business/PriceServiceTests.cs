using JsPlc.Ssc.PetrolPricing.Business;
using JsPlc.Ssc.PetrolPricing.Repository;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using JsPlc.Ssc.PetrolPricing.Core.Interfaces;

namespace JsPlc.Ssc.PetrolPricing.UnitTests.Business
{
    [TestFixture]
    public class PriceServiceTests : TestBase
    {
        Mock<IPetrolPricingRepository> _mockRepository;
        Mock<IAppSettings> _mockAppSettings;
        Mock<ILookupService> _mockLookupSerivce;
		Mock<IFactory> _mockFactory;
        Mock<ILogger> _mockLogger;

        Models.Site _site;
        PriceCalculationTaskData _calcTaskData;

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
			
			_mockFactory = new Mock<IFactory>();
			_mockFactory
				.Setup(f => f.Create<IPetrolPricingRepository>(CreationMethod.ServiceLocator, null))
				.Returns(_mockRepository.Object);
            _mockLogger = new Mock<ILogger>();
            
            _mockAppSettings = new Mock<IAppSettings>();
            _mockLookupSerivce = new Mock<ILookupService>();
			
            _calcTaskData = new PriceCalculationTaskData
            {
                ForDate = DateTime.Today,
                FileUpload = new Models.FileUpload
                {
                    Id = 1
                }
            };

            _site = new Models.Site
            {
                CatNo = 1,
                IsActive = true,
                IsSainsburysSite = true,
                Id = 1
            };
        }

        #region CalcPrice Tests

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

            PriceService sut = new PriceService(_mockRepository.Object, _mockAppSettings.Object, _mockLookupSerivce.Object, _mockFactory.Object);

            //Act
            sut.CalcPrice(_mockRepository.Object, _site, fuelTypeId, _calcTaskData);

            //Assert
            /*Assert.DoesNotThrow(delegate
            {
                _mockRepository
                    .Verify(h => h.AddOrUpdateSitePriceRecord(It.Is<Models.SitePrice>(
                        arg =>
                            suggestedPrice == arg.SuggestedPrice
                            && competitorId == arg.CompetitorId
                            && DateTime.Today == arg.DateOfCalc
                            && fuelTypeId == arg.FuelTypeId
                            && markup == arg.Markup
                            && false == arg.IsTrailPrice)), Times.Once());
            });*/
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
			#region Arrange
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

            PriceService sut = new PriceService(_mockRepository.Object, _mockAppSettings.Object, _mockLookupSerivce.Object, _mockFactory.Object);
			#endregion

			//Act
            sut.CalcPrice(_mockRepository.Object, _site, fuelTypeId, _calcTaskData);

            //Assert
            /*Assert.DoesNotThrow(delegate
            {
                _mockRepository
                    .Verify(h => h.AddOrUpdateSitePriceRecord(It.Is<Models.SitePrice>(
                        arg =>
                            suggestedPrice == arg.SuggestedPrice
                            && competitorId == arg.CompetitorId
                            && DateTime.Today == arg.DateOfCalc
                            && fuelTypeId == arg.FuelTypeId
                            && true == arg.IsTrailPrice)), Times.Once());
            });*/
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

            PriceService sut = new PriceService(_mockRepository.Object, _mockAppSettings.Object, _mockLookupSerivce.Object, _mockFactory.Object);

            //Act
            sut.CalcPrice(_mockRepository.Object, _site, fuelTypeId, _calcTaskData);

            //Assert
            /*Assert.DoesNotThrow(delegate
            {
                _mockRepository
                    .Verify(v => v.AddOrUpdateSitePriceRecord(It.Is<Models.SitePrice>(
                        arg =>
                            0 == arg.SuggestedPrice
                            && null == arg.CompetitorId
                            && DateTime.Today == arg.DateOfCalc
                            && DateTime.Today == arg.DateOfPrice
                            && fuelTypeId == arg.FuelTypeId
                            && 0 == arg.UploadId
                            && _site.Id == arg.SiteId)), Times.Once());
            });*/
        }

        #endregion

        #region DoCalcDailyPrices Tests

        [Test]
        public void When_DoCalcDailyPrices_Method_Called_Then_Valid_Prices_Should_Be_Calculated()
        {
			//Arrange
			#region Arrange
			var testFileUpload = new Models.FileUpload { Id = 1 };
            //GetDailyFileAvailableForCalc call
            _mockRepository
                .Setup(r => r.GetDailyFileAvailableForCalc(DateTime.Today))
                .Returns(testFileUpload);

            //AnyDailyPricesForFuelOnDate call
            _mockRepository
                .Setup(r => r.AnyDailyPricesForFuelOnDate(It.IsAny<int>(), DateTime.Today, testFileUpload.Id))
                .Returns(true);

            //GetDailyFileWithCalcRunningForDate call
            _mockRepository
                .Setup(r => r.GetDailyFileWithCalcRunningForDate(DateTime.Today))
                .Returns((Models.FileUpload)null);

            //GetJsSites call
            _mockRepository
                .Setup(r => r.GetJsSites())
                .Returns(new List<Models.Site> { _site });

            //GetFuelTypes call
            _mockLookupSerivce
                .Setup(r => r.GetFuelTypes())
                .Returns(DummyFuelTypes);

            PriceService sut = new PriceService(_mockRepository.Object, _mockAppSettings.Object, _mockLookupSerivce.Object, _mockFactory.Object);
			#endregion

			//Act
            sut.DoCalcDailyPrices(DateTime.Today);

            //Assert
            //Begin calculation 11
            Assert.DoesNotThrow(delegate
            {
                _mockRepository
                    .Verify(v => v.UpdateImportProcessStatus(11, It.Is<Models.FileUpload>(
                        arg =>
                            1 == arg.Id)), Times.Once());
            });

            //Success calculation 10
            Assert.DoesNotThrow(delegate
            {
                _mockRepository
                    .Verify(v => v.UpdateImportProcessStatus(10, It.Is<Models.FileUpload>(
                        arg =>
                            1 == arg.Id)), Times.Once());
            });

			//Super unleaded price should be created
			//can't test superunleaded price calculation as it's done in stored procedure
			//test call to CreateMissingSuperUnleadedFromUnleaded
        }

        #endregion

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
