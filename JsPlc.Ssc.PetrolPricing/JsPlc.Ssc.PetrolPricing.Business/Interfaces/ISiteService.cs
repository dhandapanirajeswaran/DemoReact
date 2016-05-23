﻿using JsPlc.Ssc.PetrolPricing.Models;
using JsPlc.Ssc.PetrolPricing.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JsPlc.Ssc.PetrolPricing.Business
{
    public interface ISiteService
    {
        IEnumerable<Site> GetJsSites();

        IEnumerable<Site> GetSites();

        Dictionary<string, int> GetCompanies();

        IEnumerable<Site> GetSitesWithEmailsAndPrices(DateTime? forDate = null);

        IEnumerable<SitePriceViewModel> GetSitesWithPrices(DateTime forDate, string storeName = "", int catNo = 0, int storeNo = 0, string storeTown = "", int siteId = 0, int pageNo = 1, int pageSize = Constants.PricePageSize);

        IEnumerable<SitePriceViewModel> GetCompetitorsWithPrices(DateTime forDate, int siteId = 0, int pageNo = 1, int pageSize = Constants.PricePageSize);

        Site GetSite(int id);

        Site NewSite(Site site);

        SitePriceViewModel GetSiteAndPrices(int siteId, DateTime date, string storeName);

		bool ExistsSite(Site site);

        bool UpdateSite(Site site);

        bool HasDuplicateEmailAddresses(Site site);

		bool IsUnique(Site site);
    }
}
