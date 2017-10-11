using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JsPlc.Ssc.PetrolPricing.Models
{
    public class ImportSiteEmailSettings
    {
        public bool ImportCatNo { get; set; } = false;
        public bool ImportPfsNo { get; set; } = false;
        public bool AllowSharedEmails { get; set; } = false;
        public bool ImportStoreNoUsingCatNo { get; set; } = false;
    }
}
