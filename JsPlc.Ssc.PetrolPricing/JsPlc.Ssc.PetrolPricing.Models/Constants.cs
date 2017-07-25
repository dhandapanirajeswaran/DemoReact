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

        public const string UploadSuccessMessageWithFormatAndTimeTaken = "File uploaded successfully: {0} - took {1}";

        public const int SitesPageSize = 400;

        public const int PricePageSize = 2000;
        
		public const int QuarterlyFileRowsBatchSize = 5000; // 3 secs
		
		public const int DailyFileRowsBatchSize = 1000; 
        
		public const string EmailPriceReplacementStringForZero = "N/A";

        public const string EmailPriceForNoPriceMovement = "----";
    }
}
