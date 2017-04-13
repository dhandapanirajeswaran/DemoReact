using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JsPlc.Ssc.PetrolPricing.Models.ViewModels.Diagnostics
{
    public class DiagnosticsFileUploadSummaryViewModel
    {
        public DateTime? NewestDateTime { get; set; }
        public DateTime? OldestDateTime { get; set; }

        public int TotalFileCount { get; set; }
        public long TotalFileSize { get; set; }
        public int FilesInLast7Days { get; set; }
        public int FilesOlderThan7Days { get; set; }
        public int FilesOlderThan30Days { get; set; }
        public int FilesOlderThan60Days { get; set; }
        public int FilesOlderThan90Days { get; set; }
        public int FilesOlderThan1Year { get; set; }

        public DiagnosticsFileUploadSummaryViewModel()
        {
            this.TotalFileCount = 0;
            this.TotalFileSize = 0;
            this.FilesInLast7Days = 0;
            this.FilesOlderThan7Days = 0;
            this.FilesOlderThan30Days = 0;
            this.FilesOlderThan60Days = 0;
            this.FilesOlderThan90Days = 0;
            this.FilesOlderThan1Year = 0;
        }
    }
}
