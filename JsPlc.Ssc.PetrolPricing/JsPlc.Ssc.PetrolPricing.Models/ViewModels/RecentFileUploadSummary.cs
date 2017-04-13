using JsPlc.Ssc.PetrolPricing.Models.ViewModels.UserPermissions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JsPlc.Ssc.PetrolPricing.Models.ViewModels
{
    public class RecentFileUploadSummary
    {
        public UserAccessViewModel UserAccess = new UserAccessViewModel();

        public List<RecentFileUploadSummaryItem> Files = new List<RecentFileUploadSummaryItem>();
    }
}
