define(['cookie'],
    function (cookieMonster) {

        var cookieName = 'uiSettings',
            days = 365,
            seperater = '|',
            names = [
                'pricing.showBarChart',
                'pricing.expandGrid',
                'pricing.highlightTrialPrices',
                'pricing.highlightMatchCompetitors',
                'pricing.priceChangeFilterDown',
                'pricing.priceChangeFilterNone',
                'pricing.priceChangeFilterUp',
                'pricing.highlightNoNearbyGrocerPrices',
                'pricing.highlightHasNearbyGrocerPrices',

                'sites.showActiveSites',
                'sites.showInactiveSites',
                'sites.highlightTrialPrices',
                'sites.highlightMatchCompetitors',
                'sites.highlightNonePrices'
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

        function write(name, value) {
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

        // API
        return {
            read: read,
            readBoolean: readBoolean,
            write: write,
            writeBoolean: writeBoolean,
            restore: restoreObject,
            update: updateObject
        };
    }
);