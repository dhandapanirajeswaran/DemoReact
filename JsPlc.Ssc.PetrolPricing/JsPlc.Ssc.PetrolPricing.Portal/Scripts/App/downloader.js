define(["jquery", "common", "cookie"],
    function ($, common, cookieMonster) {

        var frequency = 1000,
            ui,
            downloads = {};
        
        var states = {
            'started': 'started',
            'finished': 'finished',
            'unknown': 'unknown'
        };

        var durations = {
            show: 2000,
            done: 3000,
            hide: 4000
        };

        var classes = {
            'active': 'active',
            'done': 'done'
        };

        var positions = {
            'hide': {'bottom': '-5em'},
            'show': {'bottom': '0'}
        };

        function injectUI() {
            ui = $('<div class="downloader" title="Downloading..."><span id="DownloaderCount" class="active-count">0</span> <i class="fa fa-download fa-2x pulse-size"></i><br /><span id="DownloaderMessage" class="message"></span></div>');
            ui.hide().addClass(classes.active).css(positions.hide);
            ui.appendTo('body');
            redrawUI();
        };

        function redrawUI() {
            if (ui && 'find' in ui) {
                var stats = getStats(),
                    ticks = new Date().getTime() - stats.started,
                    taken = formatTicks(ticks),
                    message = stats.remaining == 0 ? 'Done' : taken;
                ui.find('#DownloaderMessage').html(message);
                ui.find('#DownloaderCount').text(stats.remaining);
            }
        };

        function getTicksDuration(ticks) {
            var totalSeconds = Math.floor(ticks / 1000);

            return {
                milliSeconds: ticks,
                totalSeconds: totalSeconds,
                minutes: Math.floor(totalSeconds / 60),
                seconds: totalSeconds % 60
            };
        }

        function formatTicks(ticks) {
            if (!ticks)
                return 'n/a';

            var duration = getTicksDuration(ticks),
                mm = (100 + duration.minutes).toString().substring(1),
                ss = (100 + duration.seconds).toString().substring(1);

            return mm + ':' + ss;
        };

        function formatFriendlyTicks(ticks) {
            if (!ticks)
                return 'n/a';

            var duration = getTicksDuration(ticks),
                totalSeconds = duration.totalSeconds;

            if (totalSeconds < 5)
                return 'a few seconds';
            if (totalSeconds < 60)
                return totalSeconds + ' seconds';
            if (duration.minutes == 1 && duration.seconds == 0)
                return '1 minute';
            if (duration.seconds == 0)
                return duration.minutes + ' minutes';
            return duration.minutes + ' minutes and ' + duration.seconds + ' seconds';
        };

        function generateId() {
            return new Date().getTime();
        };


        function start(opts) {
            var download = {
                id: opts.id,
                started: new Date().getTime(),
                ticksTaken: 0,
                status: states.started,
                opts: opts,
                friendlyTimeTaken: 'n/a'
            };

            downloads[opts.id] = download;

            ui.removeClass(classes.done).addClass(classes.active);

            if (!ui.is(':visible')) {
                redrawUI();
                ui.animate(positions.show, durations.show).show();
                $(document).trigger('download-ui-show');
            }

            if ('element' in opts) {
                $(opts.element).attr('disabled', 'disabled').removeClass('btn-primary').addClass('btn-danger');
            };
        };

        function getInfo(id) {
            return downloads.hasOwnProperty(id) 
                ? downloads[id] 
                : {
                    id: 0,
                    started: 0,
                    status: states.unknown
                };
        };

        function hasFinished(id) {
            return getInfo(id).status == states.finished;
        };

        function monitor() {
            var cookies = cookieMonster.allCookies(),
                id,
                changed = false,
                download,
                opts,
                hitlist = [];

            for (id in downloads) {
                if (cookies.hasOwnProperty(id) && downloads[id].status != states.finished) {
                    download = downloads[id];
                    download.status = states.finished;
                    download.ticksTaken = new Date() - download.started;
                    download.friendlyTimeTaken = formatFriendlyTicks(download.ticksTaken);
                    opts = download.opts;
                    changed = true;
                    cookieMonster.removeCookie(id);

                    if ('element' in opts)
                        $(opts.element).attr('disabled', null).removeClass('btn-danger').addClass('btn-primary');

                    if ('complete' in opts)
                        opts.complete(download);

                    $(document).trigger('download-complete', download);

                    hitlist.push(id);
                }
            }
            for (id in hitlist)
                delete downloads[id];

            redrawUI();

            if (getStats().remaining == 0 && changed) {
                ui.removeClass(classes.active).addClass(classes.done).delay(durations.done).animate(positions.hide, durations.hide, function () {
                    ui.hide();
                });
                $(document).trigger('download-ui-hide');
            }

            setTimeout(monitor, frequency);
        };

        function getStats() {
            var remaining = 0,
                id,
                started = null,
                download;
            for (id in downloads) {
                download = downloads[id];
                if (download.status == states.started) {
                    remaining++;
                    started = Math.min(download.started, started || download.started);
                }
            }
            return {
                remaining: remaining,
                started: started
            };
        };

        function docReady() {
            injectUI();
        };

        function init() {
            setTimeout(monitor, frequency);
            $(docReady);
        };

        init();

        // API
        return {
            generateId: generateId,
            start: start,
            hasFinished: hasFinished,
            getInfo: getInfo,
            getStats: getStats,
            formatTicks: formatTicks
        };
    }
);