using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JsPlc.Ssc.PetrolPricing.Models.WindowsService
{
    public class WinServiceSchedule
    {
        public int WinServiceScheduleId { get; set; }
        public bool IsActive { get; set; }
        public WinServiceEventType EventTypeId { get; set; }
        public DateTime ScheduledFor { get; set; }
        public DateTime? LastPolledOn { get; set; }
        public DateTime? LastStartedOn { get; set; }
        public DateTime? LastCompletedOn { get; set; }
        public WinServiceEventStatus EventStatusId { get; set; }
        public string EmailAddress { get; set; }
        public string EventStatusName {
            get { return this.EventStatusId.ToString(); }
        }
        public string EventTypename
        {
            get { return this.EventTypeId.ToString(); }
        }

        public WinServiceSchedule()
        {
            this.WinServiceScheduleId = 0;
            this.IsActive = false;
            this.EventTypeId = WinServiceEventType.None;
            this.ScheduledFor = DateTime.Now.Date.AddYears(1000);
            this.EventStatusId = WinServiceEventStatus.Paused;
            this.EmailAddress = "";
        }
    }
}