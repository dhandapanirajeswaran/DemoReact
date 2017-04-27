using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JsPlc.Ssc.PetrolPricing.Models.ViewModels
{
    public class UploadViewModel
    {
        public int SelectedFileUploadType { get; set; } 

        public IEnumerable<UploadType> UploadTypes { get; set; }
        public DateTime UploadDate { get; set; }

        public UploadViewModel()
        {
            this.SelectedFileUploadType = 1; // DailyPriceData
        }
    }
}
