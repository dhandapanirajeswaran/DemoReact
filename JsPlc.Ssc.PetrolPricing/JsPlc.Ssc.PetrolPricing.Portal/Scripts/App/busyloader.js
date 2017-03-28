define(["jquery", "common", "text!busyloader.html"],
    function ($, common, busyloaderHtml) {

        "use strict";

        var showing = false,
            overlay = $(busyloaderHtml),
            lightbox = $('<div class="lightbox-overlay"></div>'),
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

            var animations = {
                lightbox: {
                    show: { 'opacity': 0.5 },
                    hide: { 'opacity': 0.0 }
                }
            };

        // init styles
            overlay.css({ 'zIndex': 2010 });
            lightbox.css({ 'position': 'fixed', 'top': 0, 'bottom': 0, 'left': 0, 'right': 0, 'backgroundColor': '#000', 'zIndex': 2000, 'opacity': 0 });

        // attach to DOM
            overlay.appendTo(document.body);
            lightbox.hide().appendTo(document.body);

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
                showtime = parseInt(opts.showtime, 10),
                dull = !!opts.dull;
            showing = true;

            if (dull != lightbox.is(':visible')) {
                dull ? showLightbox() : hideLightbox();
            }

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
            hideLightbox();
            showing = false;
        };

        function showLightbox() {
            lightbox.show().animate(animations.lightbox.show, 1000);
        };

        function hideLightbox() {
            lightbox.animate(animations.lightbox.hide, 2000, function () {
                lightbox.hide();
            });
        }

        function hide() {
            overlay.hide();
            hideLightbox();
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