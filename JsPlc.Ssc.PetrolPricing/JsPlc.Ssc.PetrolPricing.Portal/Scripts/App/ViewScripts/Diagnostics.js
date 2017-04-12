define(["jquery", "common"],
    function ($, common) {
        "use strict";

        // placeholder...


        function docReady() {
            $('[data-toggle-expand]').on('click', function () {
                var btn = $(this),
                    target = $(btn.data('toggle-expand')),
                    isCollapsed = target.hasClass('collapsed'),
                    icon = btn.find('span i.fa:first');

                if (isCollapsed) {
                    target.removeClass('collapsed').addClass('expanded');
                    icon.removeClass('fa-caret-square-o-down').addClass('fa-caret-square-o-up');
                }
                else {
                    target.removeClass('expanded').addClass('collapsed');
                    icon.removeClass('fa-caret-square-o-up').addClass('fa-caret-square-o-down');
                }
            });

            $('[data-expand-all').on('click', function () {
                var btn = $(this),
                    container = $(btn.data('expand-all'));
                
                container.find('.collapsed [data-toggle-expand]').each(function () {
                    $(this).trigger('click');
                });
            });

            $('[data-collapse-all').on('click', function () {
                var btn = $(this),
                    container = $(btn.data('collapse-all'));

                container.find('.expanded [data-toggle-expand]').each(function () {
                    $(this).trigger('click');
                });
            });

        };

        $(docReady);

        // API
        return {

        };
    }
);