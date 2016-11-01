using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;

namespace JsPlc.Ssc.PetrolPricing.Models
{
    public class LatestPrice
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public int? UploadId { get; set; }
        public FileUpload DailyUpload { get; set; } // DateOfUpload is more significant, apparently, than DateOfPrice..

        public int PfsNo { get; set; }
        public int StoreNo { get; set; }

        public int FuelTypeId { get; set; }
        public FuelType FuelType { get; set; }

       
        public int ModalPrice { get; set; }
         
  
        
    }
}
