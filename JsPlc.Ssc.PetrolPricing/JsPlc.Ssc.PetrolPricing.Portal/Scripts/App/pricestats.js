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

        function createDataset() {

            var data = [],
                keys = {};

            function countPriceChange(delta, count) {
                var item,
                    index;
                if (delta in keys) {
                    item = keys[delta];
                } else {
                    index = data.length;
                    item = {
                        delta: delta,
                        count: 0
                    };
                    data[index] = item;
                    keys[delta] = item;
                }
                item.count += count;
            };

            function sort() {
                var i;
                data.sort(function (a, b) {
                    return b.count > a.count - b.count < a.count;
                });
                keys = {};
                // rebuild keys from sorted data
                for (i = 0; i < data.length; i++) {
                    keys[data[i].delta] = data[i];
                }
            };
            // dataset
            return {
                data: data,
                keys: keys,
                countPriceChange: countPriceChange,
                sort: sort
            }
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
                        priceChanges: createStat(),
                        priceDataset: createDataset(),
                        priceChangeDataset: createDataset()
                    },
                    today: {
                        price: createStat(),
                        priceDataset: createDataset()
                    }
                },
                unleaded: {
                    tomorrow: {
                        price: createStat(),
                        priceChanges: createStat(),
                        priceDataset: createDataset(),
                        priceChangeDataset: createDataset()
                    },
                    today: {
                        price: createStat(),
                        priceDataset: createDataset()
                    }
                },
                diesel: {
                    tomorrow: {
                        price: createStat(),
                        priceChanges: createStat(),
                        priceDataset: createDataset(),
                        priceChangeDataset: createDataset()
                    },
                    today: {
                        price: createStat(),
                        priceDataset: createDataset()
                    }
                },
                superUnleaded: {
                    tomorrow: {
                        price: createStat(),
                        priceChanges: createStat(),
                        priceDataset: createDataset(),
                        priceChangeDataset: createDataset()
                    },
                    today: {
                        price: createStat(),
                        priceDataset: createDataset()
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

        function updateStatValues(stat, delta, dataset) {
            stat.count++;
            stat.total += delta;
            stat.min = stat.min == undefined ? delta : Math.min(stat.min, delta);
            stat.max = stat.max == undefined ? delta : Math.max(stat.max, delta);

            dataset.countPriceChange(delta.toFixed(2), 1);
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

            // reset all stats
            stats = zeroStats();

            // loop
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
                    updateStatValues(stats.unleaded.tomorrow.priceChanges, unleaded.change, stats.unleaded.tomorrow.priceChangeDataset);

                if (diesel.change != 0)
                    updateStatValues(stats.diesel.tomorrow.priceChanges, diesel.change, stats.diesel.tomorrow.priceChangeDataset);

                if (superUnleaded.change != 0)
                    updateStatValues(stats.superUnleaded.tomorrow.priceChanges, superUnleaded.change, stats.superUnleaded.tomorrow.priceChangeDataset);

                // tomorrow (aka suggested) prices
                if (unleaded.tomorrow != 0)
                    updateStatValues(stats.unleaded.tomorrow.price, unleaded.tomorrow, stats.unleaded.tomorrow.priceDataset);

                if (diesel.tomorrow != 0)
                    updateStatValues(stats.diesel.tomorrow.price, diesel.tomorrow, stats.diesel.tomorrow.priceDataset);

                if (superUnleaded.tomorrow != 0)
                    updateStatValues(stats.superUnleaded.tomorrow.price, superUnleaded.tomorrow, stats.superUnleaded.tomorrow.priceDataset);

                // today (aka current) prices
                if (unleaded.today != 0)
                    updateStatValues(stats.unleaded.today.price, unleaded.today, stats.unleaded.today.priceDataset);

                if (diesel.today != 0)
                    updateStatValues(stats.diesel.today.price, diesel.today, stats.diesel.today.priceDataset);

                if (superUnleaded.today != 0)
                    updateStatValues(stats.superUnleaded.today.price, superUnleaded.today, stats.superUnleaded.today.priceDataset);
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

            // combine the actual dataset values
            stats.combined.tomorrow.priceDataset = combineDatasets(
                [
                    stats.unleaded.tomorrow.priceDataset,
                    stats.diesel.tomorrow.priceDataset,
                    stats.superUnleaded.tomorrow.priceDataset
                ]
            );
            stats.combined.tomorrow.priceChangeDataset = combineDatasets(
                [
                    stats.unleaded.tomorrow.priceChangeDataset,
                    stats.diesel.tomorrow.priceChangeDataset,
                    stats.superUnleaded.tomorrow.priceChangeDataset
                ]
            );
            stats.combined.today.priceDataset = combineDatasets(
                [
                    stats.unleaded.today.priceDataset,
                    stats.diesel.today.priceDataset,
                    stats.superUnleaded.today.priceDataset
                ]
            );

            // sort the all datasets
            stats.combined.today.priceDataset.sort();
            stats.combined.tomorrow.priceDataset.sort();
            stats.combined.tomorrow.priceChangeDataset.sort();
            stats.unleaded.today.priceDataset.sort();
            stats.unleaded.tomorrow.priceDataset.sort();
            stats.unleaded.tomorrow.priceChangeDataset.sort();
            stats.diesel.today.priceDataset.sort();
            stats.diesel.tomorrow.priceDataset.sort();
            stats.diesel.tomorrow.priceChangeDataset.sort();
            stats.superUnleaded.today.priceDataset.sort();
            stats.superUnleaded.tomorrow.priceDataset.sort();
            stats.superUnleaded.tomorrow.priceChangeDataset.sort();

            debugdataset(stats.unleaded.tomorrow.priceChangeDataset);
        };

        function debugdataset(dataset) {
            //console.log('-------------- dataset -----------------');
            //var i,
            //    item;
            //for (i = 0; i < dataset.data.length; i++) {
            //    item = dataset.data[i];
            //    console.log(item.delta, item.count);
            //}
        };

        function combineDatasets(datasets) {
            var combined = createDataset(),
                dataset,
                i,
                item,
                key;
            for (i = 0; i < datasets.length; i++) {
                dataset = datasets[i];
                for (key in dataset.keys) {
                    item = dataset.keys[key];
                    combined.countPriceChange(key, item.count);
                }
            }
            return combined;
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

        function getData() {
            return stats;
        };

        // API
        return {
            gatherSiteFuelStats: gatherSiteFuelStats,
            format: format,
            getData: getData
        };
    }
);