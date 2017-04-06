require(["jquery", "common", "busyloader", "notify", "downloader"],
    function ($, common, busyloader, notify, downloader) {

        $("document").ready(function () {

            var rootFolder = common.reportRootFolder();

            $("#SiteId").focus();

            $("#SiteId").change(function () {
                var siteId = $(this).val();

                busyloader.showViewingReport();

                window.location.href = rootFolder + '/PriceReports/CompetitorSites/?siteId=' + siteId;
            });

            $("#btnExportReport").click(function () {
                var siteId = $("#SiteId").val(),
                    downloadId = downloader.generateId();

                busyloader.showExportToExcel();

                downloader.start({
                    id: downloadId,
                    element: '#btnExportReport',
                    complete: function (download) {
                        notify.success('Export completed - took ' + download.friendlyTimeTaken);
                    }
                });

                window.location.href = rootFolder + '/PriceReports/ExportCompetitorSites/?downloadId=' + downloadId + '&siteId=' + siteId;
            });

            $('#btnResetReport').click(function () {
                window.location.href = rootFolder + '/PriceReports/CompetitorSites';
            });
        });
    }
);
