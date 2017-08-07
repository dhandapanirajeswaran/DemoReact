using System;

namespace JsPlc.Ssc.PetrolPricing.Models
{
    public class SchedulerStatus
    {
        public bool IsRunning { get; set; } = false;
        public DateTime? LastStarted { get; set; }
        public DateTime? LastStopped { get; set; }
        public DateTime? LastPolled { get; set; }
        public DateTime? LastErrored { get; set; }
        public string LastErrorMessage { get; set; } = "";
    }
}