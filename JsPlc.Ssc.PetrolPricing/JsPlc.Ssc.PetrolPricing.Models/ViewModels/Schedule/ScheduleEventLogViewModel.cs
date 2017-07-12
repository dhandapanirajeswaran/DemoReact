using JsPlc.Ssc.PetrolPricing.Models.WindowsService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JsPlc.Ssc.PetrolPricing.Models.ViewModels.Schedule
{
    public class ScheduleEventLogViewModel
    {
        public int WinServiceEventLogId { get; set; }
        public DateTime CreatedOn { get; set; }
        public int WinServiceScheduleId { get; set; }
        public WinServiceEventStatus WinServiceEventStatusId { get; set; }
        public string Message { get; set; }
        public string Exception { get; set; }

        public string EventStatusName
        {
            get { return this.WinServiceEventStatusId.ToString(); }
        }

        public ScheduleEventLogViewModel()
        {
            this.WinServiceEventLogId = 0;
            this.CreatedOn = DateTime.MinValue;
            this.WinServiceScheduleId = 0;
            this.WinServiceEventStatusId = WinServiceEventStatus.None;
            this.Message = "";
            this.Exception = "";
        }
    }
}