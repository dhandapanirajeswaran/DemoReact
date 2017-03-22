define(["jquery", "bootstrap", "scrollToTop"],
    function ($, bootstrap, scrollToTop) {
        "use strict";

        var navSectionMap = {
            "nav-home": ["^\/$"],
            "nav-login": ["^/account/login"],
            "nav-account": ["^/account", "^/manage"],
            "nav-about": ["^/home/about"],
            "nav-contact": ["^/home/contact"],
            "nav-upload": ["^/file"],
            "nav-site-pricing": ["^/sites/prices"],
            "nav-site-maintenance": ["^/sites"],
            "nav-reports": ["^/pricereports"],
            "nav-users": ["^/ppusers"]
        };

        function setNavigationTab() {
            var activeTab = null,
                locationPath = location.pathname.toLowerCase().replace(/^\/petrolpricing\//i, '/'),
                key,
                paths,
                path;

            for (key in navSectionMap) {
                paths = navSectionMap[key];
                $.each(navSectionMap[key], function (i, value) {
                    if (new RegExp(value, 'i').test(locationPath))
                        activeTab = key;
                });
                if (activeTab)
                    break;
            }
            $(".navbar-nav li").removeClass("active");
            $(".navbar-nav li[data-menu-item='" + activeTab + "']").addClass("active");
        };

        function init() {
            scrollToTop.init();
            setNavigationTab();
        };

        // API
        return {
            init: init
        };
    }
);