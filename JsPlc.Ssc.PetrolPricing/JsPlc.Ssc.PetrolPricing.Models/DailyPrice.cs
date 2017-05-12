using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;

namespace JsPlc.Ssc.PetrolPricing.Models
{
    public class DailyPrice
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public int? DailyUploadId { get; set; }
        public FileUpload DailyUpload { get; set; } // DateOfUpload is more significant, apparently, than DateOfPrice..

        public int CatNo { get; set; }

        public int FuelTypeId { get; set; }
        public FuelType FuelType { get; set; }
        
        public int AllStarMerchantNo { get; set; }
        public DateTime DateOfPrice { get; set; } // As per Catalist file, not significant
        public int ModalPrice { get; set; }
    }

    public class CheapestCompetitor
    {
        public SiteToCompetitor CompetitorWithDriveTime { get; set; }
        public DailyPrice DailyPrice { get; set; } // Specific product price
        public LatestCompPrice LatestCompPrice { get; set; } // Specific product price
    }

}
