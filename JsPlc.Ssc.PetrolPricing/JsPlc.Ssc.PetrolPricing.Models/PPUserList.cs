using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JsPlc.Ssc.PetrolPricing.Models
{
    public class PPUserList
    {
        public IEnumerable<PPUser> Users;
        public int SelectedUserId = 0;
        public string ErrorMessage = "";
        public string SuccessMessage = "";
    }
}
