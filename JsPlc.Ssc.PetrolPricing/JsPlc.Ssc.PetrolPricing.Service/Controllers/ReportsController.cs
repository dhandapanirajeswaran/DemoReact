using JsPlc.Ssc.PetrolPricing.Business;
using System;
using System.Web.Http;
using JsPlc.Ssc.PetrolPricing.Models.ViewModels;
using System.Linq;
using AutoMapper;
using JsPlc.Ssc.PetrolPricing.Models;

namespace JsPlc.Ssc.PetrolPricing.Service.Controllers
{
    public class ReportsController : ApiController
    {
        IReportService _reportService;
        IFileService _fileService;

        public ReportsController(IReportService reportService, IFileService fileService)
        {
            _reportService = reportService;
            _fileService = fileService;
        }

        /// <summary>
        /// Competitor Sites report
        /// </summary>
        /// <param name="siteId"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("api/GetCompetitorSites/{siteId}")]
        public IHttpActionResult GetCompetitorSites([FromUri]int siteId = 0)
        {
            var result = _reportService.GetReportCompetitorSites(siteId);
            return Ok(result);
        }

        /// <summary>
        /// PricePoints Report
        /// </summary>
        /// <param name="when"></param>
        /// <param name="fuelTypeId"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("api/GetPricePoints/{when}/{fuelTypeId}")]
        public IHttpActionResult GetPricePoints([FromUri]DateTime when, [FromUri]int fuelTypeId)
        {
            var result = _reportService.GetReportPricePoints(when, fuelTypeId);
            return Ok(result);
        }

        /// <summary>
        /// National average report
        /// </summary>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("api/GetNationalAverage/{startDate}/{endDate}")]
        public IHttpActionResult GetNationalAverage([FromUri]DateTime startDate, [FromUri]DateTime endDate)
        {

            var result = _reportService.GetReportNationalAverage(startDate, endDate);
            return Ok(result);
        }

        /// <summary>
        /// National average 2 report
        /// </summary>
        /// <param name="when",name="ViewAllCompetitors"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("api/GetNationalAverage2/{when}/{ViewAllCompetitors}")]
        public IHttpActionResult GetNationalAverage2([FromUri]DateTime when, [FromUri]bool ViewAllCompetitors)
        {
            var result = _reportService.GetReportNationalAverage2(when, ViewAllCompetitors);
            return Ok(result);
        }
               

         /// <summary>
        /// GetReportcompetitorsPriceRange report
        /// </summary>
        /// <param name="when"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("api/GetReportcompetitorsPriceRange/{when}")]
        public IHttpActionResult GetReportcompetitorsPriceRange([FromUri]DateTime when)
        {
            var result = _reportService.GetReportcompetitorsPriceRange(when);
            return Ok(result);
        }


        
        /// <summary>
        /// Competitors Price Range By Company report
        /// </summary>
        /// <param name="when"></param>
        /// <param name="companyName"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("api/GetCompetitorsPriceRangeByCompany/{when}/{companyName}/{brandName}")]
        public IHttpActionResult GetCompetitorsPriceRangeByCompany([FromUri]DateTime when, [FromUri]string companyName, [FromUri]string brandName)
        {
            var result = _reportService.GetReportCompetitorsPriceRangeByCompany(when, companyName, brandName);
            return Ok(result);
        }

        /// <summary>
        /// Compliance Report
        /// </summary>
        /// <param name="when"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("api/GetComplianceReport/{when}")]
        public IHttpActionResult GetComplianceReport([FromUri]DateTime when)
        {
            var result = _reportService.GetReportCompliance(when);
            return Ok(result);
        }

        /// <summary>
        /// PriceMovement Report
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <param name="fuelTypeId"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("api/GetPriceMovement/{from}/{to}/{fuelTypeId}/{brandName}/{siteName}")]
        public IHttpActionResult GetPriceMovement([FromUri]string brandName, [FromUri]DateTime from, [FromUri]DateTime to, [FromUri]int fuelTypeId, [FromUri]string siteName)
        {
            var result = _reportService.GetReportPriceMovement(brandName, from, to, fuelTypeId, siteName);
            return Ok(result);
        }

        [HttpGet]
        [Route("api/GetQuarterlyFileUploadOptions")]
        public IHttpActionResult GetQuarterlyFileUploadOptions()
        {
            var result = _reportService.GetQuarterlyFileUploadOptions();
            return Ok(result);
        }

        [HttpGet]
        [Route("api/GetQuarterlySiteAnalysisReport/{leftId}/{rightId}")]
        public IHttpActionResult GetQuarterlySiteAnalysisReport([FromUri] int leftId, [FromUri] int rightId)
        {
            var result = _reportService.GetQuarterlySiteAnalysisReport(leftId, rightId);
            return Ok(result);
        }

        [HttpGet]
        [Route("api/GetQuarterlySiteAnalysisContainerViewModel/{leftFileUploadId}/{rightFileUploadId}")]
        public IHttpActionResult GetQuarterlySiteAnalysisContainerViewModel([FromUri] int leftFileUploadId, [FromUri] int rightFileUploadId)
        {
            var leftFile = _fileService.GetFileUploadInformation(leftFileUploadId);
            var rightFile = _fileService.GetFileUploadInformation(rightFileUploadId);

            var errorMessage = leftFileUploadId == 0
                ? "Please select a left Quarterly File to compare"
                : rightFileUploadId == 0
                ? "Please select a right Quarterly File to compare"
                : leftFileUploadId == rightFileUploadId
                ? "You cannot compare the same Quarterly file to itself. Please select two different files"
                : "";


            var options = _reportService.GetQuarterlyFileUploadOptions().ToList();
            options.Insert(0, new SelectItemViewModel()
            {
                Id = 0,
                Name = "--- Please select ---"
            }
            );


            var model = new QuarterlySiteAnalysisContainerViewModel()
            {
                ErrorMessage = errorMessage,
                LeftFileUploadId = leftFileUploadId,
                RightFileUploadId = rightFileUploadId,
                LeftFile = Mapper.Map<FileUpload,FileUploadViewModel>(leftFile),
                RightFile = Mapper.Map<FileUpload, FileUploadViewModel>(rightFile),
                FileUploadOptions = options,
                Report = new QuarterlySiteAnalysisReportViewModel()
            };

            if (String.IsNullOrWhiteSpace(errorMessage))
                model.Report = _reportService.GetQuarterlySiteAnalysisReport(leftFileUploadId, rightFileUploadId);

            return Ok(model);
        }

        [HttpGet]
        [Route("api/GetLastSitePricesViewModel/{forDate}")]
        public IHttpActionResult GetLastSitePricesViewModel([FromUri] DateTime forDate)
        {
            var result = _reportService.GetLastSitePricesViewModel(forDate);
            return Ok(result);
        }

    }
}
