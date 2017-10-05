define([],
    function () {
        "use strict";

        function isNumber(value) {
            return value != '' && !isNaN(value);
        };

        function isNonZeroNumber(value) {
            return isNumber(value) && value > 0;
        };

        function isNumberInRange(value, min, max) {
            if (!isNumber(value))
                return false;
            return Number(value) >= min && Number(value) <= max;
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

        function isSainsburysEmail(email) {
            return email && /@sainsburys\.co\.uk$/i.test(email);
        };

        // API
        return {
            isNumber: isNumber,
            isNonZeroNumber: isNonZeroNumber,
            isNumberInRange: isNumberInRange,
            isDriveTime: isDriveTime,
            isMarkup: isMarkup,
            isSainsburysEmail: isSainsburysEmail
        };
    }
);