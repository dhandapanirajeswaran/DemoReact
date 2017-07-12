define(["jquery", "common", "PetrolPricingService"],
    function ($, common, petrolPricingService) {
        "use strict";


        function loadSchedule(success, failure, winScheduleId) {
            var url = "Settings/LoadEmailScheduleItem/?winScheduleId=" + winScheduleId;
            var promise = common.callService("get", url, null);
            promise.done(function (response, textStatus, jqXhr) {
                success(response);
            });
            promise.fail(failure);
        };

        function saveSchedule(success, failure, winServiceSchedule) {
            var url = "Settings/SaveEmailScheduleItem";
            var promise = common.callService("post", url, winServiceSchedule);
            promise.done(function (response, textStatus, jqXhr) {
                success(response);
            });
            promise.fail(failure);
        };

        function runSchedule(success, failure) {
            var url = "Settings/ExecuteWinServiceSchedule";
            var promise = common.callService("get", url, null);
            promise.done(function (response, textStatus, jqXhr) {
                success(response);
            });
            promise.fail(failure);
        };

        function clearEventLog(success, failure) {
            var url = "Settings/ClearWinServiceEventLog";
            var promise = common.callService("get", url, null);
            promise.done(function (response, textStatus, jqXhr) {
                success(response);
            });
            promise.fail(failure);
        };

        // API
        return {
            loadSchedule: loadSchedule,
            saveSchedule: saveSchedule,
            runSchedule: runSchedule,
            clearEventLog: clearEventLog
        };
    }
);