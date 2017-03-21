define(["bootstrap", "scrollToTop"],
    function (bootstrap, scrollToTop) {
        "use strict";

        //function setNavigationTab() {
        //    //navigation
        //    $(".navbar-nav li").removeClass("active");
        //    var navItemAttributeName = "data-menu-item";
        //    $(".navbar-nav li").each(function () {
        //        $this = $(this);
        //        var locationPath = location.pathname.toLowerCase();

        //        // ignore dev URL
        //        locationPath = locationPath.replace(/^\/petrolpricing\//i, '/');
        //        //nav-home // /
        //        if (locationPath == '/'
        //            && $this.attr(navItemAttributeName) == "nav-home") {

        //            $this.addClass("active");
        //            return false;
        //        }
        //            //nav-login // /Account/Login
        //        else if (locationPath.indexOf("/account/login") == 0
        //            && $this.attr(navItemAttributeName) == "nav-login") {
        //            $this.addClass("active");
        //            return false;
        //        }
        //            //nav-account // /Manage || /Account
        //        else if ((locationPath.indexOf("/account") == 0
        //                    || location.pathname.indexOf("/Manage") == 0
        //            )
        //            && $this.attr(navItemAttributeName) == "nav-account") {
        //            $this.addClass("active");
        //            return false;
        //        }
        //            //nav-about // /Home/About
        //        else if (locationPath.indexOf("/home/about") == 0
        //            && $this.attr(navItemAttributeName) == "nav-about") {
        //            $this.addClass("active");
        //            return false;
        //        }
        //            //nav-contact // /Home/Contact
        //        else if (locationPath.indexOf("/home/contact") == 0
        //            && $this.attr(navItemAttributeName) == "nav-contact") {
        //            $this.addClass("active");
        //            return false;
        //        }
        //            //nav-upload // /File
        //        else if (locationPath.indexOf("/file") == 0
        //            && $this.attr(navItemAttributeName) == "nav-upload") {
        //            $this.addClass("active");
        //            return false;
        //        }
        //            //nav-site-pricing // /Sites/Prices
        //        else if (locationPath.indexOf("/sites/prices") == 0
        //            && $this.attr(navItemAttributeName) == "nav-site-pricing") {
        //            $this.addClass("active");
        //            return false;
        //        }
        //            //nav-site-maintenance // /Sites
        //        else if (locationPath.indexOf("/sites") == 0
        //            && $this.attr(navItemAttributeName) == "nav-site-maintenance") {
        //            $this.addClass("active");
        //            return false;
        //        }
        //            //nav-reports // /PriceReports
        //        else if (locationPath.indexOf("/pricereports") == 0
        //            && $this.attr(navItemAttributeName) == "nav-reports") {
        //            $this.addClass("active");
        //            return false;
        //        }

        //            //nav-reports // /Users
        //        else if (locationPath.indexOf("/ppusers") == 0
        //            && $this.attr(navItemAttributeName) == "nav-users") {
        //            $this.addClass("active");
        //            return false;
        //        }
        //    });
        //}

        function init() {
            scrollToTop.init();
            //setNavigationTab();
        };

        // API
        return {
            init: init
        };
    }
);