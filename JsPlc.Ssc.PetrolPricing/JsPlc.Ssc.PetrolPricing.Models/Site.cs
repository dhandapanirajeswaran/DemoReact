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
    public class Site
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Display(Name = "Cat no")]
        public int? CatNo { get; set; } // Catalist no.

        public string Brand { get; set; }

        [Display(Name = "Site name")]
        public string SiteName { get; set; }

        public string Address { get; set; }

        public string Suburb { get; set; }

        public string Town { get; set; }

        [Display(Name = "Post code")]
        [StringLength(20)]
        [MaxLength(20)]
        public string PostCode { get; set; }
        public string Company { get; set; }
        public string Ownership { get; set; }

        [Display(Name = "Store no")]
        public int? StoreNo { get; set; }

        [Display(Name = "Pfs no")]
        [Range(1, int.MaxValue)]
        public int? PfsNo { get; set; }

        [Display(Name = "Is JS site")]
        public bool IsSainsburysSite { get; set; }

        [Display(Name = "Is active")]
        public bool IsActive { get; set; }

        public virtual ICollection<SiteEmail> Emails { get; set; }

        public virtual ICollection<SitePrice> Prices { get; set; }

        public virtual ICollection<SiteToCompetitor> Competitors { get; set; }

        public int? TrailPriceCompetitorId { get; set; }

        public double CompetitorPriceOffset { get; set; }

        public double CompetitorPriceOffsetNew { get; set; }

        public bool hasNotes
        {
            get { return String.IsNullOrWhiteSpace(Notes) == false; }
        }
        public string Notes { get; set; }

        [NotMapped]
        public bool HasNearbyCompetitorDieselPrice { get; set; }

        [NotMapped]
        public bool HasNearbyCompetitorUnleadedPrice { get; set; }

        [NotMapped]
        public bool HasNearbyCompetitorSuperUnleadedPrice { get; set; }
    }

    public class SiteEmail
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [EmailAddress(ErrorMessage = "Invalid Email Address")]
        public string EmailAddress { get; set; }

        public int SiteId { get; set; }

        [ForeignKey("SiteId")]
        public virtual Site Site { get; set; }
    }

    public class SiteToCompetitor
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        //[Key, Column(Order = 0)]
        public int SiteId { get; set; }

        //[Key, Column(Order = 1)]
        public int CompetitorId { get; set; }

        public virtual Site Site { get; set; }
        public virtual Site Competitor { get; set; }

        public float Distance { get; set; } // miles 1.42, 3.57, 2.51 etc.
        public float DriveTime { get; set; } // mins = 6.66, 10.90, 11.93 etc.
        public int Rank { get; set; } // 1,2,3,4 etc.
        public int IsExcluded { get; set; } // 0 or 1.
    }
}
