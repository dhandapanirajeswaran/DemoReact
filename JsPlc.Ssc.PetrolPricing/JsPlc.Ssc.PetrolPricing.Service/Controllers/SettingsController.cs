using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Compilation;
using System.Web.Http;
using System.Web.Http.Results;
using JsPlc.Ssc.PetrolPricing.Business;
using JsPlc.Ssc.PetrolPricing.Core;
using JsPlc.Ssc.PetrolPricing.Core.Interfaces;
using JsPlc.Ssc.PetrolPricing.Models;
using JsPlc.Ssc.PetrolPricing.Repository;
using Newtonsoft.Json;
using JsPlc.Ssc.PetrolPricing.Models.ViewModels.SystemSettings;
using AutoMapper;

namespace JsPlc.Ssc.PetrolPricing.Service.Controllers
{
	public class SettingsController : ApiController
	{
		IAppSettings _appSettings;
        ISystemSettingsService _systemSettingsService;
	    private ILogger _logger;

        public SettingsController(IAppSettings appSettings, ISystemSettingsService systemSettingsService)
		{
			_appSettings = appSettings;
            _systemSettingsService = systemSettingsService;
		    _logger = new PetrolPricingLogger();
		}

		[Route("api/settings/{key}")]
		public async Task<IHttpActionResult> Get(string key)
		{
			if (String.IsNullOrEmpty(key))
			{
				return BadRequest("Invalid passed data: setting key");
			}
			try
			{
				var val = _appSettings.GetSetting(key);
				return Ok(val);
			}
			catch (Exception ex)
			{
                _logger.Error(ex);
				return new ExceptionResult(ex, this);
			}
		}

        [Route("api/GetSystemSettings")]
        public async Task<IHttpActionResult> GetSystemSettings()
        {
            try
            {
                var entity = _systemSettingsService.GetSystemSettings();
                var model = Mapper.Map<SystemSettings, SystemSettingsViewModel>(entity);
                return Ok(model);
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
                return new ExceptionResult(ex, this);
            }
        }

        [Route("api/UpdateSystemSettings")]
        public async Task<IHttpActionResult>UpdateSystemSettings(SystemSettingsViewModel model)
        {
            try
            {
                var entity = Mapper.Map<SystemSettingsViewModel, SystemSettings>(model);
                var result = _systemSettingsService.UpdateSystemSettings(entity);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
                return new ExceptionResult(ex, this);
            }
        }

        [Route("api/GetSitePricingSettings")]
        public async Task<IHttpActionResult> GetSitePricingSettings()
        {
            try
            {
                var result = _systemSettingsService.GetSitePricingSettings();
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
                return new ExceptionResult(ex, this);
            }
        }

        [Route("api/GetAllDriveTimeMarkups")]
        public async Task<IHttpActionResult> GetAllDriveTimeMarkups()
        {
            try
            {
                var result = _systemSettingsService.GetAllDriveTimeMarkups();
                var model = Mapper.Map<IEnumerable<DriveTimeMarkup>, IEnumerable<DriveTimeMarkupViewModel>>(result);
                return Ok(model);
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
                return new ExceptionResult(ex, this);
            }
        }

        [Route("api/UpdateDriveTimeMarkups")]
        public async Task<IHttpActionResult> UpdateDriveTimeMarkups(IEnumerable<DriveTimeMarkupViewModel> model)
        {
            try
            {
                var entities = Mapper.Map<IEnumerable<DriveTimeMarkupViewModel>, IEnumerable<DriveTimeMarkup>>(model);
                var result = _systemSettingsService.UpdateDriveTimeMarkups(entities);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
                return new ExceptionResult(ex, this);
            }
        }
	}
}
