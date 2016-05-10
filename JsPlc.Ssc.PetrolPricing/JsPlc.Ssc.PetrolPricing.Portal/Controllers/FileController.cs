using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Security.Cryptography;
using System.Security.Principal;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using JsPlc.Ssc.PetrolPricing.Models;
using JsPlc.Ssc.PetrolPricing.Models.Common;
using JsPlc.Ssc.PetrolPricing.Models.ViewModels;
using JsPlc.Ssc.PetrolPricing.Portal.Facade;
using Microsoft.Ajax.Utilities;
using JsPlc.Ssc.PetrolPricing.Core;
using System.Net.Http;

namespace JsPlc.Ssc.PetrolPricing.Portal.Controllers
{
     [NoCache]
	[Authorize]
	public class FileController : Controller
	{
		readonly ServiceFacade _serviceFacade = new ServiceFacade();

		public async Task<ActionResult> Index(string msg = "")
		{
			// Display list of existing files along with their status
			ViewBag.Message = msg;

			using (var svc = new ServiceFacade())
			{
				var model = await svc.GetFileUploads(null, null);
				return View(model);
			}
		}

		public async Task<ActionResult> Upload(string errMsg = "")
		{
			ViewBag.ErrorMessage = errMsg;

			var model = new UploadViewModel
			{
				UploadTypes = GetUploadTypes(),
				UploadDate = DateTime.Now
			};
			var existingUploads = await ExistingDailyUploads(model.UploadDate);
			if (existingUploads.Any())
			{
				ViewBag.WarningMessage = StringMessages.Warning_DailyPriceFileAlreadyUploaded;
			}
			return View(model);
		}

		[HttpPost]
		public async Task<ActionResult> Upload(HttpPostedFileBase file, int uploadTypes, DateTime? uploadDate)
		{
			var model = new UploadViewModel
			{
				UploadTypes = GetUploadTypes(),
				UploadDate = DateTime.Now
			};

			string errorMessage = string.Empty;

			try
			{
				if (file == null || file.ContentLength <= 0)
				{
					throw new ApplicationException(StringMessages.Error_UploadedFileIsEmpty);
				}

				var fu = file.ToFileUpload(User.Identity.Name, uploadDate, uploadTypes);

				if (uploadTypes == (int)FileUploadTypes.DailyPriceData
					&& fu.OriginalFileName.ToLowerInvariant().EndsWith(".xlsx"))
				{
					ViewBag.ErrorMessage = StringMessages.Error_InvalidFileFormat_DailyPriceData;
					return View(model);
				}

				var fum = new FileUploadModel(fu, new ServiceFacade());

				var status = await fum.UploadFile(file);

				switch (status) // Store fu to state
				{
					case FileUploadStatus.InvalidUpload:
						return RedirectToAction("Upload", new { errMsg = StringMessages.Error_InvalidUploadFile });

					case FileUploadStatus.Held: // Initiate second step - get User confirmation
						var holdKey = Guid.NewGuid().ToString();
						Session[holdKey] = fu;
						return RedirectToAction("ConfirmUpload", new { guidKey = holdKey });

					case FileUploadStatus.Saved:
						return RedirectToAction("Index", new { msg = String.Format(Constants.UploadSuccessMessageWithFormat, fum.OriginalFileName) });
				}
			}
			catch (ApplicationException ex)
			{
				errorMessage = ex.Message;
			}
			catch (Exception)
			{
				errorMessage = StringMessages.Error_TryAgain;
			}
			return RedirectToAction("Upload", new { errMsg = errorMessage });
		}

		public async Task<ActionResult> ConfirmUpload(string guidKey)
		{
			var fileUpload = Session[guidKey] as FileUpload;
			if (fileUpload == null)
			{
				return RedirectToAction("Upload", new { errMsg = StringMessages.Error_ReuploadFile });
			}

			var model = new UploadConfirmationViewModel
			{
				OriginalFileName = fileUpload.OriginalFileName,
				Guid = guidKey,
				ExistingFiles = await ExistingDailyUploads(fileUpload.UploadDateTime)
			};
			return View(model);
		}

		public ActionResult UploadConfirmation(string response, string guidKey)
		{
			var fileUpload = Session[guidKey] as FileUpload;
			if (fileUpload == null)
			{
				return RedirectToAction("Upload", new { errMsg = StringMessages.Error_ReuploadFile });
			}

			var fum = new FileUploadModel(fileUpload, new ServiceFacade());

			if (response == "Overwrite")
			{
				fum.ConfirmedUploadByUser();
				return RedirectToAction("Index", new { msg = String.Format(Constants.UploadSuccessMessageWithFormat, fum.OriginalFileName) });
			}

			fum.CleanupUpload();
			return RedirectToAction("Overwrite");
		}

		public async Task<ActionResult> Details(int id)
		{
			// Return file upload details with processing steps and errors if any
			var model = await _serviceFacade.GetFileUpload(id);
			return View(model);
		}

		[NonAction]
		public string CleanupIntegrationTestsData(string testUserName)
		{
			return _serviceFacade.CleanupIntegrationTestsData(testUserName);
		}

