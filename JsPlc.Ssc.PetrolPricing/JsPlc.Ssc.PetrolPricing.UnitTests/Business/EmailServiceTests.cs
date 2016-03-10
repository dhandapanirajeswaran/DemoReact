using JsPlc.Ssc.PetrolPricing.Business;
using JsPlc.Ssc.PetrolPricing.Core;
using JsPlc.Ssc.PetrolPricing.Repository;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
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
		Mock<IFactory> _mockFactory;

		Mock<ISmtpClient> _mockSmtpClient;

		Models.Site _site;

		const string _expectedEmailAddress = "andrey.shihov@sainsburys.co.uk";

		[SetUp]
		public void SetUp()
		{
			#region Setup Repository

			_mockRepository = new Mock<IPetrolPricingRepository>();

			#endregion

			_mockSettingsService = new Mock<ISettingsService>();
			_mockSettingsService.Setup(ss => ss.EmailSubject()).Returns("Test Email Subject");
			_mockSettingsService.Setup(ss => ss.EmailFrom()).Returns(_expectedEmailAddress);
			_mockSettingsService.Setup(ss => ss.FixedEmailTo()).Returns(_expectedEmailAddress);
			_mockSettingsService.Setup(ss => ss.MailHostSelector()).Returns("localhost");

			_mockSmtpClient = new Mock<ISmtpClient>();

			_mockFactory = new Mock<IFactory>();
			_mockFactory
				.Setup(f => f.Create<ISmtpClient>(CreationMethod.ServiceLocator, null))
				.Returns(_mockSmtpClient.Object);


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
		public void When_BuildEmailBody_Method_Called_And_All_Requirements_Met_Then_Valid_Email_Body_Should_Be_Returned
		(
			BuildEmailBodyTestCases testCase
		)
		{

			//Arrange
			#region Arrange
			populateSitePricesWithChanges(testCase);
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
		public void When_BuildEmailBody_Method_Called_And_Some_Requirements_Are_NOT_Met_Then_EMPTY_Email_Body_Should_Be_Returned
		(
			BuildEmailBodyTestCases testCase
		)
		{
			//Arrange
			#region Arrange
			populateSitePricesWithoutChanges(testCase);
			#endregion

			//Act
			var result = EmailService.BuildEmailBody(_site, DateTime.Today);

			//Assert
			//email body is not empty
			Assert.IsEmpty(result);
		}

		[TestCase(BuildEmailBodyTestCases.NoPreviousTradeDatePriceFound)]
		[TestCase(BuildEmailBodyTestCases.FuelTypeTradeChange)]
		[TestCase(BuildEmailBodyTestCases.PriceDifferenceChange)]
		public void When_SendEmailAsync_Method_Called_And_All_Requirements_Are_Met_Then_Email_Should_Be_Sent
		(
			BuildEmailBodyTestCases testCase
		)
		{
			//Arrange
			#region Arrange
			populateSitePricesWithChanges(testCase);

			_site.Emails = new List<Models.SiteEmail>
			{
				new Models.SiteEmail {
					EmailAddress = _expectedEmailAddress
				}
			};

			var expectedEmailBody = EmailService.BuildEmailBody(_site, DateTime.Today);

			List<Models.Site> sites = new List<Models.Site> { _site };

			var sut = new EmailService(_mockRepository.Object, _mockSettingsService.Object, _mockFactory.Object);

			#endregion
			//Act
			var result = sut.SendEmailAsync(sites, DateTime.Today, _expectedEmailAddress).Result;

			//Assert
			Assert.IsTrue(result.Count == 1);
			Assert.IsTrue(result.First().Value.IsSuccess);
			Assert.DoesNotThrow(delegate
			{
				_mockSmtpClient
					.Verify(v => v.Send(It.Is<MailMessage>(arg =>
						arg.Body == expectedEmailBody
						&& arg.To.First().Address == _expectedEmailAddress)), Times.Once());
			});
		}

		[TestCase(BuildEmailBodyTestCases.NoPreviousTradeDatePriceFound)]
		[TestCase(BuildEmailBodyTestCases.FuelTypeTradeChange)]
		[TestCase(BuildEmailBodyTestCases.PriceDifferenceChange)]
		public void When_SendEmailAsync_Method_Called_And_There_Is_No_Price_Change_Then_Email_Should_NOT_Be_Sent
		(
			BuildEmailBodyTestCases testCase
		)
		{
			//Arrange
			#region Arrange

			//test when there is not change in price - email should NOT be sent
			populateSitePricesWithoutChanges(testCase);

			_site.Emails = new List<Models.SiteEmail>
			{
				new Models.SiteEmail {
					EmailAddress = _expectedEmailAddress
				}
			};

			var expectedEmailBody = EmailService.BuildEmailBody(_site, DateTime.Today);

			List<Models.Site> sites = new List<Models.Site> { _site };

			var sut = new EmailService(_mockRepository.Object, _mockSettingsService.Object, _mockFactory.Object);

			#endregion
			//Act
			var result = sut.SendEmailAsync(sites, DateTime.Today, _expectedEmailAddress).Result;

			//Assert
			Assert.IsTrue(result.Count == 1);
			Assert.IsFalse(result.First().Value.IsSuccess);
			Assert.DoesNotThrow(delegate
			{
				_mockSmtpClient
					.Verify(v => v.Send(It.Is<MailMessage>(arg =>
						arg.Body == expectedEmailBody
						&& arg.To.First().Address == _expectedEmailAddress)), Times.Never());
			});
		}

		#region Private Methods
		private void populateSitePricesWithoutChanges(BuildEmailBodyTestCases testCase)
		{
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
				//test when there are same fuel types sold for two trade dates and price is not changed
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
				//test when there is no price change for two trade dates
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
		}

		private void populateSitePricesWithChanges(BuildEmailBodyTestCases testCase)
		{
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
		}
		#endregion
	}
}
