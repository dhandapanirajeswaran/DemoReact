define(["jquery"],
    function () {
        "use strict";

        var ele,
            countdown;

        function redraw() {
            var mins = Math.floor(countdown / 60),
                seconds = countdown % 60;

            if (mins == 0)
                ele.html(seconds + ' seconds');
            else
                ele.html(mins + ' mins, ' + seconds + ' seconds');
        };

        function monitor() {
            redraw();
        };

        function start(selector, seconds) {
            ele = $(selector);
            countdown = seconds;
            redraw();
        };

        function stop() {
        };

        // API
        return {
            start: start,
            stop: stop
        }
    }
);