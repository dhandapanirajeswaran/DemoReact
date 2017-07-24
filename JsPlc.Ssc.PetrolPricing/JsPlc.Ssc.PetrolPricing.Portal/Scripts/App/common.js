define(['jquery'], function ($) {
    "use strict";

    var uiDateFormat = "DD/MM/YYYY";
    var serverDateFormat = "YYYYMMDD";

    var uiDateTimeFormat = "DD/MM/YYYY HH:mm:ss";

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
            //contentType: 'application/json;',
            //traditional: true,
            dataType: "json"
        });

        return $promise;
    };

    var callTraditionalService = function (verb, url, jsonArgs) {
        var $promise = $.ajax({
            data: jsonArgs,
            url: siteRoot + url,
            type: verb,
            contentType: 'application/json;',
            traditional: true,
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

    function isLocalHost() {
        return /^localhost/i.test(window.location.hostname);
    };

    function getRootSiteFolder() {
        var rootFolder = /\/petrolpricing\//i.test(window.location.href) ? "/petrolpricing/" : "/";
        return rootFolder;
    };

    return {
        splitArray: splitArray,
        setSiteRoot: setSiteRoot,
        getSiteRoot: getSiteRoot,
        randomString: randomString,
        uiDateFormat: uiDateFormat,
        uiDateTimeFormat: uiDateTimeFormat,
        serverDateFormat: serverDateFormat,
        callService: callService,
        callTraditionalService: callTraditionalService,
        htmlEncode: htmlEncode,
        reportRootFolder: reportRootFolder,
        isLocalHost: isLocalHost,
        getRootSiteFolder: getRootSiteFolder
    };
});



