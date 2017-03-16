define(["jquery", "common", "text!busyloader.html"],
    function ($, common, busyloaderHtml) {

        var showing = false,
            overlay = $(busyloaderHtml),
            messageSelector = '#BusyLoaderMessage',
            positions = {
                start: { 'top': '400px', 'opacity': 0 },
                visible: { 'top': '100px', 'opacity': 1 },
                hidden: { 'top': '0px', 'opacity': 0 }
            },
            presets = {
                viewingReport: {
                    message: 'Viewing Report. Please wait...',
                    showtime: 2000
                },
                exportToExcel: {
                    message: 'Exporting to Excel. Please wait...',
                    showtime: 3000
                }
            };

        // attach to DOM
        overlay.appendTo(document.body);

        function show(opts) {
            var begin = function() {
                launch(opts);
            }
            if (showing)
                overlay.stop(true, false)
                    .animate(positions.hidden, 500, begin);
            else
                begin();
        };

        function launch(opts) {
            var message = opts.message || 'Busy - Please wait...',
                showtime = parseInt(opts.showtime, 10);
            showing = true;

            overlay.find(messageSelector).text(message);
            overlay.css(positions.start)
                .show()
                .animate(positions.visible, 1000);

            if (showtime) {
                overlay.delay(showtime)
                    .animate(positions.hidden, 2000, onHide);
            }
        };

        function onHide() {
            showing = false;
        };

        function hide() {
            overlay.hide();
            showing = false;
        };

        function showViewingReport(showtime) {
            var opts = $.extend({}, presets.viewingReport, { showtime: showtime });
            show(opts);
        };

        function showExportToExcel(showtime) {
            var opts = $.extend({}, presets.exportToExcel, { showtime: showtime });
            show(opts);
        };

        return {
            show: show,
            hide: hide,
            showViewingReport: showViewingReport,
            showExportToExcel: showExportToExcel
        };
    });