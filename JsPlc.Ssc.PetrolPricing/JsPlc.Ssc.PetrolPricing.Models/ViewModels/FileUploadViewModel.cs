using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JsPlc.Ssc.PetrolPricing.Models.ViewModels
{
    public class FileUploadViewModel
    {
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

        public ImportProcessStatus Status { get; set; }

        [Required]
        public string UploadedBy { get; set; } // Emailaddr/Username of Uploader

        public virtual ICollection<ImportProcessError> ImportProcessErrors { get; set; }
    }
}
