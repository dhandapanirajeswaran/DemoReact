define([],
    function () {
        "use strict";

        function isNumber(value) {
            return value != '' && !isNaN(value);
        };

        function isDriveTime(value) {
            if (!isNumber(value))
                return false;
            return /^\d{1,3}$/.test(value);
        };

        function isMarkup(value) {
            if (!isNumber(value))
                return false;
            return /^\d{1,3}$/.test(value);
        };

        // API
        return {
            isNumber: isNumber,
            isDriveTime: isDriveTime,
            isMarkup: isMarkup
        };
    }
);