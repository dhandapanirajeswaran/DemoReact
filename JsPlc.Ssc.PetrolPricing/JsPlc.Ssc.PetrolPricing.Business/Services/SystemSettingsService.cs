using JsPlc.Ssc.PetrolPricing.Core;
using JsPlc.Ssc.PetrolPricing.Core.Interfaces;
using JsPlc.Ssc.PetrolPricing.Exporting.Exporters;
using JsPlc.Ssc.PetrolPricing.Models;
using JsPlc.Ssc.PetrolPricing.Models.ViewModels;
using JsPlc.Ssc.PetrolPricing.Models.ViewModels.Schedule;
using JsPlc.Ssc.PetrolPricing.Models.ViewModels.SystemSettings;
using JsPlc.Ssc.PetrolPricing.Models.WindowsService;
using JsPlc.Ssc.PetrolPricing.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ClosedXML;
using System.Net.Mail;
using System.IO;

namespace JsPlc.Ssc.PetrolPricing.Business.Services
{
    public class SystemSettingsService : ISystemSettingsService
    {
        private IPetrolPricingRepository _repository;
        private ISiteService _siteService;

        protected readonly IFactory _factory;
        protected readonly IAppSettings _appSettings;

        private ILogger _logger;

        public SystemSettingsService(IPetrolPricingRepository repository, 
            ISiteService siteService, 
            IAppSettings appSettings,
            IFactory factory)
        {
            _repository = repository;
            _siteService = siteService;
            _factory = factory;
            _appSettings = appSettings;
            _logger = new PetrolPricingLogger();
        }

        public SystemSettings GetSystemSettings()
        {
            return _repository.GetSystemSettings();
        }

        public SystemSettings UpdateSystemSettings(SystemSettings settings)
        {
            _repository.UpdateSystemSettings(settings);
            return _repository.GetSystemSettings();
        }

        public SitePricingSettings GetSitePricingSettings()
        {
            return _repository.GetSitePricingSettings();
        }

        public IEnumerable<DriveTimeMarkup> GetAllDriveTimeMarkups()
        {
            return _repository.GetAllDriveTimeMarkups();
        }

        public StatusViewModel UpdateDriveTimeMarkups(IEnumerable<DriveTimeMarkup> driveTimeMarkups)
        {
            return _repository.UpdateDriveTimeMarkup(driveTimeMarkups);
        }

        public BrandsCollectionSettingsViewModel GetBrandCollectionSettings()
        {
            return _repository.GetBrandCollectionSettings();
        }
        public StatusViewModel UpdateBrandCollectionSettings(BrandsSettingsUpdateViewModel brandsCollectionSettings)
        {
            var result = _repository.UpdateBrandCollectionSettings(brandsCollectionSettings);
            if (result)
                return new StatusViewModel()
                {
                    SuccessMessage = "Updated Brands Settings",
                    ErrorMessage = ""
                };
            else
                return new StatusViewModel()
                {
                    SuccessMessage = "",
                    ErrorMessage = "Update to save Brands Settings"
                };
        }
        public BrandsCollectionSummaryViewModel GetBrandCollectionSummary()
        {
            return _repository.GetBrandCollectionSummary();
        }

        public IEnumerable<ScheduleItemViewModel> GetWinServiceScheduledItems()
        {
            return _repository.GetWinServiceScheduledItems();
        }

        public IEnumerable<ScheduleEventLogViewModel> GetWinServiceEventLog()
        {
            return  _repository.GetWinServiceEventLog();
        }

        public ScheduleItemViewModel GetWinServiceScheduleItem(int winServiceScheduleId)
        {
            return _repository.GetWinServiceScheduleItem(winServiceScheduleId);
        }

        public ScheduleItemViewModel UpsertWinServiceSchedule(ScheduleItemViewModel model)
        {
            return _repository.UpsertWinServiceSchedule(model);
        }

        public StatusViewModel RunWinServiceSchedule()
        {
            var scheduledItems = _repository.GetWinServiceScheduledItems();
            foreach(var item in scheduledItems)
            {
                if (item.IsActive)
                {
                    if (item.WinServiceEventTypeId == WinServiceEventType.DailyPriceEmail)
                        return ProcessEmailSchedule(item);
                }
            }

            return new StatusViewModel()
            {
                ErrorMessage = "There are no active scheduled items"
            };
        }

        public StatusViewModel ClearWinServiceEventLog()
        {
            try
            {
                _repository.ClearWinServiceEventLog();
                return new StatusViewModel()
                {
                    SuccessMessage = "Schedule Event Log has been cleared"
                };
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
                return new StatusViewModel()
                {
                    ErrorMessage = "Unable to clear Schedule Event Log"
                };
            }
        }

