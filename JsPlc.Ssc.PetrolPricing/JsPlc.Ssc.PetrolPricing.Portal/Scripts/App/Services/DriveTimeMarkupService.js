define(["jquery", "common"],
    function ($, common) {
        "use strict";

        function updateDriveTimeMarkups(success, failure, driveTimeMarkups) {
            //jQuery.ajaxSettings.traditional = true;

            // Fix for passing JavaScript Arrays to MVC (arrgghh)
            var data = JSON.stringify({ model: driveTimeMarkups });

            var url = "Settings/UpdateDriveTimeMarkups";
            var promise = common.callTraditionalService("post", url, data);
            promise.done(function (response, textStatus, jqXhr) {
                success(response);
            });
            promise.fail(failure);
        };

        function loadDriveTimeMarkups(success, failure) {
            var url = "Settings/GetDriveTimeMarkupsJson";
            var promise = common.callService("get", url);
            promise.done(function (response, textStatus, jqXhr) {
                success(response);
            });
            promise.fail(failure);
        };

        // API
        return {
            updateDriveTimeMarkups: updateDriveTimeMarkups,
            loadDriveTimeMarkups: loadDriveTimeMarkups
        };
    }
);
