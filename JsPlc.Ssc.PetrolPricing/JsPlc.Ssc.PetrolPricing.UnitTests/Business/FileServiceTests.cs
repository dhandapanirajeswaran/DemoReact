using JsPlc.Ssc.PetrolPricing.Business;
using JsPlc.Ssc.PetrolPricing.Core;
using JsPlc.Ssc.PetrolPricing.Models;
using JsPlc.Ssc.PetrolPricing.Repository;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Data;
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
		Mock<IDataFileReader> _mockDataFileReader;

		[SetUp]
		public void SetUp()
		{
			_mockRepository = new Mock<IPetrolPricingRepository>();

			_mockSettingsService = new Mock<ISettingsService>();
			_mockSettingsService.Setup(s => s.GetUploadPath()).Returns(TestFileFolderPath);
			_mockSettingsService.Setup(s => s.ExcelFileSheetName()).Returns(QuarterlyFileDataSheetName);

			_mockPriceService = new Mock<IPriceService>();

			_mockDataFileReader = new Mock<IDataFileReader>();
		}

		[Test]
		public void When_ProcessDailyPrice_Method_Called_Then_Valid_DailyPrice_Items_Should_Be_Recorded()
		{
			//Arrange
			#region Arrange
			var testFileToUpload = DummyFileUploads.First(fu => fu.UploadTypeId == (int)UploadTypes.DailyPriceData);

			int numberOfLinesInTestFile = getNumberOfLinesInDailyTestFile(testFileToUpload);

			_mockRepository
				.Setup(r =>
					r.NewDailyPrices(
					It.IsAny<List<Models.DailyPrice>>(),
					It.Is<Models.FileUpload>(arg => ComparePrimaryFileUploadAttributes(testFileToUpload, arg)),
					It.IsAny<int>()))
				.Returns(true);

			var sut = new FileService(_mockRepository.Object, _mockPriceService.Object, _mockSettingsService.Object, _mockDataFileReader.Object);
			#endregion

			//Act
			var result = sut.ProcessDailyPrice(DummyFileUploads.Where(fu => fu.UploadTypeId == (int)UploadTypes.DailyPriceData).ToList());

			//Assert
			#region Assert
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
						It.Is<Models.FileUpload>(arg => ComparePrimaryFileUploadAttributes(testFileToUpload, arg)),
						0
						), Times.Once());
			});

			//verify LAST batch has been uploaded
			Assert.DoesNotThrow(delegate
			{
				_mockRepository
					.Verify(v => v.NewDailyPrices(
						It.IsAny<List<Models.DailyPrice>>(),
						It.Is<Models.FileUpload>(arg => ComparePrimaryFileUploadAttributes(testFileToUpload, arg)),
						(numberOfLinesInTestFile / Constants.DailyFileRowsBatchSize) * Constants.DailyFileRowsBatchSize
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

			testFileToUpload.StatusId = 10;
			AssertExtensions.PropertyValuesAreEquals(result, testFileToUpload);
			#endregion
		}

		[Test]
		public void When_ProcessDailyPrice_Method_Called_And_Exception_Occured_Then_Error_Should_Be_Recorded_And_Status_Updated_To_Failed()
		{
			//Arrange
			#region Arrange
			var testFileToUpload = DummyFileUploads.First(fu => fu.UploadTypeId == (int)UploadTypes.DailyPriceData);

			int numberOfLinesInTestFile = getNumberOfLinesInDailyTestFile(testFileToUpload);

			_mockRepository
				.Setup(r =>
					r.NewDailyPrices(
					It.IsAny<List<Models.DailyPrice>>(),
					It.Is<Models.FileUpload>(arg => ComparePrimaryFileUploadAttributes(testFileToUpload, arg)),
					It.IsAny<int>()))
				.Throws(new ApplicationException());

			var sut = new FileService(_mockRepository.Object, _mockPriceService.Object, _mockSettingsService.Object, _mockDataFileReader.Object);
			#endregion

			//Act
			sut.ProcessDailyPrice(DummyFileUploads.Where(fu => fu.UploadTypeId == (int)UploadTypes.DailyPriceData).ToList());

			//Assert
			#region Assert
			//verify LogImportError call
			Assert.DoesNotThrow(delegate
			{
				_mockRepository
					.Verify(v => v.LogImportError(
						It.Is<Models.FileUpload>(arg =>
						ComparePrimaryFileUploadAttributes(testFileToUpload, arg)), It.IsAny<string>(), It.IsAny<int?>()), Times.Once());
			});


			//verify import process status changed to 15 - Failed
			Assert.DoesNotThrow(delegate
			{
				_mockRepository
					.Verify(v => v.UpdateImportProcessStatus(15, It.Is<Models.FileUpload>(arg =>
						ComparePrimaryFileUploadAttributes(testFileToUpload, arg))), Times.Once());
			});
			#endregion
		}

#if !DEBUG
		//workaround for AppVeyor
		//System.InvalidOperationException : The 'Microsoft.ACE.OLEDB.12.0' provider is not registered on the local machine.
    [Ignore("Only valid for debug")] 
#endif
		[Test]
		public void When_ProcessQuarterlyFileNew_Method_Called_Then_Valid_Site_And_SiteToCompetitor_Items_Should_Be_Recorded()
		{
			//Arrange
			var testFileToUpload = DummyFileUploads.First(fu => fu.UploadTypeId == (int)UploadTypes.QuarterlySiteData);

			var testFilePathAndName = Path.Combine(TestFileFolderPath, testFileToUpload.StoredFileName);

			var testFileData = new DataFileReader().GetQuarterlyData(testFilePathAndName, QuarterlyFileDataSheetName);

			_mockDataFileReader
				.Setup(dfr => dfr.GetQuarterlyData(testFilePathAndName, QuarterlyFileDataSheetName))
				.Returns(testFileData);

			_mockRepository
				.Setup(r => r.NewQuarterlyRecords(It.IsAny<List<Models.ViewModels.CatalistQuarterly>>(), It.Is<Models.FileUpload>(arg => ComparePrimaryFileUploadAttributes(testFileToUpload, arg)), It.IsAny<int>()))
				.Returns(true);

			var sut = new FileService(_mockRepository.Object, _mockPriceService.Object, _mockSettingsService.Object, _mockDataFileReader.Object);

			//Act
			var result = sut.ProcessQuarterlyFileNew(DummyFileUploads.Where(fu => fu.UploadTypeId == (int)UploadTypes.QuarterlySiteData).ToList());

			//Assert
			#region Assert
			//verify import process status changed to 5 - Processing
			Assert.DoesNotThrow(delegate
			{
				_mockRepository
					.Verify(v => v.UpdateImportProcessStatus(5, It.Is<Models.FileUpload>(arg =>
						ComparePrimaryFileUploadAttributes(testFileToUpload, arg))), Times.Once());
			});

			//verify DeleteRecordsForQuarterlyUploadStaging call
			Assert.DoesNotThrow(delegate
			{
				_mockRepository
					.Verify(v => v.DeleteRecordsForQuarterlyUploadStaging(), Times.Once());
			});

			//verify FIRST batch has been uploaded
			Assert.DoesNotThrow(delegate
			{
				_mockRepository
					.Verify(v => v.NewQuarterlyRecords(
						It.IsAny<List<Models.ViewModels.CatalistQuarterly>>(),
						It.Is<Models.FileUpload>(arg => ComparePrimaryFileUploadAttributes(testFileToUpload, arg)),
						0
						), Times.Once());
			});

			//verify LAST batch has been uploaded
			var lastBatchLeg = (testFileData.Rows.Count / Constants.QuarterlyFileRowsBatchSize) * Constants.QuarterlyFileRowsBatchSize;
			_mockRepository
					.Verify(v => v.NewQuarterlyRecords(
						It.IsAny<List<Models.ViewModels.CatalistQuarterly>>(),
						It.Is<Models.FileUpload>(arg => ComparePrimaryFileUploadAttributes(testFileToUpload, arg)),
						lastBatchLeg
						), Times.Once());

			//verify ImportQuarterlyUploadStaging call
			//this will run stored procedure on SQL server 
			Assert.DoesNotThrow(delegate
			{
				_mockRepository
					.Verify(v => v.ImportQuarterlyUploadStaging(testFileToUpload.Id), Times.Once());
			});

			//verify import process status changed to 10 - Success
			Assert.DoesNotThrow(delegate
			{
				_mockRepository
					.Verify(v => v.UpdateImportProcessStatus(10, It.Is<Models.FileUpload>(arg => arg.Id == testFileToUpload.Id)), Times.Once());
			});

			testFileToUpload.StatusId = 10;
			AssertExtensions.PropertyValuesAreEquals(result, testFileToUpload);

			#endregion
		}

		[Test]
		public void When_ProcessQuarterlyFileNew_Method_Called_And_Exception_Occured_Then_Error_Should_Be_Recorded_And_Status_Updated_To_Failed()
		{
			//Arrange
			var testFileToUpload = DummyFileUploads.First(fu => fu.UploadTypeId == (int)UploadTypes.QuarterlySiteData);

			_mockRepository
				.Setup(r => r.DeleteRecordsForQuarterlyUploadStaging())
				.Throws(new ApplicationException());

			var sut = new FileService(_mockRepository.Object, _mockPriceService.Object, _mockSettingsService.Object, _mockDataFileReader.Object);

			//Act
			sut.ProcessQuarterlyFileNew(DummyFileUploads.Where(fu => fu.UploadTypeId == (int)UploadTypes.QuarterlySiteData).ToList());

			//Assert
			#region Assert
			//verify LogImportError call
			Assert.DoesNotThrow(delegate
			{
				_mockRepository
					.Verify(v => v.LogImportError(
						It.Is<Models.FileUpload>(arg =>
						ComparePrimaryFileUploadAttributes(testFileToUpload, arg)), It.IsAny<string>(), It.IsAny<int?>()), Times.Once());
			});

			//verify import process status changed to 15 - Failed
			Assert.DoesNotThrow(delegate
			{
				_mockRepository
					.Verify(v => v.UpdateImportProcessStatus(15, It.Is<Models.FileUpload>(arg =>
						ComparePrimaryFileUploadAttributes(testFileToUpload, arg))), Times.Once());
			});

			#endregion
		}

#if !DEBUG
		//workaround for AppVeyor
		//System.InvalidOperationException : The 'Microsoft.ACE.OLEDB.12.0' provider is not registered on the local machine.
    [Ignore("Only valid for debug")] 
#endif
		[TestCase((int)UploadTypes.DailyPriceData)] // Daliy
		[TestCase((int)UploadTypes.QuarterlySiteData)] // Quarterly
		public void When_NewUpload_Method_Called_Then_Daily_Price_Calculation_Should_Be_Fired(
			int uploadTypeId
			)
		{
			//Arrange
			var expectedFileUploadForRecalculation = DummyFileUploads.First(fu => fu.UploadTypeId == (int)UploadTypes.DailyPriceData);

			var testFileToUpload = DummyFileUploads.First(fu => fu.UploadTypeId == uploadTypeId);

			testFileToUpload.UploadTypeId = uploadTypeId;

			if (uploadTypeId == (int)UploadTypes.QuarterlySiteData)
			{
				var testFilePathAndName = Path.Combine(TestFileFolderPath, testFileToUpload.StoredFileName);

				var testFileData = new DataFileReader().GetQuarterlyData(testFilePathAndName, QuarterlyFileDataSheetName);

				_mockDataFileReader
					.Setup(dfr => dfr.GetQuarterlyData(testFilePathAndName, QuarterlyFileDataSheetName))
					.Returns(testFileData);
			}

			_mockRepository
				.Setup(r => r.NewUpload(testFileToUpload))
				.Returns(testFileToUpload);

			//GetFileUploads setup
			_mockRepository
				.Setup(r => r.GetDailyFileAvailableForCalc(testFileToUpload.UploadDateTime))
				.Returns(expectedFileUploadForRecalculation);

			_mockRepository
				.Setup(r => r.NewQuarterlyRecords(It.IsAny<List<Models.ViewModels.CatalistQuarterly>>(), It.Is<Models.FileUpload>(arg => ComparePrimaryFileUploadAttributes(testFileToUpload, arg)), It.IsAny<int>()))
				.Returns(true);

			var sut = new FileService(_mockRepository.Object, _mockPriceService.Object, _mockSettingsService.Object, _mockDataFileReader.Object);

			//Act
			sut.NewUpload(testFileToUpload);

			//Assert
			#region Assert
			//verify that file has been sent for recalculation
			Assert.DoesNotThrow(delegate
			{
				_mockPriceService
					.Verify(v => v.DoCalcDailyPrices(expectedFileUploadForRecalculation.UploadDateTime), Times.Once());
			});
			#endregion
		}


#if !DEBUG
		//workaround for AppVeyor
		//System.InvalidOperationException : The 'Microsoft.ACE.OLEDB.12.0' provider is not registered on the local machine.
    [Ignore("Only valid for debug")] 
#endif
		[Test]
		public void When_NewUpload_Method_Called_And_New_Daily_Upload_File_Not_Found_Then_Daily_Price_Calculation_Should_NOT_Be_Fired()
		{
			//Arrange
			var expectedFileUploadForRecalculation = DummyFileUploads.First(fu => fu.UploadTypeId == (int)UploadTypes.DailyPriceData);

			var testFileToUpload = DummyFileUploads.First(fu => fu.UploadTypeId == (int)UploadTypes.QuarterlySiteData);

			var testFilePathAndName = Path.Combine(TestFileFolderPath, testFileToUpload.StoredFileName);

			var testFileData = new DataFileReader().GetQuarterlyData(testFilePathAndName, QuarterlyFileDataSheetName);

			_mockDataFileReader
				.Setup(dfr => dfr.GetQuarterlyData(testFilePathAndName, QuarterlyFileDataSheetName))
				.Returns(testFileData);

			_mockRepository
				.Setup(r => r.NewUpload(testFileToUpload))
				.Returns(testFileToUpload);

			//GetFileUploads setup
			_mockRepository
				.Setup(r => r.GetDailyFileAvailableForCalc(testFileToUpload.UploadDateTime))
				.Returns(null as FileUpload);

			_mockRepository
				.Setup(r => r.NewQuarterlyRecords(It.IsAny<List<Models.ViewModels.CatalistQuarterly>>(), It.Is<Models.FileUpload>(arg => ComparePrimaryFileUploadAttributes(testFileToUpload, arg)), It.IsAny<int>()))
				.Returns(true);

			var sut = new FileService(_mockRepository.Object, _mockPriceService.Object, _mockSettingsService.Object, _mockDataFileReader.Object);

			//Act
			sut.NewUpload(testFileToUpload);

			//Assert
			#region Assert
			//verify that file has been sent for recalculation
			Assert.DoesNotThrow(delegate
			{
				_mockPriceService
					.Verify(v => v.DoCalcDailyPrices(expectedFileUploadForRecalculation.UploadDateTime), Times.Never);
			});
			#endregion
		}

		#region private Methods

		private int getNumberOfLinesInDailyTestFile(Models.FileUpload testFileToUpload)
		{
			int linesNumber = 0;

			using (var file = new StreamReader(Path.Combine(TestFileFolderPath, testFileToUpload.StoredFileName)))
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
