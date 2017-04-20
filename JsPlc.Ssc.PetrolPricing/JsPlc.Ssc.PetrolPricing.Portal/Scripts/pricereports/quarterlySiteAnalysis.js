require(["jquery", "common", "busyloader", "notify", "downloader"],
    function ($, common, busyloader, notify, downloader) {
        "use strict";

        var rootFolder = common.reportRootFolder();

        var controls = {
            leftMenu: null,
            rightMenu: null,
            swapButton: null,
            resetButton: null,
            exportButton: null
        };

        function submitForm() {
            $('form:first')[0].submit();
        };

        function resetForm() {
            controls.leftMenu.val('0');
            controls.rightMenu.val('0');
            submitForm();
        };

        function exportReport() {
            var leftVal = controls.leftMenu.val(),
                rightVal = controls.rightMenu.val(),
                downloadId = downloader.generateId();

            if (leftVal == 0 || rightVal == 0) {
                notify.warning('Please select the Quarterly files to compare');
                return;
            }

            if (leftVal == rightVal)
            {
                notify.warning('Please choose two different Quarterly files to compare');
                return;
            }

            busyloader.showExportToExcel(1000);

            downloader.start({
                id: downloadId,
                element: '#btnExportReport',
                complete: function (download) {
                    notify.success('Export complete - took ' + download.friendlyTimeTaken);
                }
            });

            window.location = rootFolder + '/PriceReports/ExportQuarterlySiteAnalysis'
                + '?downloadId=' + downloadId
                + '&leftFileUploadId=' + leftVal
                + '&rightFileUploadId=' + rightVal;
        };

        function swapFiles() {
            var leftVal = controls.leftMenu.val(),
                rightVal = controls.rightMenu.val();

            controls.leftMenu.val(rightVal);
            controls.rightMenu.val(leftVal);

            submitForm();
        };

        function findControls() {
            controls.leftMenu = $('#LeftFileUploadId');
            controls.rightMenu = $('#RightFileUploadId');
            controls.swapButton = $('#btnSwapFiles');
            controls.resetButton = $('#btnResetReport');
            controls.exportButton = $('#btnExportReport');
        };

        function bindEvents() {
            controls.leftMenu.on('change', submitForm);
            controls.rightMenu.on('change', submitForm);
            controls.resetButton.on('click', resetForm);
            controls.swapButton.on('click', swapFiles);
            controls.exportButton.on('click', exportReport);
        };

        function docReady() {
            findControls();
            bindEvents();
        };

        $(docReady);

        // API
        return {

        };
    }
);