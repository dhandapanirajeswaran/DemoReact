using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;

namespace JsPlc.Ssc.PetrolPricing.Models
{
    public class FileUpload
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public string OriginalFileName { get; set; }

        [Required]
        public string StoredFileName { get; set; }

        public int UploadTypeId { get; set; } 

        [Required]
        public UploadType UploadType { get; set; } // Daily, Quarterly

        [Required]
        public DateTime UploadDateTime { get; set; }

        public int StatusId { get; set; }

        [Required]
        public ImportProcessStatus Status { get; set; }

        [Required]
        public string UploadedBy { get; set; } // Emailaddr/Username of Uploader
    }
}
