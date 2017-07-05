define(["jquery"],
    function ($) {
        "use strict";

        var settings = {
            scrollSpeed: 500,
            removeDelay: 3000
        };

        function scrollTo(selector) {
            var ele = $(selector),
                elePos,
                top;
            if (ele.length == 0) {
                console.log('Unable to find scrollToElement: ' + selector);
                return false;
            }

            elePos = ele.position();

            top = Math.max(0, elePos.top - $(window).height() / 2);

            $('html, body').animate({ "scrollTop": top }, settings.scrollSpeed, function () {
                ele.removeClass('ping-yellow').addClass('ping-yellow');
                setTimeout(function () {
                    ele.removeClass('ping-yellow');
                }, settings.removeDelay);
            });
            return true;
        };

        // API
        return {
            scrollTo: scrollTo
        };
    }
);