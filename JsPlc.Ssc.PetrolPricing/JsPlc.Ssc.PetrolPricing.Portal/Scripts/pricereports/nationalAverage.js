require(["jquery", "common", "busyloader", "bootstrap-datepicker", "notify", "downloader"],
    function ($, common, busyloader, bsdatepicker, notify, downloader) {
        $("document").ready(function () {

            var rootFolder = common.reportRootFolder();

            //var forDp = $('.datepicker').datepicker({
            //    language: "en-GB",
            //    autoClose: true,
            //    format: 'd-M-yyyy',
            //    todayBtn: "linked",
            //    todayHighlight: true,
            //    orientation: 'auto top',
            //    endDate: '1d'
            //});

            //$("#btnViewReport").click(function () {
            //    var dt = forDp.val();
            //    busyloader.showViewingReport();
            //    window.location.href = rootFolder + '/PriceReports/nationalAverage?For=' + dt;
            //});


            var forFromDp = $('#DateFrom').datepicker({
                language: "en-GB",
                autoClose: true,
                format: 'd-M-yyyy',
                todayBtn: "linked",
                todayHighlight: true,
                orientation: 'auto top',
                endDate: '1d'
            });


            var forToDp = $('#DateTo').datepicker({
                language: "en-GB",
                autoClose: true,
                format: 'd-M-yyyy',
                todayBtn: "linked",
                todayHighlight: true,
                orientation: 'auto top',
                endDate: '1d'
            });


            $("#btnViewReport").click(function () {
   
                busyloader.showViewingReport();
                var dt1 = forFromDp ? forFromDp.val() : $('#DateFrom').val();
                var dt2 = forToDp ? forToDp.val() : $('#DateTo').val();
                window.location.href = rootFolder + '/PriceReports/nationalAverage?fromDate=' + dt1 + '&toDate=' + dt2;
                });

            $("#btnExportReport").click(function () {
                //var dt = forDp.val(),
                var dt1 = forFromDp ? forFromDp.val() : $('#DateFrom').val();
                var dt2 = forToDp ? forToDp.val() : $('#DateTo').val();
                downloadId = downloader.generateId();
                busyloader.showExportToExcel(1000);

                downloader.start({
                    id: downloadId,
                    element: '#btnExportReport',
                    complete: function (download) {
                        notify.success('Export complete - took ' + download.friendlyTimeTaken);
                    }
                });

                window.location.href = rootFolder + '/PriceReports/ExportNationalAverage?downloadId=' + downloadId + '&fromDate=' + dt1 + '&toDate=' + dt2;
            });

            $('#btnResetReport').click(function () {
                window.location.href = rootFolder + '/PriceReports/nationalAverage';
            });
        });
    }
);

