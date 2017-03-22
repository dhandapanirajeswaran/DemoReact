define(["jquery", "common"],
    function ($, common) {
        "use strict";

        var states = {
            isScrolling: false
        };

        var mode = states.hide;

        var defaults = {
            minY: 20,
            maxY: 200,
            scrollSpeed: 300
        };

        var ui = $('<div id="ScrollToTopComponent" class="scroll-to-top" title="Scroll to top of window"><div class="scroll-to-top-inner"><i class="fa fa-eject"></i> Top</div></div>');

        function injectDom() {
            ui.css('opacity', 0).appendTo(document.body);
        };

        function scrollClick() {
            states.isScrolling = true;
            $("html, body").animate({ "scrollTop": "0px" }, defaults.scrollSpeed, function () {
                states.isScrolling = false;
            });
        };

        function limit(value, min, max) {
            return value < min ? min : value > max ? max : value;
        };

        function windowScrolled() {
            var scroll = parseInt($(window).scrollTop(), 10),
                opacity = scroll / (defaults.maxY - defaults.minY);

            opacity = limit(opacity, 0, 1);
            ui.css('opacity', opacity);
        };

        function bindEvents() {
            $('#ScrollToTopComponent').off().on('click', scrollClick);
            $(window.document).scroll(windowScrolled);
        };

        function init() {
            injectDom();
            windowScrolled();
            bindEvents();
        };

        // API
        return {
            init: init
        };
    }
);