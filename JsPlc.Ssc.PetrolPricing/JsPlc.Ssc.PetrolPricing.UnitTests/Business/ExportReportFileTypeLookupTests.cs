using System;
using JsPlc.Ssc.PetrolPricing.Core;
using NUnit.Framework;

namespace JsPlc.Ssc.PetrolPricing.UnitTests.Business
{
    public class ExportReportFileTypeLookupTests
    {
        private ExportReportFileTypeLookup _exportReportFileTypeLookup;

        [SetUp]
        public void SetUp()
        {
            _exportReportFileTypeLookup = new ExportReportFileTypeLookup();
        }

        [TestCase("CompetitorSites", ReportExportFileType.CompetitorSites)]
        [TestCase("CompetitorsPriceRange", ReportExportFileType.CompetitorsPriceRange)]
        [TestCase("CompetitorsPriceRangeByCompany", ReportExportFileType.CompetitorsPriceRangeByCompany)]
        [TestCase("NationalAverageReport", ReportExportFileType.NationalAverageReport)]
        [TestCase("NationalAverageReport2", ReportExportFileType.NationalAverageReport2)]
        [TestCase("PriceMovementReport", ReportExportFileType.PriceMovementReport)]
        [TestCase("PricePointsReport", ReportExportFileType.PricePointsReport)]
        [TestCase("QuarterlySiteAnalysisReport", ReportExportFileType.QuarterlySiteAnalysis)]
        [TestCase("LastSitePrices", ReportExportFileType.LastSitePrices)]
        [TestCase("Compliance", ReportExportFileType.Compliance)]
        public void Given_Filename_Returns_Expected_Report_Type(string fileName, ReportExportFileType expectedType)
        {
            var exportReportType = _exportReportFileTypeLookup.GetReportType(fileName);
            Assert.That(exportReportType.Equals(expectedType));
        }

        [TestCase("SAINSBURYS PriceMovementReport", ReportExportFileType.PriceMovementReport)]
        [TestCase("TESCO PriceMovementReport", ReportExportFileType.PriceMovementReport)]
        [TestCase("ASDA PriceMovementReport", ReportExportFileType.PriceMovementReport)]
        public void Given_Prefixed_PriceMovementReport_Filename_Returns_Expected_Report_Type(string fileName, ReportExportFileType expectedType)
        {
            var exportReportType = _exportReportFileTypeLookup.GetReportType(fileName);
            Assert.That(exportReportType.Equals(expectedType));
        }

        [Test]
        public void Given_Unknown_Filename_Throws_Exception()
        {
            var ex = Assert.Throws<ArgumentException>(() => _exportReportFileTypeLookup.GetReportType("Unknown file name"));
            Assert.That(ex.Message.Equals("Unknown report type for filename: Unknown file name"));
        }

        [Test]
        public void Given_Empty_Filename_Throws_Exception()
        {
            var ex = Assert.Throws<ArgumentException>(() => _exportReportFileTypeLookup.GetReportType(""));
            Assert.That(ex.Message.Equals($"Unknown report type for filename: "));
        }
    }
}