        private StatusViewModel ProcessEmailSchedule(ScheduleItemViewModel scheduleItem)
        {
            var now = DateTime.Now;

            if (scheduleItem.WinServiceEventStatusId == WinServiceEventStatus.Paused)
                return new StatusViewModel()
                {
                    ErrorMessage = "Email Schedule Item is paused"
                };

            if (scheduleItem.WinServiceEventStatusId == WinServiceEventStatus.Running)
                return new StatusViewModel()
                {
                    ErrorMessage = "Email Schedule is currently running..."
                };

            // for future ?
            if (scheduleItem.ScheduledFor > now)
            {
                scheduleItem.LastPolledOn = now;
                scheduleItem.WinServiceEventStatusId = WinServiceEventStatus.Sleeping;
                _repository.UpsertWinServiceSchedule(scheduleItem);
                return new StatusViewModel()
                {
                    SuccessMessage = "Waiting for next Scheduled " + scheduleItem.ScheduledFor.ToString("dd-MMM-yyyy HH:mm")
                };
            }

            // overdue
            try
            {
                // mark as Running
                scheduleItem.WinServiceEventStatusId = WinServiceEventStatus.Running;
                scheduleItem.LastStartedOn = now;
                scheduleItem.LastPolledOn = now;
                _repository.UpsertWinServiceSchedule(scheduleItem);

                _repository.AddWinServiceEventLog(scheduleItem.WinServiceScheduleId, WinServiceEventStatus.Running, "Started");

                // get the site prices data

                var forDate = now.Date;

                IEnumerable<SitePriceViewModel> sitesViewModelsWithPrices = _siteService.GetSitesWithPrices(forDate);

                var pfsList = _siteService.GetJsSitesByPfsNum().ToList();
                var workbook = new JsSitesWithPricesExporter().ToExcelWorkbook(forDate, sitesViewModelsWithPrices, pfsList);
                var excelFilename = String.Format("SiteWithPrices[{0}].xlsx", forDate.ToString("dd-MMM-yyyy"));

                var emailError = SendDailyPriceEmail(scheduleItem, excelFilename, workbook);

                if (!String.IsNullOrEmpty(emailError))
                {
                    _repository.AddWinServiceEventLog(scheduleItem.WinServiceScheduleId, WinServiceEventStatus.Failed, "Email Error", emailError);

                    // mark as failed email
                    scheduleItem.WinServiceEventStatusId = WinServiceEventStatus.Failed;
                    _repository.UpsertWinServiceSchedule(scheduleItem);
                    return new StatusViewModel()
                    {
                        ErrorMessage = "Daily Price Email Failed to send"
                    };
                }

                _repository.AddWinServiceEventLog(scheduleItem.WinServiceScheduleId, WinServiceEventStatus.Success, "Finished");

                // mark as completed
                scheduleItem.WinServiceEventStatusId = WinServiceEventStatus.Success;
                scheduleItem.ScheduledFor = scheduleItem.ScheduledFor.AddDays(1);
                scheduleItem.LastCompletedOn = DateTime.Now;
                _repository.UpsertWinServiceSchedule(scheduleItem);

                return new StatusViewModel()
                {
                    SuccessMessage = "Successfully processed schedule"
                };
            }
            catch (Exception ex)
            {
                _repository.AddWinServiceEventLog(scheduleItem.WinServiceScheduleId, WinServiceEventStatus.Failed, "Exception", ex.ToString());

                scheduleItem.WinServiceEventStatusId = WinServiceEventStatus.Failed;
                _repository.UpsertWinServiceSchedule(scheduleItem);
                return new StatusViewModel()
                {
                    ErrorMessage = "Failed"
                };
            }
        }

        private string SendDailyPriceEmail(ScheduleItemViewModel scheduleItem, string excelFilename, ClosedXML.Excel.XLWorkbook workbook)
        {
            var hasEmail = !String.IsNullOrEmpty(scheduleItem.EmailAddress);
            try
            {
                var emailSettings = new EmailServiceSettings(_appSettings, _factory);

                var smtpClient = emailSettings.CreateSmtpClient();

                var message = new MailMessage();
                message.From = new MailAddress(_appSettings.EmailFrom, _appSettings.EmailFrom);
                message.Subject = "Petrol Pricing Daily Price File for " + DateTime.Now.ToShortDateString();
                message.Body = "Hi, please find attached the Daily Price File";
                message.BodyEncoding = Encoding.ASCII;
                message.IsBodyHtml = true;

                if (hasEmail)
                    message.To.Add(scheduleItem.EmailAddress);

                var memoryStream = new MemoryStream();
                workbook.SaveAs(memoryStream);
                memoryStream.Position = 0;

                message.Attachments.Add(new Attachment(memoryStream, excelFilename));

                if (hasEmail)
                    smtpClient.Send(message);

                return "";
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
            }
            return "Unable to send";
        }
    }
}
