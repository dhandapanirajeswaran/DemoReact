define(["jquery", "common", "ServiceUtils"],
    function ($, common, serviceUtils) {
        "use strict";

        function getPriceFreezeEvent(success, failure, id) {
            var url = "Settings/GetPriceFreezeEvent/?eventId=" + id;
            return serviceUtils.standardGetRequest(success, failure, url);
        };

        function updatePriceFreezeEvent(success, failure, event) {
            var data = event;
            var url = "Settings/UpdatePriceFreezeEvent";
            return serviceUtils.standardPostRequest(success, failure, url, data);
        };

        function deletePriceFreezeEvent(success, failure, id) {
            var url = "Settings/DeletePriceFreezeEvent/?eventId=" + id;
            return serviceUtils.standardGetRequest(success, failure, url);
        };

        function getPriceFreezeEventForDate(success, failure, forDate) {
            var url = "Sites/GetPriceFreezeEventForDate/?forDate=" + serviceUtils.dateToUrlYYYYMMMDD(forDate);
            return serviceUtils.standardGetRequest(success, failure, url);
        };

        // API
        return {
            getPriceFreezeEvent: getPriceFreezeEvent,
            updatePriceFreezeEvent: updatePriceFreezeEvent,
            deletePriceFreezeEvent: deletePriceFreezeEvent,
            getPriceFreezeEventForDate: getPriceFreezeEventForDate
        };
    }
);