define(['jquery'], function ($) {
    "use strict";

    var uiDateFormat = "DD/MM/YYYY";
    var serverDateFormat = "YYYYMMDD";

    var divEncoder = $('<div />');

    //Root page of web app
    var siteRoot = '/';

    siteRoot = $('base').attr('href');

    var getSiteRoot = function () {
        return siteRoot;
    };

    var setSiteRoot = function (pathString) {
        siteRoot = pathString;
    };

    var randomString = function () {
        return Math.random().toString(36).substring(7);
    };

    var callService = function (verb, url, jsonArgs) {
        var $promise = $.ajax({
            data: jsonArgs,
            url: siteRoot + url,
            type: verb,
            dataType: "json"
        });

        return $promise;
    };

    function splitArray(a, size) {
        var len = a.length, out = [], i = 0;
        while (i < len) {
            out.push(a.slice(i, i += size));
        }
        return out;
    }

    function htmlEncode(text) {
        return $('<div />').text(text).html()
    };

    function reportRootFolder() {
        var rootFolder = /\/petrolpricing\//i.test(window.location.href) ? "/petrolpricing" : "";
        return rootFolder;
    };

    return {
        splitArray: splitArray,
        setSiteRoot: setSiteRoot,
        getSiteRoot: getSiteRoot,
        randomString: randomString,
        uiDateFormat: uiDateFormat,
        serverDateFormat: serverDateFormat,
        callService: callService,
        htmlEncode: htmlEncode,
        reportRootFolder: reportRootFolder
    };
});



