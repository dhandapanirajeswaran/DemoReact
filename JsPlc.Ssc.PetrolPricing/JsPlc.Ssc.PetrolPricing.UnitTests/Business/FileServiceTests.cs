using JsPlc.Ssc.PetrolPricing.Business;
using JsPlc.Ssc.PetrolPricing.Core;
using JsPlc.Ssc.PetrolPricing.Repository;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;

namespace JsPlc.Ssc.PetrolPricing.UnitTests.Business
{
	[TestFixture]
	public class FileServiceTests : TestBase
	{
		Mock<IPetrolPricingRepository> _mockRepository;
		Mock<ISettingsService> _mockSettingsService;
		Mock<IPriceService> _mockPriceService;

		[SetUp]
		public void SetUp()
		{
			_mockRepository = new Mock<IPetrolPricingRepository>();

			_mockSettingsService = new Mock<ISettingsService>();
			_mockSettingsService.Setup(s => s.GetUploadPath()).Returns(TestFileFolderPath);

			_mockPriceService = new Mock<IPriceService>();
		}

		[Test]
		public void When_ProcessDailyPrice_Method_Called_Then_Valid_DailyPrice_Item_SHould_Be_Recorded()
		{
			//Arrange
			var testFileToUpload = DummyFileUploads.First(fu => fu.UploadTypeId == (int)UploadTypes.DailyPriceData);

			int numberOfLinesInTestFile = getNumberOfLinesInTestFile(testFileToUpload);

			_mockRepository
				.Setup(r => 
					r.NewDailyPrices(
					It.IsAny<List<Models.DailyPrice>>(), 
					It.Is<Models.FileUpload>(arg => arg.Id == testFileToUpload.Id
					&& arg.OriginalFileName == testFileToUpload.OriginalFileName
					&& arg.UploadTypeId == testFileToUpload.UploadTypeId), 
					It.IsAny<int>()))
				.Returns(true);

			var sut = new FileService(_mockRepository.Object, _mockPriceService.Object, _mockSettingsService.Object);

			//Act
			sut.ProcessDailyPrice(DummyFileUploads.Where(fu => fu.UploadTypeId == (int)UploadTypes.DailyPriceData).ToList());

			//Assert
			//verify import process status changed to 5 - Processing
			Assert.DoesNotThrow(delegate
			{
				_mockRepository
					.Verify(v => v.UpdateImportProcessStatus(5, It.Is<Models.FileUpload>(arg => arg.Id == testFileToUpload.Id)), Times.Once());
			});

			//verify FIRST batch has been uploaded
			Assert.DoesNotThrow(delegate
			{
				_mockRepository
					.Verify(v => v.NewDailyPrices(
						It.IsAny<List<Models.DailyPrice>>(),
						It.Is<Models.FileUpload>(arg => arg.Id == testFileToUpload.Id),
						0
						), Times.Once());
			});
			
			//verify LAST batch has been uploaded
			Assert.DoesNotThrow(delegate
			{
				_mockRepository
					.Verify(v => v.NewDailyPrices(
						It.IsAny<List<Models.DailyPrice>>(), 
						It.Is<Models.FileUpload>(arg => arg.Id == testFileToUpload.Id),
						(numberOfLinesInTestFile / 1000) * 1000
						), Times.Once());
			});

			//verify import process status changed to 10 - Success
			Assert.DoesNotThrow(delegate
			{
				_mockRepository
					.Verify(v => v.UpdateImportProcessStatus(10, It.Is<Models.FileUpload>(arg => arg.Id == testFileToUpload.Id)), Times.Once());
			});

			//verify deletion of today's daily prices upload attempts
			Assert.DoesNotThrow(delegate
			{
				_mockRepository
					.Verify(v => v.DeleteRecordsForOlderImportsOfDate(DateTime.Today, testFileToUpload.Id), Times.Once());
			});
		}

		#region private Methods

		private int getNumberOfLinesInTestFile(Models.FileUpload testFileToUpload)
		{
			int linesNumber = 0;

			using (var file = new StreamReader(Path.Combine(TestFileFolderPath, testFileToUpload.StoredFileName).ToString(CultureInfo.InvariantCulture)))
			{
				while (file.Peek() >= 0)
				{
					linesNumber++;

					string line = file.ReadLine();
				}
			}

			return linesNumber;
		}
		#endregion
	}
}
