using JsPlc.Ssc.PetrolPricing.Business;
using JsPlc.Ssc.PetrolPricing.Repository;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JsPlc.Ssc.PetrolPricing.UnitTests.Business
{
	[TestFixture]
	public class EmailServiceTests : TestBase
	{
		public enum BuildEmailBodyTestCases
		{
			NoPreviousTradeDatePriceFound,
			FuelTypeTradeChange,
			PriceDifferenceChange
		}

		Mock<IPetrolPricingRepository> _mockRepository;
		Mock<ISettingsService> _mockSettingsService;

		Models.Site _site;

		[SetUp]
		public void SetUp()
		{
			#region Setup Repository

			_mockRepository = new Mock<IPetrolPricingRepository>();

			#endregion

			_mockSettingsService = new Mock<ISettingsService>();

			_site = new Models.Site
			{
				CatNo = 1,
				IsActive = true,
				IsSainsburysSite = true,
				Id = 1,
				SiteName = "Test site"
			};
		}

		[TestCase(BuildEmailBodyTestCases.NoPreviousTradeDatePriceFound)]
		public void When_BuildEmailBody_MethodCalled_And_There_Is_No_Prices_For_The_Previous_Trade_Day_Then_Email_Body_Should_Be_Returned
		(
			BuildEmailBodyTestCases testCase
		)
		{
			
			//Arrange
			#region Arrange
			if (testCase == BuildEmailBodyTestCases.NoPreviousTradeDatePriceFound)
			{
				//test when there is no price for the previous trade day
				_site.Prices = new List<Models.SitePrice> { 
					new Models.SitePrice {
						DateOfCalc = DateTime.Today,
						FuelTypeId = 1,
						SuggestedPrice = 1000,
						OverriddenPrice = 1001
					}
				};
			} else if (testCase == BuildEmailBodyTestCases.FuelTypeTradeChange)
			{
				//test when there is no price for the previous trade day
				_site.Prices = new List<Models.SitePrice> { 
					new Models.SitePrice {
						DateOfCalc = DateTime.Today,
						FuelTypeId = 1,
						SuggestedPrice = 1000,
						OverriddenPrice = 1001
					}
				};
			}
			#endregion

			//Act
			var result = EmailService.BuildEmailBody(_site, DateTime.Today);

			//Assert
			//email body is not empty
			Assert.IsNotEmpty(result);

			//overriden price appeard in email's body
			Assert.IsTrue(result.Contains("100.1"));
		}
	}
}
