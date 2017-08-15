define(["jquery"],
    function ($) {

        "use strict";

        var classes = {
            group: 'choose-one-group',
            hide: 'choose-one-group-hide',
            show: 'choose-one-group-show'
        };

        function drawGroup(groupName, show) {
            var group = $('[data-choose-one-group="' + groupName + '"]');

            if (show)
                group.removeClass(classes.hide).addClass(classes.show);
            else
                group.removeClass(classes.show).addClass(classes.hide);
        };

        function docReady() {
            var groups = $('[data-choose-one-group]');
            groups.each(function () {
                var ele = $(this);
                ele.addClass(classes.group).addClass(classes.hide);
            });
        };

        function init() {
            $(docReady);
        };

        // API
        return {
            init: init,
            drawGroup: drawGroup
        }
    }
);