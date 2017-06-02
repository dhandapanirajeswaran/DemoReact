using JsPlc.Ssc.PetrolPricing.Business.Interfaces;
using JsPlc.Ssc.PetrolPricing.Core;
using JsPlc.Ssc.PetrolPricing.Core.Interfaces;
using JsPlc.Ssc.PetrolPricing.Models;
using JsPlc.Ssc.PetrolPricing.Repository;
using System.Collections.Generic;

namespace JsPlc.Ssc.PetrolPricing.Business.Services
{
    public class EmailTemplateService : IEmailTemplateService
    {
        protected readonly IPetrolPricingRepository _db;
        protected readonly ILogger _logger;

        public EmailTemplateService(IPetrolPricingRepository db)
        {
            _db = db;
            _logger = new PetrolPricingLogger();
        }

        #region implementation of IEmailTemplateService

        public EmailTemplate CreateEmailTemplateClone(int ppUserId, int emailTemplateId, string templateName)
        {
            return _db.CreateEmailTemplateClone(ppUserId, emailTemplateId, templateName);
        }

        public EmailTemplate GetEmailTemplate(int emailTemplateId)
        {
            return _db.GetEmailTemplate(emailTemplateId);
        }

        public IEnumerable<EmailTemplateName> GetEmailTemplateNames()
        {
            return _db.GetEmailTemplateNames();
        }

        public EmailTemplate UpdateEmailTemplate(EmailTemplate template)
        {
            return _db.UpdateEmailTemplate(template);
        }

        public bool DeleteEmailTemplate(int ppUserId, int emailTemplateId)
        {
            return _db.DeleteEmailTemplate(ppUserId, emailTemplateId);
        }

        #endregion implementation of IEmailTemplateService
    }
}