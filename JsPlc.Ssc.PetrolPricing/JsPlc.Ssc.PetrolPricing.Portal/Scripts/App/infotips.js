define(["jquery", "common"],
    function ($, common) {
        "use strict";

        // force build !!

        var ui = $('<div class="infotip">testing</div>'),
            isVisible = false,
            element = null,
            lastArea = {
                left: 0,
                top: 0,
                width: 0,
                height: 0
            };

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

            var markup = convertToMarkup(ele.attr('data-infotip')),
                dock = (ele.data('infotip-dock') || 'above').toLowerCase(),
                offset = ele.offset(),
                eleWidth = Math.floor(ele.width()),
                eleHeight = ele.height(),
                uiWidth = Math.floor(ui.width()),
                uiHeight = ui.height(),
                uiMarginWidth = 10,
                uiMarginHeight = 26,
                top,
                left,
                gap = 16;

            lastArea.left = offset.left;
            lastArea.top = offset.top;
            lastArea.width = eleWidth;
            lastArea.height = eleHeight;

            element = ele;
            ui.detach()
            ui.html(markup);
            ui.removeClass('infotip-above infotip-below').addClass('infotip-' + dock)

            switch (dock) {
                case 'above':
                    top = Math.floor(offset.top + 0.5 - uiHeight - uiMarginHeight - gap);
                    left = Math.floor(offset.left + (eleWidth - uiWidth) / 2 - uiMarginWidth);
                    break;
                case 'below':
                    top = Math.floor(offset.top + 0.5 + eleHeight + uiMarginHeight + gap),
                    left = Math.floor(offset.left + (eleWidth - uiWidth)/2 - uiMarginWidth)
                    break;
            }

            ui.css({top: top, left: left})
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

        function isMouseInsideArea(ev, area) {
            if (!area || area.width == 0 || area.height == 0)
                return false;
            return ev.pageX >= area.left
                && ev.pageX <= (area.left + area.width)
                && ev.pageY >= area.top
                && ev.pageY <= (area.top + area.height);
        }

        function mouseMoved(ev) {
            var ele = $(ev.target).closest('[data-infotip]');
            if (ele.length == 0 && isMouseInsideArea(ev, lastArea) && element.is(':visible'))
                ele = element;

            if (ele.length) {
                show(ele)
            } else
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
