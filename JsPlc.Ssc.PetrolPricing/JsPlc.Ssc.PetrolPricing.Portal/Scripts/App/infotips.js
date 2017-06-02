define(["jquery", "common"],
    function ($, common) {
        "use strict";

        var isEnabled = true,
            ui = $('<div class="infotip">testing</div>'),
            isVisible = false,
            element = null,
            lastArea = {
                left: 0,
                top: 0,
                width: 0,
                height: 0
            },
            lastTrigger = {
                left: 0,
                top: 0,
                text: '',
                dock: '',
                width: 0,
                height: 0
            };

        var settings = {
            marginWidth: 10,
            marginHeight: 26,
            gap: 16
        };

        function convertToMarkup(text) {
            return text.split('[').join('<').split(']').join('>');
        };

        function hasSamePropertyValues(left, right) {
            for (var prop in left) {
                if (!(prop in right) || right[prop] != left[prop])
                    return false;
            }
            return true;
        };

        function showInfotip(ele, mouseX, mouseY) {
            if (!ele) {
                hideInfotip();
                return;
            }

            var eleOffset = ele.offset(),
                trigger = {
                    left: Math.floor(eleOffset.left),
                    top: Math.floor(eleOffset.top),
                    text: ele.attr('data-infotip'),
                    dock: (ele.data('infotip-dock') || 'above').toLowerCase(),
                    width: Math.floor(ele.innerWidth()),
                    height: Math.floor(ele.innerHeight())
                };

            // already drawn same infotip ?
            if (hasSamePropertyValues(trigger, lastTrigger))
                return;

            // empty infotip ?
            if (!/\S/.test(trigger.text)) {
                hideInfotip();
                return;
            }

            lastTrigger = $.extend({}, trigger);

            ui.html(convertToMarkup(trigger.text))
                .removeClass('infotip-above infotip-below').addClass('infotip-' + trigger.dock)
                .css({ left: 0, top: 0, visibility: 'hidden' })
                .show();

            if (!setPosition(trigger))
                setPosition(trigger); // reposition due to page edge/infotip word wrap

            isVisible = true;
        };

        function setPosition(trigger) {
            var infotip = {
                top: 0,
                left: 0,
                width: Math.floor(ui.outerWidth()),
                height: Math.floor(ui.outerHeight())
            };

            switch (trigger.dock) {
                case 'above':
                    infotip.top = trigger.top - infotip.height - settings.gap;
                    infotip.left = trigger.left + (trigger.width - infotip.width) / 2;
                    break;
                case 'below':
                    infotip.top = trigger.top + trigger.height + settings.gap;
                    infotip.left = trigger.left + (trigger.width - infotip.width) / 2;
                    break;
            }

            ui.css({
                top: infotip.top,
                left: infotip.left,
                visibility: 'visible'
            });

            return Math.floor(ui.outerWidth()) == infotip.width && Math.floor(ui.outerHeight()) == infotip.height;
        };

        function hideInfotip() {
            isVisible = false;
            if (element != null) {
                element.data('infotip-active', false);
                element = null;
            }
            ui.hide();
            lastTrigger.width = 0;
        };

        function injectDom() {
            ui.hide().appendTo(document.body);
        };

        function isMouseInsideLastTrigger(ev) {
            if (lastTrigger.width == 0  || lastTrigger.height == 0)
                return false;
            return ev.pageX >= lastTrigger.left
                && ev.pageX <= (lastTrigger.left + lastTrigger.width)
                && ev.pageY >= lastTrigger.top
                && ev.pageY <= (lastTrigger.top + lastTrigger.height);
        };

        function mouseMoved(ev) {
            if (!isEnabled)
                return;

            var ele = $(ev.target).closest('[data-infotip]');
            if (ele.length == 0 && isMouseInsideLastTrigger(ev) && element && element.is(':visible'))
                ele = element;

            if (ele.length != 0 && !ele.attr('disabled')) {
                showInfotip(ele, ev.pageX, ev.pageY)
            } else
                hideInfotip();
        };

        function show() {
            isEnabled = true;
        };

        function hide() {
            if (isEnabled)
                hideInfotip();
            isEnabled = false;
        };

        function bindEvents() {
            $(document.body).on('mousemove', mouseMoved).on('scroll', mouseMoved);
        };

        function docReady() {
            injectDom();
            bindEvents();
        };

        $(docReady);
        
        // API
        return {
            show: show,
            hide: hide,
        };
    }
);
