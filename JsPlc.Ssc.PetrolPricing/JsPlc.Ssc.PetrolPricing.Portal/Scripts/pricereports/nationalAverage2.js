require(["jquery", "common", "busyloader", "bootstrap-datepicker"],
    function ($, common, busyloader, bsdatepicker) {
        $("document").ready(function () {

            var rootFolder = common.reportRootFolder();

            var forDp = $('.datepicker').datepicker({
                language: "en-GB",
                autoClose: true,
                format: 'd-M-yyyy',
                todayBtn: "linked",
                todayHighlight: true,
                orientation: 'auto top',
                endDate: '1d'
            });

            $("#btnViewReport").click(function () {
                var dt = forDp.val();
                busyloader.showViewingReport();
                window.location.href = rootFolder + '/PriceReports/nationalAverage2?For=' + dt;
            });
            $("#btnExportReport").click(function () {
                var dt = forDp.val();
                busyloader.showExportToExcel();
                window.location.href = rootFolder + '/PriceReports/ExportNationalAverage2?For=' + dt;
            });
            $('#btnResetReport').click(function () {
                window.location.href = rootFolder + '/PriceReports/nationalAverage2';
            });
        });
    }
);

