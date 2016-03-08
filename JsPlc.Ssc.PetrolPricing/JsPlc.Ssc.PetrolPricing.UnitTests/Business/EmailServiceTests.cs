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
		[TestCase(BuildEmailBodyTestCases.FuelTypeTradeChange)]
		[TestCase(BuildEmailBodyTestCases.PriceDifferenceChange)]
		public void When_BuildEmailBody_MethodCalled_And_All_Requirements_Met_Then_Valid_Email_Body_Should_Be_Returned
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
			}
			else if (testCase == BuildEmailBodyTestCases.FuelTypeTradeChange)
			{
				//test when fuels has been added/removed
				_site.Prices = new List<Models.SitePrice> { 
					new Models.SitePrice {
						DateOfCalc = DateTime.Today,
						FuelTypeId = 2,
						SuggestedPrice = 1004,
						OverriddenPrice = 1005
					},
					new Models.SitePrice {
						DateOfCalc = DateTime.Today,
						FuelTypeId = 1,
						SuggestedPrice = 1000,
						OverriddenPrice = 1001
					},
					new Models.SitePrice {
						DateOfCalc = DateTime.Today.AddDays(-1),
						FuelTypeId = 1,
						SuggestedPrice = 1002,
						OverriddenPrice = 1003
					}
				};
			}
			else if (testCase == BuildEmailBodyTestCases.PriceDifferenceChange)
			{
				//test when fuel price has changed
				_site.Prices = new List<Models.SitePrice> { 
					new Models.SitePrice {
						DateOfCalc = DateTime.Today,
						FuelTypeId = 1,
						SuggestedPrice = 1000,
						OverriddenPrice = 1001
					},
					new Models.SitePrice {
						DateOfCalc = DateTime.Today.AddDays(-1),
						FuelTypeId = 1,
						SuggestedPrice = 1002,
						OverriddenPrice = 1003
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

		[TestCase(BuildEmailBodyTestCases.NoPreviousTradeDatePriceFound)]
		[TestCase(BuildEmailBodyTestCases.FuelTypeTradeChange)]
		[TestCase(BuildEmailBodyTestCases.PriceDifferenceChange)]
		public void When_BuildEmailBody_MethodCalled_And_Some_Requirements_Are_NOT_Met_Then_EMPTY_Email_Body_Should_Be_Returned
		(
			BuildEmailBodyTestCases testCase
		)
		{
			//Arrange
			#region Arrange
			if (testCase == BuildEmailBodyTestCases.NoPreviousTradeDatePriceFound)
			{
				//test when there is price for the previous trade day and it's not changed
				_site.Prices = new List<Models.SitePrice> { 
					new Models.SitePrice {
						DateOfCalc = DateTime.Today,
						FuelTypeId = 1,
						SuggestedPrice = 1000,
						OverriddenPrice = 1001
					}, 
					new Models.SitePrice {
						DateOfCalc = DateTime.Today.AddDays(-1),
						FuelTypeId = 1,
						SuggestedPrice = 1000,
						OverriddenPrice = 1001
					}
				};
			}
			else if (testCase == BuildEmailBodyTestCases.FuelTypeTradeChange)
			{
				//test when there is no price for the previous trade day
				_site.Prices = new List<Models.SitePrice> { 
					new Models.SitePrice {
						DateOfCalc = DateTime.Today,
						FuelTypeId = 2,
						SuggestedPrice = 1004,
						OverriddenPrice = 1005
					},
					new Models.SitePrice {
						DateOfCalc = DateTime.Today.AddDays(-1),
						FuelTypeId = 2,
						SuggestedPrice = 1004,
						OverriddenPrice = 1005
					},
					new Models.SitePrice {
						DateOfCalc = DateTime.Today,
						FuelTypeId = 1,
						SuggestedPrice = 1002,
						OverriddenPrice = 1003
					},
					new Models.SitePrice {
						DateOfCalc = DateTime.Today.AddDays(-1),
						FuelTypeId = 1,
						SuggestedPrice = 1002,
						OverriddenPrice = 1003
					}
				};
			}
			else if (testCase == BuildEmailBodyTestCases.PriceDifferenceChange)
			{
				//test when there is no price for the previous trade day
				_site.Prices = new List<Models.SitePrice> { 
					new Models.SitePrice {
						DateOfCalc = DateTime.Today,
						FuelTypeId = 1,
						SuggestedPrice = 1002,
						OverriddenPrice = 1003
					},
					new Models.SitePrice {
						DateOfCalc = DateTime.Today.AddDays(-1),
						FuelTypeId = 1,
						SuggestedPrice = 1002,
						OverriddenPrice = 1003
					}
				};
			}
			#endregion

			//Act
			var result = EmailService.BuildEmailBody(_site, DateTime.Today);

			//Assert
			//email body is not empty
			Assert.IsEmpty(result);
		}
	}
}
