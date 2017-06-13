using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JsPlc.Ssc.PetrolPricing.Models.ViewModels
{
    public class FileDownloadViewModel
    {
        public string TimeStampedFileName { get; set; }
        public string FileName { get; set; }
        public byte[] FileBytes { get; set; }

        public DateTime UploadDateTime { get; set; }
        public int FileUploadTypeId { get; set; }
    }
}
