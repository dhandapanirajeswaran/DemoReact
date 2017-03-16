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

        var classes = {
            'active': 'active',
            'done': 'done'
        };

        var positions = {
            'hide': {'bottom': '-5em'},
            'show': {'bottom': '0'}
        };

        function injectUI() {
            ui = $('<div class="downloader" title="Downloading..."><i class="fa fa-download fa-2x pulse-size"></i><br /><span id="DownloaderMessage"></span></div>');
            ui.hide().addClass(classes.active).css(positions.hide);
            ui.appendTo('body');
            redrawUI();
        };

        function redrawUI() {
            var stats = getStats(),
                message = stats.remaining == 0 ? 'Done' : stats.remaining;
            ui.find('#DownloaderMessage').html(message);
        };

        function generateId() {
            return new Date().getTime();
        };

        function start(opts) {
            downloads[opts.id] = {
                id: opts.id,
                started: new Date(),
                status: states.started,
                opts: opts
            };
            ui.removeClass(classes.done).addClass(classes.active);

            if (!ui.is(':visible')) {
                redrawUI();
                ui.animate(positions.show, 2000).show();
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
                opts,
                hitlist = [];

            for (id in downloads) {
                if (cookies.hasOwnProperty(id) && downloads[id].status != states.finished) {
                    downloads[id].status = states.finished;
                    changed = true;
                    cookieMonster.removeCookie(id);
                    opts = downloads[id].opts;

                    if ('element' in opts)
                        $(opts.element).attr('disabled', null).removeClass('btn-danger').addClass('btn-primary');

                    if ('complete' in opts)
                        opts.complete(opts);

                    $(document).trigger('download-complete', downloads[id].opts);

                    hitlist.push(id);
                }
            }
            for (id in hitlist)
                delete downloads[id];

            if (changed)
                redrawUI();

            if (getStats().remaining == 0 && changed) {
                ui.removeClass(classes.active).addClass(classes.done).delay(3000).animate(positions.hide, 4000, function () {
                    ui.hide();
                });
                $(document).trigger('download-ui-hide');
            }

            setTimeout(monitor, frequency);
        };

        function getStats() {
            var remaining = 0,
                id;
            for (id in downloads) {
                if (downloads[id].status == states.started)
                    remaining++;
            }
            return {
                remaining: remaining
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
            getStats: getStats
        };
    }
);