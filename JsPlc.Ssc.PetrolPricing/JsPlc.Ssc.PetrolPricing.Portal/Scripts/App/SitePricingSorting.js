define([],
    function () {
        "use strict";

        var pricingSorting = {
            index: 2,
            desc: false,
            paused: false
        };

        var sortableColumns = [
            {
                name: 'Store No',
                pause: false,
                sorter: function (a, b) {
                    return sortCompareNumbers(a.StoreNo, b.StoreNo);
                }
            },
            {
                name: 'Email',
                pause: false,
                sorter: function (a, b) {
                    return sortCompareNumbers(a.HasEmails, b.HasEmails);
                }
            },
            {
                name: 'Store Name',
                pause: false,
                sorter: function (a, b) {
                    return sortCompareStrings(a.StoreName, b.StoreName);
                }
            },
            {
                name: 'Unleaded Competitor Price Data %',
                pause: false,
                sorter: function (a, b) {
                    var value1 = Number(a.SiteCompetitorsInfo.PriceSummaries[0].CompetitorPricePercent),
                        value2 = Number(b.SiteCompetitorsInfo.PriceSummaries[0].CompetitorPricePercent);
                    return sortCompareNumbers(value1, value2);
                }
            },
            {
                name: 'Unleaded Today Price',
                pause: false,
                sorter: function (a, b) {
                    var todayPrice1 = a.FuelPrices[1].TodayPrice,
                        todayPrice2 = b.FuelPrices[1].TodayPrice;
                    return sortCompareTodayPrices(todayPrice1, todayPrice2);
                }
            },
            {
                name: 'Unleaded Tomorrow Price',
                pause: true,
                sorter: function (a, b) {
                    var fuelPrice1 = a.FuelPricesToDisplay[0].FuelPrice,
                        fuelPrice2 = b.FuelPricesToDisplay[0].FuelPrice;
                    return sortCompareNumbers(fuelPrice1, fuelPrice2);
                }
            },
            {
                name: 'Unleaded Tomorrow Price Change',
                pause: true,
                sorter: function (a, b) {
                    var upDownValueA = Number(a.FuelPricesToDisplay[0].UpDownValue()),
                        upDownValueB = Number(b.FuelPricesToDisplay[0].UpDownValue());
                    return sortCompareTomorrowPriceChanges(upDownValueA, upDownValueB);
                }
            },
            {
                name: 'Diesel Competitor Price Data %',
                pause: false,
                sorter: function (a, b) {
                    var value1 = a.SiteCompetitorsInfo.PriceSummaries[1].CompetitorPricePercent,
                        value2 = b.SiteCompetitorsInfo.PriceSummaries[1].CompetitorPricePercent;
                    return sortCompareNumbers(value1, value2);
                }
            },
            {
                name: 'Diesel Today Price',
                pause: false,
                sorter: function (a, b) {
                    var todayPrice1 = a.FuelPrices[2].TodayPrice,
                        todayPrice2 = b.FuelPrices[2].TodayPrice;
                    return sortCompareTodayPrices(todayPrice1, todayPrice2);
                }
            },
            {
                name: 'Diesel Tomorrow Price',
                pause: true,
                sorter: function (a, b) {
                    var fuelPrice1 = a.FuelPricesToDisplay[1].FuelPrice,
                        fuelPrice2 = b.FuelPricesToDisplay[1].FuelPrice;
                    return sortCompareTomorrowPrices(fuelPrice1, fuelPrice2);
                }
            },
            {
                name: 'Diesel Tomorrow Price Change',
                pause: true,
                sorter: function (a, b) {
                    var upDownValueA = Number(a.FuelPricesToDisplay[1].UpDownValue()),
                        upDownValueB = Number(b.FuelPricesToDisplay[1].UpDownValue());
                    return sortCompareTomorrowPriceChanges(upDownValueA, upDownValueB);
                }
            },
             {
                 name: 'Super-Unleaded Competitor Price Data %',
                 pause: false,
                 sorter: function (a, b) {
                     var value1 = a.SiteCompetitorsInfo.PriceSummaries[2].CompetitorPricePercent,
                         value2 = b.SiteCompetitorsInfo.PriceSummaries[2].CompetitorPricePercent;
                     return sortCompareNumbers(value1, value2);
                 }
             },
            {
                name: 'Super-Unleaded Today Price',
                pause: false,
                sorter: function (a, b) {
                    var todayPrice1 = a.FuelPrices[2].TodayPrice,
                        todayPrice2 = b.FuelPrices[2].TodayPrice;
                    return sortCompareTodayPrices(todayPrice1, todayPrice2);
                }
            },
            {
                name: 'Super-Unleaded Tomorrow Price',
                pause: true,
                sorter: function (a, b) {
                    var fuelPrice1 = a.FuelPricesToDisplay[2].FuelPrice,
                        fuelPrice2 = b.FuelPricesToDisplay[2].FuelPrice;
                    return sortCompareTomorrowPrices(fuelPrice1, fuelPrice2);
                }
            },
            {
                name: 'Super-Unleaded Tomorrow Price Change',
                pause: true,
                sorter: function (a, b) {
                    var upDownValueA = Number(a.FuelPricesToDisplay[2].UpDownValue()),
                        upDownValueB = Number(b.FuelPricesToDisplay[2].UpDownValue());
                    return sortCompareTomorrowPriceChanges(upDownValueA, upDownValueB);
                }
            },
        ];

        function sortCompareNumbers(number1, number2) {
            var value1 = toNumberOrZero(number1),
                value2 = toNumberOrZero(number2),
                sign = (value1 > value2) - (value1 < value2);
            return pricingSorting.desc ? 0 - sign : sign;
        };

        function sortCompareStrings(string1, string2) {
            var value1 = string1.toUpperCase(),
                value2 = string2.toUpperCase(),
                sign = (value1 > value2) - (value1 < value2);
            return pricingSorting.desc ? 0 - sign : sign;
        };

        function sortCompareTodayPrices(todayPrice1, todayPrice2) {
            return sortCompareNumbers(todayPrice1, todayPrice2);
        };

        function sortCompareTomorrowPrices(fuelPrice1, fuelPrice2) {
            var overrideA = Number(fuelPrice1.OverridePrice()),
                autoA = Number(fuelPrice1.AutoPrice),
                tomorrowA = overrideA > 0 ? overrideA : autoA,
                overrideB = Number(fuelPrice2.OverridePrice()),
                autoB = Number(fuelPrice2.AutoPrice),
                tomorrowB = overrideB > 0 ? overrideB : autoB;
            return sortCompareNumbers(tomorrowA, tomorrowB);
        };

        function sortCompareTomorrowPriceChanges(upDownValue1, upDownValue2) {
            var origin = 100000,
                change1 = origin + toNumberOrZero(upDownValue1),
                change2 = origin + toNumberOrZero(upDownValue2);
            return sortCompareNumbers(change1, change2);
        };

        function isNumber(value) {
            return value != '' && !isNaN(value);
        };

        function toNumberOrZero(value) {
            return isNumber(value) ? Number(value) : 0;
        };

        function setSortColumn(index) {
            var obj = sortableColumns[index];

            if (pricingSorting.paused && pricingSorting.index == index) {
                pricingSorting.paused = false;
            } else {
                pricingSorting.paused = false;

                // change of index ?
                if (pricingSorting.index != index) {
                    pricingSorting.index = index;
                    pricingSorting.desc = false;
                } else
                    pricingSorting.desc = !pricingSorting.desc; // toggle asc <-> desc
            }

            redrawAscDesc();
        };

        function redrawAscDesc() {
            var cssClass = pricingSorting.desc ? 'sort-desc' : 'sort-asc';
            setSortCellClass(pricingSorting.index, cssClass);
        };

        function setSortCellClass(index, cssClass) {
            var allSortCells = $('.pricing-sort-row').find('.sortable'),
                sortCell = allSortCells.eq(index);

            allSortCells.each(function () {
                $(this).removeClass('sort-desc sort-asc sort-paused');
            });
            sortCell.addClass(cssClass);
        };

        function pause() {
            pricingSorting.paused = true;
            setSortCellClass(pricingSorting.index, 'sort-paused');
        };

        function pauseForColumns(indexes) {
            for (var i = 0; i < indexes.length; i++) {
                if (indexes[i] == pricingSorting.index) {
                    pause();
                    return true;
                }
            }
            return false;
        };

        function getSortIndex() {
            return pricingSorting.index;
        };

        function getSortMessage() {
            var name = sortableColumns[pricingSorting.index].name;
            return 'Sorting by ' + name + (pricingSorting.desc ? ' Descending' : '');
        };

        function sort(site1, site2) {
            if (pricingSorting.paused)
                return 0;

            var index = pricingSorting.index,
                sign = sortableColumns[index].sorter(site1, site2);
            return sign;
        };

        function resume() {
            pricingSorting.paused = false;
            redrawAscDesc();
        };

        // API
        return {
            sort: sort,
            setSortColumn: setSortColumn,
            getSortMessage: getSortMessage,
            getSortIndex: getSortIndex,
            pauseForColumns: pauseForColumns,
            resume: resume
        };
    }
);