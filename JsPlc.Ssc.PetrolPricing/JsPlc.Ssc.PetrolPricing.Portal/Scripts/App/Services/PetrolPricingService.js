define(["jquery"], function ($) {
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
        debugger;
        var jsonArgs = { siteId: siteId };
        //return callService("get", "/api/Employees", jsonArgs);
        return callService("get", "/api/" + siteId, {}); // Not ideal REST resource naming
    };

    return {
        setServiceBaseUrl: setServiceBaseUrl,
        getServiceBaseUrl: getServiceBaseUrl,
        getAllSites: getAllSites
    };

});

