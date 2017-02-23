define(["jquery", "common", "notify", "moment"],
    function ($, common, notify, moment) {
        var self = this,
            duration = 15,
            tick = 15,
            element = $('#spanTicker'),
            container = $('#FileUploadContainer'),
            lastRefresh = $('#UploadLastRefresh'),
            lastRefreshText = moment().format(common.uiDateTimeFormat),
            progress = $('#ProgressBar'),
            progressFullWidth = progress.width();

        function redraw() {
            if (tick < 0) {
                tick = duration;
                reload();
            }
            element.text(tick.toFixed(0));
            lastRefresh.text(lastRefreshText);
            progress.width((tick * progressFullWidth / duration).toFixed(0) + 'px');

            setTimeout(function () {
                redraw.call(self);
            }, 100);

            tick -= 0.1;
        };

        function start() {
            tick = duration;
            redraw();
        };

        function reload() {
            container.load('File/GetUploadsPartial', function (response, status, xhr) {
                if (status == "error") {
                    lastRefreshText = 'Error';
                } else {
                    lastRefreshText = moment().format(common.uiDateTimeFormat);
                }
            });
        };
        // API
        return {
            start: start
        }
});