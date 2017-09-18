using JsPlc.Ssc.PetrolPricing.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JsPlc.Ssc.PetrolPricing.Models
{
    public class ScheduleEmailTemplate
    {
        public int Id { get; set; }
        public ScheduleEmailType ScheduleEmailType { get; set; } = ScheduleEmailType.None;
        public string SubjectLine { get; set; } = "";
        public string ContactEmail { get; set; } = "";
        public string EmailBody { get; set; } = "";
    }
}
