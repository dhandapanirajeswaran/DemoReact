﻿define(['cookie', 'DateUtils'],
    function (cookieMonster, dateUtils) {

        var cookieName = 'uiSettings',
            days = 365,
            seperater = '|',
            names = [
                'pricing.showBarChart',
                'pricing.expandGrid',
                'pricing.highlightTrialPrices',
                'pricing.highlightMatchCompetitors',
                'pricing.highlightStandardPrices',
                'pricing.priceChangeFilterDown',
                'pricing.priceChangeFilterNone',
                'pricing.priceChangeFilterUp',
                'pricing.highlightNoNearbyGrocerPrices',
                'pricing.highlightHasNearbyGrocerPrices',
                'pricing.highlightHasNearbyGrocerWithOutPrices',
                'sites.showActiveSites',
                'sites.showInactiveSites',
                'sites.highlightTrialPrices',
                'sites.highlightMatchCompetitors',
                'sites.highlightStandardPrices',
                'sites.showHasEmails',
                'sites.showNoEmails',
                'build.date',
                'pricing.priceSummaryTabIndex',
                'pricing.autoHideOverrides',
                'pricing.includeDriveTime',
                'pricing.hideNoPrices',
                'pricing.viewDate',
                'pricing.storeNo',
                'pricing.storeName',
                'pricing.storeTown',
                'pricing.catNo',
                'cookie.updatedOn',
                'cookie.viewDateUpdatedOn',
                'help.lastPage',
                'help.isOpen'
            ],
            values = readSettingsCookie();

        function readSettingsCookie() {
            var i,
                data = (cookieMonster.getCookie(cookieName) || '').split(seperater),
                settings = {};
            for (i = 0; i < names.length; i++) {
                settings[names[i]] = i < data.length ? data[i] : '';
            }
            return settings;
        }

        function updateSettingsCookie() {
            var i,
                data = [];

            values['cookie.updatedOn'] = getTodayDateStamp();

            for (i = 0; i < names.length; i++) {
                data.push(values[names[i]]);
            }
            cookieMonster.setCookie(cookieName, data.join(seperater), days);
        };

        function read(name, defaultValue) {
            return (name in values) ? values[name] : defaultValue;
        };

        function readBoolean(name, defaultValue) {
            var value = read(name, defaultValue);
            if (value == '')
                return defaultValue;
            return value == '1' || value == 'true';
        };

        function readInteger(name, defaultValue) {
            var value = read(name, defaultValue)
            return value == '' || isNaN(value)
                ? defaultValue
                : Number(value);
        };

        function write(name, value) {
            if (name == 'pricing.viewDate') {
                values['cookie.viewDateUpdatedOn'] = getTodayDateStamp(); // capture when the 'pricing.viewDate' was last Set
            }

            if (name in values) {
                values[name] = value;
                updateSettingsCookie();
            }
            else
                console.log('Unknown setting: ' + name);
        };

        function writeBoolean(name, value) {
            write(name, value ? '1' : '0');
        };

        function writeInteger(name, value) {
            write(name, value);
        };

        function updateObject(map, obj) {
            var key;
            for (key in map) {
                if (key in obj)
                    writeBoolean(map[key], obj[key]);
            }
        };

        function restoreObject(map, obj) {
            var key;
            for (key in map) {
                if (key in obj)
                    obj[key] = readBoolean(map[key], false);
            }
        };

        function getTodayDateStamp() {
            return dateUtils.format('YYYY-MM-DD', new Date());
        };

        function wasCookieUpdatedToday() {
            var crumb = read('cookie.updatedOn', '');
            return crumb && crumb == getTodayDateStamp();
        };

        function wasViewDateUpdatedToday() {
            var crumb = read('cookie.viewDateUpdatedOn', '');
            return crumb && crumb == getTodayDateStamp();
        };

        // API
        return {
            read: read,
            readBoolean: readBoolean,
            readInteger: readInteger,
            write: write,
            writeBoolean: writeBoolean,
            writeInteger: writeInteger,
            restore: restoreObject,
            update: updateObject,
            wasCookieUpdatedToday: wasCookieUpdatedToday,
            wasViewDateUpdatedToday: wasViewDateUpdatedToday
        };
    }
);