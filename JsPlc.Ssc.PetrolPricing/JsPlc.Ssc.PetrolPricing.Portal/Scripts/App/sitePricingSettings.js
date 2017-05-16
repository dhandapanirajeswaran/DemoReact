define(["common", "jquery", "notify"],
    function (common, $, notify) {

        var isLoaded = false,
            error = "",
            settings = {
                MinUnleadedPrice: undefined,
                MaxUnleadedPrice: undefined,
                MinDieselPrice: undefined,
                MaxDieselPrice: undefined,
                MinSuperUnleadedPrice: undefined,
                MaxSuperUnleadedPrice: undefined,
                MinUnleadedPriceChange: undefined,
                MaxUnleadedPriceChange: undefined,
                MinDieselPriceChange: undefined,
                MaxDieselPriceChange: undefined,
                MinSuperUnleadedPriceChange: undefined,
                MaxSuperUnleadedPriceChange: undefined,
                MaxGrocerDriveTimeMinutes: undefined,
                PriceChangeVarianceThreshold: undefined,
                SuperUnleadedMarkupPrice: undefined
            };

        function getSettings() {
            return settings;
        };

        function load(opts) {
            var url = "Sites/SitePricingSettings",
                promise = common.callService("get", url, null);

            promise.done(function (serverData, textStatus, jqXhr) {
                isLoaded = true;
                settings = $.extend({}, serverData);

                if (opts && $.isFunction(opts.success))
                    opts.success(settings);
            })
            .fail(function () {
                isLoaded = false;
                if (opts && $.isFunction(opts.failure))
                    opts.failure();
                console.log('Unable to load Site Pricing Settings');
            });
        };

        // API
        return {
            getSettings: getSettings,
            load: load
        };
    }
);