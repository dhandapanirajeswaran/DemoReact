using JsPlc.Ssc.PetrolPricing.Models;
using System.Collections.Generic;

namespace JsPlc.Ssc.PetrolPricing.Business.Interfaces
{
    public interface IEmailTemplateService
    {
        IEnumerable<EmailTemplateName> GetEmailTemplateNames();

        EmailTemplate CreateEmailTemplateClone(int ppUserId, int emailTemplateId, string templateName);

        EmailTemplate GetEmailTemplate(int emailTemplateId);

        EmailTemplate UpdateEmailTemplate(EmailTemplate template);

        bool DeleteEmailTemplate(int ppUserId, int emailTemplateId);
    }
}