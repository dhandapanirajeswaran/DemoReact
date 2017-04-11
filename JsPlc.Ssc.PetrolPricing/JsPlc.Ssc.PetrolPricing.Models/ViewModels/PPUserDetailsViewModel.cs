using JsPlc.Ssc.PetrolPricing.Models.ViewModels.UserPermissions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JsPlc.Ssc.PetrolPricing.Models.ViewModels
{
    public class PPUserDetailsViewModel
    {
        public StatusViewModel Status { get; set; }

        public PPUser User { get; set; }

        public EditUserPermissionsViewModel Permissions { get; set; }

        public PPUserDetailsViewModel()
        {
            this.Status = new StatusViewModel();
            this.User = new PPUser();
            this.Permissions = new EditUserPermissionsViewModel();
        }
    }
}
