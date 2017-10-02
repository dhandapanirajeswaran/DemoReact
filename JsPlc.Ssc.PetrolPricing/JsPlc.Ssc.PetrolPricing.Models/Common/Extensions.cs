using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Net.Mail;
using System.Threading;
using JsPlc.Ssc.PetrolPricing.Models.Dtos;
using JsPlc.Ssc.PetrolPricing.Models.ViewModels;
using System.Globalization;
using JsPlc.Ssc.PetrolPricing.Models.Enums;

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
                Town = site.Town,
                CompetitorPriceOffset = site.CompetitorPriceOffset,
                TrailPriceCompetitorId = site.TrailPriceCompetitorId,
                PriceMatchType = (PriceMatchType)site.PriceMatchType
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
                ExcludeCompetitors = site.Competitors == null ? new List<int>() : (from competitor in site.Competitors where competitor.IsExcluded == 1 select competitor).Select(x => x.Competitor.Id).ToList(),
                IsActive = site.IsActive,
                IsSainsburysSite = site.IsSainsburysSite,
                Ownership = site.Ownership,
                PfsNo = site.PfsNo,
                PostCode = site.PostCode,
                SiteName = site.SiteName,
                StoreNo = site.StoreNo,
                Suburb = site.Suburb,
                Town = site.Town,
                TrailPriceCompetitorId = site.TrailPriceCompetitorId,
                CompetitorPriceOffsetNew = site.CompetitorPriceOffsetNew,
                CompetitorPriceOffset = site.CompetitorPriceOffset,
                PriceMatchType = (PriceMatchType)site.PriceMatchType
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
                TrailPriceCompetitorId = site.TrailPriceCompetitorId,
                CompetitorPriceOffsetNew = site.CompetitorPriceOffsetNew,
                CompetitorPriceOffset = site.CompetitorPriceOffset,
                PriceMatchType = (int)site.PriceMatchType
            };

            return siteVm;
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
            dt.Columns.Add("StoreNo");
            dt.Columns.Add("PfsNo");
            dt.Columns.Add("CatNo");

            // Setup Table Columns - Sites  Date1   Date2   Date3...
            if (reportContainer.PriceMovementReport.ReportRows.Count == 0 || !reportContainer.PriceMovementReport.ReportRows.First().DataItems.Any())
            {
                dt.Columns.Add("Status");
            }
            var datesAsString = reportContainer.PriceMovementReport.Dates.Select(x => x.ToString("dd-MMM-yyyy")).ToArray();
            foreach (var dateString in datesAsString)
            {
                dt.Columns.Add("U_" + dateString);
                dt.Columns.Add("D_" + dateString);
                dt.Columns.Add("S_" + dateString);
            }
            foreach (var siteRow in reportContainer.PriceMovementReport.ReportRows)
            {
                DataRow dr = dt.NewRow();
                dr[0] = siteRow.SiteName;
                dr[1] = siteRow.StoreNo.HasValue ? siteRow.StoreNo.Value.ToString() : "n/a";
                dr[2] = siteRow.PfsNo.HasValue ? siteRow.PfsNo.Value.ToString() : "n/a";
                dr[3] = siteRow.CatNo.HasValue ? siteRow.CatNo.Value.ToString() : "n/a";

                var i = 4;
                foreach (var dataItem in siteRow.DataItems)
                {
                    var item = dr[i];
                    foreach (var fuelPrice in dataItem.FuelPrices)
                    {
                        dr[i] = (fuelPrice.PriceValue / 10.0).ToString("###0.0");
                        i++;
                    }
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

                dt.Columns.Add("Brands");
                // Setup Table Columns - Price(£) Brand1    Brand2   Brand3...
                foreach (var priceVM in report.PricePointReportRows.First().PricePointPrices)
                {
                    dt.Columns.Add(((priceVM.Price / 10).ToString("###0.0")));
                }
                // Add row data to table..
                foreach (var row in report.PricePointReportRows)
                {
                    DataRow dr = dt.NewRow();
                    // 1st column is Price value
                    dr[0] = row.Brand;
                    var i = 1;
                    // 2nd col onwards are brand counts
                    foreach (PricePointPriceViewModel priceVM in row.PricePointPrices)
                    {
                        dr[i] = priceVM.Count;
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
            dt.Columns.Add("Date");
            dt.Columns.Add("Day");
            dt.Columns.Add("Brands");

            foreach (var fuelType in reportContainer.NationalAverageReport.Fuels)
            {
                dt.Columns.Add(fuelType.FuelName + " (£)");
            }
            foreach (var fuelType in reportContainer.NationalAverageReport.Fuels)
            {
                dt.Columns.Add("Variance " + fuelType.FuelName + " (£)");
            }

            dt.Columns.Add("Average Variance (£)");
            DataRow dr = dt.NewRow();

            dr[0] = reportContainer.ForDate.Value.ToString("dd-MMM");
            dr[1] = reportContainer.ForDate.Value.DayOfWeek;

            int nRowCount = 0;
            // Setup Table Columns - Fuel Type Brand1   Brand2   Brand3...
            foreach (var brand in reportContainer.NationalAverageReport.Fuels.First().Brands)
            {
                int i = 2;
                if (nRowCount > 0) dr = dt.NewRow();
                dr[i++] = brand.BrandName;
                if (brand.BrandName != "All other independent brands" && brand.BrandName != null)
                {
                    foreach (var fuelType in reportContainer.NationalAverageReport.Fuels)
                    {
                        var brandfromFType = fuelType.Brands.Where(x => x.BrandName == @brand.BrandName).FirstOrDefault();
                        dr[i++] = ((brandfromFType.Average / 10.0).ToString("###0.0"));
                    }
                    int average = 0;
                    foreach (var fuelType in reportContainer.NationalAverageReport.Fuels)
                    {
                        var brandfromFType = fuelType.Brands.Where(x => x.BrandName == @brand.BrandName).FirstOrDefault();
                        int diff = brandfromFType.Average > 0 ? (brandfromFType.Average - fuelType.SainsburysPrice) : 0;
                        average += diff;
                        dr[i++] = ((diff / 10.0).ToString("###0.0"));
                    }
                    average = average / 2;
                    dr[i++] = ((average / 10.0).ToString("###0.0"));
                }
                dt.Rows.Add(dr);
                nRowCount++;
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
            dt.Columns.Add("   -   ");
            dt.Columns.Add("   -        ");
            foreach (var brand in reportContainer.NationalAverageReport.Fuels.First().Brands)
            {
                dt.Columns.Add(brand.BrandName);
                dt.Columns.Add(brand.BrandName + " ");
            }
            DataRow dr = dt.NewRow();
            dr[0] = "Date";
            dr[1] = "Day";
            if (!reportContainer.NationalAverageReport.Fuels.First().Brands.Any())
            {
                dt.Columns.Add("Status");
            }

            int i = 2;
            // Setup Table Columns - Fuel Type Brand1   Brand2   Brand3...
            foreach (var brand in reportContainer.NationalAverageReport.Fuels.First().Brands)
            {
                if (i <= 8)
                {
                    dr[i] = brand.BrandName.Replace("SAINSBURYS", "JS") + " Unl";
                    dr[i + 1] = brand.BrandName.Replace("SAINSBURYS", "JS") + " Derv";
                }
                else
                {
                    dr[i] = brand.BrandName + " U";
                    dr[i + 1] = brand.BrandName + " D";
                }
                i = i + 2;
            }
            dt.Rows.Add(dr);

            dr = dt.NewRow();
            dr[0] = reportContainer.ForDate.Value.ToString("dd-MMM");
            dr[1] = reportContainer.ForDate.Value.DayOfWeek;
            i = 2;
            foreach (var fuelType in reportContainer.NationalAverageReport.Fuels)
            {
                foreach (var brand in fuelType.Brands)
                {
                    dr[i] = (brand.Average / 10.0).ToString("###0.00");
                    i += 2;
                }
                i = 3;
            }
            dt.Rows.Add(dr);

            dr = dt.NewRow();
            i = 4;
            foreach (var fuelType in reportContainer.NationalAverageReport.Fuels)
            {
                foreach (var brand in fuelType.Brands)
                {
                    if (brand.BrandName == "SAINSBURYS") continue;
                    int diff = brand.Average > 0 ? (brand.Average - fuelType.SainsburysPrice) : 0;
                    dr[i] = (diff / 10.0).ToString("###0.00");
                    i += 2;
                }
                i = 5;
            }

            dt.Rows.Add(dr);
            return dt;
        }

        public static DataTable ToCompetitorSitesDataTable(
          this CompetitorSiteReportViewModel reportContainer, string tableName = "CompetitorSites")
        {
            var dt = new DataTable(tableName);
            dt.Columns.Add("Brand Name");
            dt.Columns.Add("0-5");
            dt.Columns.Add("5-10");
            dt.Columns.Add("10-15");
            dt.Columns.Add("15-20");
            dt.Columns.Add("20-25");
            dt.Columns.Add("25-30");
            dt.Columns.Add(">30");

            foreach (var brandTimes in reportContainer.BrandTimes)
            {
                DataRow dr = dt.NewRow();
                dr[0] = brandTimes.BrandName;
                dr[1] = brandTimes.Count0To5;
                dr[2] = brandTimes.Count5To10;
                dr[3] = brandTimes.Count10To15;
                dr[4] = brandTimes.Count15To20;
                dr[5] = brandTimes.Count20To25;
                dr[6] = brandTimes.Count25To30;
                dr[7] = brandTimes.CountMoreThan30;
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

        public static List<DataTable> ToQuarterlySiteAnalysisDataTable(this QuarterlySiteAnalysisContainerViewModel source, string tableName = "Quarterly Site Analysis")
        {
            var dt = new DataTable(tableName);
            dt.Columns.Add("CatNo");
            dt.Columns.Add("SiteName");
            dt.Columns.Add("LeftOwnership");
            dt.Columns.Add("RightOwnership");
            dt.Columns.Add("HasOwnershipChanged");
            dt.Columns.Add("WasSiteAdded");
            dt.Columns.Add("WasSiteDeleted");
            dt.Columns.Add("Changed");

            foreach (var row in source.Report.Rows)
            {
                var dr = dt.NewRow();
                dr[0] = row.CatNo;
                dr[1] = row.SiteName;
                dr[2] = row.LeftOwnership;
                dr[3] = row.RightOwnership;
                dr[4] = row.HasOwnershipChanged;
                dr[5] = row.WasSiteAdded;
                dr[6] = row.WasSiteDeleted;
                dr[7] = row.HasOwnershipChanged || row.WasSiteAdded || row.WasSiteDeleted;

                dt.Rows.Add(dr);
            }

            return new List<DataTable>()
            {
                dt
            };
        }

        public static DataTable ToComplianceReport(this ComplianceReportViewModel report, string tableName = "Compliance")
        {
            var dt = new DataTable(tableName);
            dt.Columns.Add("PfsNo");
            dt.Columns.Add("CatNo");
            dt.Columns.Add("StoreNo");
            dt.Columns.Add("SiteName");
            dt.Columns.Add("Complies");

            dt.Columns.Add("Unleaded Catalist");
            dt.Columns.Add("Unleaded Expected");
            dt.Columns.Add("Unleaded Difference");

            dt.Columns.Add("Diesel Catalist");
            dt.Columns.Add("Diesel Expected");
            dt.Columns.Add("Diesel Difference");

            foreach (var row in report.ReportRows)
            {
                var compliesString = row.DataItems.Take(2).Any(x => x.DiffValid && x.Diff == 0) ? "Yes" : "No";

                var unleaded = row.DataItems.FirstOrDefault(x => x.FuelTypeId == (int)FuelTypeItem.Unleaded);
                var diesel = row.DataItems.FirstOrDefault(x => x.FuelTypeId == (int)FuelTypeItem.Diesel);

                var dr = dt.NewRow();
                var index = 0;

                dr[index++] = row.PfsNo;
                dr[index++] = row.CatNo;
                dr[index++] = row.StoreNo;
                dr[index++] = row.SiteName;
                dr[index++] = compliesString;

                index = AddComplianceReportFuel(index, dr, unleaded);
                index = AddComplianceReportFuel(index, dr, diesel);

                dt.Rows.Add(dr);
            }

            return dt;
        }

        #region private methods

        private static int AddComplianceReportFuel(int index, DataRow row, ComplianceReportDataItem dataItem)
        {
            var catalistString = "";
            var expectedString = "";
            var differenceString = "";

            if (dataItem != null)
            {
                catalistString = dataItem.FoundCatPrice ? (dataItem.CatPriceValue / 10.0).ToString("###0.0") : "n/a";
                expectedString = dataItem.FoundExpectedPrice ? (dataItem.ExpectedPriceValue / 10.0).ToString("###0.0") : "n/a";
                differenceString = dataItem.DiffValid ? dataItem.Diff.ToString("0.0") : "n/a";
            }

            row[index++] = catalistString;
            row[index++] = expectedString;
            row[index++] = differenceString;

            return index;
        }

        #endregion private methods
    }
}