using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JsPlc.Ssc.PetrolPricing.Models.ViewModels
{
    public class QuaterlySiteAnalysisStats
    {
        public int LeftTotalRecordCount { get; set; }
        public int RightTotalRecordCount { get; set; }

        public QuaterlySiteAnalysisStats()
        {
            this.LeftTotalRecordCount = 0;
            this.RightTotalRecordCount = 0;
        }
    }

    public class QuarterlySiteAnalysisReport
    {
        public IEnumerable<QuarterlySiteAnalysisReportRowViewModel> Rows { get; set; }

        public QuaterlySiteAnalysisStats Stats { get; set; }

        public QuarterlySiteAnalysisReport()
        {
            this.Rows = new List<QuarterlySiteAnalysisReportRowViewModel>();
            this.Stats = new QuaterlySiteAnalysisStats();
        }
    }
}
