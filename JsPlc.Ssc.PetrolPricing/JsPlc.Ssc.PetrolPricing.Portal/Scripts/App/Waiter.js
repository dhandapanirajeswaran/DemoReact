define(["jquery", "common", "text!waiter.html"],
    function ($, common, waiterHtml) {

        "use strict";

        var state = {
            visible: false
        };

        var overlay = $(waiterHtml),
            lightbox = $('<div class="lightbox-overlay"></div>');

        var selectors = {
            title: '[data-waiter-id="title"]',
            message: '[data-waiter-id="message"]'
        };

        var timings = {
            show: 100,
            hide: 500
        };

        var defaultOptions = {
            title: 'Please wait',
            message: 'Loading Data. Please wait'
        };

        function initDom() {
            overlay.css({ 'zIndex': 2010 }).hide();
            lightbox.css({ 'position': 'fixed', 'top': 0, 'bottom': 0, 'left': 0, 'right': 0, 'backgroundColor': '#000', 'zIndex': 2000, 'opacity': 0 }).hide();

            overlay.appendTo(document.body);
            lightbox.appendTo(document.body);
        };


        function hide() {
            state.visible = false;
            overlay.fadeOut(timings.hide);
            lightbox.fadeOut(timings.hide);
        };

        function show(opts) {
            var options = $.extend({}, defaultOptions, opts);

            overlay.find(selectors.title).html(options.title);
            overlay.find(selectors.message).html(options.message);

            if (state.visible)
                return;

            state.visible = true;

            lightbox.show();
            overlay.show().fadeIn(timings.show);
        };

        initDom();

        // API
        return {
            show: show,
            hide: hide
        };
    }
);