using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;

namespace JsPlc.Ssc.PetrolPricing.Models
{
    public class Site
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public string SiteName { get; set; }

        [Required]
        public string Town { get; set; }

        public int? CatNo { get; set; } // Catalist no.
        public int? StoreNo { get; set; }
        public int? PfsNo { get; set; }
        public string Brand { get; set; }
        public string Company { get; set; }
        public string Ownership { get; set; }
        public string Address { get; set; }
        public string Suburb { get; set; }
        public string PostCode { get; set; }

        public bool IsSainsburysSite { get; set; } // defaults to false
        public bool IsActive { get; set; } // defaults to false
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
    }
}
