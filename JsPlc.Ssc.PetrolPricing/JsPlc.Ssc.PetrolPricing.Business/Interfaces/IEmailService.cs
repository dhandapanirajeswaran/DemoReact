using JsPlc.Ssc.PetrolPricing.Models;
using JsPlc.Ssc.PetrolPricing.Models.Dtos;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.Mail;
using System.Threading.Tasks;
using JsPlc.Ssc.PetrolPricing.Models.ViewModels;

namespace JsPlc.Ssc.PetrolPricing.Business
{
    public interface IEmailService
    {
        Task<ConcurrentDictionary<int, EmailSendLog>> SendEmailAsync(List<SitePriceViewModel> listSites,
            DateTime endTradeDate,
            string reportBackEmailAddr);

        Task<List<EmailSendLog>> SaveEmailLogToRepositoryAsync(List<EmailSendLog> logEntries);

        Task<List<EmailSendLog>> GetEmailSendLog(int siteId, DateTime? forDate);

        string SendTestEmail();
    }
}
