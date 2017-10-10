using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JsPlc.Ssc.PetrolPricing.Models.ViewModels
{
    public class SiteEmailSendStatusViewModel
    {
        public int SiteId { get; set; }
        public DateTime EndTradeDate { get; set; }
        public DateTime SendDate { get; set; }
        public bool IsSuccess { get; set; }
        public int EmailSendLogId { get; set; }
    }
}
