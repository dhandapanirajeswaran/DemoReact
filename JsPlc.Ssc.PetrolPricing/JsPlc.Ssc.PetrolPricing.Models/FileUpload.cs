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
        [Display(Name = "Original file name")]
        public string OriginalFileName { get; set; }

        [Required]
        [Display(Name = "Stored file name")]
        public string StoredFileName { get; set; }

        public int UploadTypeId { get; set; } 

        [Required]
        [Display(Name = "Upload type")]
        public UploadType UploadType { get; set; } // Daily, Quarterly

        [Required]
        [Display(Name = "Upload date and time")]
        public DateTime UploadDateTime { get; set; }

        public int StatusId { get; set; }

        [Display(Name = "Upload status")]
        public ImportProcessStatus Status { get; set; }

        [Required]
        [Display(Name = "Uploaded by")]
        public string UploadedBy { get; set; } // Emailaddr/Username of Uploader

        public virtual ICollection<ImportProcessError> ImportProcessErrors { get; set; }
    }
}
