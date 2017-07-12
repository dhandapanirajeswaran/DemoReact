using JsPlc.Ssc.PetrolPricing.Models.WindowsService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JsPlc.Ssc.PetrolPricing.Models.ViewModels.Schedule
{
    public class ScheduleItemViewModel
    {
        public int WinServiceScheduleId { get; set; }
        public bool IsActive { get; set; }
        public WinServiceEventType WinServiceEventTypeId { get; set; }
        public DateTime ScheduledFor { get; set; }
        public DateTime? LastPolledOn { get; set; }
        public DateTime? LastStartedOn { get; set; }
        public DateTime? LastCompletedOn { get; set; }
        public string EmailAddress { get; set; }
        public WinServiceEventStatus WinServiceEventStatusId { get; set; }

        public string EventTypeName
        {
            get { return this.WinServiceEventTypeId.ToString(); }
        }
        public string EventStatusName
        {
            get { return this.WinServiceEventStatusId.ToString(); }
        }

        public ScheduleItemViewModel()
        {
            this.WinServiceScheduleId = 0;
            this.IsActive = false;
            this.WinServiceEventTypeId = WinServiceEventType.None;
            this.ScheduledFor = DateTime.Now.AddYears(1000);
            this.WinServiceEventStatusId = WinServiceEventStatus.None;
            this.EmailAddress = "";
        }
    }
}