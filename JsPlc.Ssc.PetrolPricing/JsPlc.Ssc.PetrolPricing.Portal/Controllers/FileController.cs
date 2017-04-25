﻿using System;
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
using System.Globalization;
using JsPlc.Ssc.PetrolPricing.Core.Interfaces;
using JsPlc.Ssc.PetrolPricing.Portal.Helper;
using JsPlc.Ssc.PetrolPricing.Core.StringFormatters;

namespace JsPlc.Ssc.PetrolPricing.Portal.Controllers
{

    [Authorize]
    public class FileController : Controller
    {
        readonly ServiceFacade _serviceFacade;
        private readonly ILogger _logger;

        public FileController()
        {
            _logger = new PetrolPricingLogger();
            _serviceFacade = new ServiceFacade(_logger);
        }
      
        public async Task<ActionResult> Index(string msg = "")
        {
            // Display list of existing files along with their status
            ViewBag.Message = msg;

            using (var svc = new ServiceFacade(_logger))
            {
                var model = await svc.GetFileUploads(null, null);
                var viewModel = model.ConvertToFileUploadViewModel();
                return View(viewModel);
            }
        }

        public async Task<ActionResult> GetUploadsPartial()
        {
            using (var svc = new ServiceFacade(_logger))
            {
                var model = await svc.GetFileUploads(null, null);
                var viewModel = model.ConvertToFileUploadViewModel();
                return PartialView("~/Views/File/_FileUploadsList.cshtml", viewModel);
            }
        }

        public async Task<ActionResult> Upload(string errMsg = "")
        {
            ViewBag.ErrorMessage = errMsg;

            try
            {
                await DataCleanseFileUploads();
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
            }

            var model = new UploadViewModel
            {
                UploadTypes = GetUploadTypes(),
                UploadDate = DateTime.Now
            };
            var existingUploads = await ExistingDailyUploads(model.UploadDate);
            if (existingUploads != null && existingUploads.Any())
            {
                ViewBag.WarningMessage = StringMessages.Warning_DailyPriceFileAlreadyUploaded;
                _logger.Information(StringMessages.Warning_DailyPriceFileAlreadyUploaded);
            }
            return View(model);
        }

