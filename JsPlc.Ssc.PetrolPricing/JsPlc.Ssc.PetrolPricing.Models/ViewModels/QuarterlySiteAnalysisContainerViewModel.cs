using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JsPlc.Ssc.PetrolPricing.Models.ViewModels
{
    public class QuarterlySiteAnalysisContainerViewModel
    {
        public string ErrorMessage { get; set; }

        public int LeftFileUploadId { get; set; }
        public int RightFileUploadId { get; set; }

        public FileUploadViewModel LeftFile { get; set; }
        public FileUploadViewModel RightFile { get; set; }

        public List<SelectItemViewModel> FileUploadOptions = new List<SelectItemViewModel>();

        public QuarterlySiteAnalysisReportViewModel Report = new QuarterlySiteAnalysisReportViewModel();
    }
}
