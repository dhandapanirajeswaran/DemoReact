using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JsPlc.Ssc.PetrolPricing.Models.ViewModels
{
    public class SiteEmailTodaySendStatusViewModel
    {
        public List<SiteEmailTodaySendStatusRowViewModel> SiteStatuses = new List<SiteEmailTodaySendStatusRowViewModel>();
    }

    public class SiteEmailTodaySendStatusRowViewModel
    {
        public int SiteId { get; set; }
        public bool WasEmailSentToday { get; set; }
        public DateTime? EmailLastSent { get; set; }
    }
}