        [HttpPost]
        public async Task<ActionResult> Upload(HttpPostedFileBase file, int uploadTypes, DateTime? uploadDate)
        {
            uploadDate = uploadDate + new TimeSpan(DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second);

            var model = new UploadViewModel
            {
                UploadTypes = GetUploadTypes(),
                UploadDate = uploadTypes == 2 ? DateTime.Now : uploadDate.Value
            };

            string errorMessage = string.Empty;

            try
            {
                if (file == null || file.ContentLength <= 0)
                {
                    _logger.Error(new ApplicationException(StringMessages.Error_UploadedFileIsEmpty));
                    throw new ApplicationException(StringMessages.Error_UploadedFileIsEmpty);
                }
                else if (file.ContentLength > 3145728 && uploadTypes == 1)
                {
                    _logger.Error(new ApplicationException(StringMessages.Error_UploadedFileLengthGreaterThanMaxSize));
                    throw new ApplicationException(StringMessages.Error_UploadedFileLengthGreaterThanMaxSize);
                }



                var fu = file.ToFileUpload(User.Identity.Name, uploadDate, uploadTypes);

                if (uploadTypes == (int)FileUploadTypes.DailyPriceData
                    && fu.OriginalFileName.ToLowerInvariant().EndsWith(".xlsx"))
                {
                    ViewBag.ErrorMessage = StringMessages.Error_InvalidFileFormat_DailyPriceData;
                    return View(model);
                }
                else if (fu.UploadTypeId == 2 && !(fu.OriginalFileName.ToLowerInvariant().EndsWith(".xlsx") || fu.OriginalFileName.ToLowerInvariant().EndsWith(".xls")))
                {
                    ViewBag.ErrorMessage = StringMessages.Error_InvalidFileFormat_QuarterlyPriceData;
                    return View(model);
                }


                var fum = new FileUploadModel(fu, new ServiceFacade(new PetrolPricingLogger()));

                var started = DateTime.Now;
                var status = await fum.UploadFile(file);
                var finished = DateTime.Now;

                switch (status) // Store fu to state
                {
                    case FileUploadStatus.InvalidUpload:
                        return RedirectToAction("Upload", new { errMsg = StringMessages.Error_InvalidUploadFile });

                    case FileUploadStatus.Held: // Initiate second step - get User confirmation
                        var holdKey = Guid.NewGuid().ToString();
                        Session[holdKey] = fu;
                        return RedirectToAction("ConfirmUpload", new { guidKey = holdKey });

                    case FileUploadStatus.Saved:
                        var msg = String.Format(Constants.UploadSuccessMessageWithFormatAndTimeTaken,
                            fum.OriginalFileName,
                            DateAndTimeFormatter.FormatFriendlyTimeAgo(finished.Subtract(started))
                            );
                        return RedirectToAction("Index", new { msg = msg });
                }
            }
            catch (ApplicationException ex)
            {
                _logger.Error(ex);
                errorMessage = ex.Message;
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
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

            var fileUploads = await ExistingDailyUploads(fileUpload.UploadDateTime);

            var model = new UploadConfirmationViewModel
            {
                OriginalFileName = fileUpload.OriginalFileName,
                Guid = guidKey,
                ExistingFiles = fileUploads.ConvertToFileUploadViewModel()
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

            var fum = new FileUploadModel(fileUpload, new ServiceFacade(new PetrolPricingLogger()));

            if (response == "Overwrite")
            {
                var started = DateTime.Now;
                fum.ConfirmedUploadByUser();
                var finished = DateTime.Now;

               var msg = String.Format(Constants.UploadSuccessMessageWithFormatAndTimeTaken, 
                   fum.OriginalFileName,
                   DateAndTimeFormatter.FormatFriendlyTimeAgo(finished.Subtract(started))
                  );

                return RedirectToAction("Index", new { msg = msg });
            }

            fum.CleanupUpload();
            return RedirectToAction("Upload");
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


        public async Task<ActionResult> Download(int id)
        {
            var model = await _serviceFacade.DownloadFile(id);
            return File(model.FileBytes, System.Net.Mime.MediaTypeNames.Application.Octet, model.FileName);
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
            using (var svcFacade = new ServiceFacade(new PetrolPricingLogger()))
            {
                return svcFacade.GetUploadTypes();
            }
        }

        private async Task<bool> DataCleanseFileUploads()
        {
            return await _serviceFacade.DataCleanseFileUploads();
        }

        #endregion
    }

    public static class FileUploadExtensions
    {
        public static IEnumerable<FileUploadViewModel> ConvertToFileUploadViewModel(this IEnumerable<FileUpload> uploads)
        {
            var viewModel = new List<FileUploadViewModel>();

            var lastDate = DateTime.Now.Date.AddYears(1);
            var mostRecent = new Dictionary<int, FileUploadViewModel>();

            foreach (var upload in uploads)
            {
                var vm = new FileUploadViewModel()
                {
                    Id = upload.Id,
                    OriginalFileName = upload.OriginalFileName,
                    StoredFileName = upload.StoredFileName,
                    UploadTypeId = upload.UploadTypeId,
                    UploadType = upload.UploadType,
                    UploadDateTime = upload.UploadDateTime,
                    StatusId = upload.StatusId,
                    Status = upload.Status,
                    UploadedBy = upload.UploadedBy,
                    FileExists = upload.FileExists,
                    IsMostRecentForDate = false,
                    IsForDifferentDay = false
                };

                if (mostRecent.ContainsKey(vm.UploadTypeId) == false)
                {
                    vm.IsMostRecentForDate = true;
                    mostRecent.Add(vm.UploadTypeId, vm);
                }

                if (vm.UploadDateTime.Date != lastDate)
                {
                    vm.IsForDifferentDay = true;
                    lastDate = vm.UploadDateTime.Date;
                }

                viewModel.Add(vm);
            }
            return viewModel;
        }

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
        public IEnumerable<FileUploadViewModel> ExistingFiles { get; set; } // Show list of files existing for the day
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
            if (_serviceFacade.SaveHoldFile(heldFile, destFile) != null)
            {
                FileUpload fileUpload = RecordUpload();
            }

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

                FileUpload fileUpload = RecordUpload();
                if (fileUpload == null) _uploadStatus = FileUploadStatus.InvalidUpload;
            }

            // Simply save the file to Hold or Save path
            return _uploadStatus;
        }

        private void SetUploadAndHoldPaths()
        {
            _uploadPath = _serviceFacade.GetUploadPath();
            _uploadPath = _uploadPath + "\\"; //  Functions.EnsurePathEndsWithSlash(_uploadPath);
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
            _serviceFacade.SaveFile(_uploadedFile, Constants.UploadHoldPath + "\\" + _fileUpload.StoredFileName);// PERSIST The File to Upload Holding area
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

        private FileUpload RecordUpload()
        {
            return _serviceFacade.NewUpload(_fileUpload);
        }

        public void CleanupUpload()
        {
            var heldFile = Path.Combine(_uploadHoldPath, _fileUpload.StoredFileName);
            File.Delete(heldFile);
        }

    }
}