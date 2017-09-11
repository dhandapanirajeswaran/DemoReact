define(["common"],
    function (common) {
        "use strict";

        var months = [
            'JAN',
            'FEB',
            'MAR',
            'APR',
            'MAY',
            'JUN',
            'JUL',
            'AUG',
            'SEP',
            'OCT',
            'NOV',
            'DEC'
        ];

        function getDateFromDDMMYYYY(value) {
            var parts = value.split('/');
            return new Date(Number(parts[2]), Number(parts[1]) - 1, Number(parts[0]), 0, 0, 0, 0);
        };

        function dateToUrlYYYYMMMDD(value) {
            var date = getDateFromDDMMYYYY(value),
                yyyy = date.getFullYear(),
                mm = date.getMonth(),
                dd = date.getDate();
            return yyyy + months[mm] + (dd < 10 ? '0' + dd : dd);
        };

        function standardGetRequest(success, failure, url) {
            var promise = common.callService("get", url, null);
            promise.done(function (response, textStatus, jqXhr) {
                success(response);
            });
            promise.fail(failure);
            return promise;
        };

        function standardPostRequest(success, failure, url, data) {
            var promise = common.callService("post", url, data);
            promise.done(function (response, textStatus, jqXhr) {
                success(response);
            });
            promise.fail(failure);
            return promise;
        };

        // API
        return {
            dateToUrlYYYYMMMDD: dateToUrlYYYYMMMDD,
            standardGetRequest: standardGetRequest,
            standardPostRequest: standardPostRequest
        };
    }
);