using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Net.Mail;
using System.Threading;
using JsPlc.Ssc.PetrolPricing.Models.Dtos;
using JsPlc.Ssc.PetrolPricing.Models.ViewModels;

namespace JsPlc.Ssc.PetrolPricing.Models.Common
{
    public static class Extensions
    {
        public static SiteEmailViewModel ToSiteEmailViewModel(this SiteEmail siteEmail)
        {
            return new SiteEmailViewModel
            {
                Id = siteEmail.Id,
                EmailAddress = siteEmail.EmailAddress,
                SiteId = siteEmail.SiteId
            };
        }

        public static List<SiteEmailViewModel> ToSiteEmailViewModelList(this List<SiteEmail> siteEmails)
        {
            IEnumerable<SiteEmailViewModel> retval = siteEmails.Select(x => x.ToSiteEmailViewModel());
            return retval.ToList();
        }

        public static List<SiteViewModel> ToSiteViewModelList(this List<Site> sites)
        {
            var sitesVm = new List<SiteViewModel>();
            sites.ForEach(x => sitesVm.Add(new SiteViewModel
            {
                Id = x.Id,
                Address = x.Address,
                Brand = x.Brand,
                CatNo = x.CatNo,
                Company = x.Company,
                Emails = x.Emails.ToList().ToSiteEmailViewModelList(),
                IsActive = x.IsActive,
                IsSainsburysSite = x.IsSainsburysSite,
                Ownership = x.Ownership,
                PfsNo = x.PfsNo,
                PostCode = x.PostCode,
                SiteName = x.SiteName,
                StoreNo = x.StoreNo,
                Suburb = x.Suburb,
                Town = x.Town
            }));
            return sitesVm;
        }

        public static List<DataRow> ToDataRowsList(this DataTable dataTable)
        {
            var rowCount = dataTable.Rows.Count;
            DataRow[] retval = {};
            if (rowCount <= 0) return retval.ToList();

            var rowsArr = new DataRow[rowCount];
            dataTable.Rows.CopyTo(rowsArr, 0);
            retval = rowsArr;
            return retval.ToList();
        }

        public static EmailSendLog AddErrorMessageToLogEntry(this EmailSendLog logEntry, string message)
        {
            logEntry.IsError = true;
            logEntry.ErrorMessage += message;
            return logEntry;
        }

        public static EmailSendLog AddWarningMessageToLogEntry(this EmailSendLog logEntry, string message)
        {
            logEntry.IsWarning = true;
            logEntry.WarningMessage += message;
            return logEntry;
        }

        public static EmailSendLog SetSuccessful(this EmailSendLog logEntry)
        {
            logEntry.IsSuccess = true;
            return logEntry;
        }

        /// <summary>
        /// Setsup a log entry object
        /// We can add errorMessage and status to it on SendCompleted
        /// </summary>
        /// <param name="logEntry"></param>
        /// <param name="siteId"></param>
        /// <param name="message"></param>
        /// <param name="emailToSet"></param>
        /// <param name="endTradeDate"></param>
        /// <param name="loginUser"></param>
        /// <param name="sendDateTime"></param>
        /// <param name="errMessage"></param>
        /// <param name="warningMessage"></param>
        /// <returns></returns>
        public static EmailSendLog SetupLogEntry1(this EmailSendLog logEntry, int siteId, DateTime endTradeDate,
            string loginUser, DateTime sendDateTime, string errMessage = "", string warningMessage = "")
        {
            logEntry.SiteId = siteId;
            logEntry.EndTradeDate = endTradeDate;
            logEntry.LoginUser = loginUser;
            logEntry.SendDate = sendDateTime;
            if (errMessage != "") logEntry.AddErrorMessageToLogEntry(errMessage);
            if (warningMessage != "") logEntry.AddWarningMessageToLogEntry(warningMessage);
            return logEntry;
        }

        public static EmailSendLog SetupLogEntry2(this EmailSendLog logEntry, MailMessage message, EmailToSet emailToSet)
        {
            logEntry.EmailBody = message.Body;
            logEntry.EmailFrom = message.From.Address;
            logEntry.EmailSubject = message.Subject;
            logEntry.FixedEmailTo = emailToSet.FixedEmailTo;
            logEntry.ListOfEmailTo = emailToSet.CommaSeprListOfEmailTo;
            logEntry.IsTest = !String.IsNullOrEmpty(emailToSet.FixedEmailTo);
            return logEntry;
        }

        /// <summary>
        /// Creates a summary string for email send task from the EmailSendLog..
        /// </summary>
        /// <param name="logEntries"></param>
        /// <returns></returns>
        public static string ToSendSummary(this List<EmailSendLog> logEntries)
        {
            var retval = "";
            var totalCount = logEntries.Count;
            var successCount = logEntries.Count(x => x.IsSuccess);
            var warningCount = logEntries.Count(x => x.IsWarning);
            var errorCount = logEntries.Count(x => x.IsError);
            retval = String.Format("Email send summary: \n Successful: {1} of {0} \n Errors: {2} of {0} \n Warnings: {3} of {0} \n ",
                totalCount, successCount, errorCount, warningCount);
            return retval;
        }

        public static DataTable ToPriceMovementReportDataTable(
            this PriceMovementReportContainerViewModel reportContainer, string tableName = "PriceMovementReport")
        {
            var dt = new DataTable(tableName);
            dt.Columns.Add("Sites");

            // Setup Table Columns - Sites  Date1   Date2   Date3...
            if (!reportContainer.PriceMovementReport.ReportRows.First().DataItems.Any())
            {
                dt.Columns.Add("Status");
            }
            var datesAsString = reportContainer.PriceMovementReport.Dates.Select(x => x.ToString("dd-MMM-yyyy")).ToArray();
            foreach (var dateString in datesAsString)
            {
                dt.Columns.Add(dateString);
            }
            foreach (var siteRow in reportContainer.PriceMovementReport.ReportRows)
            {
                DataRow dr = dt.NewRow();
                dr[0] = siteRow.SiteName;
                var i = 1;
                foreach (var dataItem in siteRow.DataItems)
                {
                    dr[i] = (dataItem.PriceValue/10.0).ToString("###0.0");
                    i += 1;
                }
                dt.Rows.Add(dr);
            }
            return dt;
        }
    }
}
