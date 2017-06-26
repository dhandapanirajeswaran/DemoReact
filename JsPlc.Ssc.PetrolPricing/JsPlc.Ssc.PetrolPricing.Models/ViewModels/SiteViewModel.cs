﻿using JsPlc.Ssc.PetrolPricing.Models.Enums;
using JsPlc.Ssc.PetrolPricing.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.Core.Metadata.Edm;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;

namespace JsPlc.Ssc.PetrolPricing.Models
{
    public class SiteViewModel
    {
        public SiteSectionType CalledFromSection { get; set; }

        public RecentFileUploadSummary RecentFileUploads { get; set; }

        public int Id { get; set; }

        [Required]
        [Display(Name = "Cat no")]
        public int? CatNo { get; set; } // Catalist no.
        public string Brand { get; set; }

        [Required]
        [Display(Name = "Site name")]
        public string SiteName { get; set; }

        public string Address { get; set; }
        public string Suburb { get; set; }

        [Required]
        public string Town { get; set; }

        [Display(Name = "Post code")]
        public string PostCode { get; set; }
        public string Company { get; set; }
        public string Ownership { get; set; }

        [Display(Name = "Store no")]
        [Range(1, int.MaxValue)]
        public int? StoreNo { get; set; }

        [Display(Name = "Pfs no")]
        [Range(1, int.MaxValue)]
        public int? PfsNo { get; set; }

        [DefaultValue(true)]
        [Display(Name = "Is JS site")]
        public bool IsSainsburysSite { get; set; } // defaults to false

        [Display(Name = "Is active")]
        public bool IsActive { get; set; } // defaults to false

        [Display(Name = "Match competitor")]
        [DefaultValue(null)]
        public int? TrailPriceCompetitorId { get; set; }

        public ICollection<SiteEmailViewModel> Emails { get; set; }

        public List<SiteViewModel> Competitors { get; set; }


        [Display(Name = "Exclude Competitors")]
        [DefaultValue(null)]
        public List<int> ExcludeCompetitors { get; set; }

        public List<int> ExcludeCompetitorsOrg { get; set; }


        [Display(Name = "Exclude Brands")]
        [DefaultValue(null)]
        public List<string> ExcludeBrands { get; set; }

        public List<string> ExcludeBrandsOrg { get; set; }

        public List<string> AllBrands { get; set; }

        [Display(Name = "Trial price (+/-)")]
        [Range(-4000, 4000)]
        public double CompetitorPriceOffset { get; set; }


        [Display(Name = "Competitor Trial price (+/-)")]
        [Range(-4000, 4000)]
        public double CompetitorPriceOffsetNew { get; set; }

        public string Notes { get; set; }

        public bool HasNearbyUnleadedGrocers { get; set; }
        public bool HasNearbyUnleadedGrocersPriceData { get; set; }

        public bool HasNearbyDieselGrocers { get; set; }
        public bool HasNearbyDieselGrocersPriceData { get; set; }

        public bool HasNearbySuperUnleadedGrocers { get; set; }
        public bool HasNearbySuperUnleadedGrocersPriceData { get; set; }

        [Required]
        [Display(Name ="Price Match Type")]
        public PriceMatchType PriceMatchType { get; set; }

        public SiteViewModel()
        {
            this.CalledFromSection = SiteSectionType.None;
            this.Competitors = new List<SiteViewModel>();
            this.ExcludeCompetitors = new List<int>();
            this.RecentFileUploads = new RecentFileUploadSummary();
        }
    }
    public class SiteEmailViewModel
    {
        public int Id { get; set; }

        [EmailAddress(ErrorMessage = "Invalid Email Address")]
        public string EmailAddress { get; set; }

        public int SiteId { get; set; }

    }

}
