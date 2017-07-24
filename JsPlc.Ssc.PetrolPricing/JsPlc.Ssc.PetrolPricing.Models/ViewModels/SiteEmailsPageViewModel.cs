using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JsPlc.Ssc.PetrolPricing.Models.ViewModels
{
    public class SiteEmailsPageViewModel
    {
        public IEnumerable<SiteEmailAddressViewModel> SiteEmails { get; set; } = new List<SiteEmailAddressViewModel>();
    }
}
