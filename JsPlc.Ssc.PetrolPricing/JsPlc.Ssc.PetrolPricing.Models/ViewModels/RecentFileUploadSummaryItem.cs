using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JsPlc.Ssc.PetrolPricing.Models.ViewModels
{
    public class RecentFileUploadSummaryItem
    {
        public string UploadTypeName { get; set; }
        public string ImportStatus { get; set; }
        public int FileUploadId { get; set; }
        public string OriginalFileName { get; set; }
        public DateTime UploadDateTime { get; set; }
        public string UploadedBy { get; set; }
    }
}
