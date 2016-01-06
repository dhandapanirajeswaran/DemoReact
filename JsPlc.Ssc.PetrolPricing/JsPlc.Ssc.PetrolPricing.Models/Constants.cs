using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JsPlc.Ssc.PetrolPricing.Models
{
    public class Constants
    {
        public const string UploadHoldPath = "Hold";
        public const string UploadSuccessMessageWithFormat = "File uploaded successfully: {0}";

        public const int SitesPageSize = 350;

        public const int PricePageSize = 350;
        public const int QuarterlyFileRowsBatchSize = 5000; // 3 secs
        public const string EmailPriceReplacementStringForZero = "N/a";
    }
}
