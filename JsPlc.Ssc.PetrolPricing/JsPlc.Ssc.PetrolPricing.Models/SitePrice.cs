using JsPlc.Ssc.PetrolPricing.Models.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;

namespace JsPlc.Ssc.PetrolPricing.Models
{
    public class SitePrice
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public int SiteId { get; set; }
        public Site JsSite { get; set; } // JSSite

        public int FuelTypeId { get; set; }
        public FuelType FuelType { get; set; } // FuelType

        public DateTime DateOfCalc { get; set; } // DateOfCalculation (when was this calculated)
        public DateTime DateOfPrice { get; set; } // DateOfPrice (from DailyPrice - as per UploadDateTime), we dont use Dates specified in Catalist info

        public int? UploadId { get; set; } // Which uploadId did this price come from

        public DateTime? EffDate { get; set; } // Price Effective From (normally next day)
        
        // UI Concern: Don't use this Entity directly as VM for View since it holds Raw values.. View needs Pence values
        public int SuggestedPrice { get; set; } // Stored as raw value 1069 (note, we will have to convert to Pence/Raw value to/from UI), defaults to 0
        public int OverriddenPrice { get; set; } // Stored as raw value 1079, defaults to 0

        // Email/Reporting concern: Make sure we are aware or 0 values in SuggestedPrice/OverriddenPrice as we dont want 0 price going out to Stores..

        public int? CompetitorId { get; set; } // selected competitor id

        public int Markup { get; set; } // selected competitor markup to the site

        public bool IsTrailPrice { get; set; } // if true, then trial price has been selected 

        public PriceReasonFlags  PriceReasonFlags { get; set; }
    }
}
