using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JsPlc.Ssc.PetrolPricing.Models
{
    public class PPUserDetails
    {
        public GenericStatus Status { get; set; }

        public PPUser User { get; set; }

        public PPUserPermissions Permissions { get; set; }
    }
}

