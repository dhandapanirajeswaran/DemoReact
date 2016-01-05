using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;

namespace JsPlc.Ssc.PetrolPricing.Models.Common
{
    public static class Extensions
    {
        public static SiteEmailViewModel ToSiteEmailViewModel(this SiteEmail siteEmail)
        {
            return new SiteEmailViewModel
            {
                Id = siteEmail.Id, EmailAddress = siteEmail.EmailAddress, SiteId = siteEmail.SiteId
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
    }
}