		#region private methods
		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		// To check for warning where file exists (daily file type check for the date)
		private async Task<IEnumerable<FileUpload>> ExistingDailyUploads(DateTime uploadDateTime)
		{
			return await _serviceFacade.ExistingDailyUploads(uploadDateTime);
		}

		private static IEnumerable<UploadType> GetUploadTypes()
		{
			using (var svcFacade = new ServiceFacade())
			{
				return svcFacade.GetUploadTypes();
			}
		}
		#endregion
	}

	public static class FileUploadExtensions
	{
		public static FileUpload ToFileUpload(this HttpPostedFileBase uploadedFile, string userName, DateTime? uploadDate, int uploadTypeId)
		{
			var uploadDateTime = uploadDate ?? DateTime.Now;
			var fileName = Path.GetFileName(uploadedFile.FileName);
			var originalFileName = fileName;

			var savedFileName = String.Format("{1} {2}hrs - {0}", fileName,
				uploadDateTime.ToString("yyyyMMdd"),
				uploadDateTime.ToString("HHmmss"));
			return new FileUpload
			{
				OriginalFileName = originalFileName,
				StoredFileName = savedFileName,
				UploadedBy = userName,
				StatusId = 1,
				UploadDateTime = uploadDateTime,
				UploadTypeId = uploadTypeId
			};
		}
	}

	public enum FileUploadStatus
	{
		Unknown = 0, Read = 1, Held = 2, Confirmed = 3, Saved = 4, InvalidUpload = -1
	}

	public class UploadConfirmationViewModel
	{
		public string OriginalFileName { get; set; }
		public string Guid { get; set; } // key to session object
		public bool Response { get; set; } // Upload, Cancel
		public IEnumerable<FileUpload> ExistingFiles { get; set; } // Show list of files existing for the day
	}

	public class FileUploadModel
	{
		private string _holdKey;
		private FileUpload _fileUpload;
		private FileUploadStatus _uploadStatus = FileUploadStatus.Unknown;
		private ServiceFacade _serviceFacade;

		private HttpPostedFileBase _uploadedFile;

		private string _uploadPath;
		private string _uploadHoldPath;

		public FileUploadModel(FileUpload fileUpload, ServiceFacade serviceFacade)
		{
			_fileUpload = fileUpload;
			_uploadStatus = FileUploadStatus.Unknown;
			_serviceFacade = serviceFacade;

			SetUploadAndHoldPaths();
		}

		public string OriginalFileName // Or we could expose entire FileUpload object
		{
			get { return _fileUpload.OriginalFileName; }
		}

		public void ConfirmedUploadByUser()
		{
			var heldFile = Path.Combine(_uploadHoldPath, _fileUpload.StoredFileName);
			var destFile = Path.Combine(_uploadPath, _fileUpload.StoredFileName);
			File.Move(heldFile, destFile);
			RecordUpload();
		}

		public async Task<FileUploadStatus> UploadFile(HttpPostedFileBase uploadedFile)
		{
			_uploadedFile = uploadedFile;

			if (uploadedFile == null || uploadedFile.ContentLength <= 0)
			{
				return FileUploadStatus.InvalidUpload;
			}
			var existingUploads = await _serviceFacade.ExistingDailyUploads(_fileUpload.UploadDateTime);
			if (_fileUpload.UploadTypeId == 1 && existingUploads.Any())
			{
				_holdKey = Guid.NewGuid().ToString();

				_uploadStatus = PersistToHoldFile();
			}
			else
			{
                _uploadStatus = PersistToSaveFile();
              
				RecordUpload();
			}

			// Simply save the file to Hold or Save path
			return _uploadStatus;
		}

		private void SetUploadAndHoldPaths()
		{
			_uploadPath = _serviceFacade.GetUploadPath();
			_uploadPath = Functions.EnsurePathEndsWithSlash(_uploadPath);
			_uploadHoldPath = String.Format("{0}{1}", _uploadPath, Constants.UploadHoldPath);
			CreateOrEnsureHoldPathExists();
		}

		private void CreateOrEnsureHoldPathExists()
		{
			if (!Directory.Exists(_uploadHoldPath)) Directory.CreateDirectory(_uploadHoldPath);

		}

		private FileUploadStatus PersistToHoldFile()
		{
			//_uploadPath = Functions.EnsurePathEndsWithSlash(_uploadPath);
			//var uploadHoldPath = String.Format("{0}{1}", _uploadPath, Constants.UploadHoldPath);
		//	var path = Path.Combine(uploadHoldPath, _fileUpload.StoredFileName);
            _serviceFacade.SaveFile(_uploadedFile, Constants.UploadHoldPath+"\\"+_fileUpload.StoredFileName);// PERSIST The File to Upload Holding area
			return FileUploadStatus.Held;
		}

		private FileUploadStatus PersistToSaveFile()
		{
            if (_serviceFacade.SaveFile(_uploadedFile, _fileUpload.StoredFileName) != null)
            {
                return FileUploadStatus.Saved;
            }
            else
            {
                return FileUploadStatus.InvalidUpload;
            }
		}

		private void RecordUpload()
		{
			_serviceFacade.NewUpload(_fileUpload);
		}

		public void CleanupUpload()
		{
			var heldFile = Path.Combine(_uploadHoldPath, _fileUpload.StoredFileName);
			File.Delete(heldFile);
		}
	}
}