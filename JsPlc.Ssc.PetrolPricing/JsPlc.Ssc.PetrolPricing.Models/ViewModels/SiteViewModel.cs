﻿using System;
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
        public int Id { get; set; }

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

        public int? StoreNo { get; set; }
        public int? PfsNo { get; set; }

        [DefaultValue(true)]
        [Display(Name = "Is Sainsburys site")]
        public bool IsSainsburysSite { get; set; } // defaults to false
        
        [Display(Name = "Is active")]
        public bool IsActive { get; set; } // defaults to false

        [Display(Name = "Inherit price from")]
        [DefaultValue(null)]
        public int? TrailPriceCompetitorId { get; set; }

        public ICollection<SiteEmailViewModel> Emails { get; set; }

        public List<SiteViewModel> Competitors { get; set; }

        public SiteViewModel() {
            Competitors = new List<SiteViewModel>();
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
