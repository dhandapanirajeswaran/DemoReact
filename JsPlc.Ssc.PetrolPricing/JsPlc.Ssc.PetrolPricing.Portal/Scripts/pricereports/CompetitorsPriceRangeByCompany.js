require(["jquery", "common", "busyloader", "bootstrap-datepicker", "notify", "downloader"],
    function ($, common, busyloader, bsdatepicker, notify, downloader) {

        var rootFolder = common.reportRootFolder();

        $("document").ready(function () {
            var forDp = $('.datepicker').datepicker({
                language: "en-GB",
                autoClose: true,
                format: 'd-M-yyyy',
                todayBtn: "linked",
                todayHighlight: true,
                orientation: 'auto top',
                endDate: '1d'
            });

            $("#DateFor, #SelectedCompanyName, #SelectedBrandName").each(function () {
                $this = $(this);
                $("#" + $this.attr("name") + "Copy").val($this.val());
            });

            $("#DateFor, #SelectedCompanyName, #SelectedBrandName").change(function () {
                $this = $(this);
                $("#" + $this.attr("name") + "Copy").val($this.val());
            });

            $('#btnExportReport').off().on('click', function () {
                var downloadId = downloader.generateId();

                busyloader.showExportToExcel();

                downloader.start({
                    id: downloadId,
                    element: '#btnExportReport',
                    complete: function (download) {
                        notify.success('Export complete - took ' + download.friendlyTimeTaken);
                    }
                });

                window.location.href = rootFolder + '/PriceReports/ExportCompetitorsPriceRangeByCompany?downloadId=' + downloadId
                    + '&DateFor=' + $('#DateForCopy').val()
                    + '&SelectedCompanyName=' + $('#SelectedCompanyNameCopy').val()
                    + '&SelectedBrandName=' + $('#SelectedBrandNameCopy').val()

                return false;
            });

            $('#btnViewReport').off().on('click', function () {
                busyloader.showViewingReport();
                return true;
            });

            $('#btnResetReport').click(function () {
                window.location.href = rootFolder + '/PriceReports/CompetitorsPriceRangeByCompany';
            });

        });
    }
);