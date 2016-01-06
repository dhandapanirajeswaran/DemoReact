using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;

namespace JsPlc.Ssc.PetrolPricing.Models.Dtos
{
    public class EmailToSet
    {
        public string FixedEmailTo { get; set; }
        public List<string> ListOfEmailTo { get; set; }
        public string CommaSeprListOfEmailTo { get; set; }
    }

}
