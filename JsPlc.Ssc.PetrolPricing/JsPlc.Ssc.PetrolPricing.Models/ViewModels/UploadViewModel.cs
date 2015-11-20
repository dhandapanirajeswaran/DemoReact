using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JsPlc.Ssc.PetrolPricing.Models.ViewModels
{
    public class UploadViewModel
    {
        public IEnumerable<UploadType> UploadTypes { get; set; }
        public DateTime UploadDate { get; set; }
    }
}
