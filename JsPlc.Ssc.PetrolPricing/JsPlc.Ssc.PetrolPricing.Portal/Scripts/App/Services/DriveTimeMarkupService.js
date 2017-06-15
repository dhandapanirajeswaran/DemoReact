define(["jquery", "common"],
    function ($, common) {
        "use strict";

        function updateDriveTimeMarkups(success, failure, driveTimeMarkups) {
            //jQuery.ajaxSettings.traditional = true;

            // Fix for passing JavaScript Arrays to MVC (arrgghh)
            var data = JSON.stringify({ model: driveTimeMarkups });

            var url = "Settings/UpdateDriveTimeMarkups";
            var promise = common.callService("post", url, data);
            promise.done(function (response, textStatus, jqXhr) {
                success(response);
            });
            promise.fail(failure);
        };

        // API
        return {
            updateDriveTimeMarkups: updateDriveTimeMarkups
        };
    }
);
