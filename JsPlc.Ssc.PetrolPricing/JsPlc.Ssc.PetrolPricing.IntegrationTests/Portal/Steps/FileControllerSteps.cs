using JsPlc.Ssc.PetrolPricing.Core;
using JsPlc.Ssc.PetrolPricing.IntegrationTests.Core;
using JsPlc.Ssc.PetrolPricing.Models;
using JsPlc.Ssc.PetrolPricing.Portal.Controllers;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using TechTalk.SpecFlow;

namespace JsPlc.Ssc.PetrolPricing.IntegrationTests.Portal.Steps
{
	[Binding]
	public class FileControllerSteps : StepsBase
	{
		enum ContextKeys { HttpTestPostedFile, UploadDateTime, FileType, InvalidUploadFileTypeResult }

		[Given(@"I have valid Quarterly Data File for upload")]
		public void GivenIHaveValidQuarterlyDataFileForUpload()
		{
//#if !DEBUG
//			ScenarioContext.Current.Pending();
//#endif

			//Arrange
			var filePathAndName = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TestFiles/QuarterlyUpload.xlsx");

			ScenarioContext.Current[ContextKeys.FileType.ToString()] = 2;

			//Act
			var testFile = new HttpTestPostedFile(filePathAndName);

			ScenarioContext.Current[ContextKeys.HttpTestPostedFile.ToString()] = testFile;

			//Assert
			Assert.AreEqual(testFile.FileName, "QuarterlyUpload.xlsx");
		}


		[Given(@"I have valid Daily Price Data File for upload")]
		public void GivenIHaveValidDailyPriceDataFileForUpload()
		{
//#if !DEBUG
//	ScenarioContext.Current.Pending();
//#endif
			//Arrange
			var filePathAndName = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TestFiles/DailyUpload.txt");

			ScenarioContext.Current[ContextKeys.FileType.ToString()] = 1;

			//Act
			var testFile = new HttpTestPostedFile(filePathAndName);

			ScenarioContext.Current[ContextKeys.HttpTestPostedFile.ToString()] = testFile;

			//Assert
			Assert.AreEqual(testFile.FileName, "DailyUpload.txt");
		}

		[When(@"I press Upload file button")]
		public void WhenIPressUploadFileButton()
		{
//#if !DEBUG
//	ScenarioContext.Current.Pending();
//#endif
			//Arrange
			var fileToUpload = (HttpTestPostedFile)ScenarioContext.Current[ContextKeys.HttpTestPostedFile.ToString()];

			var expectedUploadMessage = String.Format(Constants.UploadSuccessMessageWithFormat, fileToUpload.FileName);

			FileController fileController = new FileController();

			fileController.ControllerContext = MockControllerContext.Object;

			var uploadDateTime = DateTime.Now;

			ScenarioContext.Current[ContextKeys.UploadDateTime.ToString()] = uploadDateTime;

			var fileTypeId = (int)ScenarioContext.Current[ContextKeys.FileType.ToString()];

			//Act
			var uploadResult = fileController.Upload(fileToUpload, fileTypeId, uploadDateTime).Result as RedirectToRouteResult;

			//Assert
			Assert.Greater(uploadResult.RouteValues.Count, 0);
			Assert.IsTrue(uploadResult.RouteValues.Any(k => k.Key == "action"));

			//if this is the first file upload for today - redirect to Index
			if (uploadResult.RouteValues["Action"].ToString() == "Index")
			{
				Assert.AreEqual(expectedUploadMessage, uploadResult.RouteValues["msg"]);
			}
			//if this is not the only file uplad for today - redirect ot ConfirmUpload
			else if (uploadResult.RouteValues["Action"].ToString() == "ConfirmUpload")
			{
				Assert.IsNotNull(uploadResult.RouteValues["guidKey"]);

				//and confirm upload
				var confirmUploadResult = fileController.UploadConfirmation("Overwrite", uploadResult.RouteValues["guidKey"].ToString()) as RedirectToRouteResult;

				Assert.AreEqual(expectedUploadMessage, confirmUploadResult.RouteValues["msg"]);

				Assert.AreEqual("Index", confirmUploadResult.RouteValues["Action"]);
			}
		}

