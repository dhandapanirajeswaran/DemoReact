﻿using System;
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

        public DateTime DateOfCalc { get; set; } // DateOfCalculation (when was this calculated)
        public DateTime DateOfPrice { get; set; } // DateOfPrice (from DailyPrice)
        public DateTime? EffDate { get; set; } // Price Effective From (normally next day)
        
        // UI Concern: Don't use this Entity directly as VM for View since it holds Raw values.. View needs Pence values
        public int SuggestedPrice { get; set; } // Stored as raw value 1069 (note, we will have to convert to Pence/Raw value to/from UI), defaults to 0
        public int OverriddenPrice { get; set; } // Stored as raw value 1079, defaults to 0

        // Email/Reporting concern: Make sure we are aware or 0 values in SuggestedPrice/OverriddenPrice as we dont want 0 price going out to Stores..
    }
}
