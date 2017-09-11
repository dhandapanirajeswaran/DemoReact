define([],
    function ($) {
        "use strict";

        var months = ['Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun', 'Jul', 'Aug', 'Sep', 'Oct', 'Nov', 'Dec'];
        var days = ['Sun', 'Mon', 'Tue', 'Wed', 'Thu', 'Fri', 'Sat'];

        function add(dt, years, days, hours, mins, secs, ms) {
            var yyyy = dt.getFullYear() + Number(years),
                mm = dt.getMonth(),
                dd = dt.getDate() + Number(days),
                h = dt.getHours() + Number(hours),
                m = dt.getMinutes() + Number(mins),
                s = dt.getSeconds() + Number(secs),
                mils = dt.getMilliseconds() + Number(ms);

            return new Date(yyyy, mm, dd, h, m, s, mils);
        };

        function now() {
            return new Date();
        };

        function today() {
            var dt = new Date();
            return new Date(dt.getFullYear(), dt.getMonth(), dt.GetDate(), 0, 0, 0, 0);
        };

        function yesterday() {
            return add(today(), 0, -1, 0, 0, 0, 0);
        };

        function tomorrow() {
            return add(today(), 0, 1, 0, 0, 0, 0);
        };

        function addDays(days, dt) {
            return add(dt, 0, days, 0, 0, 0, 0);
        };

        function addYears(years, dt) {
            return add(dt, years, 0, 0, 0, 0, 0);
        };

        function isDate(value) {
            if (!value)
                return false;
            return isNaN(Date.parse(value));
        };

        function isSameDay(dt1, dt2) {
            return dt1.getFullYear() == dt2.getFullyear()
                && dt1.getMonth() == dt2.getMonth()
                && dt1.getDate() == dt2.getDate();
        };

        function format(template, dt) {
            var yyyy = dt.getFullYear(),
                mm = dt.getMonth() + 1,
                dd = dt.getDate(),
                formatted = template.replace('YYYY', yyyy)
                .replace('MMM', months[mm - 1].toUpperCase())
                .replace('MM', (mm < 10 ? '0' + mm : mm))
                .replace('DD', (dd < 10 ? '0' + dd : dd));

            return formatted;
        };

        function dayDiff(dt1, dt2) {
            var ticks = Math.abs(addDays(1, startOfDay(dt2)).getTime() - startOfDay(dt1).getTime());
            return Math.ceil(ticks / (1000 * 60 * 60 * 24));
        };

        function convertJsonDate(date) {
            if (!date)
                return;
            var ticks = ('' + date).replace(/\D/g, '');
            return new Date(Number(ticks));
        };

        function startOfDay(dt) {
            var yyyy = dt.getFullYear(),
                mm = dt.getMonth(),
                dd = dt.getDate();
            return new Date(yyyy, mm, dd);
        };

        function dateFromDDMMYYYY(value) {
            if (!/^\d{2}\/\d{2}\/\d{4}$/.test(value))
                return undefined;
            var parts = value.split('/');
            return new Date(Number(parts[2]), Number(parts[1]), Number(parts[0]));
        };

        // API
        return {
            now: now,
            today: today,
            tomorrow: tomorrow,
            yesterday: yesterday,
            addDays: addDays,
            addYears: addYears,
            isDate: isDate,
            isSameDay: isSameDay,
            format: format,
            dayDiff: dayDiff,
            startOfDay: startOfDay,
            convertJsonDate: convertJsonDate,
            dateFromDDMMYYYY: dateFromDDMMYYYY
        };
    }
);
