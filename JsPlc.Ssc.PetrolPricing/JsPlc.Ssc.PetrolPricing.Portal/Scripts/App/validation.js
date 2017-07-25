define([],
    function () {
        "use strict";

        function isNumber(value) {
            return value != '' && !isNaN(value);
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
            isNumberInRange: isNumberInRange,
            isDriveTime: isDriveTime,
            isMarkup: isMarkup,
            isSainsburysEmail: isSainsburysEmail
        };
    }
);