require(["jquery", "common", "busyloader", "bootstrap-datepicker", "notify", "downloader"],
    function ($, common, busyloader, bsdatepicker, notify, downloader) {

        $("document").ready(function () {

            var rootFolder = common.reportRootFolder();

            var forDp = $('#DateFrom').datepicker({
                language: "en-GB",
                autoClose: true,
                format: 'd-M-yyyy',
                todayBtn: "linked",
                todayHighlight: true
            });

            var toDp = $('#DateTo').datepicker({
                language: "en-GB",
                autoClose: true,
                format: 'd-M-yyyy',
                todayBtn: "linked",
                todayHighlight: true
            });

            var state = {
                showZeros: true
            };

            var selectors = {
                grid: '#tableDiv',
                zeros: '.zero',
                showButton: '#btnShowZeroValues',
                hideButton: '#btnHideZeroValues'
            }

            var controls = {
                showButton: $(selectors.showButton),
                hideButton: $(selectors.hideButton)
            };

            $("#btnViewReport").click(function () {
                $('#errorMsgs').html("");
                $('#msgs').html("");

                var id = $('#FuelTypeId').val();
                var brandName = $('#Brand').val();
                var siteName = $('#SiteName').val();
                var dt1 = forDp ? forDp.val() : $('#DateFrom').val();
                var dt2 = toDp ? toDp.val() : $('#DateTo').val();
                if (id == 0) {
                    $('#errorMsgs').html("Please select a fuel");
                    $('#fuelTypes').focus();
                    return false;
                }

                busyloader.showViewingReport();

                window.location.href = rootFolder + '/PriceReports/PriceMovement?DateFrom=' + dt1 + "&DateTo=" + dt2 + "&FuelTypeId=" + id + "&BrandName=" + brandName + "&SiteName=" + siteName;
                return true;
            });
            $("#btnExportReport").click(function () {
                $('#errorMsgs').html("");
                $('#msgs').html("");

                var id = $('#FuelTypeId').val(),
                    brandName = $('#Brand').val(),
                    dt1 = forDp ? forDp.val() : $('#DateFrom').val(),
                    dt2 = toDp ? toDp.val() : $('#DateTo').val(),
                    downloadId = downloader.generateId();
                if (id == 0) {
                    $('#errorMsgs').html("Please select a fuel");
                    $('#fuelTypes').focus();
                    return false;
                }

                busyloader.showExportToExcel();

                downloader.start({
                    id: downloadId,
                    element: '#btnExportReport',
                    complete: function (download) {
                        notify.success('Export complete - took ' + download.friendlyTimeTaken);
                    }
                });

                window.location.href = rootFolder + '/PriceReports/ExportPriceMovement?downloadId=' + downloadId + '&DateFrom=' + dt1 + "&DateTo=" + dt2 + "&FuelTypeId=" + id + "&BrandName=" + brandName;
                return true;
            });

            $('#btnResetReport').click(function () {
                busyloader.show({
                    message: 'Reset Report. Please wait...',
                    showtime: 3000
                })
                window.location.href = rootFolder + '/PriceReports/PriceMovement';
            });

            function redrawShowHideButtons() {
                var activeButton = state.showZeros ? controls.showButton : controls.hideButton
                inactiveButton = state.showZeros ? controls.hideButton : controls.showButton;

                activeButton.removeClass('btn-default').addClass('btn-primary');
                inactiveButton.removeClass('btn-primary').addClass('btn-default');
            };

            function redrawZeros() {
                var grid = $(selectors.grid),
                    zeros = grid.find(selectors.zeros);

                state.showZeros ? zeros.show() : zeros.hide();
            };

            controls.showButton.off().click(function () {
                state.showZeros = true;
                redrawShowHideButtons();
                redrawZeros();
                notify.info('Showing 0 values in report');
            });

            controls.hideButton.off().click(function () {
                state.showZeros = false;
                redrawShowHideButtons();
                redrawZeros();
                notify.info('Hiding 0 values in report');
            });

        });
    }
);
