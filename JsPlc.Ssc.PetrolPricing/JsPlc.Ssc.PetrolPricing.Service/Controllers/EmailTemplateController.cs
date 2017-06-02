using JsPlc.Ssc.PetrolPricing.Business.Interfaces;
using JsPlc.Ssc.PetrolPricing.Core;
using JsPlc.Ssc.PetrolPricing.Core.Interfaces;
using JsPlc.Ssc.PetrolPricing.Models;
using System;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Results;

namespace JsPlc.Ssc.PetrolPricing.Service.Controllers
{
    public class EmailTemplateController : ApiController
    {
        private IEmailTemplateService _emailTemplateService;
        private ILogger _logger;

        public EmailTemplateController(IEmailTemplateService emailTemplateService)
        {
            _emailTemplateService = emailTemplateService;
            _logger = new PetrolPricingLogger();
        }

        [System.Web.Http.HttpGet]
        [Route("api/GetEmailTemplateNames")]
        public async Task<IHttpActionResult> GetEmailTemplateNames()
        {
            try
            {
                var result = _emailTemplateService.GetEmailTemplateNames();
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
                return new ExceptionResult(ex, this);
            }
        }

        [System.Web.Http.HttpGet]
        [Route("api/CreateEmailTemplateClone/{ppUserId}/{emailTemplateId}/{templateName}")]
        public async Task<IHttpActionResult> CreateEmailTemplateClone([FromUri] int ppUserId, [FromUri] int emailTemplateId, [FromUri] string templateName)
        {
            try
            {
                var result = _emailTemplateService.CreateEmailTemplateClone(ppUserId, emailTemplateId, templateName);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
                return new ExceptionResult(ex, this);
            }
        }

        [System.Web.Http.HttpGet]
        [Route("api/GetEmailTemplate/{emailTemplateId}")]
        public async Task<IHttpActionResult> GetEmailTemplate([FromUri] int emailTemplateId)
        {
            try
            {
                var result = _emailTemplateService.GetEmailTemplate(emailTemplateId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
                return new ExceptionResult(ex, this);
            }
        }

        [System.Web.Http.HttpPost]
        [Route("api/UpdateEmailTemplate")]
        public async Task<IHttpActionResult> UpdateEmailTemplate([FromBody] EmailTemplate emailTemplate)
        {
            try
            {
                var rawTemplate = new EmailTemplate()
                {
                    EmailTemplateId = emailTemplate.EmailTemplateId,
                    IsDefault = emailTemplate.IsDefault,
                    TemplateName = emailTemplate.TemplateName,
                    PPUserId = emailTemplate.PPUserId,
                    SubjectLine = System.Web.HttpUtility.HtmlDecode(emailTemplate.SubjectLine),
                    EmailBody = System.Web.HttpUtility.HtmlDecode(emailTemplate.EmailBody)
                };

                var result = _emailTemplateService.UpdateEmailTemplate(rawTemplate);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
                return new ExceptionResult(ex, this);
            }
        }

        [System.Web.Http.HttpGet]
        [Route("api/DeleteEmailTemplate/{ppUserId}/{emailTemplateId}")]
        public async Task<IHttpActionResult> DeleteEmailTemplate([FromUri] int ppUserId, [FromUri] int emailTemplateId)
        {
            try
            {
                var result = _emailTemplateService.DeleteEmailTemplate(ppUserId, emailTemplateId);
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