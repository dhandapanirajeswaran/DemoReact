define(["jquery", "common"],
    function ($, common) {
        "use strict";

        var ui = $('<div class="infotip">testing</div>'),
            isVisible = false,
            element = null;

        var timings = {
            fadeOut: 1000,
            fadeIn: 100
        };

        function convertToMarkup(text) {
            return text.split('[').join('<').split(']').join('>');
        };

        function show(ele) {
            if (!ele || ele == element)
                return;

            var markup = convertToMarkup(ele.data('infotip')),
                offset = ele.offset(),
                eleWidth = Math.floor(ele.width()),
                eleHeight = ele.height(),
                uiWidth = Math.floor(ui.width()),
                uiHeight = ui.height(),
                uiMarginWidth = 16,
                uiMarginHeight = 24;

            element = ele;
            ui.detach()
            ui.html(markup);

            ui.css({
                top: Math.floor(offset.top + 0.5) - uiHeight - uiMarginHeight,
                left: Math.floor(offset.left + (eleWidth - uiWidth)/2 - uiMarginWidth)
            })
                .show()
                .appendTo(document.body);
            isVisible = true;
        };

        function hide() {
            isVisible = false;
            if (element == null)
                return;
            ui.hide();
        };

        function injectDom() {
            ui.hide().appendTo(document.body);
        };

        function mouseMoved(ev) {
            var ele = $(ev.target).closest('[data-infotip]');
            if (ele.length) {
                ev.preventDefault();
                ev.stopPropagation();
                show(ele);
            }
            else
                hide();
        };

        function bindEvents() {
            $(document.body).on('mousemove', mouseMoved);
        };

        function docReady() {
            injectDom();
            bindEvents();
        };

        $(docReady);
        
        // API
        return {
            show: show,
            hide: hide
        };
    }
);
