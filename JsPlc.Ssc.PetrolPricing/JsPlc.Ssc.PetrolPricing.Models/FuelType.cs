using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;

namespace JsPlc.Ssc.PetrolPricing.Models
{
    public class FuelType
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public string FuelTypeName { get; set; } // 1=Super, 2=Unleaded, 5=Super Dis, 6=Std Dis, 7=LPG
    }
}
