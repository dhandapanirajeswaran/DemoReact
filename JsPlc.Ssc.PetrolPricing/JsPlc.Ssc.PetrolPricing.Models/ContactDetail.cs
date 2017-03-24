using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JsPlc.Ssc.PetrolPricing.Models
{
    public class ContactDetail
    {
        public int Id { get; set; }
        public string Heading { get; set; }
        public string Address { get; set; }
        public string PhoneNumber { get; set; }
        public string EmailName { get; set; }
        public string EmailAddress { get; set; }
        public bool IsActive { get; set; }
    }
}
