define(["jquery", "underscore", "EasyTemplate", "text!App/DriveTimeChart.html"],
    function ($, _, easyTemplate ,driveTimeChartMarkup) {
        "use strict";

        var panel = $(driveTimeChartMarkup),
            target;

        var templates = {
            row: null
        };

        var controls = {
            showButton: null,
            hideButton: null
        };

        function getTemplates() {
            templates.row = easyTemplate.extractTemplate($('[data-template="row"]'));
        };

        function findControls() {
            controls.showButton = $('#btnDriveTimeChartShow');
            controls.hideButton = $('#btnDriveTimeChartHide');
        };

        function setButtonState(button, isPrimary) {
            isPrimary
            ? button.removeClass('btn-default').addClass('btn-primary')
            : button.removeClass('btn-primary').addClass('btn-default');
        };

        function expandClick() {
            var button = $(this),
                target = $(button.data('expand'));

            target.removeClass('hide').slideDown(1000);

            setButtonState(controls.showButton, true);
            setButtonState(controls.hideButton, false);
        };

        function collapseClick() {
            var button = $(this),
                target = $(button.data('collapse'));

            target.slideUp(500);

            setButtonState(controls.showButton, false);
            setButtonState(controls.hideButton, true);
        };

        function bindEvents() {
            controls.showButton.off().click(expandClick);
            controls.hideButton.off().click(collapseClick);
        };

        function init(opts) {
            if (!opts || !opts.selector)
            {
                console.log('No Jquery Selector!');
                return;
            }

            var placeholder = $(opts.selector);

            if (!placeholder.length) {
                console.log('Unable to find placeholder, selector:' + opts.selector);
                return;
            }

            placeholder.replaceWith(panel);

            getTemplates();

            findControls();
            bindEvents();
        };

        function sequenceDriveTime(maxDriveTime, fuelMarkups) {
            var sequence = [],
                driveTime,
                markup = 0,
                index = 0,
                isBoundary = false;

            for (driveTime = 0; driveTime <= maxDriveTime; driveTime++) {
                if (index < fuelMarkups.length) {
                    if (driveTime == fuelMarkups[index].DriveTime) {
                        markup = fuelMarkups[index].Markup;
                        index++;
                        isBoundary = true;
                    }
                }
                sequence[driveTime] = {
                    markup: markup,
                    isBoundary: isBoundary
                };
                isBoundary = false;
            }
            return sequence;
        };

        function render(driveTimeMarkups) {
            var maxDriveTime = 0,
                getDriveTime = function (x) { return x.DriveTime; },
                unleaded = driveTimeMarkups.Unleaded,
                diesel = driveTimeMarkups.Diesel,
                superUnleaded = driveTimeMarkups.SuperUnleaded,
                tbody = panel.find('tbody'),
                i,
                tr,
                tokens;

            if (unleaded.length)
                maxDriveTime = Math.max(maxDriveTime, _.max(unleaded, getDriveTime).DriveTime);
            if (diesel.length)
                maxDriveTime = Math.max(maxDriveTime, _.max(diesel, getDriveTime).DriveTime);
            if (superUnleaded.length)
                maxDriveTime = Math.max(maxDriveTime, _.max(superUnleaded, getDriveTime).DriveTime);
            
            var sequences = {
                unleaded: sequenceDriveTime(maxDriveTime, unleaded),
                diesel: sequenceDriveTime(maxDriveTime, diesel),
                superUnleaded: sequenceDriveTime(maxDriveTime, superUnleaded)
            };

            tbody.children().remove();

            for (i = 0; i <= maxDriveTime; i++) {
                tokens = {
                    '{DriveTime}': i,
                    '{UnleadedMarkup}': sequences.unleaded[i].markup,
                    '{DieselMarkup}': sequences.diesel[i].markup,
                    '{SuperUnleadedMarkup}': sequences.superUnleaded[i].markup,
                    '{UnleadedBoundaryCss}': sequences.unleaded[i].isBoundary ? 'boundary' : '',
                    '{DieselBoundaryCss}': sequences.diesel[i].isBoundary ? 'boundary' : '',
                    '{SuperUnleadedBoundaryCss}': sequences.superUnleaded[i].isBoundary ? 'boundary' : ''
                };

                tr = $(easyTemplate.replaceTokens(templates.row, tokens));
                tr.appendTo(tbody);
            }
        };

        // API
        return {
            init: init,
            render: render                    
        };
    }
);