define(["common", "jquery", "underscore"],
    function(common, $, _) {
        "use strict";

        var stats = zeroStats();

        function createStat() {
            return {
                count: 0,
                total: 0,
                min: undefined,
                max: undefined,
                average: undefined
            };
        };

        function zeroStats() {
            return {
                sites: {
                    count: 0,
                    active: 0,
                    withEmails: 0,
                    withNoEmails: 0
                },
                combined: {
                    tomorrow: {
                        price: createStat(),
                        priceChanges: createStat()
                    },
                    today: {
                        price: createStat()
                    }
                },
                unleaded: {
                    tomorrow: {
                        price: createStat(),
                        priceChanges: createStat()
                    },
                    today: {
                        price: createStat()
                    }
                },
                diesel: {
                    tomorrow: {
                        price: createStat(),
                        priceChanges: createStat()
                    },
                    today: {
                        price: createStat()
                    }
                },
                superUnleaded: {
                    tomorrow: {
                        price: createStat(),
                        priceChanges: createStat()
                    },
                    today: {
                        price: createStat()
                    }
                }
            };
        };

        function isNumber(value) {
            return value != '' && !isNaN(value);
        };

        function getNumberOrZero(value) {
            return value != '' && !isNaN(value) ? Number(value) : 0;
        };

        function simplifyFuelPrice(fuelPriceToDisplay) {
            var autoprice = getNumberOrZero(fuelPriceToDisplay.FuelPrice.AutoPrice),
                override = getNumberOrZero(fuelPriceToDisplay.FuelPrice.OverridePrice()),
                today = getNumberOrZero(fuelPriceToDisplay.FuelPrice.TodayPrice),
                tomorrow = override > 0 ? override : autoprice,
                change = today > 0 && tomorrow > 0 ? tomorrow - today : 0;

            return {
                autoprice: autoprice,
                override: override,
                today: today,
                tomorrow: tomorrow,
                change: change
            };
        };

        function updateStatValues(obj, value) {
            obj.count++;
            obj.total += value;
            obj.min = obj.min == undefined ? value : Math.min(obj.min, value);
            obj.max = obj.max == undefined ? value : Math.max(obj.max, value);
        };

        function combineStatFuelPrices(obj, fuel1, fuel2, fuel3) {
            var mins = filterNumbersOnly(fuel1.min, fuel2.min, fuel3.min),
                maxs = filterNumbersOnly(fuel1.max, fuel2.max, fuel3.max);

            obj.count = fuel1.count + fuel2.count + fuel3.count;
            obj.total = fuel1.total + fuel2.total + fuel3.total;
            obj.min = mins.length ? Math.min.apply(null, mins) : 0;
            obj.max = maxs.length ? Math.max.apply(null, maxs) : 0;
        };

        function filterNumbersOnly(value1, value2, value3) {
            var filtered = [];
            if (isNumber(value1))
                filtered.push(value1);
            if (isNumber(value2))
                filtered.push(value2);
            if (isNumber(value3))
                filtered.push(value3);
            return filtered;
        };

        function populateStatAverages(items) {
            var i,
                item;
            for (i = 0; i < items.length; i++) {
                item = items[i];
                item.average = (item.count == 0 || !isNumber(item.count) || !isNumber(item.total))
                    ? 0
                    : item.total / item.count;
            }
        };

        function formatStatsValues(stat) {
            var count = stat.count,
                min = count == 0 ? '-' : stat.min.toFixed(1),
                max = count == 0 ? '-' : stat.max.toFixed(1),
                average = count == 0 ? '-' : stat.average.toFixed(1);

            return {
                count: count,
                min: min,
                max: max,
                average: average
            };
        };

        //
        // loop through each site gathering count, total, min and max values for each fuel
        //
        function gatherStats(sites) {

            stats = zeroStats();

            _.each(sites, function (siteItem) {
                var unleaded = simplifyFuelPrice(siteItem.FuelPricesToDisplay[0]),
                    diesel = simplifyFuelPrice(siteItem.FuelPricesToDisplay[1]),
                    superUnleaded = simplifyFuelPrice(siteItem.FuelPricesToDisplay[2]),
                    minPriceChange = Math.min(unleaded.change, diesel.change, superUnleaded.change),
                    maxPriceChange = Math.max(unleaded.change, diesel.change, superUnleaded.change);

                stats.sites.active++;

                if (siteItem.HasEmails)
                    stats.sites.withEmails++;
                else
                    stats.sites.withNoEmails++;

                // tomorrow (aka suggested) price changes
                if (unleaded.change != 0)
                    updateStatValues(stats.unleaded.tomorrow.priceChanges, unleaded.change);

                if (diesel.change != 0)
                    updateStatValues(stats.diesel.tomorrow.priceChanges, diesel.change);

                if (superUnleaded.change != 0)
                    updateStatValues(stats.superUnleaded.tomorrow.priceChanges, superUnleaded.change);

                // tomorrow (aka suggested) prices
                if (unleaded.tomorrow != 0)
                    updateStatValues(stats.unleaded.tomorrow.price, unleaded.tomorrow);

                if (diesel.tomorrow != 0)
                    updateStatValues(stats.diesel.tomorrow.price, diesel.tomorrow);

                if (superUnleaded.tomorrow != 0)
                    updateStatValues(stats.superUnleaded.tomorrow.price, superUnleaded.tomorrow);

                // today (aka current) prices
                if (unleaded.today != 0)
                    updateStatValues(stats.unleaded.today.price, unleaded.today);

                if (diesel.today != 0)
                    updateStatValues(stats.diesel.today.price, diesel.today);

                if (superUnleaded.today != 0)
                    updateStatValues(stats.superUnleaded.today.price, superUnleaded.today);
            });
        };

        //
        // calculate the average/aggregation stats
        //
        function calculate() {
            // averages for each Fuel price and price change
            populateStatAverages([
                stats.unleaded.tomorrow.price,
                stats.unleaded.tomorrow.priceChanges,

                stats.diesel.tomorrow.price,
                stats.diesel.tomorrow.priceChanges,

                stats.superUnleaded.tomorrow.price,
                stats.superUnleaded.tomorrow.priceChanges,

                stats.unleaded.today.price,
                stats.diesel.today.price,
                stats.superUnleaded.today.price
            ]);

            // build combined stats from ALL fuels
            combineStatFuelPrices(stats.combined.tomorrow.price,
                stats.unleaded.tomorrow.price,
                stats.diesel.tomorrow.price,
                stats.superUnleaded.tomorrow.price
                );

            combineStatFuelPrices(stats.combined.tomorrow.priceChanges,
                stats.unleaded.tomorrow.priceChanges,
                stats.diesel.tomorrow.priceChanges,
                stats.superUnleaded.tomorrow.priceChanges
                );

            combineStatFuelPrices(stats.combined.today.price,
                stats.unleaded.today.price,
                stats.diesel.today.price,
                stats.superUnleaded.today.price
                );

            // average for combined stats
            populateStatAverages([
                stats.combined.tomorrow.price,
                stats.combined.tomorrow.priceChanges,
                stats.combined.today.price
            ]);
        };

        function gatherSiteFuelStats(sites) {
            gatherStats(sites);
            calculate(sites);
        };

        function format() {
            var formatted = {
                sites: {
                    count: stats.sites.count,
                    active: stats.sites.active,
                    withEmails: stats.sites.withEmails,
                    withNoEmails: stats.sites.withNoEmails
                },
                unleaded: {
                    tomorrow: {
                        price: formatStatsValues(stats.unleaded.tomorrow.price),
                        priceChanges: formatStatsValues(stats.unleaded.tomorrow.priceChanges)
                    },
                    today: {
                        price: formatStatsValues(stats.unleaded.today.price)
                    }
                },
                diesel: {
                    tomorrow: {
                        price: formatStatsValues(stats.diesel.tomorrow.price),
                        priceChanges: formatStatsValues(stats.diesel.tomorrow.priceChanges)
                    },
                    today: {
                        price: formatStatsValues(stats.diesel.today.price)
                    }
                },
                superUnleaded: {
                    tomorrow: {
                        price: formatStatsValues(stats.superUnleaded.tomorrow.price),
                        priceChanges: formatStatsValues(stats.superUnleaded.tomorrow.priceChanges)
                    },
                    today: {
                        price: formatStatsValues(stats.superUnleaded.today.price)
                    }
                },
                combined: {
                    tomorrow: {
                        price: formatStatsValues(stats.combined.tomorrow.price),
                        priceChanges: formatStatsValues(stats.combined.tomorrow.priceChanges)
                    },
                    today: {
                        price: formatStatsValues(stats.combined.today.price)
                    }
                }
            };

            return formatted;
        };

        // API
        return {
            gatherSiteFuelStats: gatherSiteFuelStats,
            format: format
        };
    }
);