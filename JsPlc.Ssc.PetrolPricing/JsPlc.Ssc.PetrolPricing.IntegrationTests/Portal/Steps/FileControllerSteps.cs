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
#if !DEBUG
	//workaround for AppVeyor
	//System.InvalidOperationException : The 'Microsoft.ACE.OLEDB.12.0' provider is not registered on the local machine.
	[Ignore("Only valid for debug")]
#endif
	[Binding]
	public class FileControllerSteps : StepsBase
	{
		enum ContextKeys { HttpTestPostedFile, UploadDateTime }

#if !DEBUG
	//workaround for AppVeyor
	//System.InvalidOperationException : The 'Microsoft.ACE.OLEDB.12.0' provider is not registered on the local machine.
	[Ignore("Only valid for debug")]
#endif
		[Given(@"I have valid Daily Price Data File for upload")]
		public void GivenIHaveValidDailyPriceDataFileForUpload()
		{
			//Arrange
			var filePathAndName = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TestFiles/DailyUpload.txt");

			//Act
			var testFile = new HttpTestPostedFile(filePathAndName);

			ScenarioContext.Current[ContextKeys.HttpTestPostedFile.ToString()] = testFile;

			//Assert
			Assert.AreEqual(testFile.FileName, "DailyUpload.txt");
		}

#if !DEBUG
	//workaround for AppVeyor
	//System.InvalidOperationException : The 'Microsoft.ACE.OLEDB.12.0' provider is not registered on the local machine.
	[Ignore("Only valid for debug")]
#endif
		[When(@"I press Upload file button")]
		public void WhenIPressUploadFileButton()
		{
			//Arrange
			var fileToUpload = (HttpTestPostedFile)ScenarioContext.Current[ContextKeys.HttpTestPostedFile.ToString()];

			var expectedUploadMessage = String.Format(Constants.UploadSuccessMessageWithFormat, fileToUpload.FileName);

			FileController fileController = new FileController();

			fileController.ControllerContext = MockControllerContext.Object;

			var uploadDateTime = DateTime.Now;

			ScenarioContext.Current[ContextKeys.UploadDateTime.ToString()] = uploadDateTime;

			//Act
			var uploadResult = fileController.Upload(fileToUpload, 1, uploadDateTime).Result as RedirectToRouteResult;

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

#if !DEBUG
	//workaround for AppVeyor
	//System.InvalidOperationException : The 'Microsoft.ACE.OLEDB.12.0' provider is not registered on the local machine.
	[Ignore("Only valid for debug")]
#endif
		[Then(@"the test file should be visible in the list and its status should be Success")]
		public void ThenTheTestFileShouldBeVisibleInTheListAndItsStatusShouldBeSuccess()
		{
			//Arrange 
			FileController fileController = new FileController();

			fileController.ControllerContext = MockControllerContext.Object;

			var uploadedFile = (HttpTestPostedFile)ScenarioContext.Current[ContextKeys.HttpTestPostedFile.ToString()];

			var uploadDateTime = (DateTime)ScenarioContext.Current[ContextKeys.UploadDateTime.ToString()];

			//Act 
			var indexResultFiles = ((ViewResult)fileController.Index().Result).Model as IEnumerable<FileUpload>;

			//Assert
			Assert.IsTrue(indexResultFiles.Any(f => 
				f.OriginalFileName == uploadedFile.FileName 
				&& f.UploadTypeId == 1
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

		[Then(@"the test data should be deleted")]
		public void ThenTheTestDataShouldBeDeleted()
		{
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
