using JsPlc.Ssc.PetrolPricing.Core;
using JsPlc.Ssc.PetrolPricing.Core.Interfaces;
using JsPlc.Ssc.PetrolPricing.Models;
using JsPlc.Ssc.PetrolPricing.Models.ViewModels;
using JsPlc.Ssc.PetrolPricing.Portal.Controllers.BaseClasses;
using JsPlc.Ssc.PetrolPricing.Portal.Facade;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Script.Services;

namespace JsPlc.Ssc.PetrolPricing.Portal.Controllers
{
    public class EmailTemplateController : BaseController
    {
        private readonly ServiceFacade _serviceFacade;
        private readonly ILogger _logger;

        public EmailTemplateController()
        {
            _logger = new PetrolPricingLogger();
            _serviceFacade = new ServiceFacade(_logger);
        }

        [ScriptMethod(UseHttpGet = true)]
        public async Task<JsonResult> CreateEmailTemplateClone([FromUri] int emailTemplateId, [FromUri] string templateName)
        {
            var response = await _serviceFacade.CreateEmailTemplateClone(PPUserId, emailTemplateId, templateName);
            return base.StandardJsonResultMessage(response);
        }

        [ScriptMethod(UseHttpGet = true)]
        public async Task<JsonResult> GetEmailTemplateNames()
        {
            var response = await _serviceFacade.GetEmailTemplateNames();
            return base.StandardJsonResultMessage(response);
        }

        [ScriptMethod(UseHttpGet = true)]
        public async Task<JsonResult> GetEmailTemplate([FromUri] int emailTemplateId)
        {
            var response = await _serviceFacade.GetEmailTemplate(emailTemplateId);
            return base.StandardJsonResultMessage(response);
        }

        [ScriptMethod(UseHttpGet = false)]
        public async Task<JsonResult> UpdateEmailTemplate([FromBody] EmailTemplateViewModel template)
        {
            var response = await _serviceFacade.UpdateEmailTemplate(template);
            return base.StandardJsonResultMessage(response);
        }

        [ScriptMethod(UseHttpGet = true)]
        public async Task<JsonResult> DeleteEmailTemplate([FromUri] int emailTemplateId)
        {
            var response = await _serviceFacade.DeleteEmailTemplate(PPUserId, emailTemplateId);
            return base.StandardJsonResultMessage(response);
        }

        #region private methods

        private int PPUserId
        {
            get
            {
                return base.GetUserAccessModel().PPUserId;
            }
        }

        #endregion private methods
    }
}