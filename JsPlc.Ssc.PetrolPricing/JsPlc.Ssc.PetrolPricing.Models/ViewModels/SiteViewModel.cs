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
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public int? CatNo { get; set; } // Catalist no.
        public string Brand { get; set; }

        [Required]
        public string SiteName { get; set; }

        public string Address { get; set; }
        public string Suburb { get; set; }
        
        [Required]
        public string Town { get; set; }

        public string PostCode { get; set; }
        public string Company { get; set; }
        public string Ownership { get; set; }

        public int? StoreNo { get; set; }
        public int? PfsNo { get; set; }

        [DefaultValue(true)]
        public bool IsSainsburysSite { get; set; } // defaults to false
        public bool IsActive { get; set; } // defaults to false

        public ICollection<SiteEmailViewModel> Emails { get; set; }
    }
    public class SiteEmailViewModel
    {
        public int Id { get; set; }

        [EmailAddress(ErrorMessage = "Invalid Email Address")]
        public string EmailAddress { get; set; }

        public int SiteId { get; set; }

    }

}
