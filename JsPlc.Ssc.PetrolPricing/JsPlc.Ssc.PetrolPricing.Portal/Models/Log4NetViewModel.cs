using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace JsPlc.Ssc.PetrolPricing.Portal.Models
{
    public class Log4NetViewModel
    {
        public string FileName { get; set; }
        public DateTime? LastModified { get; set; }
        public string Message { get; set; }
        public string FileDump { get; set; }

        public Log4NetViewModel()
        {
            this.FileName = "";
            this.LastModified = null;
            this.Message = "";
            this.FileDump = "";
        }
    }
}