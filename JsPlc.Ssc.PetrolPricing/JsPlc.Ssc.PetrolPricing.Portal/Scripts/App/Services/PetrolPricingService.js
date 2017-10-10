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

    function triggerDailyPriceRecalculation(success, failure) {
        var url = "Sites/TriggerDailyPriceRecalculation";
        var promise = common.callService("get", url, null);
        promise.done(success);
        promise.fail(failure);
    };

    function removeAllSiteEmailAddresses(success, failure) {
        var url = "Sites/RemoveAllSiteEmailAddresses";
        var promise = common.callService("get", url, null);
        promise.done(success);
        promise.fail(failure);
    };
    function exportSiteEmailAddresses(success, failure, downloadId) {
        var url = "Sites/ExportSiteEmails?downloadId=" + downloadId;
        var promise = common.callService("get", url, null);
        promise.done(success);
        promise.fail(failure);
    };
    function getHistoricSitePrices(success, failure, data) {
        var url = "Sites/HistoricPricesForSite?siteId=" + data.siteId + '&startDate=' + data.startDate + '&endDate=' + data.endDate;
        var promise = common.callService("get", url, null);
        promise.done(success);
        promise.fail(failure);
    };

    function getSiteEmailTodaySendStatuses(success, failure, forDate) {
        var url = "Sites/GetSiteEmailTodaySendStatuses?forDate=" + forDate;
        common.standardGetPromise(url, success, failure);
    };

    function getJsPriceOverrides(success, failure, fileUploadId) {
        var url = "Sites/GetJsPriceOverrides?FileUploadId=" + fileUploadId;
        common.standardGetPromise(url, success, failure);
    };

    function getEmailSendLog(success, failure, emailSendLogId) {
        var url = "Sites/GetEmailSendLogView?emailSendLogId=" + emailSendLogId;
        common.standardGetPromise(url, success, failure);
    };

    return {
        setServiceBaseUrl: setServiceBaseUrl,
        getServiceBaseUrl: getServiceBaseUrl,
        getAllSites: getAllSites,
        getSiteNote: getSiteNote,
        saveSiteNote: saveSiteNote,
        deleteSiteNote: deleteSiteNote,
        recalculateDailyPrices: recalculateDailyPrices,
        saveExcludedBrands: saveExcludedBrands,
        triggerDailyPriceRecalculation: triggerDailyPriceRecalculation,
        removeAllSiteEmailAddresses: removeAllSiteEmailAddresses,
        exportSiteEmailAddresses: exportSiteEmailAddresses,
        getHistoricSitePrices: getHistoricSitePrices,
        getSiteEmailTodaySendStatuses: getSiteEmailTodaySendStatuses,
        getJsPriceOverrides: getJsPriceOverrides,
        getEmailSendLog: getEmailSendLog
    };
});

