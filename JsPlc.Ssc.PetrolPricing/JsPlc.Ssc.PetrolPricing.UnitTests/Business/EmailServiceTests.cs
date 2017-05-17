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
using JsPlc.Ssc.PetrolPricing.Core.Interfaces;
using JsPlc.Ssc.PetrolPricing.Models.ViewModels;

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
		Mock<IAppSettings> _mockAppSettings;
		Mock<IFactory> _mockFactory;
        Mock<ISystemSettingsService> _mockSystemSettings;

        Mock<ISmtpClient> _mockSmtpClient;

	
        SitePriceViewModel _siteVM;

		const string _expectedEmailAddress = "andrey.shihov@sainsburys.co.uk";

		[SetUp]
		public void SetUp()
		{
			#region Setup Repository

			_mockRepository = new Mock<IPetrolPricingRepository>();

			#endregion

			_mockAppSettings = new Mock<IAppSettings>();
			_mockAppSettings.Setup(ss => ss.EmailSubject).Returns("Test Email Subject");
			_mockAppSettings.Setup(ss => ss.EmailFrom).Returns(_expectedEmailAddress);
			_mockAppSettings.Setup(ss => ss.FixedEmailTo).Returns(_expectedEmailAddress);
			_mockAppSettings.Setup(ss => ss.MailHostSelector).Returns("localhost");

			_mockSmtpClient = new Mock<ISmtpClient>();

			_mockFactory = new Mock<IFactory>();
			_mockFactory
				.Setup(f => f.Create<ISmtpClient>(CreationMethod.ServiceLocator, null))
				.Returns(_mockSmtpClient.Object);

            _mockSystemSettings = new Mock<ISystemSettingsService>();
		
            _siteVM = new SitePriceViewModel
            {
                CatNo = 1,                                 
                SiteId = 1,
                StoreName = "Test site"
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
            var sut = new EmailService(_mockRepository.Object, _mockAppSettings.Object, _mockFactory.Object, _mockSystemSettings.Object);

            var result = sut.BuildEmailBody(_siteVM, DateTime.Today);

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
			var siteNewVM = new SitePriceViewModel
            {
                CatNo = 1,
                SiteId = 1,
                StoreName = "Test site"
            };
            #endregion

            //Act
            var sut = new EmailService(_mockRepository.Object, _mockAppSettings.Object, _mockFactory.Object, _mockSystemSettings.Object);
            var result = sut.BuildEmailBody(siteNewVM, DateTime.Today);

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

			_siteVM.Emails = new List<String>
			{
				 _expectedEmailAddress
				
			};

            var sut = new EmailService(_mockRepository.Object, _mockAppSettings.Object, _mockFactory.Object, _mockSystemSettings.Object);

            var expectedEmailBody = sut.BuildEmailBody(_siteVM, DateTime.Today);

            List<SitePriceViewModel> sites = new List<SitePriceViewModel> { _siteVM };

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

            _siteVM.Emails = new List<String>
			{
				 _expectedEmailAddress
				
			};

            var siteNewVM = _siteVM;
            foreach(var fuelprice in siteNewVM.FuelPrices)
            {
                fuelprice.OverridePrice = 0;
                fuelprice.TodayPrice = fuelprice.AutoPrice;
            }

            var sut = new EmailService(_mockRepository.Object, _mockAppSettings.Object, _mockFactory.Object, _mockSystemSettings.Object);
            var expectedEmailBody = sut.BuildEmailBody(siteNewVM, DateTime.Today);

            List<SitePriceViewModel> sites = new List<SitePriceViewModel> { siteNewVM };

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
                _siteVM.FuelPrices = new List<FuelPriceViewModel> { 
					new FuelPriceViewModel {
						FuelTypeId = 1,
						AutoPrice = 1000,
						OverridePrice = 1001
					}, 
					new FuelPriceViewModel {
						FuelTypeId = 1,
						AutoPrice = 1000,
						OverridePrice = 1001
					}
				};
			}
			else if (testCase == BuildEmailBodyTestCases.FuelTypeTradeChange)
			{
				//test when there are same fuel types sold for two trade dates and price is not changed
                _siteVM.FuelPrices = new List<FuelPriceViewModel> { 
					new FuelPriceViewModel {
						FuelTypeId = 2,
						AutoPrice = 1004,
						OverridePrice = 1005
					},
					new FuelPriceViewModel {
						FuelTypeId = 2,
						AutoPrice = 1004,
						OverridePrice = 1005
					},
					new FuelPriceViewModel {
						FuelTypeId = 1,
						AutoPrice = 1002,
						OverridePrice = 1003
					},
					new FuelPriceViewModel {
						FuelTypeId = 1,
						AutoPrice = 1002,
						OverridePrice = 1003
					}
				};
			}
			else if (testCase == BuildEmailBodyTestCases.PriceDifferenceChange)
			{
				//test when there is no price change for two trade dates
                _siteVM.FuelPrices = new List<FuelPriceViewModel> { 
					new FuelPriceViewModel {					
                        FuelTypeId = 1,
						AutoPrice = 1002,
						OverridePrice = 1003
					},
					new FuelPriceViewModel {
						FuelTypeId = 1,
						AutoPrice = 1002,
						OverridePrice = 1003
					}
				};
			}
		}

		private void populateSitePricesWithChanges(BuildEmailBodyTestCases testCase)
		{
			if (testCase == BuildEmailBodyTestCases.NoPreviousTradeDatePriceFound)
			{
				//test when there is no price for the previous trade day
                _siteVM.FuelPrices = new List<FuelPriceViewModel> { 
					new FuelPriceViewModel {					
						FuelTypeId = 1,
						AutoPrice = 1000,
						OverridePrice = 1001
					}
				};
			}
			else if (testCase == BuildEmailBodyTestCases.FuelTypeTradeChange)
			{
				//test when fuels has been added/removed
                _siteVM.FuelPrices = new List<FuelPriceViewModel> { 
					new FuelPriceViewModel {
						FuelTypeId = 2,
						AutoPrice = 1004,
						OverridePrice = 1005
					},
					new FuelPriceViewModel {
						FuelTypeId = 6,
						AutoPrice = 1000,
						OverridePrice = 1001
					},
					new FuelPriceViewModel {
						FuelTypeId = 1,
						AutoPrice = 1002,
						OverridePrice = 1003
					}
				};
			}
			else if (testCase == BuildEmailBodyTestCases.PriceDifferenceChange)
			{
				//test when fuel price has changed
                _siteVM.FuelPrices = new List<FuelPriceViewModel> { 
					new FuelPriceViewModel {
						FuelTypeId = 1,
						AutoPrice = 1000,
						OverridePrice = 1001
					},
					new FuelPriceViewModel {
						FuelTypeId = 2,
						AutoPrice = 1002,
						OverridePrice = 1003
					}
				};
			}
		}
		#endregion
	}
}
