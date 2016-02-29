﻿using System;
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

        public static SiteEmail ToSiteEmail(this SiteEmailViewModel siteEmail)
        {
            return new SiteEmail
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

        public static List<SiteEmail> ToSiteEmailList(this List<SiteEmailViewModel> siteEmails)
        {
            IEnumerable<SiteEmail> retval = siteEmails.Select(x => x.ToSiteEmail());
            return retval.ToList();
        }

        public static List<SiteViewModel> ToSiteViewModelList(this List<Site> sites)
        {
            var sitesVm = new List<SiteViewModel>();
            sites.ForEach(site => sitesVm.Add(new SiteViewModel
            {
                Id = site.Id,
                Address = site.Address,
                Brand = site.Brand,
                CatNo = site.CatNo,
                Company = site.Company,
                Emails = site.Emails.ToList().ToSiteEmailViewModelList(),
                IsActive = site.IsActive,
                IsSainsburysSite = site.IsSainsburysSite,
                Ownership = site.Ownership,
                PfsNo = site.PfsNo,
                PostCode = site.PostCode,
                SiteName = site.SiteName,
                StoreNo = site.StoreNo,
                Suburb = site.Suburb,
                Town = site.Town
            }));
            return sitesVm;
        }

        public static SiteViewModel ToSiteViewModel(this Site site)
        {
            var siteVm = new SiteViewModel
            {
                Id = site.Id,
                Address = site.Address,
                Brand = site.Brand,
                CatNo = site.CatNo,
                Company = site.Company,
                Emails = site.Emails == null ? new List<SiteEmailViewModel>() : site.Emails.ToList().ToSiteEmailViewModelList(),
                Competitors = site.Competitors == null ? new List<SiteViewModel>() : site.Competitors.Select(x => x.Competitor).ToList().ToSiteViewModelList(),
                IsActive = site.IsActive,
                IsSainsburysSite = site.IsSainsburysSite,
                Ownership = site.Ownership,
                PfsNo = site.PfsNo,
                PostCode = site.PostCode,
                SiteName = site.SiteName,
                StoreNo = site.StoreNo,
                Suburb = site.Suburb,
                Town = site.Town,
                TrailPriceCompetitorId = site.TrailPriceCompetitorId
            };

            return siteVm;
        }

        public static Site ToSite(this SiteViewModel site)
        {
            var siteVm = new Site
            {
                Id = site.Id,
                Address = site.Address,
                Brand = site.Brand,
                CatNo = site.CatNo,
                Company = site.Company,
                Emails = site.Emails.ToList().ToSiteEmailList(),
                IsActive = site.IsActive,
                IsSainsburysSite = site.IsSainsburysSite,
                Ownership = site.Ownership,
                PfsNo = site.PfsNo,
                PostCode = site.PostCode,
                SiteName = site.SiteName,
                StoreNo = site.StoreNo,
                Suburb = site.Suburb,
                Town = site.Town,
                TrailPriceCompetitorId = site.TrailPriceCompetitorId
            };

            return siteVm;
        }

        public static List<DataRow> ToDataRowsList(this DataTable dataTable)
        {
            var rowCount = dataTable.Rows.Count;
            DataRow[] retval = { };
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
            this PriceMovementReportContainerViewModel reportContainer, string tableName = "Price Movement Report")
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
                    dr[i] = (dataItem.PriceValue / 10.0).ToString("###0.0");
                    i += 1;
                }
                dt.Rows.Add(dr);
            }
            return dt;
        }

        // No export needed for compliance report.. as per Prem.
        public static DataTable ToComplianceReportDataTable(
            this ComplianceReportContainerViewModel reportContainer, string tableName = "Compliance Report")
        {
            var dt = new DataTable(tableName);
            dt.Columns.Add("Sites");

            // Setup Table Columns - Sites  Date1   Date2   Date3...
            if (!reportContainer.ComplianceReport.ReportRows.First().DataItems.Any())
            {
                dt.Columns.Add("Status");
            }
            List<FuelType> fuels = reportContainer.ComplianceReport.ReportRows.First().DataItems.Select(x => new FuelType { Id = x.FuelTypeId, FuelTypeName = x.FuelTypeName }).ToList();

            foreach (var fuel in fuels)
            {
                dt.Columns.Add(fuel.FuelTypeName);
            }
            return dt;
        }

        public static List<DataTable> ToPricePointsReportDataTable(
        this PricePointReportContainerViewModel reportContainer)
        {
            var retval = new List<DataTable>();
            foreach (var report in reportContainer.PricePointReports)
            {
                var dt = new DataTable(report.FuelTypeName);

                if (!report.PricePointReportRows.Any())
                {
                    dt.Columns.Add("No Data");
                    var dr = dt.NewRow();
                    DateTime forDate = reportContainer.ForDate.HasValue ? reportContainer.ForDate.Value : DateTime.Now;
                    dr[0] = String.Format("Sorry, no {0} fuel data was found on {1}", report.FuelTypeName, forDate.ToString("dd-MMM-yyyy"));
                    dt.Rows.Add(dr);
                    retval.Add(dt);
                    continue;
                }

                dt.Columns.Add("Price (£)");
                // Setup Table Columns - Price(£) Brand1    Brand2   Brand3...
                foreach (var brand in report.PricePointReportRows.First().PricePointBrands)
                {
                    dt.Columns.Add(brand.Name);
                }
                // Add row data to table..
                foreach (var row in report.PricePointReportRows)
                {
                    DataRow dr = dt.NewRow();
                    // 1st column is Price value
                    dr[0] = ((row.Price / 10).ToString("###0.0"));
                    var i = 1;
                    // 2nd col onwards are brand counts
                    foreach (PricePointBrandViewModel brand in row.PricePointBrands)
                    {
                        dr[i] = brand.Count;
                        i += 1;
                    }
                    dt.Rows.Add(dr);
                }
                retval.Add(dt);
            }
            return retval;
        }

        public static DataTable ToNationalAverageReportDataTable(
            this NationalAverageReportContainerViewModel reportContainer, string tableName = "National Average")
        {
            var dt = new DataTable(tableName);
            dt.Columns.Add("FuelType");

            if (!reportContainer.NationalAverageReport.Fuels.First().Brands.Any())
            {
                dt.Columns.Add("Status");
            }

            // Setup Table Columns - Fuel Type Brand1   Brand2   Brand3...
            foreach (var brand in reportContainer.NationalAverageReport.Fuels.First().Brands)
            {
                dt.Columns.Add(brand.BrandName);
            }

            // Data Rows
            foreach (var fuelType in reportContainer.NationalAverageReport.Fuels)
            {
                DataRow dr = dt.NewRow();
                dr[0] = fuelType.FuelName;
                var i = 1;

                if (!fuelType.Brands.Any())
                {
                    dr[1] = "There is no data available";
                }
                foreach (var brand in fuelType.Brands)
                {
                    dr[i] = (brand.Average / 10.0).ToString("###0.0");
                    i += 1;
                }
                dt.Rows.Add(dr);
            }
            return dt;
        }

        public static DataTable ToCompetitorsPriceRangeByBrandDataTable(
            this NationalAverageReportContainerViewModel reportContainer, string tableName = "By Brand")
        {
            var dt = new DataTable(tableName);
            dt.Columns.Add("Brand");

            //groups
            string[] groups = { "Avg retails", "Difference", "Pricing range" };

            foreach (var group in groups)
                foreach (var fuelType in reportContainer.NationalAverageReport.Fuels)
                {
                    dt.Columns.Add(string.Format("{0} ({1})", fuelType.FuelName, group));
                }

            foreach (var brand in reportContainer.NationalAverageReport.Fuels.First().Brands)
            {
                var dr = dt.NewRow();
                dr[0] = string.Concat(brand.BrandName, "(£)");
                var i = 1;

                //avg
                foreach (var fuelType in reportContainer.NationalAverageReport.Fuels)
                {
                    var fuelBrand = fuelType.Brands.First(b => b.BrandName == brand.BrandName);
                    dr[i] = ((fuelBrand.Average / 10.0).ToString("###0.0"));
                    i++;
                }

                //difference
                foreach (var fuelType in reportContainer.NationalAverageReport.Fuels)
                {
                    var fuelBrand = fuelType.Brands.First(b => b.BrandName.Equals(brand.BrandName, StringComparison.InvariantCultureIgnoreCase));

                    int diff = fuelBrand.Average > 0 ? (fuelBrand.Average - fuelType.SainsburysPrice) : 0;

                    dr[i] = ((diff / 10.0).ToString("###0.0"));
                    i++;
                }

                //range
                foreach (var fuelType in reportContainer.NationalAverageReport.Fuels)
                {
                    var fuelBrand = fuelType.Brands.First(b => brand.BrandName.Equals(b.BrandName, StringComparison.InvariantCultureIgnoreCase));

                    dr[i] = string.Concat(((fuelBrand.Min / 10.0).ToString("###0.0")), " - ", ((fuelBrand.Max / 10.0).ToString("###0.0")));
                    i++;
                }
                dt.Rows.Add(dr);
            }

            return dt;
        }

        public static DataTable ToNationalAverageReport2DataTable(
            this NationalAverageReportContainerViewModel reportContainer, string tableName = "National Average")
        {
            var dt = new DataTable(tableName);
            dt.Columns.Add("FuelType");

            if (!reportContainer.NationalAverageReport.Fuels.First().Brands.Any())
            {
                dt.Columns.Add("Status");
            }

            // Setup Table Columns - Fuel Type Brand1   Brand2   Brand3...
            foreach (var brand in reportContainer.NationalAverageReport.Fuels.First().Brands)
            {
                dt.Columns.Add(brand.BrandName);
            }

            // Data Rows
            foreach (var fuelType in reportContainer.NationalAverageReport.Fuels)
            {
                //average fuel price row
                DataRow dr = dt.NewRow();
                dr[0] = fuelType.FuelName;
                var i = 1;

                if (!fuelType.Brands.Any())
                {
                    dr[1] = "There is no data available";
                }
                foreach (var brand in fuelType.Brands)
                {
                    dr[i] = (brand.Average / 10.0).ToString("###0.0");
                    i += 1;
                }
                dt.Rows.Add(dr);

                //price difference row
                dr = dt.NewRow();
                dr[0] = "Difference";

                i = 1;

                if (!fuelType.Brands.Any())
                {
                    dr[1] = "There is no data available";
                }
                foreach (var brand in fuelType.Brands)
                {
                    int diff = brand.Average > 0 ? (brand.Average - fuelType.SainsburysPrice) : 0;
                    dr[i] = (diff / 10.0).ToString("###0.0");
                    i += 1;
                }
                dt.Rows.Add(dr);
            }
            return dt;
        }

        public static DataTable ToCompetitorsPriceRangeByCompanyDataTable(this CompetitorsPriceRangeByCompanyViewModel source, string tableName = "By company")
        {
            var dt = new DataTable(tableName);
            dt.Columns.Add("Company");
            dt.Columns.Add("Brand");

            //groups
            string[] groups = { "Avg retails", "Difference", "Pricing range" };

            foreach (var group in groups)
                foreach (var fuelType in source.FuelTypes)
                {
                    dt.Columns.Add(string.Format("{0} ({1})", fuelType.FuelTypeName, group));
                }

            foreach (var company in source.ReportCompanies)
            {
                var dr = dt.NewRow();
                dr[0] = company.CompanyName;

                dt.Rows.Add(dr);

                foreach (var brand in company.Brands)
                {
                    dr = dt.NewRow();
                    var i = 1;
                    dr[i] = string.Concat(brand.BrandName, "(£)");
                    i++;

                    //avg
                    foreach (var fuelType in source.FuelTypes)
                    {
                        var fuelBrand = brand.Fuels.FirstOrDefault(f => f.FuelTypeId == fuelType.Id);
                        dr[i] = fuelBrand == null ? "n/a" : ((fuelBrand.Average / 10.0).ToString("###0.0"));
                        i++;
                    }

                    //difference
                    foreach (var fuelType in source.FuelTypes)
                    {
                        var fuelBrand = brand.Fuels.FirstOrDefault(f => f.FuelTypeId == fuelType.Id);

                        if (fuelBrand == null || source.SainsburysPrices.ContainsKey(fuelType.Id) == false)
                        {
                            dr[i] = "n/a";
                        }
                        else
                        {
                            int diff = fuelBrand.Average > 0 ? (fuelBrand.Average - source.SainsburysPrices[fuelType.Id]) : 0;
                            dr[i] = ((diff / 10.0).ToString("###0.0"));
                        }
                        i++;
                    }

                    //range
                    foreach (var fuelType in source.FuelTypes)
                    {
                        var fuelBrand = brand.Fuels.FirstOrDefault(f => f.FuelTypeId == fuelType.Id);

                        dr[i] = fuelBrand == null ? "n/a" : string.Concat(((fuelBrand.Min / 10.0).ToString("###0.0")), " - ", ((fuelBrand.Max / 10.0).ToString("###0.0")));
                        i++;
                    }
                    dt.Rows.Add(dr);
                }
            }

            return dt;
        }
    }
}
