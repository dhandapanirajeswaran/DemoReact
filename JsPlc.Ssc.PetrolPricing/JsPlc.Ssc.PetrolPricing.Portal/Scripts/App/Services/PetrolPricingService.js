define(["jquery", "common"],
    function ($, common) {
    "use strict";

    var serviceBaseUrl = "";

    var callService = function (verb, url, jsonArgs) {
        var $promise = $.ajax({
            data: jsonArgs,
            url: serviceBaseUrl + url,
            type: verb,
            dataType: "json"
        });

        return $promise;
    };

    var setServiceBaseUrl = function (pathString) {
        serviceBaseUrl = pathString;
    };

    var getServiceBaseUrl = function () {
        return serviceBaseUrl;
    };

    var getAllSites = function (siteId) {
        var jsonArgs = { siteId: siteId };
        //return callService("get", "/api/Employees", jsonArgs);
        return callService("get", "/api/" + siteId, {}); // Not ideal REST resource naming
    };

    function getSiteNote(siteId, success, failure) {
        var url = "Sites/GetSiteNote?siteId=" + siteId;
        var promise = common.callService("get", url, null);
        promise.done(function (response, textStatus, jqXhr) {
            var siteNote = response.JsonObject;
            if (response.JsonStatusCode.CustomStatusCode == "ApiSuccess")
                success(siteNote);
            else
                failure();
        })
        .fail(function () {
            failure();
        });
    };

    function saveSiteNote(model, success, failure) {
        var promise = common.callService("post", "Sites/UpdateSiteNote", model);
        promise.done(function (response, textStatus, jqXhr) {
            var result = response;
            if (result && result.Success)
                success(result)
            else
                failure(result);
        })
        .fail(function () {
            failure();
        });
    };

    function deleteSiteNote(siteId, success, failure) {
        var url = "Sites/DeleteSiteNote?siteId=" + siteId;
        var promise = common.callService("post", url, null);
        promise.done(function (response, textStatus, jqXhr) {
            var result = response;
            if (result && result.Success)
                success(result);
            else
                failure(result);
        })
        .fail(function () {
            failure();
        });
    };

    function recalculateDailyPrices(success, failure) {
        var url = "Sites/RecalculateDailyPrices";
        var promise = common.callService("get", url, null);
        promise.done(function (response, textStatus, jqXhr) {
            success(response);
        });
        promise.fail(failure);
    };

    function saveExcludedBrands(success, failure, excludedBrands) {
        var url = "Sites/SaveExcludeBrands?excludbrands=" + excludedBrands;
        var promise = common.callService("get", url, null);
        promise.done(success);
        promise.fail(failure);
    };

    return {
        setServiceBaseUrl: setServiceBaseUrl,
        getServiceBaseUrl: getServiceBaseUrl,
        getAllSites: getAllSites,
        getSiteNote: getSiteNote,
        saveSiteNote: saveSiteNote,
        deleteSiteNote: deleteSiteNote,
        recalculateDailyPrices: recalculateDailyPrices,
        saveExcludedBrands: saveExcludedBrands
    };
});

