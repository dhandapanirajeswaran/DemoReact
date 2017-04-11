using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JsPlc.Ssc.PetrolPricing.Models.ViewModels.UserPermissions
{
    public class UserPermissionsOptionListViewModel
    {
        public string SelectedValue { get; private set; }

        public IEnumerable<UserPermissionsOptionViewModel> Options;

        public UserPermissionsOptionListViewModel(string selectedValue, IEnumerable<UserPermissionsOptionViewModel> options)
        {
            this.Options = options;

            if (options.Any(x => x.Value.Equals(selectedValue, StringComparison.InvariantCultureIgnoreCase)))
                this.SelectedValue = selectedValue;
            else
                this.SelectedValue = "";
        }
    }

}
