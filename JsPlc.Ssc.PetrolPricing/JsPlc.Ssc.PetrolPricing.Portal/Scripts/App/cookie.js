define([],
    function () {

        function setCookie(name, value, days) {
            document.cookie = name + "=" + value + (days ? '; expires=' + new Date(new Date().getTime() + days * 86400000).toGMTString() : '') + "; path=/";
        };

        function removeCookie(name) {
            setCookie(name, null, -365);
        };

        function allCookies() {
            var kvp = {},
                parts = (document.cookie || '').split(/\s*;\s*/);
            while (parts.length)
                kvp[parts[0].split('=')[0]] = parts.shift().split('=')[1];
            return kvp;
        };

        function getCookie(name) {
            var cookies = allCookies();
            return cookies.hasOwnProperty(name) ? cookies[name] : null;
        };

        // API
        return {
            setCookie: setCookie,
            removeCookie: removeCookie,
            allCookies: allCookies,
            getCookie: getCookie
        };
    }
);