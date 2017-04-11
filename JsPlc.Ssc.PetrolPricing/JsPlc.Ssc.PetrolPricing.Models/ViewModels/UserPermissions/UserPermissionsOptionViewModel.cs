using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JsPlc.Ssc.PetrolPricing.Models.ViewModels.UserPermissions
{
    public class UserPermissionsOptionViewModel
    {
        public string Value { get; private set; }
        public string Text { get; private set; }

        public UserPermissionsOptionViewModel(Enum flags, string text)
        {
            this.Value = Convert.ToInt16(flags).ToString();
            this.Text = text;
        }

        public UserPermissionsOptionViewModel(string value, string text)
        {
            this.Value = value;
            this.Text = text;
        }
    }

}