		[Then(@"the test file should be visible in the list and its status should be Success")]
		public void ThenTheTestFileShouldBeVisibleInTheListAndItsStatusShouldBeSuccess()
		{
#if !DEBUG
	ScenarioContext.Current.Pending();
#endif
			//Arrange 
			FileController fileController = new FileController();

			fileController.ControllerContext = MockControllerContext.Object;

			var uploadedFile = (HttpTestPostedFile)ScenarioContext.Current[ContextKeys.HttpTestPostedFile.ToString()];

			var uploadDateTime = (DateTime)ScenarioContext.Current[ContextKeys.UploadDateTime.ToString()];

			var fileTypeId = (int)ScenarioContext.Current[ContextKeys.FileType.ToString()];

			//Act 
			var indexResultFiles = ((ViewResult)fileController.Index().Result).Model as IEnumerable<FileUpload>;

			//Assert
			Assert.IsTrue(indexResultFiles.Any(f =>
				f.OriginalFileName == uploadedFile.FileName
				&& f.UploadTypeId == fileTypeId
				&& f.UploadDateTime.Date == uploadDateTime.Date
				&& f.UploadDateTime.Hour == uploadDateTime.Hour
				&& f.UploadDateTime.Minute == uploadDateTime.Minute
					//+/- 2 seconds
				&& f.UploadDateTime.Second >= uploadDateTime.AddSeconds(-2).Second
				&& f.UploadDateTime.Second <= uploadDateTime.AddSeconds(2).Second
				&& f.StatusId == (int)ImportProcessStatuses.Success
				&& f.UploadedBy == TestUserName
				));
		}

		[When(@"I select Daily Price Date as File Type and press Upload file button")]
		public void WhenISelectDailyPriceDateAsFileTypeAndPressUploadFileButton()
		{
//#if !DEBUG
//	ScenarioContext.Current.Pending();
//#endif
			//Arrange
			var fileToUpload = (HttpTestPostedFile)ScenarioContext.Current[ContextKeys.HttpTestPostedFile.ToString()];

			var expectedErrorMessage = String.Format(Constants.UploadSuccessMessageWithFormat, fileToUpload.FileName);

			FileController fileController = new FileController();

			fileController.ControllerContext = MockControllerContext.Object;

			var uploadDateTime = DateTime.Now;

			ScenarioContext.Current[ContextKeys.UploadDateTime.ToString()] = uploadDateTime;

			//so we get file upload error
			var fileTypeId = 1;

			//Act
			var invalidUploadResult = fileController.Upload(fileToUpload, fileTypeId, uploadDateTime).Result as ViewResult;

			ScenarioContext.Current[ContextKeys.InvalidUploadFileTypeResult.ToString()] = invalidUploadResult;

			//Assert
			Assert.IsNotNull(invalidUploadResult);
		}

		[Then(@"Invalid Upload File Type error should appear")]
		public void ThenInvalidUploadFileTypeErrorShouldAppear()
		{
//#if !DEBUG
//	ScenarioContext.Current.Pending();
//#endif
			//Arrange
			var invalidUploadResult = (ViewResult)ScenarioContext.Current[ContextKeys.InvalidUploadFileTypeResult.ToString()];

			//Act
			string resultErrorMessage = invalidUploadResult.ViewBag.ErrorMessage;

			//Assert
			Assert.AreEqual(JsPlc.Ssc.PetrolPricing.Portal.StringMessages.Error_InvalidFileFormat_DailyPriceData, resultErrorMessage);
			
		}


		[Then(@"the test data should be deleted")]
		public void ThenTheTestDataShouldBeDeleted()
		{
//#if !DEBUG
//	ScenarioContext.Current.Pending();
//#endif
			//Arrange
			FileController fileController = new FileController();

			fileController.ControllerContext = MockControllerContext.Object;

			//Act
			var cleanUpResult = fileController.CleanupIntegrationTestsData(TestUserName);

			//Assert
			Assert.IsNotNull(cleanUpResult);
		}
	}
}
