define(["jquery", "infotips", "notify", "DriveTimeMarkupService"],
    function ($, infotips, notify, driveTimeMarkupService) {
        "use strict";

        var panels = {
            unleaded: null,
            diesel: null,
            superUnleaded: null
        };

        var events = {
            dataSaved: 'data-saved'
        };

        var callback = function () { };

        function saveClick() {
            var data = [],
                failure = function () {
                    notify.error('Unable to save Drive Time Markup data');
                },
                success = function (serverData) {
                    if (!serverData || !serverData.Result)
                        failure();

                    var status = serverData.Result;

                    if (status.SuccessMessage) {
                        notify.success(status.SuccessMessage);
                        callback(events.dataSaved);
                    }
                    else if (status.ErrorMessage)
                        notify.error(status.ErrorMessage);
                    else
                        notify.info('Unknown response when saving Drive Time Markup data');
                };

            if (!panels.unleaded.isValid()) {
                notify.error('Please fix the error(s) for Unleaded');
                return;
            }
            if (!panels.diesel.isValid()) {
                notify.error('Please fix the error(s) for Diesel')
                return;
            }
            if (!panels.superUnleaded.isValid()) {
                notify.error('Please fix the error(s) for Super-Unleaded')
                return;
            }

            data = data.concat(panels.unleaded.serialise(),
                panels.diesel.serialise(),
                panels.superUnleaded.serialise()
                );

            driveTimeMarkupService.updateDriveTimeMarkups(success, failure, data);
        };

        function bindEvents() {
            $('#btnSaveDriveTimeMarkups').off().click(saveClick);
        };

        function init(fuels, eventHandler) {
            callback = eventHandler || function () { };

            panels.unleaded = fuels.unleaded;
            panels.diesel = fuels.diesel;
            panels.superUnleaded = fuels.superUnleaded;

            bindEvents();
        };

        // API
        return {
            init: init
        };
    });