using JsPlc.Ssc.PetrolPricing.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JsPlc.Ssc.PetrolPricing.Models.ViewModels.Schedule
{
    public class ScheduleViewModel
    {
        public string NotifyMessage { get; set; }
        public string NotifyClass { get; set; }
        public NotifyMessageType NotifyMessageType { get; set; }

        public IEnumerable<ScheduleItemViewModel> ScheduledItems { get; set; }

        public IEnumerable<ScheduleEventLogViewModel> EventLogItems { get; set; }

        public ScheduleViewModel()
        {
            this.NotifyMessage = "";
            this.NotifyMessageType = NotifyMessageType.None;
            this.ScheduledItems = new List<ScheduleItemViewModel>();
            this.EventLogItems = new List<ScheduleEventLogViewModel>();
        }
    }
}
