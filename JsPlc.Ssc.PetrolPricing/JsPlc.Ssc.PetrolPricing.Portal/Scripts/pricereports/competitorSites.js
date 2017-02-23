require(["jquery", "common", "busyloader"],
    function ($, common, busyloader) {

        $("document").ready(function () {

            var rootFolder = common.reportRootFolder();

            $("#SiteId").focus();

            $("#SiteId").change(function () {
                var siteId = $(this).val();
                busyloader.showViewingReport();
                window.location.href = rootFolder + '/PriceReports/CompetitorSites/?siteId=' + siteId;
            });

            $("#btnExportReport").click(function () {
                busyloader.showExportToExcel();
                var siteId = $("#SiteId").val();
                window.location.href = rootFolder + '/PriceReports/ExportCompetitorSites/?siteId=' + siteId;
            });
        });
    }
);
