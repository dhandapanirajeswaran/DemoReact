using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;

namespace JsPlc.Ssc.PetrolPricing.Models
{
    public class QuarterlyUploadStaging
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public int QuarterlyUploadId { get; set; } 
        public FileUpload QuarterlyUpload { get; set; }

        public int SainsSiteCatNo { get; set; } // As this file does not drive the Sainsburys store master data, we only keep Sains CatNo (col 3)

        public int Rank { get; set; }
        public float DriveDist { get; set; }
        public float DriveTime { get; set; }
        public int CatNo { get; set; }
        public string Brand { get; set; }
        public string SiteName { get; set; }
        public string Addr { get; set; }
        public string Suburb { get; set; }
        public string Town { get; set; }
        public string PostCode { get; set; }
        public string Company { get; set; }
        public string Ownership { get; set; }
    }
}
