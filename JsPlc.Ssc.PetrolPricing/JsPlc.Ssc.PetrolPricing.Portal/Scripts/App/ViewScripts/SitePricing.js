define(["jquery", "knockout", "moment", "bootstrap-datepicker", "bootstrap-datepickerGB", "underscore", "common", "helpers", "URI", "competitorPricePopup", "notify", "busyloader", "cookieSettings", "bootbox", "sitePricingSettings", "EmailTemplateService", "SiteEmailPopup", "DriveTimeMarkupService", "validation"],
    function ($, ko, moment, datepicker, datePickerGb, _, common, helpers, URI, competitorPopup, notify, busyloader, cookieSettings, bootbox, sitePricingSettings, emailTemplateService, siteEmailPopup, driveTimeMarkupService, validation) {
        // constants
        var FUEL_SUPER_UNLEADED = 1,
            FUEL_UNLEADED = 2,
            FUEL_DIESEL = 6;

        var getMessages = function (task) {
            switch (task) {
                case "Save":
                    return {
                        success: "Site prices updated",
                        failure: "Site price update failed"
                    };
                case "Email":
                    return {
                        success: "Email sent successfully",
                        failure: "Email delivery failed"
                    };
                default:
                    return "Undefined page mode";
            };
        };

        var dmyFormatString = "DD/MM/YYYY";
        var dmonyFormatString = "DD MMM YYYY";
        var ymdFormatString = "YYYY-MM-DD";

        var fuelTypeNames = {
            "1": "Super Unleaded",
            "2": "Unleaded",
            "6": "Diesel"
        };

        var pricingSettings = {
            maxGrocerDriveTimeMinutes: 5.0,
            priceChangeVarianceThreshold: 0.2,
            superUnleadedMarkupPrice: 5.0
        };

        var fuelOverrideLimits = {
            "1": {
                name: "Super Unleaded",
                change: { min: -3.0, max: +3.0 },
                absolute: { min: 50.0, max: 400.0 }
            },
            "2": {
                name: "Unleaded",
                change: { min: -3.0, max: +3.0 },
                absolute: { min: 50.0, max: 400.0 }
            },
            "6": {
                name: "Diesel",
                change: { min: -3.0, max: +3.0 },
                absolute: { min: 50.0, max: 400.0 }
            }
        };

        var isApplyingMassFuelPriceOverride = false;

        var escapeRegEx = function (value) {
            return value.replace(/[\-\[\]{}()*+?.,\\\^$|#\s]/g, "\\$&");
        };

        var replaceTokens = function (format, tokens) {
            var result = format,
                key,
                regex;
            for (key in tokens) {
                if (tokens.hasOwnProperty(key)) {
                    regex = new RegExp(escapeRegEx(key), 'g');
                    result = result.replace(regex, tokens[key]);
                }
            }
            return result;
        };

        var isNumber = function (value) {
            return value != '' && !isNaN(value);
        };

        var formatNumber1DecimalWithPlusMinus = function (value) {
            return value == 0
                ? '0.0'
                : value < 0
                ? '-' + Math.abs(value).toFixed(1)
                : '+' + Math.abs(value).toFixed(1);
        };

        var formatNumberTo1DecimalPlace = function (value) {
            if (isNumber(value))
                return Number(value).toFixed(1);
            else
                return value;
        };

        function formatNonZeroStat(stat, zeroString) {
            if (!isNumber || stats == 0)
                return zeroString;
            return stat;
        };

        var formatPriceSource = function (source) {
            return source == "null"
                ? 'n/a'
                : source;
        };
        var formatPriceSourceDateTime = function (datetime) {
            return datetime == "null"
                ? "n/a"
                : ('' + datetime).replace(' ', ' at ');
        };

        var replaceLastDigitWith9 = function (value) {
            if (value == '' || isNaN(value))
                return value;
            return Math.floor(value) + '.9';
        };

        var messageFormats = {
            priceIsNotAvailableForTrialPrice: '[i class="fa fa-flask"][/i] [b]Trial Price[/b] not available',
            priceIsNotAvailableForMatchCompetitor: '[i class="fa fa-clone"][/i] [b]Match Competitor[/b] Price not available [br /] for Competitor [b]{NAME}[/b]',
            priceIsNotAvailableForSoloPrice: '[i class="fa fa-dot-circle-o"][/i] [b]Solo Price[/b] not available',
            trialPriceInfoTip: '[q][i class="fa fa-flask"][/i] Trial Price:[/q] [b]{NAME}[/b] - [q]Markup:[/q] [b]{MARKUP}[/b] [br /]Distance: [b]{DISTANCE}[/b] miles = [b]{DRIVETIME}[/b] minutes [br /] Price Source: [b]{PRICESOURCE}[/b] on [b]{PRICESOURCEDATETIME}[/b]',
            matchCompetitorInfoTip: '[q][i class="fa fa-clone"][/i] Match Competitor[/q] : [b]{NAME}[/b] -[q] MarkUp:[/q] [b]{MARKUP}[/b] [br /]Distance: [b]{DISTANCE}[/b] miles = [b]{DRIVETIME}[/b] minutes [br /] Price Source: [b]{PRICESOURCE}[/b] on [b]{PRICESOURCEDATETIME}[/b]',
            soloPriceInfoTip: '[q][i class="fa fa-dot-circle-o"][/i] Solo Price[/q]',
            soloPriceBasedOnCompetitorInfoTip: '[q][i class="fa fa-dot-circle-o"][/i] Solo Price[/q] based on Competitor: [b]{COMPETITOR}[/b][br /] Distance: [b]{DISTANCE}[/b] miles = [b]{DRIVETIME}[/b] minutes[br /] Price Source: [b]{PRICESOURCE}[/b] on [b]{PRICESOURCEDATETIME}[/b]',
            currentPriceInfoTip: 'Current Price for [b]{NAME}[/b]',
            priceOverrideInfoTip: 'Override Price for [b]{NAME}[/b]',
            priceOverrideIncreaseExceedsInfoTip: 'Override Price for [b]{NAME}[/b][br /] [b][i class="fa fa-warning"][/i] Warning:[/b] Increase exceeds [b]{INCMIN}[/b] to [b]{INCMAX}[/b]',
            priceOverrideInvalidValueInfoTip: 'Override Price for [b]{NAME}[/b][br /][em][i class="fa fa-bug"][/i]Invalid Price[/em] (range [b]{ABSMIN}[/b] to [b]{ABSMAX}[/b])',
            withinPriceChangeVarianceInfoTip: 'Override Price [em]Ignored[/em] [br /]( within [b]Price Change Variance[/b] of [b]+/- {VARIANCE}[/b] )',
            yestPriceInfoTip: 'Yesterday Price of [b]{VALUE}[/b] for [b]{NAME}[/b]',
            compTodayPriceInfotip: 'Today Price of [b]{VALUE}[/b] for [b]{NAME}[/b]',
            updownInfoTips: {
                '-1': '[u][i class="fa fa-arrow-down"][/i] Price Decrease[/u] of [b]-{VALUE}[/b] for [b]{NAME}[/b]',
                '0': '[u][i class="fa fa-minus"][/i] No Price Change[/u] for [b]{NAME}[/b]',
                '1': '[u][i class="fa fa-arrow-up"][/i] Price Increase[/u] of [b]+{VALUE}[/b] for [b]{NAME}[/b]'
            },
            barChartCellsNAInfoTips: 'There are [b]{COUNT}[/b] Sites with a [br /] [b]N/A[/b] Price Difference',
            barChartCellsInfoTips: {
                '-1': 'There are [b]{COUNT}[/b] Sites with a [br /][i class=&quot;fa fa-arrow-down&quot;][/i] Price Decrease of [b]{VALUE}[/b]',
                '0': 'There are [b]{COUNT}[/b] Sites with [br /][i class=&quot;fa fa-minus&quot;][/i] No Price Change',
                '1': 'There are [b]{COUNT}[/b] Sites with a [br /][i class=&quot;fa fa-arrow-up&quot;][/i] Price Increase of [b]{VALUE}[/b]',
            },
            noNearbyGrocersInfotip: 'There are [b]No Grocers[/b] within [b]{DRIVETIME} Minutes[/b] Drive Time',
            nearbyGrocersWithoutPriceInfoTip: '[b]Grocers[/b] found but [b]No Price[/b] Available within {DRIVETIME} Minutes for Date',
            nearbyGrocersWithPriceInfoTip: '[b]Grocers[/b] and Price are [b]Available[/b] within {DRIVETIME} Minutes for Date'
        };

        var markupFormats = {
            noNearbyGrocers: '<span class="no-nearby-grocer-price" data-infotip="There are [i]No Grocers[/i] within [b]{DRIVETIME} Minutes[/b] Drive Time"><i class="fa fa-times"></i></span>',
            nearbyGrocersWithoutPrices: '<span class="has-nearby-grocer-with-out-price" data-infotip="[u]Grocers[/u] found but [em]No Price[/em] Available within [b]{DRIVETIME} Minutes[/b] for Date"><i class="fa fa-question"></i></span>',
            nearbyGrocersAndPrices: '<span class="has-nearby-grocer-price" data-infotip="[b]Grocers Prices[/b] are [u]Available[/u] within [b]{DRIVETIME} Minutes[/b] for Date"><i class="fa fa-gbp"></i></span>'
        };

        var delayedBlurs = (function () {
            var blurSiteId = 0,
                blurFunc = function () { },
                ticker = null;

            function clearBlur() {
                if (blurSiteId != 0)
                    blurFunc();
                blurSiteId = 0;
                if (ticker)
                    clearTimeout(ticker);
            };

            function focusSite(siteId) {
                if (siteId != blurSiteId) // change of Site ?
                    clearBlur();
                blurSiteId = 0;
            };

            function blurSite(siteId, fn) {
                var ctx = this,
                    action = function () {
                        clearBlur.call(ctx);
                    };
                clearBlur();
                blurSiteId = siteId;
                blurFunc = fn;
                ticker = setTimeout(action, 300);
            };

            return {
                focusSite: focusSite,
                blurSite: blurSite
            };
        })();

        var driveTimeMarkups = {
            Unleaded: [],
            Diesel: [],
            SuperUnleaded: []
        };

        var ukDateSample = moment("16/12/2015", dmyFormatString).format(dmyFormatString);
        var usDateSample = moment("16/12/2015", dmyFormatString).format(ymdFormatString);
        var todayDateStringUkFormat = function () {
            var currentDate = new Date();
            var day = currentDate.getDate();
            var month = currentDate.getMonth() + 1;
            var year = currentDate.getFullYear();
            var dtString = (day + "/" + month + "/" + year);
            return moment(dtString, dmyFormatString).format(dmyFormatString);
        };
        var todaysDateUkformat = todayDateStringUkFormat();

        var dmyStringToYmdString = function (ukDateString) {
            if (ukDateString == "") ukDateString = todaysDateUkformat;

            var ymdDateString = moment(ukDateString, dmyFormatString).isValid() ?
                moment(ukDateString, dmyFormatString).format(ymdFormatString) : "";
            return ymdDateString; // returns blank if invalid ukdate
        }

        var dmyStringToDMonYString = function (ukDateString) {
            if (ukDateString == "") ukDateString = todaysDateUkformat;

            var dmonyDateString = moment(ukDateString, dmyFormatString).isValid() ?
                moment(ukDateString, dmyFormatString).format(dmonyFormatString) : "";
            return dmonyDateString; // returns blank if invalid ukdate
        }

        var fieldErrors = (function () {
            var errors = {}
            function hasAny() {
                for (var key in errors) {
                    return true;
                }
                return false;
            };
            function count() {
                var total = 0,
                    key;
                for (var key in errors)
                    total++;
                return total;
            };
            function removeAll() {
                errors = {};
            };
            function add(siteId, fuelTypeId) {
                errors[siteId + "_" + fuelTypeId] = true;
            };
            function remove(siteId, fuelTypeId) {
                delete errors[siteId + "_" + fuelTypeId];
            };
            function clearForFuel(fuelTypeId) {
                var deletions = [];
                for (var key in errors) {
                    if (key.indexOf("_" + fuelTypeId) >= 0)
                        deletions.push(key);
                };
                for (var key in deletions) {
                    delete errors[key];
                }
            };

            return {
                removeAll: removeAll,
                hasAny: hasAny,
                add: add,
                remove: remove,
                clearForFuel: clearForFuel,
                count: count
            };
        })();

        // VM for Page
        function PageViewModel() {
            var self = this;

            // Define fields that need to be bound to the page

            self.dataModel = ko.observable("");
            self.dataAvailable = ko.observable(true);
            self.dataLoading = ko.observable(true);
            self.hasSites = ko.observable(false);
            self.hasAllSitesHidden = ko.observable(false);

            self.InitDate = ko.observable('');
            self.StoreName = ko.observable('');
            self.StoreTown = ko.observable('');
            self.CatNo = ko.observable('');
            self.StoreNo = ko.observable('');
            self.PriceDifferences = ko.observableArray([]);

            self.HasUnsavedChanges = ko.observable(false); // default to false

            self.ViewingHistorical = ko.observable(false); // TODO useful to have for disabling few things

            self.unleadedOffset = ko.observable('');
            self.dieselOffset = ko.observable('');
            self.superOffset = ko.observable('');
            self.changeTypeValues = ["(+/-n)", "£"];
            self.changeTypeValuePlusMinus = self.changeTypeValues[0];
            self.changeTypeValueOverrideAll = self.changeTypeValues[1];
            self.changeType = ko.observable(self.changeTypeValuePlusMinus);

            self.HasValidationErrors = ko.observable(false);

            self.HasSuccessMessage = ko.observable(false);
            self.HasErrorMessage = ko.observable(false);

            self.emailPopupSiteItemModel = ko.observable(); // List<SitePriceViewModel> object to be bound to main list
            self.showEmailPopup = ko.observable(false);

            self.showEmailAllPopup = ko.observable(false);
            self.sendingEmails = ko.observable(false);

            self.busyLoadingData = ko.observable(true);
            self.firstPageLoad = ko.observable(true);
            self.hasBadPriceChangeValue = ko.observable(false);
            self.hasAnyFieldErrors = ko.observable(false);
            self.fieldErrorCount = ko.observable(0);

            self.showCompetitorPricesPopup = ko.observable(true);

            self.competitorPricesPopupSiteItemModel = ko.observable({});

            self.checkedEmailCount = ko.observable(0);

            self.showCompetitorNotePopup = ko.observable(false);

            // stats:start
            self.stats_Sites_Count = ko.observable(0);
            self.stats_Sites_Active = ko.observable(0);
            self.stats_Sites_WithEmails = ko.observable(0);
            self.stats_Sites_WithNoEmails = ko.observable(0);

            // unleaded tomorrow prices
            self.stats_Unleaded_Tomorrow_Price_Average = ko.observable(0);
            self.stats_Unleaded_Tomorrow_Price_Count = ko.observable(0);
            self.stats_Unleaded_Tomorrow_Price_Max = ko.observable(0);
            self.stats_Unleaded_Tomorrow_Price_Min = ko.observable(0);

            // unleaded tomorrow price changes
            self.stats_Unleaded_Tomorrow_PriceChanges_Average = ko.observable(0);
            self.stats_Unleaded_Tomorrow_PriceChanges_Count = ko.observable(0);
            self.stats_Unleaded_Tomorrow_PriceChanges_Max = ko.observable(0);
            self.stats_Unleaded_Tomorrow_PriceChanges_Min = ko.observable(0);

            // unleaded today prices
            self.stats_Unleaded_Today_Price_Average = ko.observable(0);
            self.stats_Unleaded_Today_Price_Count = ko.observable(0);
            self.stats_Unleaded_Today_Price_Min = ko.observable(0);
            self.stats_Unleaded_Today_Price_Max = ko.observable(0);

            // diesel tomorrow prices
            self.stats_Diesel_Tomorrow_Price_Average = ko.observable(0);
            self.stats_Diesel_Tomorrow_Price_Count = ko.observable(0);
            self.stats_Diesel_Tomorrow_Price_Max = ko.observable(0);
            self.stats_Diesel_Tomorrow_Price_Min = ko.observable(0);

            // diesel tomorrow price changes
            self.stats_Diesel_Tomorrow_PriceChanges_Average = ko.observable(0);
            self.stats_Diesel_Tomorrow_PriceChanges_Count = ko.observable(0);
            self.stats_Diesel_Tomorrow_PriceChanges_Max = ko.observable(0);
            self.stats_Diesel_Tomorrow_PriceChanges_Min = ko.observable(0);

            // diesel today prices
            self.stats_Diesel_Today_Price_Average = ko.observable(0);
            self.stats_Diesel_Today_Price_Count = ko.observable(0);
            self.stats_Diesel_Today_Price_Min = ko.observable(0);
            self.stats_Diesel_Today_Price_Max = ko.observable(0);

            // super-unleaded tomorrow prices
            self.stats_SuperUnleaded_Tomorrow_Price_Average = ko.observable(0);
            self.stats_SuperUnleaded_Tomorrow_Price_Count = ko.observable(0);
            self.stats_SuperUnleaded_Tomorrow_Price_Max = ko.observable(0);
            self.stats_SuperUnleaded_Tomorrow_Price_Min = ko.observable(0);

            // super-unleaded tomorrow price changes
            self.stats_SuperUnleaded_Tomorrow_PriceChanges_Average = ko.observable(0);
            self.stats_SuperUnleaded_Tomorrow_PriceChanges_Count = ko.observable(0);
            self.stats_SuperUnleaded_Tomorrow_PriceChanges_Max = ko.observable(0);
            self.stats_SuperUnleaded_Tomorrow_PriceChanges_Min = ko.observable(0);

            // super-unleaded today prices
            self.stats_SuperUnleaded_Today_Price_Average = ko.observable(0);
            self.stats_SuperUnleaded_Today_Price_Count = ko.observable(0);
            self.stats_SuperUnleaded_Today_Price_Min = ko.observable(0);
            self.stats_SuperUnleaded_Today_Price_Max = ko.observable(0);

            // combined tomorrow prices
            self.stats_Combined_Tomorrow_Prices_Average = ko.observable(0);
            self.stats_Combined_Tomorrow_Prices_Count = ko.observable(0);
            self.stats_Combined_Tomorrow_Prices_Max = ko.observable(0);
            self.stats_Combined_Tomorrow_Prices_Min = ko.observable(0);

            // combined tomorrow price changes
            self.stats_Combined_Tomorrow_PriceChanges_Average = ko.observable(0);
            self.stats_Combined_Tomorrow_PriceChanges_Count = ko.observable(0);
            self.stats_Combined_Tomorrow_PriceChanges_Max = ko.observable(0);
            self.stats_Combined_Tomorrow_PriceChanges_Min = ko.observable(0);

            // combined today prices
            self.stats_Combined_Today_Prices_Average = ko.observable(0);
            self.stats_Combined_Today_Prices_Count = ko.observable(0);
            self.stats_Combined_Today_Prices_Max = ko.observable(0);
            self.stats_Combined_Today_Prices_Min = ko.observable(0);
            // stats:end

            // tabs
            self.showingEmptyTab = ko.observable(true);
            self.showingPriceSummaryTab = ko.observable(false);
            self.showingCompetitorDifferencesTab = ko.observable(false);

            self.showEmptySummaryTab = function () {
                self.showingEmptyTab(true);
                self.showingPriceSummaryTab(false);
                self.showingCompetitorDifferencesTab(false);
                notify.info('Hiding summary tab');
            };

            self.showPriceSummaryTab = function () {
                self.showingEmptyTab(false);
                self.showingPriceSummaryTab(true);
                self.showingCompetitorDifferencesTab(false);
                notify.info('Showing Price/Price Change summary tab');
            };

            self.showCompetitorDifferencesTab = function () {
                self.showingEmptyTab(false);
                self.showingPriceSummaryTab(false);
                self.showingCompetitorDifferencesTab(true);
                notify.info('Showing Competitor Price Differences tab');
            };

            // price change filter
            self.priceChangeFilter = ko.observable('All');
            self.priceChangeFilterUp = ko.observable(true);
            self.priceChangeFilterNone = ko.observable(true);
            self.priceChangeFilterDown = ko.observable(true);

            // user input limits
            self.priceMovementLowerLimit = ko.observable(-100);
            self.priceMovementUpperLimit = ko.observable(100);
            // filters
            self.priceMovementMinFilter = ko.observable('');
            self.priceMovementMaxFilter = ko.observable('');
            // unlimited value
            self.filterPriceMovementMinValue = ko.observable(-100000);
            self.filterPriceMovementMaxValue = ko.observable(+100000);

            self.priceMovementMinFilterCss = function () {
                return isValidPriceMovementMinFilter(true)
                    ? ''
                    : 'invalid';
            };
            self.priceMovementMaxFilterCss = function () {
                return isValidPriceMovementMaxFilter(true)
                    ? ''
                    : 'invalid';
            };

            function isValidPriceMovementMinFilter(allowEmpty) {
                var value = self.priceMovementMinFilter();
                if (value == '' && allowEmpty)
                    return true;
                return validation.isNumberInRange(value, self.priceMovementLowerLimit(), self.priceMovementUpperLimit());
            };

            function isValidPriceMovementMaxFilter(allowEmpty) {
                var value = self.priceMovementMaxFilter();
                if (value == '' && allowEmpty)
                    return true;
                return validation.isNumberInRange(value, self.priceMovementLowerLimit(), self.priceMovementUpperLimit());
            };

            self.applyPriceMovementFilter = function () {
                if (isValidPriceMovementMinFilter(true) && isValidPriceMovementMaxFilter(true)) {
                    applyPriceMovementFiltersToSites();
                    notify.success('Applied the Price Movement filters');
                } else {
                    applyPriceMovementFiltersToSites();
                    notify.error('Please enter a valid values for the Price Movement filters');
                }
            };

            self.resetPriceMovementFilters = function () {
                self.priceMovementMinFilter('');
                self.priceMovementMaxFilter('');
                applyPriceMovementFiltersToSites();
                notify.info('Removed the Price Movement filters');
            };

            self.highlightTrialPrices = ko.observable(false);
            self.highlightMatchCompetitors = ko.observable(false);
            self.highlightSoloPrices = ko.observable(false);
            self.highlightNoNearbyGrocerPrices = ko.observable(false);
            self.highlightHasNearbyGrocerPrices = ko.observable(false);

            self.highlightHasNearbyGrocerWithOutPrices = ko.observable(false);

            self.maxGrocerDriveTimeMinutes = ko.observable(5);

            self.shouldShouldHighlightReset = function () {
                return self.highlightTrialPrices()
                    || self.highlightMatchCompetitors()
                    || self.highlightSoloPrices()
                    || self.highlightNoNearbyGrocerPrices()
                    || self.highlightHasNearbyGrocerPrices()
                || self.highlightHasNearbyGrocerWithOutPrices();
            }

            self.siteEmailFilter = ko.observable("All Sites");

            self.siteEmailFilter.subscribe(function (newValue) {
                var messages = {
                    "All Sites": "Showing all Sites - no filter",
                    "No Emails": "Showing Sites with no email address",
                    "With Emails": "Showing Sites with email addresses",
                    "Selected": "Showing Selected Sites only",
                    "Not Selected": "Showing Non-Selected Sites only",
                    "With Overrides": "Showing Sites with Price Overrides",
                    "No Overrides": "Showing Sites with No Price Overrides"
                };
                if (newValue in messages)
                    notify.info(messages[newValue]);
            });

            self.resetSiteEmailFilter = function () {
                self.siteEmailFilter("All Sites")
            };

            self.isAnySiteChecked = ko.observable(false);

            self.getCheckedSitesStatus = function () {
                var sitesVm = ko.utils.unwrapObservable(self.dataModel);
                var sites = sitesVm.sites;
                var siteIdsList = ko.observableArray();

                var siteFound = ko.utils.arrayFirst(sites, function (site) {
                    if (site.checkedEmail()) return true;
                });
                return siteFound != null;
            }

            self.sendEmailSelectedSites = function () {
                var sitesVm = ko.utils.unwrapObservable(self.dataModel);
                var sites = sitesVm.sites;
                var siteIdsList = ko.observableArray();

                ko.utils.arrayForEach(sites, function (siteItem, index) {
                    if (siteItem.checkedEmail()) siteIdsList.push(siteItem.SiteId)
                    if (sites.length - 1 == index) {
                        self.sendEmailToSite(null, siteIdsList);
                    }
                });
            }

            self.selectAll = function () {
                var sitesVm = ko.utils.unwrapObservable(self.dataModel);
                var sites = sitesVm.sites;
                var count = 0;

                ko.utils.arrayForEach(sites, function (siteItem) {
                    if (siteItem.HasEmails) {
                        siteItem.checkedEmail(true);
                        count++;
                    }
                });

                if (count == 0) {
                    notify.warning('There are 0 Sites with an Email address');
                } else {
                    notify.success('Selected ' + count + ' Sites with an Email Address');
                }

                self.checkedEmailCount(count);
            }

            self.clearAll = function () {
                var sitesVm = ko.utils.unwrapObservable(self.dataModel);
                var sites = sitesVm.sites;

                ko.utils.arrayForEach(sites, function (siteItem) {
                    siteItem.checkedEmail(false);
                });
                notify.info('Deselected All Sites');

                self.checkedEmailCount(0);
            }

            self.selectSitesByPriceChange = function () {
                var min = 2.0;

                bootbox.confirm({
                    title: 'Select Sites by Price Change',
                    message: 'Please enter the Minimum threshold amount for a Price Change <br />'
                            + '<br />'
                            + '<label for="txtMinPriceChange">Min Price Change </label>'
                            + '<input type="number" min="0.0" max="+50.0" step="0.1" value="' + min + '" id="txtMinPriceChange" class="form-control font125pc" style="font-family: monospace" />'
                            + '<p><em>Example:</em>  <strong>2.0</strong> will find Price Changes of <strong>-2.0 or less</strong> or <strong>+2.0 or more</strong>)</p>'
                            + '<br />'
                            + '<label><input type="checkbox" id="chkClearCheckedSites" value="1" /> Clear currently selected sites</label>'
                            ,
                    buttons: {
                        cancel: {
                            label: '<i class="fa fa-times"></i> Close',
                            className: 'btn-default'
                        },
                        confirm: {
                            label: '<i class="fa fa-check"></i> Select Sites',
                            className: 'btn-success'
                        }
                    },
                    callback: function (result) {
                        if (result) {
                            var min = $('#txtMinPriceChange').val(),
                                clearAll = $('#chkClearCheckedSites').is(':checked'),
                                error = '';

                            if (!isNumber(min)) {
                                error = 'Min Price Change value "{MIN}" is not a number';
                            }

                            if (error == '') {
                                self.selectSitesWithPriceChangeOfAtleast(clearAll, min);
                                $('.bootbox.modal').hide();
                                return true;
                            } else {
                                error = replaceTokens(error, {
                                    '{MIN}': min
                                });

                                notify.error(error);
                                return false;
                            }
                        }
                    }
                });
            }

            self.getSitesWithPriceChangeOfAlleast = function (threshold) {
                var sites = [],
                    allsites = self.dataModel().sites;
                _.each(allsites, function (siteItem) {
                    var checked = false,
                        fuels = siteItem.FuelPricesToDisplay,
                        fuelPrice,
                        change,
                        pricePerLitre,
                        i,
                        diff,
                        autoPrice,
                        todayPrice,
                        overridePrice;

                    // check if site has any emails
                    if (siteItem.HasEmails) {
                        // look for any Price Change for any Fuel within min or more
                        for (i = 0; i < fuels.length; i++) {
                            fuelPrice = fuels[i].FuelPrice;
                            autoPrice = fuelPrice.AutoPrice;
                            todayPrice = fuelPrice.TodayPrice;
                            overridePrice = fuelPrice.OverridePrice();
                            change = 0;

                            if (isNumber(todayPrice)) {
                                if (isNumber(overridePrice))
                                    change = overridePrice - todayPrice;
                                else if (isNumber(autoPrice))
                                    change = autoPrice - todayPrice;
                            }

                            if (Math.abs(change) >= threshold) {
                                checked = true;
                                break;
                            }
                        }

                        if (checked)
                            sites.push(siteItem);
                    }
                });
                return sites;
            };

            self.selectSitesWithPriceChangeOfAtleast = function (clearAll, threshold) {
                var sites = self.getSitesWithPriceChangeOfAlleast(threshold),
                    allsites = self.dataModel().sites,
                    count = sites.length,
                    message,
                    format = count == 0
                        ? 'There are no Sites with an Email address with a Price Change of {MIN} or more'
                        : 'Selected {COUNT} Sites with a Price Change of {MIN} or more';

                message = replaceTokens(format, {
                    '{MIN}': threshold,
                    '{COUNT}': count
                });

                if (count == 0) {
                    notify.error(message);
                } else {
                    if (clearAll) {
                        _.each(allsites, function (siteItem) {
                            siteItem.checkedEmail(false);
                        });
                    }

                    _.each(sites, function (siteItem) {
                        siteItem.checkedEmail(true);
                    });
                    notify.success(message);
                }

                self.checkedEmailCount(self.getSelectedSiteCount())
            };

            self.getSelectedSiteCount = function () {
                var count = 0,
                    allsites = self.dataModel().sites;
                _.each(allsites, function (siteItem) {
                    if (siteItem.checkedEmail())
                        count++;
                });
                return count;
            };

            self.shouldShowSorryResults = ko.computed(function () {
                return self.hasAllSitesHidden()
                    || (!self.hasSites() && !self.busyLoadingData());
            });

            self.InitDateInUsFormat = function () {
                return moment(self.InitDate(), dmyFormatString).format(ymdFormatString);
            }

            self.resetViewingDate = function () {
                self.InitDate(todaysDateUkformat);
                self.loadPageWithParams();
            };

            self.NoNearbyGrocersCount = ko.observable(0);
            self.NearbyGrocersWithoutPricesCount = ko.observable(0);
            self.NearbyGrocersAndPricesCount = ko.observable(0);
            self.TrialPricesCount = ko.observable(0);
            self.MatchCompetitorsCount = ko.observable(0);
            self.SoloPricesCount = ko.observable(0);

            self.redrawFuelOverrideExceeds = function () {
                //console.log('test');
            };

            self.unleadedExceedMessage = ko.observable('');
            self.dieselExceedMessage = ko.observable('');
            self.superUnleadedExceedMessage = ko.observable('');

            self.populateOverrideExceedMessages = function () {
                self.unleadedExceedMessage('Range is ' + fuelOverrideLimits[2].change.min + ' and ' + fuelOverrideLimits[2].change.max);
                self.dieselExceedMessage('Range is ' + fuelOverrideLimits[6].change.min + ' and ' + fuelOverrideLimits[6].change.max);
                self.superUnleadedExceedMessage('Range is ' + fuelOverrideLimits[1].change.min + ' and ' + fuelOverrideLimits[1].change.max);
            };

            self.unleadedExceedMessage = ko.observable('Range is ' + fuelOverrideLimits[2].change.min + ' and ' + fuelOverrideLimits[2].change.max);
            self.dieselExceedMessage = ko.observable('Range is ' + fuelOverrideLimits[6].change.min + ' and ' + fuelOverrideLimits[6].change.max);
            self.superUnleadedExceedMessage = ko.observable('Range is ' + fuelOverrideLimits[1].change.min + ' and ' + fuelOverrideLimits[1].change.max);

            self.hasUnleadedOverrideExceededLimits = function () {
                return self.doesValueExceedOverrideLimits(FUEL_UNLEADED, self.unleadedOffset());
            };

            self.hasDieselOverrideExceededLimits = function () {
                return self.doesValueExceedOverrideLimits(FUEL_DIESEL, self.dieselOffset());
            };

            self.hasSuperUnleadedOverrideExceededLimits = function () {
                return self.doesValueExceedOverrideLimits(FUEL_SUPER_UNLEADED, self.superOffset());
            };

            self.hasAnyFuelOverrideExceededLimits = ko.observable(function () {
                return self.hasUnleadedOverrideExceededLimits()
                    || self.hasDieselOverrideExceededLimits()
                    || self.hasSuperUnleadedOverrideExceededLimits();
            });

            self.doesValueExceedOverrideLimits = function (fuelTypeId, value) {
                var limits = fuelOverrideLimits[fuelTypeId],
                    num = parseFloat(value);

                if (value == '')
                    return false;

                switch (self.changeType()) {
                    case self.changeTypeValuePlusMinus:
                        return num < limits.change.min || num > limits.change.max;
                        break;
                    case self.changeTypeValueOverrideAll:
                        return num < limits.absolute.min || num > limits.absolute.max;
                        break;
                }
                return false;
            };

            // Ajax Call to SendEmailToSite (both params could be null)
            self.sendEmailToSite = function (popupObj, listOfSiteIds) {
                var siteIds = ko.utils.arrayMap(listOfSiteIds(), function (id) {
                    return id;
                });

                commonShowEmailModal(siteIds);
            }

            // Handle SendEmailToSite response // TODO If any errors in log, we summarise accordingly
            self.sendEmail = function (promise, popupObj, siteItem) {
                var messages = getMessages("Email");

                promise.done(function (response, textStatus, jqXhr) {
                    var siteId = 0;
                    if (siteItem != null) siteId = siteItem.SiteId;

                    var emailSendLog = response.JsonObject; // serverData = List<EmailSendLog>
                    if (response.JsonStatusCode.CustomStatusCode == "ApiSuccess") {
                        $('#msgSuccess').html("<pre>" + response.SummaryString + "</pre>");
                        self.HasSuccessMessage(true);
                        attachEmailSendLogsToSites(null, siteId, emailSendLog); // pickup sites from VM
                    } else if (response.JsonStatusCode.CustomStatusCode == "ApiFail") {
                        $('#msgError').html("<pre>" + messages.failure + ": " + response.ErrorSummaryString + "</pre>");
                        self.HasErrorMessage(true);
                        attachEmailSendLogsToSites(null, siteId, emailSendLog); // pickup sites from VM
                    }
                    self.sendingEmails(false);
                })
                    .fail(function () {
                        $('#msgError').html(messages.failure + ": " + response.ErrorSummaryString);
                        self.HasErrorMessage(true);
                        self.sendingEmails(false);
                    });
                self.showEmailPopup(false);
                self.showEmailAllPopup(false);
            };

            self.loadEmailSendLogForDate = function (site) {
                var url = "";
                var siteId = 0;
                var yyyymmdd = dmyStringToYmdString(self.InitDate());

                if (site != null) siteId = site.SiteId;

                var filter = "?date=" + yyyymmdd + "&siteId=" + siteId;

                url = "Sites/GetEmailSendLog" + filter; // Get log for single site or all (siteId = 0)
                var $promise = common.callService("get", url, null); // args - maybe page no. (assuming no paging for now)
                self.loadEmailLogs($promise, site);
            };

            self.loadEmailLogs = function (promise, siteItem) {
                promise.done(function (response, textStatus, jqXhr) {
                    var siteId = 0;
                    if (siteItem != null) siteId = siteItem.SiteId;

                    var emailSendLog = response.JsonObject; // serverData = List<EmailSendLog>
                    if (response.JsonStatusCode.CustomStatusCode == "ApiSuccess") {
                        attachEmailSendLogsToSites(null, siteId, emailSendLog); // pickup sites from VM
                    } else if (response.JsonStatusCode.CustomStatusCode == "ApiFail") {
                        $('#msgError').html("Failure: Unable to provide Email status..");
                        self.HasErrorMessage(true);
                        attachEmailSendLogsToSites(null, siteId, emailSendLog); // pickup sites from VM
                    }
                })
                    .fail(function () {
                        $('#msgError').html("Failure occured - unable to get email statuses");
                        self.HasErrorMessage(true);
                    });
            };

            self.bind = function () {
            };

            // End define

            var fuelsToDisplay = [2, 6, 1]; // fuel columns (Unleaded, Diesel, SuperUnleaded)

            var sitePricingView;

            self.gotoFirstInvalidOverrideField = function () {
                var field = $('.override-invalid:first');
                field.focus();
            };

            self.setupSearchFields = function (ukDate, storeName, catNo, storeNo, storeTown) {
                moment.locale("en-gb"); // Set Locale for moment (aka moment.locale("en-gb"))
                self.InitDate(ukDate);

                self.ViewingHistorical(true);
                if (self.InitDate() == todaysDateUkformat) {
                    self.ViewingHistorical(false);
                }

                if (storeName && storeName != 'undefined')
                    self.StoreName(storeName);

                if (catNo && catNo != 'undefined')
                    self.CatNo(catNo);

                if (storeNo && storeNo != 'undefined')
                    self.StoreNo(storeNo);

                if (storeTown && storeTown != 'undefined')
                    self.StoreTown(storeTown);

                try {
                    refreshDates(ukDate);
                }
                catch (err) {
                    // Handle error(s) here
                }
            }

            self.switchChangeType = function () {
                self.unleadedOffset('');
                self.dieselOffset('');
                self.superOffset('');
                self.hasBadPriceChangeValue(false);
                return true;
            }

            self.setOverrideModePlusMinus = function () {
                self.changeType(self.changeTypeValuePlusMinus);
                self.switchChangeType();

                self.unleadedExceedMessage('Change outside ' + fuelOverrideLimits[2].change.min + ' and ' + fuelOverrideLimits[2].change.max);
                self.dieselExceedMessage('Change outside ' + fuelOverrideLimits[6].change.min + ' and ' + fuelOverrideLimits[6].change.max);
                self.superUnleadedExceedMessage('Change outside ' + fuelOverrideLimits[1].change.min + ' and ' + fuelOverrideLimits[1].change.max);
            };

            self.setOverrideModeOverrideAll = function () {
                self.changeType(self.changeTypeValueOverrideAll);
                self.switchChangeType();

                self.unleadedExceedMessage('Price outside ' + fuelOverrideLimits[2].absolute.min + ' and ' + fuelOverrideLimits[2].absolute.max);
                self.dieselExceedMessage('Price outside ' + fuelOverrideLimits[6].absolute.min + ' and ' + fuelOverrideLimits[6].absolute.max);
                self.superUnleadedExceedMessage('Price outside ' + fuelOverrideLimits[1].absolute.min + ' and ' + fuelOverrideLimits[1].absolute.max);
            };

            var changePricePerLitreForFuel = function (pplVal, fuelTypeId) {
                return amendOverridePrice(pplVal, fuelTypeId);
            }

            var amendOverridePrice = function (pplVal, fuelTypeId) {
                var success = true;
                var sitesVm = ko.utils.unwrapObservable(self.dataModel);
                var sites = sitesVm.sites;
                _.each(sites, function (siteItem) {
                    var selectedFuel = getFuelDisplayById(siteItem.FuelPricesToDisplay, fuelTypeId);
                    var obj = selectedFuel.Fuel;
                    pplVal = pplVal.trim();
                    fieldErrors.remove(siteItem.SiteId, fuelTypeId);
                    self.hasAnyFieldErrors(fieldErrors.hasAny());
                    self.fieldErrorCount(fieldErrors.count());
                    if (pplVal == "") {
                        obj.clearOverridePpl(obj);
                    } else {
                        var changeTypeValue = self.changeType();

                        if (changeTypeValue == self.changeTypeValuePlusMinus) {
                            if (!obj.setPlusMinusPpl(obj, pplVal)) {
                                success = false;
                                fieldErrors.add(siteItem.siteId, fuelTypeId);
                                self.hasAnyFieldErrors(fieldErrors.hasAny());
                                self.fieldErrorCount(fieldErrors.count());
                            }
                        }
                        else {
                            obj.setOverrideAllPpl(obj, pplVal);
                        }
                    }
                });

                return success;
            }

            function setOverridePriceForSiteFuel(overridePrice, siteItem, fuelTypeId) {
                var selectedFuel = getFuelDisplayById(siteItem.FuelPricesToDisplay, fuelTypeId);
                var obj = selectedFuel.Fuel;

                fieldErrors.remove(siteItem.SiteId, fuelTypeId);
                self.hasAnyFieldErrors(fieldErrors.hasAny());
                self.fieldErrorCount(fieldErrors.count());

                obj.setOverrideAllPpl(obj, formatNumberTo1DecimalPlace(overridePrice));
            };

            function setSuperUnleadedToUnleadedPlusMarkup() {
                var sitesVm = ko.utils.unwrapObservable(self.dataModel);
                var sites = sitesVm.sites;
                _.each(sites, function (siteItem) {
                    var unleaded = getFuelDisplayById(siteItem.FuelPricesToDisplay, FUEL_UNLEADED),
                        superUnleaded = getFuelDisplayById(siteItem.FuelPricesToDisplay, FUEL_SUPER_UNLEADED),
                        unleadedOverridePrice = Number(unleaded.Fuel.getOverridePrice()),
                        superUnleadedOverridePrice = unleadedOverridePrice + pricingSettings.superUnleadedMarkupPrice;

                    setOverridePriceForSiteFuel(superUnleadedOverridePrice, siteItem, FUEL_SUPER_UNLEADED);
                })
            };

            var commonChangePricePerLitre = function (pplVal, fuelTypeId) {
                var message = 'Applying Override to ' + fuelTypeNames[fuelTypeId];

                if (fuelTypeId == FUEL_UNLEADED)
                    message += ' and ' + fuelTypeNames[FUEL_SUPER_UNLEADED];

                applier = function () {
                    fieldErrors.clearForFuel(fuelTypeId);
                    self.fieldErrorCount(fieldErrors.count());
                    applyMassFuelPriceOverride(pplVal, fuelTypeId);
                    busyloader.hide();
                };
                busyloader.show({
                    message: message,
                    dull: true
                });

                setTimeout(applier, 1500);
            };

            function applyMassFuelPriceOverride(pplVal, fuelTypeId) {
                isApplyingMassFuelPriceOverride = true;
                validateAndApplyPriceChange(pplVal, fuelTypeId);

                // handle SuperUnleaded = Unleaded + Markup
                if (fuelTypeId == FUEL_UNLEADED) {
                    if (isNumber(pplVal)) {
                        setSuperUnleadedToUnleadedPlusMarkup();
                    }
                }

                isApplyingMassFuelPriceOverride = false;

                detectUnsavedChanges();
                recalculateSainsburysStats();
            };

            self.applyOverrideAllRounding = function (value) {
                if (self.changeType() == self.changeTypeValueOverrideAll) {
                    //return replaceLastDigitWith9(value);
                    return isNumber(value) ? Number(value).toFixed(1) : value;
                } else {
                    //return Math.floor(value) + '';
                    return isNumber(value) ? Number(value).toFixed(1) : value;
                }
            };

            function applyPriceMovementFiltersToSites() {
                var min = self.priceMovementMinFilter(),
                    max = self.priceMovementMaxFilter(),
                    unfiltered = min == '' && max == '',
                    invalid = !isValidPriceMovementMinFilter(false) || !isValidPriceMovementMaxFilter(false),
                    origin = 10000;
                min = origin + (min == '' ? 0 : Number(min));
                max = origin + (max == '' ? origin : Number(max));

                var sitesVm = ko.utils.unwrapObservable(self.dataModel);
                var sites = sitesVm.sites;
                _.each(sites, function (siteItem) {
                    var show = unfiltered,
                        fuels = siteItem.FuelPricesToDisplay,
                        i,
                        priceMovement,
                        adjustedPriceMovement,
                        match;

                    for (i = 0; i < fuels.length; i++) {
                        match = false;
                        if (!invalid) {
                            priceMovement = fuels[i].UpDownValue();
                            priceMovement = priceMovement == '' || isNaN(priceMovement) ? 0 : Number(priceMovement) / 10;
                            adjustedPriceMovement = origin + priceMovement;

                            if (adjustedPriceMovement >= min && adjustedPriceMovement <= max) {
                                show = true;
                                match = true;
                            }
                        }
                        fuels[i].isMatchedByPriceMovementFilter(match);
                    }
                    siteItem.hiddenByPriceMovementFilter(!show && !unfiltered);
                });
            };

            function commonConfirmApplyFuelOverride(opts) {
                if (!isNumber(opts.value)) {
                    notify.error('Please enter a value for ' + opts.fuelName);
                    return;
                }

                if (opts.value == '' || isNaN(opts.value)) {
                    notify.error('Please enter an Override value for ' + opts.fuelName);
                    return;
                }

                var possibleIncorrectChangeType = false,
                    message,
                    otherChangeType;

                if (self.changeType() == self.changeTypeValuePlusMinus) {
                    possibleIncorrectChangeType = opts.value >= opts.limits.absolute.min;
                    message = 'The large value of <span class="font150pc">' + opts.value + '</span> looks like an absolute Override change'
                        + '<br /><br />'
                        + 'Do you wish to apply this value as an <strong>Absolute Override Price</strong> change instead?';
                    otherChangeType = self.changeTypeValueOverrideAll;
                } else {
                    possibleIncorrectChangeType = opts.value < opts.limits.absolute.min;
                    message = 'The small value of <span class="font150pc">' + opts.value + '</span> looks like a +/- Override change'
                        + '<br /><br />'
                        + 'Do you wish to apply this value as a <strong>+/-</strong> change instead?';
                    otherChangeType = self.changeTypeValuePlusMinus;
                }

                if (possibleIncorrectChangeType) {
                    bootbox.confirm({
                        title: '<i class="fa fa-question"></i> <strong>WARNING:</strong> Possibly incorrect Change Type',
                        message: message,
                        buttons: {
                            confirm: {
                                label: '<i class="fa fa-check"></i> Yes',
                                className: 'btn-success'
                            },
                            cancel: {
                                label: '<i class="fa fa-times"></i> No',
                                className: 'btn-danger'
                            }
                        },
                        callback: function (result) {
                            if (result) {
                                self.changeType(otherChangeType);
                                confirmAndApplyFuelOverride(opts);
                            }
                            else
                                confirmAndApplyFuelOverride(opts);
                        }
                    });

                } else {
                    confirmAndApplyFuelOverride(opts);
                }
            };

            function confirmAndApplyFuelOverride(opts) {
                var that = this,
                    isPlusMinus = self.changeType() == self.changeTypeValuePlusMinus,
                    message;

                if (isPlusMinus) {
                    message = 'Are you sure you want to <strong>increase</strong> the Override Price?<br />'
                            + '<br />'
                            + 'The <strong>Override Price</strong> will be based on the <strong><em>Today (' + self.InitDate() + ')</em></strong> Price <strong>' + formatNumber1DecimalWithPlusMinus(opts.value) + '</strong> for <strong>' + opts.fuelName + '</strong> for each Site.';
                } else {
                    message = 'Are you sure you want to set the <strong>Override Price</strong> to <strong class="font125pc"> ' + Number(opts.value).toFixed(1) + '</strong> for <strong>' + opts.fuelName + '</strong> for each Site';
                    if (opts.fuelName == 'Unleaded') {
                        message += '<br /><br /><strong>Note:</strong> This will also set the <strong>Super-unleaded</strong> price to <strong class="font125pc">' + formatNumberTo1DecimalPlace(Number(opts.value) + pricingSettings.superUnleadedMarkupPrice) + '</strong>';
                    }
                }

                bootbox.confirm({
                    title: '<i class="fa fa-question"></i> Confirm Fuel Override',
                    message: message,
                    buttons: {
                        confirm: {
                            label: '<i class="fa fa-check"></i> Yes',
                            className: 'btn-success'
                        },
                        cancel: {
                            label: '<i class="fa fa-times"></i> No',
                            className: 'btn-danger'
                        }
                    },
                    callback: function (result) {
                        if (result) {
                            commonChangePricePerLitre(opts.value, opts.fuelTypeId);
                        }
                    }
                });
            };

            self.toggleChangeType = function () {

                self.unleadedOffset('');
                self.dieselOffset('');
                self.superOffset('');

                if (self.changeType() == self.changeTypeValuePlusMinus) {
                    self.changeType(self.changeTypeValueOverrideAll);
                    notify.info('Change Type is now Override All. Please enter absolute prices (e.g. 120.2) ');
                } else {
                    self.changeType(self.changeTypeValuePlusMinus);
                    notify.info('Change Type is now +/-. Please enter a relative value (e.g. -2 or 3)');
                }
            };

            self.changePricePerLitreUnleaded = function () {
                var pplVal = ko.utils.unwrapObservable(self.unleadedOffset);
                pplVal = self.applyOverrideAllRounding(pplVal);
                self.unleadedOffset(pplVal);

                commonConfirmApplyFuelOverride({
                    fuelTypeId: FUEL_UNLEADED,
                    fuelName: fuelTypeNames[FUEL_UNLEADED],
                    value: pplVal,
                    limits: fuelOverrideLimits["2"]
                });
            }

            self.changePricePerLitreDiesel = function () {
                var pplVal = ko.utils.unwrapObservable(self.dieselOffset);
                pplVal = self.applyOverrideAllRounding(pplVal);
                self.dieselOffset(pplVal);

                commonConfirmApplyFuelOverride({
                    fuelTypeId: FUEL_DIESEL,
                    fuelName: fuelTypeNames[FUEL_DIESEL],
                    value: pplVal,
                    limits: fuelOverrideLimits["6"]
                });
            };

            self.changePricePerLitreSuper = function () {
                var pplVal = ko.utils.unwrapObservable(self.superOffset);
                pplVal = self.applyOverrideAllRounding(pplVal);
                self.superOffset(pplVal);

                commonConfirmApplyFuelOverride({
                    fuelTypeId: FUEL_SUPER_UNLEADED,
                    fuelName: fuelTypeNames[FUEL_SUPER_UNLEADED],
                    value: pplVal,
                    limits: fuelOverrideLimits["1"]
                });
            };

            self.closeWarningPanel = function (item, event) {
                self.hasBadPriceChangeValue(false);
            };

            var validateAndApplyPriceChange = function (pplVal, fuelTypeId) {
                var valid,
                    changeTypeValue = self.changeType();

                if (pplVal == '') {
                    valid = true;
                } else {
                    if (changeTypeValue == self.changeTypeValuePlusMinus) {
                        valid = isValidOffsetNumber(pplVal);
                    } else {
                        valid = isValidOverrideNumber(pplVal);
                    }
                }

                if (valid) {
                    removeAllFieldOverrideErrors(fuelTypeId);
                    self.HasErrorMessage(false);
                    valid = changePricePerLitreForFuel(pplVal, fuelTypeId);
                }

                if (valid) {
                    self.hasBadPriceChangeValue(false);
                } else {
                    setBadPriceChangeForFuelError(fuelTypeId);
                }
            }

            var setBadPriceChangeForFuelError = function (fuelTypeId) {
                $('#msgFuelTypeError').html(fuelTypeNames[fuelTypeId]);
                self.hasBadPriceChangeValue(true);
            };

            var isValidOffsetNumber = function (val) {
                return /^\-?\d{1,4}(\.\d{1})?$/.test(val);
            };

            var isValidOverrideNumber = function (val) {
                return /^\d{1,4}(\.\d{1})?$/.test(val) && parseFloat(val, 10) > 0.00;
            };

            var removeAllFieldOverrideErrors = function (fuelTypeId) {
                var sitesVm = ko.utils.unwrapObservable(self.dataModel),
                    sites = sitesVm.sites;
                if (sites && _.any(sites)) {
                    _.each(sites, function (siteItem) {
                        var selector = '#OverridePriceHighlight_' + siteItem.SiteId + '_' + fuelTypeId;
                        $(selector).html('');
                        fieldErrors.remove(siteItem.siteId, fuelTypeId);
                        self.hasAnyFieldErrors(fieldErrors.hasAny());
                        self.fieldErrorCount(fieldErrors.count());
                    });
                };
            };

            var buildViewModels = function (serverData) {
                // Page will have data items: List of items, pageNo, Date
                sitePricingView = {
                    // We don't know how many pages we have until we have a ajax call for count of pages
                    // or return that within this get request
                    pageNo: 1, //pageNo,
                    sites: serverData, // array/list of sitePriceViewModels
                };

                setCompetitorFields(sitePricingView.sites); // sets up 2 additional fields per site hasCompetitors and competitors = [];
                setEmailLogEntryFields(sitePricingView.sites);
                enListOnlyFuelsToDisplay(sitePricingView.sites); // adds another prop to siteItem - FuelPricesToDisplay

                self.dataModel(sitePricingView);

                // load this later once page is loaded..
                self.loadEmailSendLogForDate(null);

                buildPriceDifferences();
                renderPriceDifferencesBarChart(self.PriceDifferences);

                $('#PriceDifferencePanel').trigger('data-loaded');

                self.redrawFuelPriceChanges();
                filterPriceChangeSiteRow();

                applyPriceDifferenceToolTipFixToAllSites(sitePricingView.sites);

                self.hasSites(sitePricingView.sites.length != 0);

                calcSainsburysStats(sitePricingView.sites);
            };

            function applyPriceDifferenceToolTipFixToAllSites(sites) {
                _.each(sites, function (siteItem) {
                    self.applyPriceDiffInfoTipFix(siteItem.FuelPricesToDisplay[0]);
                    self.applyPriceDiffInfoTipFix(siteItem.FuelPricesToDisplay[1]);
                    self.applyPriceDiffInfoTipFix(siteItem.FuelPricesToDisplay[2]);
                });
            };

            var renderPriceDifferencesBarChart = function (differences) {
                var html = [],
                    ele = $('#PriceDifferenceBarChart'),
                    sorted = ko.utils.unwrapObservable(differences).splice(0).sort(function (a, b) {
                        return a.key == 'n/a' ? 1 : b.key == 'n/a' ? 0 : a.key - b.key;
                    }),
                    i,
                    count = sorted.length,
                    chartWidthPixels = 1280, // NOTE: doesn't work with 100%
                    labelWidthPixels = chartWidthPixels * 5 / 100,
                    borderWidthPixels = 2,
                    cellWidthPixels = Math.floor((chartWidthPixels - labelWidthPixels - (borderWidthPixels * count)) / count),
                    maxBarHeight = 60,
                    borderWidth = 2,
                    row1 = [],
                    row2 = [],
                    row3 = [],
                    min = 0,
                    max = 0,
                    range,
                    diff,
                    diffText,
                    siteCount,
                    totalSiteCount = 0,
                    css,
                    barHeight,
                    lowerSiteCount = 0,
                    sameSiteCount = 0,
                    higherSiteCount = 0,
                    naSiteCount = 0,
                    cellInfoTip = "",
                    format;

                // collect stats
                for (i = 0; i < count; i++) {
                    siteCount = sorted[i].value;
                    totalSiteCount += siteCount;
                    diff = sorted[i].key;
                    min = i == 0 ? siteCount : Math.min(min, siteCount);
                    max = i == 0 ? siteCount : Math.max(max, siteCount);
                    if (diff < 0)
                        lowerSiteCount += siteCount;
                    else if (diff > 0)
                        higherSiteCount += siteCount;
                    else if (diff == 0)
                        sameSiteCount += siteCount;
                    else if (diff == 'n/a')
                        naSiteCount += siteCount;
                }

                range = max - min;

                // build HTML
                html.push('<div class="price-diff-bar-chart">');
                html.push('<div class="price-diff-bar-title">');
                html.push('Competitor Price Differences for ' + totalSiteCount + ' Sites');
                html.push('</div>');

                html.push('<table>');
                row1.push('<th style="width: ' + labelWidthPixels + 'px; height: ' + (maxBarHeight + 20) + 'px"><i class="fa fa-long-arrow-up"></i><br />Sites</th>');
                row2.push('<th>Diff</th>');

                for (i = 0; i < count; i++) {
                    siteCount = sorted[i].value;
                    diff = sorted[i].key;
                    barHeight = Math.floor(maxBarHeight * siteCount / range);
                    if (barHeight == 0 && siteCount != 0)
                        barHeight = 1;

                    diffText = isNaN(diff)
                        ? diff
                        : diff.toFixed(1);

                    format = diff == 'n/a'
                        ? messageFormats.barChartCellsNAInfoTips
                        : messageFormats.barChartCellsInfoTips[Math.sign(diff)];

                    cellInfoTip = replaceTokens(format, {
                        '{VALUE}': formatNumber1DecimalWithPlusMinus(diff),
                        '{COUNT}': siteCount
                    });

                    css = diff == 'n/a' ? 'na-price' : diff < 0 ? 'lower-price' : diff > 0 ? 'higher-price' : 'same-price';
                    row1.push('<td style="width:' + cellWidthPixels + 'px;" data-infotip="' + cellInfoTip + '">');
                    row1.push('<div class="bar ' + css + '" style="border-bottom-width: ' + barHeight + 'px">' + siteCount + '</span>');
                    row1.push('</td>');
                    row2.push('<td class="diff ' + css + '">' + diffText + '</td>');
                }
                html.push('<tr>' + row1.join('') + '</tr>');
                html.push('<tr>' + row2.join('') + '</tr>');
                html.push('</table>');

                html.push('<div class="price-diff-bar-chart-key">');
                if ((lowerSiteCount + sameSiteCount + higherSiteCount) == 0) {
                    html.push('<span class="no-price-differences">There are no price differences. ');
                    html.push('Please upload a <a class="btn btn-primary btn-sm" href="' + common.getSiteRoot() + '/File/Upload"><i class="fa fa-upload"></i> Daily Price Data</a> file or choose another date or search criteria.');
                    html.push('</span>');
                }

                if (lowerSiteCount != 0)
                    html.push('<span class="lower-price">' + lowerSiteCount + ' Sites with Lower Prices <i class="fa fa-arrow-down"></i></span>');
                if (sameSiteCount != 0)
                    html.push('<span class="same-price">' + sameSiteCount + ' Sites with Same Price <i class="fa fa-minus"></i></span>');
                if (higherSiteCount != 0)
                    html.push('<span class="higher-price">' + higherSiteCount + ' Sites with Higher Prices <i class="fa fa-arrow-up"></i></span>');
                if (naSiteCount != 0)
                    html.push('<span class="na-price">' + naSiteCount + ' Sites with n/a Price</span>');
                html.push('<div>');
                html.push('</div>');
                ele.html(html.join(''));

                bindPriceDifferencesBarChartEvents();
            };

            var bindPriceDifferencesBarChartEvents = function () {
                // TODO - should be seperate controls "Price Difference" and not "Price Change" filters
                //var barchart = $('#PriceDifferenceBarChart');
                //barchart.find('.lower-price').off().on('click', self.togglePriceChangeFilterDown);
                //barchart.find('.same-price').off().on('click', self.togglePriceChangeFilterNone);
                //barchart.find('.higher-price').off().on('click', self.togglePriceChangeFilterUp);
            };

            // map all competitors from ajax call to inidividual site
            var attachCompetitorsToSite = function (site, serverCompetitors, callback) {
                // extract the competitors and attach them to their relevant site in the model
                var sitesVm = ko.utils.unwrapObservable(self.dataModel);
                var sites = sitesVm.sites;
                if (site.SiteId != 0)
                    sites = _.filter(sites, function (siteItem) {
                        return siteItem.SiteId == site.SiteId;
                    });

                if (sites && _.any(sites)) {
                    _.each(sites, function (siteItem) {
                        if (!siteItem.hasCompetitors()) {
                            var competitorsForSite = _.filter(serverCompetitors, function (compItem) {
                                return siteItem.SiteId == compItem.JsSiteId;
                            });

                            if (competitorsForSite && _.any(competitorsForSite)) {
                                siteItem.hasCompetitors(true);
                                markCompetitorDrivetimeBoundaries(competitorsForSite);
                                enListOnlyFuelsToDisplay(competitorsForSite);
                                siteItem.competitors(competitorsForSite);
                            }
                        }
                        callback(siteItem);
                    });
                }
            };

            // determine markup (PPL) for a Fuel based on DriveTime (data driven)
            function getMarkupForFuelDriveTime(fuelTypeId, driveTime, markups) {
                var item,
                    i,
                    intDriveTime = Math.floor(driveTime);

                for (i = 0; i < markups.length; i++) {
                    item = markups[i];
                    if (intDriveTime >= item.DriveTime && intDriveTime <= item.MaxDriveTime)
                        return item.Markup;
                }
                return 0;
            };

            function flagCompetitorFuelDriveTimeBoundaries(prop, competitorsForSite, fuelTypeId, markups) {
                var lastDrivePence = -1;

                _.each(competitorsForSite, function (compsite) {
                    var driveTimePence = getMarkupForFuelDriveTime(fuelTypeId, compsite.DriveTime, markups);
                    compsite[prop] = lastDrivePence != driveTimePence;
                    lastDrivePence = driveTimePence;
                });
            };

            // Mark competitors by start of Drivetime Boundaries for each Fuel
            function markCompetitorDrivetimeBoundaries(competitorsForSite) {
                flagCompetitorFuelDriveTimeBoundaries('isDriveTimeBoundaryUnleaded', competitorsForSite, FUEL_UNLEADED, driveTimeMarkups.Unleaded);
                flagCompetitorFuelDriveTimeBoundaries('isDriveTimeBoundaryDiesel', competitorsForSite, FUEL_DIESEL, driveTimeMarkups.Diesel);
                flagCompetitorFuelDriveTimeBoundaries('isDriveTimeBoundarySuperUnleaded', competitorsForSite, FUEL_SUPER_UNLEADED, driveTimeMarkups.SuperUnleaded);

                // calc Pence for DriveTime for each fuel type
                _.each(competitorsForSite, function (compsite) {
                    compsite.DrivePenceForUnleaded = getMarkupForFuelDriveTime(FUEL_UNLEADED, compsite.DriveTime, driveTimeMarkups.Unleaded);
                    compsite.DrivePenceForDiesel = getMarkupForFuelDriveTime(FUEL_DIESEL, compsite.DriveTime, driveTimeMarkups.Diesel);
                    compsite.DrivePenceForSuperUnleaded = getMarkupForFuelDriveTime(FUEL_SUPER_UNLEADED, compsite.DriveTime, driveTimeMarkups.SuperUnleaded);
                });
            };

            var attachEmailSendLogsToSites = function (sites, siteId, emailSendLog) {
                if (sites == null) {
                    var sitesVm = ko.utils.unwrapObservable(self.dataModel);
                    sites = sitesVm.sites;
                }
                if (siteId != 0) {
                    sites = _.filter(sites, function (siteItem) {
                        return siteItem.SiteId == siteId;
                    });
                }

                if (sites && _.any(sites)) {
                    _.each(sites, function (siteItem) {
                        var emailSendLogEntryForSite = _.find(emailSendLog, function (item) {
                            return (item.SiteId == siteItem.SiteId);
                        });

                        siteItem.EmailSendLogEntry(null);
                        siteItem.hasEmailSendLogEntry(false);
                        if (emailSendLogEntryForSite != null) {
                            emailSendLogEntryForSite.ErrorMessage = formatEmailErrorMessage('' + emailSendLogEntryForSite.ErrorMessage);
                            emailSendLogEntryForSite.WarningMessage = formatEmailWarningMessage('' + emailSendLogEntryForSite.WarningMessage);
                            siteItem.EmailSendLogEntry(emailSendLogEntryForSite);
                            siteItem.hasEmailSendLogEntry(true);
                        }
                    });
                };
            };
            function formatEmailErrorMessage(message) {
                return message;
            };

            function formatEmailWarningMessage(message) {
                return message.replace(/^Warning:/, '[b]Warning:[/b]')
                    .replace(/, siteName=([^\.]+)\./, ' &mdash; [b]$1[/b][br /]');
            };

            var toggleCompDisplay = function (siteItem) {
                if (siteItem.SiteId != 0) {
                    if (siteItem.showingComps()) {
                        $('#AjaxCollapseCompetitorsforsite' + siteItem.SiteId).hide();
                        IsPriceCollapsed = 0;
                        siteItem.showingComps(false);
                    } else {
                        $('#AjaxCollapseCompetitorsforsite' + siteItem.SiteId).show();
                        IsPriceCollapsed = 1;
                        siteItem.showingComps(true);
                    }
                }
            }

            var enListOnlyFuelsToDisplay = function (sites) {

                var totals_soloPrices = 0,
                    totals_trialPrices = 0,
                    totals_matchCompetitors = 0,
                    totals_noNearbyGrocers = 0,
                    totals_nearbyGrocersWithoutPrices = 0,
                    totals_nearbyGrocersAndPrices = 0;

                _.each(sites, function (siteItem) {
                    siteItem.FuelPricesToDisplay = getFuelPricesToDisplay(siteItem, siteItem.FuelPrices);

                    // get total counts
                    $.each(siteItem.FuelPricesToDisplay, function (i, fuel) {
                        switch (siteItem.PriceMatchType) {
                            case 1:
                                totals_soloPrices++;
                                break;
                            case 2:
                                totals_trialPrices++;
                                break;
                            case 3:
                                totals_matchCompetitors++;
                                break;
                        }
                    });

                    // Count total No nearby Grocers, Grocer with Prices and Grocers without Prices
                    if (siteItem.HasNearbyUnleadedGrocers) {
                        if (siteItem.HasNearbyUnleadedGrocersPriceData)
                            totals_nearbyGrocersAndPrices++;
                        else
                            totals_nearbyGrocersWithoutPrices++;
                    } else
                        totals_noNearbyGrocers++;

                    if (siteItem.HasNearbyDieselGrocers) {
                        if (siteItem.HasNearbyDieselGrocersPriceData)
                            totals_nearbyGrocersAndPrices++
                        else
                            totals_nearbyGrocersWithoutPrices++;
                    } else
                        totals_noNearbyGrocers++;

                    if (siteItem.HasNearbySuperUnleadedGrocers) {
                        if (siteItem.HasNearbySuperUnleadedGrocersPriceData)
                            totals_nearbyGrocersAndPrices++;
                        else
                            totals_nearbyGrocersWithoutPrices++;
                    } else
                        totals_noNearbyGrocers++;

                    // detect price change for ANY fuel in the site
                    siteItem.priceChangeSigns = ko.computed(function () {
                        var i,
                            fuel,
                            fuels = siteItem.FuelPricesToDisplay,
                            signs = [],
                            delta;
                        for (i = 0; i < fuels.length; i++) {
                            fuel = fuels[i];
                            delta = fuel.UpDownValue();
                            signs.push((!delta || delta == 0 || delta == '-') ? 'None' : delta <= 0.1 ? 'Down' : 'Up');
                        }
                        return signs.join(',');
                    }, siteItem);

                    // detect errors for ANY fuel
                    siteItem.hasRowErrors = ko.computed(function () {
                        var i,
                            fuel,
                            fuels = siteItem.FuelPricesToDisplay;
                        for (i = 0; i < fuels.length; i++) {
                            fuel = fuels[i];
                            if (!fuel.hasValidValue())
                                return true;
                        }
                        return false;
                    });

                    siteItem.hasOverrideIncreaseExceedingLimits = ko.computed(function () {
                        var i,
                            fuel,
                            fuels = siteItem.FuelPricesToDisplay;
                        for (i = 0; i < fuels.length; i++) {
                            fuel = fuels[i];
                            if (fuel.FuelPrice.isIncreaseOutsideLimits())
                                return true;
                        }
                        return false;
                    });

                    siteItem.siteRowCss = ko.computed(function () {
                        if ($.isFunction(siteItem.isEditing) && siteItem.isEditing())
                            return 'focused';

                        if (siteItem.HasOverrides) {

                            var hasOverrides = false,
                                i,
                                fuelsToDisplay = siteItem.FuelPricesToDisplay,
                                fuel;
                            for (i = 0; i < fuelsToDisplay.length; i++) {
                                fuel = fuelsToDisplay[i];
                                if (isNumber(fuel.FuelPrice.OverridePrice())) {
                                    hasOverrides = true;
                                    break;
                                }
                            }
                            siteItem.HasOverrides(hasOverrides);
                        }

                        var css = '',
                            emailFilter = self.siteEmailFilter(),
                            visible = (emailFilter == 'All Sites')
                                || (emailFilter == "No Emails" && !siteItem.HasEmails)
                                || (emailFilter == "With Emails" && siteItem.HasEmails)
                                || (emailFilter == "Selected" && siteItem.HasEmails && siteItem.checkedEmail())
                                || (emailFilter == "Not Selected" && siteItem.HasEmails && !siteItem.checkedEmail())
                                || (emailFilter == "With Overrides" && siteItem.HasOverrides())
                                || (emailFilter == "No Overrides" && !siteItem.HasOverrides());

                        css = (visible ? '' : ' hide') + (siteItem.hasRowErrors()
                                ? 'row-errors'
                                : siteItem.hasOverrideIncreaseExceedingLimits() ? ' row-override-increase-exceeds' : '')
                            + ('checkedEmail' in siteItem && siteItem.checkedEmail()
                                ? ' has-checked-email'
                                : ' no-checked-email');
                        return css;
                    });
                });

                // set totals
                self.TrialPricesCount(totals_trialPrices);
                self.MatchCompetitorsCount(totals_matchCompetitors);
                self.SoloPricesCount(totals_soloPrices);

                self.NoNearbyGrocersCount(totals_noNearbyGrocers);
                self.NearbyGrocersWithoutPricesCount(totals_nearbyGrocersWithoutPrices);
                self.NearbyGrocersAndPricesCount(totals_nearbyGrocersAndPrices);
            };

            var setEmailLogEntryFields = function (sites) {
                _.each(sites, function (siteItem) {
                    siteItem.EmailSendLogEntry = ko.observable();
                    siteItem.hasEmailSendLogEntry = ko.observable(false);
                });
            };

            var setCompetitorFields = function (sites) {
                _.each(sites, function (siteItem) {
                    siteItem.hiddenByPriceMovementFilter = ko.observable(false);
                    siteItem.hasCompetitors = ko.observable(false);
                    siteItem.competitors = ko.observableArray([]);
                    siteItem.showingComps = ko.observable(false);
                    siteItem.loadingCompetitors = ko.observable(false);
                    siteItem.isEditing = ko.observable(false);
                    siteItem.checkedEmail = ko.observable(false);
                    siteItem.clickCheckedEmail = function () {
                        self.checkedEmailCount(self.getSelectedSiteCount());
                        return true;
                    };

                    siteItem.checkedEmail.subscribe(function (newValue) {
                        if (newValue == true) self.isAnySiteChecked(true);
                        else {
                            self.isAnySiteChecked(self.getCheckedSitesStatus());
                        }
                    });

                    siteItem.getCompetitorDataClick = function () {
                        toggleCompDisplay(siteItem);
                        siteItem.loadingCompetitors(true);
                        self.getCompetitorDataForSite(siteItem, competitorPopup.drawPopup);
                        return true;
                    };

                    siteItem.getCompetitorPopupDataClick = function () {
                        competitorPopup.hidePrices();
                        competitorPopup.showLoading();
                        competitorPopup.populate(siteItem);

                        siteItem.loadingCompetitors(true);
                        self.getCompetitorDataForSite(siteItem, competitorPopup.drawPopup);
                        return true;
                    };

                    siteItem.canSendEmail = function () {
                        var result = false;

                        for (var i = 0; i < siteItem.FuelPrices.length; i++) {
                            var currentFuel = siteItem.FuelPrices[i];

                            if (currentFuel.OverridePrice > 0 && currentFuel.TodayPrice - currentFuel.OverridePrice != 0) {
                                result = true;
                                break;
                            }
                            else if (currentFuel.OverridePrice == 0 && currentFuel.TodayPrice - currentFuel.AutoPrice != 0) {
                                result = true;
                                break;
                            }
                        }
                        return result;
                    };
                    siteItem.HasOverrides = ko.observable(false);


                    siteItem.bindEmailTemplate = function () {
                        var siteIds = [siteItem.SiteId];
                        commonShowEmailModal(siteIds);
                        return true;
                    }
                });
            }

            var commonShowEmailModal = function (siteIds) {
                var errorCount = self.fieldErrorCount();

                if (errorCount != 0) {
                    bootbox.alert('<br /><div class="font125pc text-danger"><i class="fa fa-warning fa-2x"></i> There are ' + errorCount + ' invalid prices. <br /><br />Please review and correct the Overrides before attempting to send an email.</div>')
                    return true;
                }

                if (self.HasUnsavedChanges()) {
                    bootbox.alert('<br /><div class="font125pc"><i class="fa fa-warning fa-2x"></i> There are unsaved changes. <br /><br />Please save the Overrides before sending the site emails.</div>');
                    return true;
                }

                // find 1st site by id
                var sitesVm = ko.utils.unwrapObservable(self.dataModel);
                var sites = sitesVm.sites;
                var siteItem = _.filter(sites, function (siteItem) {
                    return siteItem.SiteId == siteIds[0];
                })[0];

                var unleadedPrice = getFuelDisplayById(siteItem.FuelPricesToDisplay, FUEL_UNLEADED); // unleaded = 2
                var dieselprice = getFuelDisplayById(siteItem.FuelPricesToDisplay, FUEL_DIESEL); // diesel = 6
                var superprice = getFuelDisplayById(siteItem.FuelPricesToDisplay, FUEL_SUPER_UNLEADED); // superunleaded = 1 , lpg = 7

                var params = {
                    siteIdList: siteIds,
                    siteName: siteItem.StoreName,
                    siteId: siteItem.SiteId,
                    dayMonthYear: dmyStringToDMonYString(self.InitDate()),
                    prices: {
                        unleaded: (!unleadedPrice.Fuel) ? 'n/a' : formatNumberTo1DecimalPlace(getOverrideOrAutoPrice(unleadedPrice.Fuel.FuelPrice.OverridePrice(), unleadedPrice.Fuel.FuelPrice.AutoPrice)),
                        diesel: (!dieselprice.Fuel) ? 'n/a' : formatNumberTo1DecimalPlace(getOverrideOrAutoPrice(dieselprice.Fuel.FuelPrice.OverridePrice(), dieselprice.Fuel.FuelPrice.AutoPrice)),
                        superUnleaded: (!superprice.Fuel) ? 'n/a' : formatNumberTo1DecimalPlace(getOverrideOrAutoPrice(superprice.Fuel.FuelPrice.OverridePrice(), superprice.Fuel.FuelPrice.AutoPrice))
                    },
                    isViewingHistorical: self.ViewingHistorical(),
                    hasUnsavedChanges: self.HasUnsavedChanges(),
                    send: function (emailTemplateId, siteIds) {
                        var url = 'Sites/SendEmailToSite?emailTemplateId=' + emailTemplateId + '&siteIdsList=' + siteIds;
                        var $promise = common.callService("get", url, null);
                        self.sendEmail($promise, null, null);
                    }
                };
                siteEmailPopup.openPopup(params);
            };

            var buildPriceDifferences = function () {
                var sitesVm = ko.utils.unwrapObservable(self.dataModel);
                var sites = sitesVm.sites;
                _.each(sites, function (siteItem) {
                    for (var i = 0; i < siteItem.FuelPrices.length; i++) {
                        var currentFuel = siteItem.FuelPrices[i];

                        if (currentFuel.FuelTypeId == 2) {
                            var difference = (currentFuel.AutoPrice - currentFuel.TodayPrice) / 10;
                            if (currentFuel.AutoPrice == 0 || currentFuel.TodayPrice == 0) difference = "n/a";
                            //if (difference != 999) {
                            var _item = ko.utils.arrayFirst(self.PriceDifferences(), function (item) {
                                return item.key == difference;
                            });

                            if (_item == null) {
                                self.PriceDifferences.push({
                                    key: difference, value: 1
                                });
                            }
                            else {
                                _item = ko.utils.arrayFirst(self.PriceDifferences(), function (item) {
                                    return item.key == difference;
                                });

                                _item.value++;
                                self.PriceDifferences.remove(function (item) {
                                    return item.key == difference;
                                });
                                self.PriceDifferences.push({
                                    key: difference, value: _item.value
                                });
                            }
                            break;
                        }
                    }
                });

                self.PriceDifferences.sort(function (item1, item2) {
                    return item1.key > item2.key;
                });
            }

            var getOverrideOrAutoPrice = function (overridePrice, autoPrice) {
                if (isNaN(overridePrice) || overridePrice == '') {
                    if (isNaN(autoPrice) || autoPrice == '') {
                        return 0;
                    } else {
                        return autoPrice;
                    }
                } else {
                    return overridePrice;
                }
            }

            var getFuelPricesToDisplay = function (siteItem, siteFuelPrices) {
                var returnValue = [];
                var siteFuels = siteFuelPrices,
                    hasOverrides = false;

                _.each(fuelsToDisplay, function (id) {
                    var fuelToDisplay = _.find(siteFuels, function (item) {
                        return (item.FuelTypeId == id);
                    });
                    if (fuelToDisplay) {
                        var f = {
                            siteItem: siteItem,
                            siteSupportsFuel: true,
                            FuelPrice: self.getFormattedObservableFuelPrice(fuelToDisplay), // FuelPrice: observable changes on OverridePrice input
                            FuelTypeId: fuelToDisplay.FuelTypeId,
                            FuelTypeName: fuelTypeNames[fuelToDisplay.FuelTypeId],
                            hasValidValue: ko.observable(true),
                            isEditingFuel: ko.observable(false),
                            isMatchedByPriceMovementFilter: ko.observable(false),
                            DriveTime: fuelToDisplay.DriveTime.toFixed(2),
                            DriveTimePence: fuelsToDisplay.DriveTimePence,
                            Distance: fuelToDisplay.Distance.toFixed(2),
                            PriceSource: fuelToDisplay.PriceSource,
                            PriceSourceDateTime: fuelToDisplay.PriceSourceDateTime,
                            hasNearbyGrocers: function () {
                                switch (f.FuelTypeId) {
                                    case FUEL_SUPER_UNLEADED:
                                        return siteItem.HasNearbySuperUnleadedGrocers;
                                    case FUEL_UNLEADED:
                                        return siteItem.HasNearbyUnleadedGrocers;
                                    case FUEL_DIESEL:
                                        return siteItem.HasNearbyDieselGrocers;
                                }
                                return false;
                            },
                            hasNearbyGrocersPriceData: function () {
                                switch (f.FuelTypeId) {
                                    case FUEL_SUPER_UNLEADED:
                                        return siteItem.HasNearbySuperUnleadedGrocersPriceData;
                                    case FUEL_UNLEADED:
                                        return siteItem.HasNearbyUnleadedGrocersPriceData;
                                    case FUEL_DIESEL:
                                        return siteItem.HasNearbyDieselGrocersPriceData;
                                }
                                return false;
                            },
                            nearbyGrocersHtml: function () {
                                var message,
                                    tokens = {
                                        '{DRIVETIME}': pricingSettings.maxGrocerDriveTimeMinutes
                                    };

                                if (f.hasNearbyGrocers()) {
                                    if (f.hasNearbyGrocersPriceData())
                                        message = markupFormats.nearbyGrocersAndPrices;
                                    else
                                        message = markupFormats.nearbyGrocersWithoutPrices;
                                }
                                else
                                    message = markupFormats.noNearbyGrocers;

                                return replaceTokens(message, tokens);
                            },

                            isDriveTimeBoundary: function () {
                                switch (fuelToDisplay.FuelTypeId) {
                                    case FUEL_SUPER_UNLEADED:
                                        return siteItem.isDriveTimeBoundarySuperUnleaded;
                                    case FUEL_UNLEADED:
                                        return siteItem.isDriveTimeBoundaryUnleaded;
                                    case FUEL_DIESEL:
                                        return siteItem.isDriveTimeBoundaryDiesel;
                                }
                            },
                            focused: function (obj, newValue) {
                                var siteId = obj.siteItem.SiteId;
                                delayedBlurs.focusSite(siteId);
                                obj.siteItem.isEditing(true);
                                f.isEditingFuel(true);
                            },
                            blurred: function (obj, newValue) {
                                var input = $(newValue.currentTarget)
                                value = input.val();

                                if (isNumber(value)) {
                                    input.val(formatNumberTo1DecimalPlace(value));
                                }

                                var siteId = obj.siteItem.SiteId,
                                    fn = function () {
                                        obj.siteItem.isEditing(false);
                                    };
                                f.isEditingFuel(false);

                                // deal with SuperUnleaded = Unleaded + Markup
                                if (obj.FuelTypeId == FUEL_UNLEADED) {
                                    if (isNumber(value)) {
                                        var superUnleaded = obj.siteItem.FuelPricesToDisplay[2],
                                            superUnleadedPrice = formatNumberTo1DecimalPlace(Number(value) + pricingSettings.superUnleadedMarkupPrice);
                                        if (value >= fuelOverrideLimits[FUEL_UNLEADED].absolute.min) {
                                            setOverridePriceForSiteFuel(superUnleadedPrice, f.siteItem, FUEL_SUPER_UNLEADED);
                                            notify.info('Updated Super-Unleaded Price to Unleaded +' + pricingSettings.superUnleadedMarkupPrice);
                                        }
                                    }
                                }

                                recalculateSainsburysStats();

                                delayedBlurs.blurSite(siteId, fn);
                            },
                            // Up = 1, down=-1, same=0, today or calc missing ="": returns computed values(1, -1, 0, "")
                            changeProp: function (obj, newValue) {
                                var newPrice = $(newValue.target).val();
                                obj.hasValidValue(self.validateOverridePrice(newPrice, siteItem.SiteId, fuelToDisplay.FuelTypeId, f.FuelPrice));
                                if (obj.hasValidValue()) {
                                    var fuelPrice = obj.FuelPrice;

                                    obj.FuelPrice.OverridePrice(newPrice);

                                    finalisePriceChange(obj, fuelPrice.TodayPrice, newPrice, fuelPrice.AutoPrice, 1);

                                    // crazy need for this?
                                    f.UpDownText(getFormattedUpDownValue(f.UpDownValue(), '-'));
                                    fuelPrice.UpDownInfoTip(self.getUpDownInfoTip(f));

                                    return true;
                                } else {
                                    obj.UpDownValue('0');
                                    self.HasValidationErrors(true);
                                    self.HasUnsavedChanges(false);
                                    return true; //false;
                                }
                            }, // end changeProp
                            setPlusMinusPpl: function (fuelObj, pplValue) {
                                var fuelPrice = fuelObj.FuelPrice;

                                var todayPrice = fuelObj.FuelPrice.TodayPrice;
                                //var autoPrice = fuelObj.FuelPrice.AutoPrice;

                                var calculatedPrice = 0;
                                if (todayPrice > 0) {
                                    calculatedPrice = +todayPrice + +pplValue;
                                    // limit to 1 decimal place
                                    calculatedPrice = parseFloat(Math.round(calculatedPrice * 10) / 10).toFixed(1);

                                    fuelObj.FuelPrice.OverridePrice(calculatedPrice);
                                    finalisePriceChange(fuelObj, fuelPrice.TodayPrice, calculatedPrice, fuelPrice.todayPrice, 1);

                                    fuelObj.hasValidValue(true);
                                    if (calculatedPrice <= 0.00) {
                                        return false;
                                    }
                                }
                                return true;
                            }, // end setPlusMinusPpl
                            setOverrideAllPpl: function (fuelObj, pplValue) {
                                var fuelPrice = fuelObj.FuelPrice;

                                var calculatedPrice = pplValue;

                                fuelObj.FuelPrice.OverridePrice(calculatedPrice);

                                finalisePriceChange(fuelObj, fuelPrice.TodayPrice, calculatedPrice, fuelPrice.AutoPrice, 1);
                                fuelObj.hasValidValue(true);

                                // crazy need for this?
                                f.UpDownText(getFormattedUpDownValue(f.UpDownValue(), '-'));
                                fuelPrice.UpDownInfoTip(self.getUpDownInfoTip(f));

                            }, // end setPlusMinusPpl
                            clearOverridePpl: function (fuelObj) {
                                var fuelPrice = fuelObj.FuelPrice;
                                fuelObj.FuelPrice.OverridePrice('');

                                finalisePriceChange(fuelObj, fuelPrice.TodayPrice, 0, fuelPrice.AutoPrice, 1);
                                fuelObj.hasValidValue(true);
                            }, // end clearOverridePpl
                            OverrideInputCss: function () {
                                var isValid = f.hasValidValue(),
                                    isEmpty = f.FuelPrice.OverridePrice() == '',
                                    exceedsLimits = f.FuelPrice.isIncreaseOutsideLimits(),
                                    isWithinPriceChangeVariance = f.FuelPrice.isWithinPriceChangeVariance();

                                if (isEmpty)
                                    return 'override-empty';
                                else if (!isValid)
                                    return 'override-invalid';
                                else if (isWithinPriceChangeVariance)
                                    return 'override-valid override-within-price-change-variance';
                                else if (exceedsLimits)
                                    return 'override-increment-exceeds';
                                else
                                    return 'override-valid';
                            },
                            getOverridePrice: function () {
                                return f.FuelPrice.OverridePrice();
                            }
                        };

                        if (isNumber(f.FuelPrice.OverridePrice())) {
                            hasOverrides = true;
                        }

                        f.UpDownValue = ko.observable(getUpDownValue(f.FuelPrice.TodayPrice, f.FuelPrice.OverridePrice(), f.FuelPrice.AutoPrice, 1)); // this works fine

                        f.UpDownIconCss = ko.observable(getUpDownIconCss(f.UpDownValue()));

                        f.UpDownText = ko.observable(getFormattedUpDownValue(f.UpDownValue(), '-'));

                        f.UpDownSignCss = ko.observable(getUpDownSignCss(f.UpDownValue()));

                        f.FuelPrice.ForecourtAutoPriceHtml = ko.observable(getFormattedForecourtPriceHtml(f.FuelPrice.AutoPrice));

                        f.FuelPrice.ForecourtTodayPriceHtml = ko.observable(getFormattedForecourtPriceHtml(f.FuelPrice.TodayPrice));

                        f.FuelPrice.ForecourtYestPriceHtml = ko.observable(getFormattedForecourtPriceHtml(f.FuelPrice.YestPrice));

                        f.FuelPrice.PriceOverrideInfoTip = function () {
                            var limits = fuelOverrideLimits[f.FuelTypeId],
                                tokens = {
                                    '{NAME}': f.FuelTypeName,
                                    '{INCMIN}': formatNumber1DecimalWithPlusMinus(limits.change.min),
                                    '{INCMAX}': formatNumber1DecimalWithPlusMinus(limits.change.max),
                                    '{ABSMIN}': formatNumber1DecimalWithPlusMinus(limits.absolute.min),
                                    '{ABSMAX}': formatNumber1DecimalWithPlusMinus(limits.absolute.max),
                                    '{VARIANCE}': pricingSettings.priceChangeVarianceThreshold.toFixed(1)
                                };

                            if (!f.hasValidValue())
                                return replaceTokens(messageFormats.priceOverrideInvalidValueInfoTip, tokens);
                            else if (f.FuelPrice.isWithinPriceChangeVariance())
                                return replaceTokens(messageFormats.withinPriceChangeVarianceInfoTip, tokens);
                            else if (f.FuelPrice.isIncreaseOutsideLimits())
                                return replaceTokens(messageFormats.priceOverrideIncreaseExceedsInfoTip, tokens)
                            else
                                return replaceTokens(messageFormats.priceOverrideInfoTip, tokens);
                        };

                        f.FuelPrice.CurrentPriceInfoTip = function () {
                            var tokens = {
                                '{NAME}': f.FuelTypeName
                            };
                            return replaceTokens(messageFormats.currentPriceInfoTip, tokens)
                        };

                        f.FuelPrice.UpDownInfoTip = ko.observable(function () {
                            return self.getUpDownInfoTip(f);
                        });

                        f.FuelPrice.TrialPriceInfoTip = function () {
                            var tokens = {
                                '{NAME}': f.FuelPrice.CompetitorName,
                                '{MARKUP}': formatNumber1DecimalWithPlusMinus(f.FuelPrice.Markup),
                                '{DISTANCE}': f.Distance,
                                '{DRIVETIME}': f.DriveTime,
                                '{PRICESOURCE}': formatPriceSource(f.PriceSource),
                                '{PRICESOURCEDATETIME}': formatPriceSourceDateTime(f.PriceSourceDateTime)
                            };
                            return f.FuelPrice.AutoPrice == 'n/a'
                                ? replaceTokens(messageFormats.priceIsNotAvailableForTrialPrice, tokens)
                                : replaceTokens(messageFormats.trialPriceInfoTip, tokens);
                        };

                        f.FuelPrice.MatchCompetitorInfoTip = function () {
                            var tokens = {
                                '{NAME}': f.FuelPrice.CompetitorName,
                                '{MARKUP}': formatNumber1DecimalWithPlusMinus(f.FuelPrice.Markup.toFixed(1)),
                                '{DISTANCE}': f.Distance,
                                '{DRIVETIME}': f.DriveTime,
                                '{PRICESOURCE}': formatPriceSource(f.PriceSource),
                                '{PRICESOURCEDATETIME}': formatPriceSourceDateTime(f.PriceSourceDateTime)
                            };
                            return f.FuelPrice.AutoPrice == 'n/a'
                                ? replaceTokens(messageFormats.priceIsNotAvailableForMatchCompetitor, tokens)
                                : replaceTokens(messageFormats.matchCompetitorInfoTip, tokens);
                        };

                        f.FuelPrice.SoloPriceInfoTip = function () {
                            var competitorName = '' + f.FuelPrice.CompetitorName,
                                tokens = {
                                    '{NAME}': siteItem.StoreName,
                                    '{COMPETITOR}': competitorName,
                                    '{DISTANCE}': f.Distance,
                                    '{DRIVETIME}': f.DriveTime,
                                    '{PRICESOURCE}': formatPriceSource(f.PriceSource),
                                    '{PRICESOURCEDATETIME}': formatPriceSourceDateTime(f.PriceSourceDateTime)
                                };
                            if (f.FuelPrice.AutoPrice == 'n/a')
                                replaceTokens(messageFormats.priceIsNotAvailableForSoloPrice, tokens)

                            if (competitorName == '')
                                return replaceTokens(messageFormats.soloPriceInfoTip, tokens);
                            else
                                return replaceTokens(messageFormats.soloPriceBasedOnCompetitorInfoTip, tokens);
                        };

                        f.FuelPrice.TodayPriceCss = ko.observable(function () {
                            switch (siteItem.PriceMatchType) {
                                case 1:
                                    return 'solo-price';
                                case 2:
                                    return 'trial-price';
                                case 3:
                                    return 'match-competitor-price';
                                default:
                                    return 'solo-price';
                            }
                        });

                        f.FuelPrice.TodayPriceTitle = ko.observable(function () {
                            switch (siteItem.PriceMatchType) {
                                case 1:
                                    return 'Solo Price ';
                                case 2:
                                    return 'Trial price: ' + f.FuelPrice.CompetitorName;
                                case 3:
                                    return 'Competitor: ' + f.FuelPrice.CompetitorName + '; Markup: ' + f.FuelPrice.Markup
                            }
                        });

                        f.FuelPrice.CompTodayPriceInfotip = function () {
                            var tokens = {
                                '{NAME}': f.FuelTypeName,
                                '{VALUE}': f.FuelPrice.TodayPrice
                            };
                            return replaceTokens(messageFormats.compTodayPriceInfotip, tokens);
                        };

                        f.FuelPrice.YestPriceInfotip = function () {
                            var tokens = {
                                '{VALUE}': f.FuelPrice.YestPrice,
                                '{NAME}': f.FuelTypeName
                            };
                            return replaceTokens(messageFormats.yestPriceInfoTip, tokens);
                        };



                        returnValue.push(f);
                    } else {
                        returnValue.push({
                            isEditing: false,
                            siteSupportsFuel: false,
                            FuelPrice: null,
                            FuelTypeId: -1,
                            UpDownValue: ko.observable('')
                        });
                    }
                });

                // detect historical Overrides
                if ('HasOverrides' in siteItem)
                    siteItem.HasOverrides(hasOverrides);

                return returnValue; // always contains 3 items only (as per length of fuelsToDisplay array
            };

            self.getUpDownInfoTip = function (f) {
                var value = f.UpDownValue(),
                    sign = getUpDownValueSign(value),
                    tokens = {
                        '{VALUE}': (Math.abs(value) / 10).toFixed(1),
                        '{NAME}': f.FuelTypeName
                    };
                return replaceTokens(messageFormats.updownInfoTips[sign], tokens);
            };

            self.validateOverridePrice = function (priceValue, siteId, fuelId, fuelPrice) {
                var limits = fuelOverrideLimits[fuelId],
                    hasAutoPrice = fuelPrice.AutoPrice > 0 && !isNaN(fuelPrice.AutoPrice),
                    hasTodayPrice = fuelPrice.TodayPrice > 0 && !isNaN(fuelPrice.TodayPrice),
                    hasOverridePrice = priceValue >= limits.absolute.min && priceValue <= limits.absolute.max,
                    diff,
                    increaseExceedsLimits = false,
                    isWithinPriceChangeVariance = false;

                // first check Override price vs AutoPrice (if any)
                if (hasAutoPrice && hasOverridePrice) {
                    diff = priceValue - fuelPrice.AutoPrice;
                    if (diff < limits.change.min || diff > limits.change.max)
                        increaseExceedsLimits = true;
                    isWithinPriceChangeVariance = Math.abs(diff) <= pricingSettings.priceChangeVarianceThreshold;
                } else if (hasTodayPrice && hasOverridePrice) {
                    // then check Override price vs TodayPrice (if any)
                    diff = priceValue - fuelPrice.TodayPrice;
                    if (diff < limits.change.min || diff > limits.change.max)
                        increaseExceedsLimits = true;
                    isWithinPriceChangeVariance = Math.abs(diff) <= pricingSettings.priceChangeVarianceThreshold;
                }

                fuelPrice.isIncreaseOutsideLimits(increaseExceedsLimits);
                fuelPrice.isWithinPriceChangeVariance(isWithinPriceChangeVariance);

                if (priceValue != '' && (priceValue < limits.absolute.min || priceValue > limits.absolute.max || isNaN(priceValue))) {
                    fieldErrors.add(siteId, fuelId);
                    self.hasAnyFieldErrors(fieldErrors.hasAny());
                    self.fieldErrorCount(fieldErrors.count());
                    $("#msgError").html("Invalid price value: " + priceValue + ' - Please enter a value between ' + formatNumber1DecimalWithPlusMinus(limits.absolute.min) + ' and ' + formatNumber1DecimalWithPlusMinus(limits.absolute.max));
                    return false;
                } else {
                    fieldErrors.remove(siteId, fuelId);
                    self.hasAnyFieldErrors(fieldErrors.hasAny());
                    self.fieldErrorCount(fieldErrors.count());
                    $("#msgError").html("");
                    return true;
                }
            }

            var findFuelPriceById = function (fuelPrices, id) {
                var fuelToFind = _.find(fuelPrices, function (item) {
                    return (item.FuelTypeId == id);
                });
                return fuelToFind;
            }
            // Notes to figure this out..
            // 1. 0 and '' values on either side mean price has not changed
            // 2. Non zero and non-blank values need to be compared.. and if unequal return true..
            // displayPrice is the one that could be blank or > 0 value.. (wonder what '' * 10 gives)
            // fuelPrice would be 0 or > 0
            var hasOverridePriceChanged = function (siteItem, fuelTypeId) {
                var fuelPrice = findFuelPriceById(siteItem.FuelPrices, fuelTypeId);
                var fuelDisplay = findFuelPriceById(siteItem.FuelPricesToDisplay, fuelTypeId);
                var todayPrice = fuelDisplay.FuelPrice.TodayPrice,
                    autoPrice = fuelDisplay.FuelPrice.AutoPrice,
                    overridePrice = fuelDisplay.FuelPrice.OverridePrice(),
                    tomorrowPrice,
                    diff;

                if (isNumber(todayPrice) && isNumber(overridePrice)) {
                    tomorrowPrice = overridePrice;
                    diff = tomorrowPrice - todayPrice;
                }
                else if (isNumber(todayPrice) && isNumber(autoPrice)) {
                    tomorrowPrice = autoPrice;
                    diff = tomorrowPrice - todayPrice;
                }
                else
                    diff = undefined;

                // ignore Override if withn PriceChangeVariance
                if (isNumber(diff) && Math.abs(diff) <= pricingSettings.priceChangeVarianceThreshold)
                    return false;

                if (fuelPrice.OverridePrice != fuelDisplay.FuelPrice.OverridePrice() * 10)
                    return true;
                else
                    return false;
            };

            var getFuelDisplayById = function (siteFuelPricesToDisplay, fuelTypeId) {
                var fuelToFind = _.find(siteFuelPricesToDisplay, function (item) {
                    return (item.FuelTypeId == fuelTypeId);
                });
                if (fuelToFind) {
                    return {
                        Fuel: fuelToFind
                    };
                } else {
                    return {
                        fuelToFind: null
                    };
                }
            };

            var getUpDownValue = function (todayPrice, overridePrice, autoPrice, divisor) {
                var chg = 0;
                if (!isNumber(todayPrice) || !isNumber(autoPrice))
                    return 0;

                chg = isNumber(overridePrice)
                    ? overridePrice - todayPrice
                    : autoPrice - todayPrice;
                chg = chg * 10;
                chg = self.formatValueTo0DP(chg.toString(10), "-", divisor, 0);
                return chg;
            }

            var getUpDownValueSign = function (upDownValue) {
                if (!upDownValue || upDownValue == 0 || upDownValue == '-')
                    return 0;
                return upDownValue <= 0.1 ? -1 : +1;
            }

            var getUpDownIconCss = function (upDownValue) {
                switch (getUpDownValueSign(upDownValue)) {
                    case -1:
                        return 'fa fa-arrow-down price-diff-down';
                    case 0:
                        return 'fa fa-arrow-right price-diff-none';
                    case 1:
                        return 'fa fa-arrow-up price-diff-up';
                }
            };

            var getFormattedUpDownValue = function (upDownValue, zeroString) {
                switch (getUpDownValueSign(upDownValue)) {
                    case -1:
                        return '-' + (Math.abs(upDownValue) / 10).toFixed(1);
                    case 0:
                        return zeroString;
                    case +1:
                        return '+' + (Math.abs(upDownValue) / 10).toFixed(1);
                }
            };

            var getFormattedForecourtPriceHtml = function (autoPrice) {
                if (autoPrice.toLowerCase() == "n/a")
                    return '<span class="price-na">n/a</span>';

                var parts = (autoPrice.toString() + '.').split('.');
                return '<span class="price-pence">' + parts[0] + '</span><span class="price-fraction">.' + parts[1] + '</span>';
            };

            var getUpDownSignCss = function (upDownValue) {
                var sign = getUpDownValueSign(upDownValue);
                return sign == 0 ? 'none' : sign < 0 ? 'down' : 'up';
            };

            var getUpDownInfoTip = function (upDownValue, fuelTypeName) {
                var sign = getUpDownValueSign(upDownValue),
                    tokens = {
                        '{VALUE}': upDownValue,
                        '{NAME}': fuelTypeName
                    };
                return replaceTokens(messageFormats.updownInfoTips[sign], tokens);
            };

            //
            // re-apply the price difference infotips - due to KO not updating correctly when overrides are modified;
            //
            self.applyPriceDiffInfoTipFix = function (fuelObj) {
                var realUpDownValue = getUpDownValue(fuelObj.FuelPrice.TodayPrice, fuelObj.FuelPrice.OverridePrice(), fuelObj.FuelPrice.AutoPrice, 1);

                var sign = getUpDownValueSign(realUpDownValue),
                    tokens = {
                        '{VALUE}': (Math.abs(realUpDownValue) / 10).toFixed(1),
                        '{NAME}': fuelObj.FuelTypeName
                    };

                self.detectAnyOverrides = function (fuelObj) {
                    var hasOverrides = false,
                        fuelsToDisplay = fuelObj.siteItem.FuelPricesToDisplay,
                        fuel,
                        i;
                    for (i = 0; i < fuelsToDisplay.length; i++) {
                        fuel = fuelsToDisplay[i];
                        if (isNumber(fuel.FuelPrice.OverridePrice())) {
                            hasOverrides = true;
                            break;
                        }
                    }
                    fuelObj.siteItem.HasOverrides(hasOverrides);
                };

                var infotip = replaceTokens(messageFormats.updownInfoTips[sign], tokens);
                fuelObj.UpDownText(getFormattedUpDownValue(realUpDownValue, '-'));
                fuelObj.FuelPrice.UpDownInfoTip(infotip);
                fuelObj.UpDownIconCss(getUpDownIconCss(realUpDownValue));
            };

            var setFuelPriceStatuses = function (fuelObj) {
                var limits = fuelOverrideLimits[fuelObj.FuelTypeId],
                    overrideValue = fuelObj.FuelPrice.OverridePrice(),
                    hasAutoPrice = fuelObj.FuelPrice.AutoPrice > 0 && !isNaN(fuelObj.FuelPrice.AutoPrice),
                    hasTodayPrice = fuelObj.FuelPrice.TodayPrice > 0 && !isNaN(fuelObj.FuelPrice.TodayPrice),
                    hasOverridePrice = overrideValue >= limits.absolute.min && overrideValue <= limits.absolute.max,
                    diff,
                    increaseExceedsLimits = false,
                    isWithinPriceChangeVariance = false,
                    tomorrowPrice;

                if (hasOverridePrice)
                    tomorrowPrice = overrideValue
                else if (hasAutoPrice)
                    tomorrowPrice = fuelObj.FuelPrice.AutoPrice;
                else
                    tomorrowPrice = undefined;

                if (hasTodayPrice && isNumber(tomorrowPrice)) {
                    diff = tomorrowPrice - fuelObj.FuelPrice.TodayPrice;
                    isWithinPriceChangeVariance = Math.abs(diff) <= pricingSettings.priceChangeVarianceThreshold;
                    if (diff < limits.change.min || diff > limits.change.max)
                        increaseExceedsLimits = true;
                }

                fuelObj.FuelPrice.isIncreaseOutsideLimits(increaseExceedsLimits);
                fuelObj.FuelPrice.isWithinPriceChangeVariance(isWithinPriceChangeVariance);
            };

            var finalisePriceChange = function (fuelObj, todayPrice, overridePrice, autoPrice, divisor) {
                var updown = getUpDownValue(todayPrice, overridePrice, autoPrice, divisor);

                fuelObj.UpDownValue(updown);

                setFuelPriceStatuses(fuelObj);

                self.HasUnsavedChanges(false);

                self.applyPriceDiffInfoTipFix(fuelObj);

                self.detectAnyOverrides(fuelObj);

                // speed up - don't serialise while performing mass Fuel Price Override
                if (isApplyingMassFuelPriceOverride == false) {
                    detectUnsavedChanges();
                }

                self.HasValidationErrors(false);


            };

            // NOTE: this is an expensive operation - it serialises ALL sites for ALL fuels and detect tomorrow Override price changes
            function detectUnsavedChanges() {
                var postback = serialisePostbackData(0);
                var hasChanges = (postback != null && postback.length > 0);
                self.HasUnsavedChanges(hasChanges);
            };

            self.getFormattedObservableFuelPrice = function (fuelPriceToDisplay) {
                return {
                    FuelTypeId: fuelPriceToDisplay.FuelTypeId,
                    YestPrice: self.formatValueTo1DP(fuelPriceToDisplay.YestPrice, "n/a"),
                    TodayPrice: self.formatValueTo1DP(fuelPriceToDisplay.TodayPrice, "n/a"),
                    Difference: fuelPriceToDisplay.Difference ? fuelPriceToDisplay.Difference : "-",
                    AutoPrice: self.formatValueTo1DP(fuelPriceToDisplay.AutoPrice, "n/a"),
                    CompetitorName: fuelPriceToDisplay.CompetitorName,
                    Markup: fuelPriceToDisplay.Markup,
                    IsTrailPrice: fuelPriceToDisplay.IsTrailPrice,
                    // OBSERVABLE as its tied to user input
                    OverridePrice: ko.observable(self.formatValueTo1DP(fuelPriceToDisplay.OverridePrice, '')),
                    isIncreaseOutsideLimits: ko.observable(false),
                    isWithinPriceChangeVariance: ko.observable(false)
                };
            }

            self.formatValueTo1DP = function (priceValue, replacementForZero, divisor) {
                if (!divisor) divisor = 10;
                if (priceValue >= 0 && priceValue <= 0.0001) return replacementForZero; // safe comparison for zero value
                else return parseFloat(Math.round((priceValue / divisor) * 100) / 100).toFixed(1); // Number formatting to 1dp
            }

            self.formatValueTo0DP = function (priceValue, replacementForZero, divisor) {
                if (!divisor) divisor = 10;
                if (priceValue >= 0 && priceValue <= 0.0001) return replacementForZero; // safe comparison for zero value
                else return parseFloat(Math.round((priceValue / divisor) * 100) / 100).toFixed(0); // Number formatting to 1dp
            }

            self.resetPriceHighlighting = function () {
                self.highlightTrialPrices(false);
                self.highlightMatchCompetitors(false);
                self.highlightSoloPrices(false);
                self.highlightNoNearbyGrocerPrices(false);
                self.highlightHasNearbyGrocerPrices(false);
                self.highlightHasNearbyGrocerWithOutPrices(false);
                self.redrawGridHighlights();
                notify.info('Removed Price Highlighting');
                cookieSettings.writeBoolean('pricing.highlightTrialPrices', false);
                cookieSettings.writeBoolean('pricing.highlightMatchCompetitors', false);
                cookieSettings.writeBoolean('pricing.highlightSoloPrices', false);
                cookieSettings.writeBoolean('pricing.highlightNoNearbyGrocerPrices', false);
                cookieSettings.writeBoolean('pricing.highlightHasNearbyGrocerPrices', false);
                cookieSettings.writeBoolean('pricing.highlightHasNearbyGrocerWithOutPrices', false);
            };

            self.toggleHighlightTrialPrices = function () {
                var enabled = !self.highlightTrialPrices(),
                    message = enabled ? 'Highlighting ' + self.TrialPricesCount() + ' Trial Prices' : '';
                self.highlightTrialPrices(enabled);
                self.redrawGridHighlights();
                notify.info(message);
                cookieSettings.writeBoolean('pricing.highlightTrialPrices', enabled);
            };

            self.toggleHighlightMatchCompetitors = function () {
                var enabled = !self.highlightMatchCompetitors(),
                    message = enabled ? 'Highlighting ' + self.MatchCompetitorsCount() + ' Competitor Matches' : '';
                self.highlightMatchCompetitors(enabled);
                self.redrawGridHighlights();
                notify.info(message);
                cookieSettings.writeBoolean('pricing.highlightMatchCompetitors', enabled);
            };

            self.toggleHighlightSoloPrices = function () {
                var enabled = !self.highlightSoloPrices(),
                    message = enabled ? 'Highlighting ' + self.SoloPricesCount() + ' Solo Prices' : '';
                self.highlightSoloPrices(enabled);
                self.redrawGridHighlights();
                notify.info(message);
                cookieSettings.writeBoolean('pricing.highlightSoloPrices', enabled);
            };

            self.toggleHighlightNoNearbyGrocerPrices = function () {
                var enabled = !self.highlightNoNearbyGrocerPrices(),
                    message = enabled ? 'Highlighting ' + self.NoNearbyGrocersCount() + ' missing Nearby Grocer Prices' : '';
                self.highlightNoNearbyGrocerPrices(enabled);
                self.redrawGridHighlights();
                notify.info(message);
                cookieSettings.writeBoolean('pricing.highlightNoNearbyGrocerPrices', enabled);
            };

            self.toggleHighlightHasNearbyGrocerPrices = function () {
                var enabled = !self.highlightHasNearbyGrocerPrices(),
                    message = enabled ? 'Highlighting ' + self.NearbyGrocersAndPricesCount() + ' Sites with Nearby Grocer Prices' : '';
                self.highlightHasNearbyGrocerPrices(enabled);
                self.redrawGridHighlights();
                notify.info(message);
                cookieSettings.writeBoolean('pricing.highlightHasNearbyGrocerPrices', enabled);
            };

            self.toggleHighlightHasNearbyGrocerWithOutPrices = function () {
                var enabled = !self.highlightHasNearbyGrocerWithOutPrices(),
                    message = enabled ? 'Highlighting ' + self.NearbyGrocersWithoutPricesCount() + ' Sites with Nearby Grocer Without Prices' : '';
                self.highlightHasNearbyGrocerWithOutPrices(enabled);
                self.redrawGridHighlights();
                notify.info(message);
                cookieSettings.writeBoolean('pricing.highlightHasNearbyGrocerWithOutPrices', enabled);
            };

            self.redrawGridHighlights = function () {
                var grid = $('.pricing-grid'),
                    showTrialPrices = self.highlightTrialPrices(),
                    showSoloPrices = self.highlightSoloPrices(),
                    showMatchCompetitorPrices = self.highlightMatchCompetitors(),
                    showNoNearbyGrocerPrices = self.highlightNoNearbyGrocerPrices(),
                    showHasNearbyGrocerPrices = self.highlightHasNearbyGrocerPrices(),
                    showHasNearbyGrocerWithOutPrices = self.highlightHasNearbyGrocerWithOutPrices();
                grid[showTrialPrices ? 'addClass' : 'removeClass']('highlight-trial-prices');
                grid[showMatchCompetitorPrices ? 'addClass' : 'removeClass']('highlight-match-competitor-prices');
                grid[showSoloPrices ? 'addClass' : 'removeClass']('highlight-solo-prices');
                grid[showNoNearbyGrocerPrices ? 'addClass' : 'removeClass']('highlight-no-nearby-grocer-prices');
                grid[showHasNearbyGrocerPrices ? 'addClass' : 'removeClass']('highlight-has-nearby-grocer-prices');
                grid[showHasNearbyGrocerWithOutPrices ? 'addClass' : 'removeClass']('highlight-has-nearby-grocer-with-out-prices');
            };

            self.commonTogglePriceChangeFilter = function (obj) {
                var toggled = !(obj.observer()),
                    message = toggled ? obj.showing : obj.hiding;
                obj.observer(toggled);
                self.redrawFuelPriceChanges();
                filterPriceChangeSiteRow();
                notify.info(message);
                if (obj.settingCookie)
                    cookieSettings.writeBoolean(obj.settingCookie, toggled);
            };

            self.togglePriceChangeFilterUp = function () {
                self.commonTogglePriceChangeFilter({
                    observer: self.priceChangeFilterUp,
                    showing: 'Showing upward price changes',
                    hiding: 'Hiding upward price changes',
                    settingCookie: 'pricing.priceChangeFilterUp'
                });
            };

            self.togglePriceChangeFilterNone = function () {
                self.commonTogglePriceChangeFilter({
                    observer: self.priceChangeFilterNone,
                    showing: 'Showing no price changes',
                    hiding: 'Hiding no price changes',
                    settingCookie: 'pricing.priceChangeFilterNone'
                });
            };

            self.togglePriceChangeFilterDown = function () {
                self.commonTogglePriceChangeFilter({
                    observer: self.priceChangeFilterDown,
                    showing: 'Showing downward price changes',
                    hiding: 'Hiding downward price changes',
                    settingCookie: 'pricing.priceChangeFilterDown'
                });
            };

            function clearAllOverridesForFuel(fuelTypeId) {
                var fuelName = fuelTypeNames[fuelTypeId];

                busyloader.show({
                    message: 'Removing Price Overrides for ' + fuelName,
                    showtime: 3000,
                    dull: true
                });

                self.savePetrolPricingForm(fuelTypeId);
            };

            function confirmRemoveAllOverridesForFuel(fuelTypeId) {
                var fuelName = fuelTypeNames[fuelTypeId];

                bootbox.confirm({
                    title: fuelName + " - Remove Price Overrides?",
                    message: '<p class="font125pc">Are you sure you want to remove all the <strong>Price Overrides</strong> for <strong>' + fuelName + ' ?</strong></big></p/>'
                            + '<br />'
                            + '<p class="text-danger text-center"><strong>NOTE:</strong> - this will refresh the page and submit other Override changes for other fuels.</p>',
                    buttons: {
                        confirm: {
                            label: '<i class="fa fa-check"></i> Yes',
                            className: 'btn-success'
                        },
                        cancel: {
                            label: '<i class="fa fa-times"></i> No',
                            className: 'btn-danger'
                        }
                    },
                    callback: function (result) {
                        if (result) {
                            clearAllOverridesForFuel(fuelTypeId);
                        }
                    }
                });
            };

            self.removeAllOverrideUnleaded = function () {
                confirmRemoveAllOverridesForFuel(FUEL_UNLEADED);
            };

            self.removeAllOverrideDiesel = function () {
                confirmRemoveAllOverridesForFuel(FUEL_DIESEL);
            };

            self.removeAllOverrideSuperUnleaded = function () {
                confirmRemoveAllOverridesForFuel(FUEL_SUPER_UNLEADED);
            };

            self.resetPriceChangeFilters = function () {
                self.priceChangeFilterDown(true);
                self.priceChangeFilterNone(true);
                self.priceChangeFilterUp(true);
                self.redrawFuelPriceChanges();
                filterPriceChangeSiteRow();
                notify.info('Price Change filters have been reset.')
                cookieSettings.writeBoolean('pricing.priceChangeFilterDown', true);
                cookieSettings.writeBoolean('pricing.priceChangeFilterNone', true);
                cookieSettings.writeBoolean('pricing.priceChangeFilterUp', true);
            };

            self.redrawFuelPriceChanges = function () {
                var grid = $('.pricing-grid'),
                    showUp = self.priceChangeFilterUp(),
                    showDown = self.priceChangeFilterDown(),
                    showNone = self.priceChangeFilterNone();
                grid[showUp ? 'removeClass' : 'addClass']('hide-fuel-price-change-up');
                grid[showDown ? 'removeClass' : 'addClass']('hide-fuel-price-change-down');
                grid[showNone ? 'removeClass' : 'addClass']('hide-fuel-price-change-none');
            };

            self.priceDiffBarChartFiltersCss = function () {
                var css = [
                    self.priceChangeFilterDown() ? 'show-price-diff-down' : 'hide-price-diff-down',
                    self.priceChangeFilterUp() ? 'show-price-diff-up' : 'hide-price-diff-up',
                    self.priceChangeFilterNone() ? 'show-price-diff-none' : 'hide-price-diff-none'
                ];

                return css.join(' ');
            };

            function filterPriceChangeSiteRow() {
                var rows = $('.actualdata .site-row'),
                    visibleCount = 0,
                    showUp = self.priceChangeFilterUp(),
                    showDown = self.priceChangeFilterDown(),
                    showNone = self.priceChangeFilterNone(),
                    emailFilter = self.siteEmailFilter();
                rows.each(function () {
                    var row = $(this),
                        signs = row.data('price-change-signs'),
                        visible,
                        hasRowErrors = row.hasClass('row-errors');
                    if (signs) {
                        visible = (signs.indexOf('Up') + 1 && showUp)
                            || (signs.indexOf('Down') + 1 && showDown)
                            || (signs.indexOf('None') + 1 && showNone);

                        if (visible || hasRowErrors) {
                            row.removeClass('hide');
                            visibleCount++;
                        } else {
                            row.addClass('hide');
                        }
                    }
                });
                self.hasAllSitesHidden(visibleCount == 0);
            };

            self.loadPageWithParams = function () {
                var url = common.getSiteRoot() + "Sites/SaveExcludeBrands?excludbrands=" + $('#lstbxexcludebrands').val();

                $.ajax({
                    type: "GET",
                    url: url,
                    data: "",
                    success: function (data) {
                        var yyyymmdd = dmyStringToYmdString(self.InitDate());

                        window.location.href = common.getSiteRoot() + "Sites/Prices?date=" + yyyymmdd
                        + "&storeName=" + escape(self.StoreName())
                        + "&catNo=" + escape(self.CatNo())
                        + "&storeNo=" + escape(self.StoreNo())
                        + "&storeTown=" + escape(self.StoreTown())
                        + "&priceChanges=" + escape(self.priceChangeFilter());
                    },
                    error: function (err) {
                        var yyyymmdd = dmyStringToYmdString(self.InitDate());

                        window.location.href = common.getSiteRoot() + "Sites/Prices?date=" + yyyymmdd
                        + "&storeName=" + escape(self.StoreName())
                        + "&catNo=" + escape(self.CatNo())
                        + "&storeNo=" + escape(self.StoreNo())
                        + "&storeTown=" + escape(self.StoreTown())
                        + "&priceChanges=" + escape(self.priceChangeFilter());
                    }
                });
            }

            self.reloadSearchPage = function (loadPage) {
                window.location.href = common.getSiteRoot() + loadPage;
                self.busyLoadingData(true);
            }

            self.searchSainsburysStores = function () {
                window.location.href = common.getSiteRoot() + "Sites?storeName=" + escape(self.StoreName())
                + "&catNo=" + escape(self.CatNo())
                + "&storeNo=" + escape(self.StoreNo())
                + "&storeTown=" + escape(self.StoreTown());
            }

            // ### GET SitePricing Data
            self.loadPageData = function (promise) {
                promise.done(function (serverData, textStatus, jqXhr) {
                    if (serverData == "Error") {
                        $('#msgError').html("Error - no data");
                        self.HasErrorMessage(true);
                        self.dataAvailable(false);
                    } else if (serverData.length > 0) {
                        self.dataLoading(true);
                        self.dataAvailable(true);
                        buildViewModels(serverData);
                        self.dataLoading(false);
                    } else {
                        //$('#msgError').html("No data found");
                        self.HasErrorMessage(false);
                        self.dataAvailable(false);
                    }
                    self.busyLoadingData(false);
                    $('.hide-first-load').removeClass('hide-first-load');
                    self.clearFirstPageLoad();
                    fieldErrors.removeAll();
                    self.fieldErrorCount(0);
                })
                .fail(function () {
                    $('#msgError').html("Error occured");
                    self.HasErrorMessage(true);
                    self.dataAvailable(false);
                    self.clearFirstPageLoad();
                });
                self.bind();
            };

            self.clearFirstPageLoad = function () {
                $('#SorryNoResultsPanel');
                self.firstPageLoad(false);
            };

            self.loadCompetitorDataForSite = function (site, promise, callback) {
                promise.done(function (serverData, textStatus, jqXhr) {
                    site.loadingCompetitors(false);
                    if (serverData == "Error") {
                        $('#msgError').html("Error - no data");
                        self.HasErrorMessage(true);
                        site.hasCompetitors(false);
                    } else {
                        attachCompetitorsToSite(site, serverData, callback); // attach the right competitor list to the + site clicked
                    }
                })
                    .fail(function () {
                        site.hasCompetitors(false);
                        site.loadingCompetitors(false);
                        $('#msgError').html("Error occured");
                        self.HasErrorMessage(true);
                    });
            }

            var serialisePostbackData = function (siteId, removeOverrideFuelTypeId) {
                var sitesVm = ko.utils.unwrapObservable(self.dataModel);
                var sites = sitesVm.sites;
                var retval = [];
                var removeThisPrice = false;
                if (siteId != 0)
                    sites = _.filter(sites, function (siteItem) {
                        return siteItem.SiteId == siteId;
                    });
                var priceHasChanged = false;
                _.each(sites, function (siteItem) {
                    _.each(siteItem.FuelPricesToDisplay, function (fuelDisplayItem) {
                        priceHasChanged = false;

                        // Use this to check if (override varies from saved values)
                        if (fuelDisplayItem.siteSupportsFuel) {
                            priceHasChanged = hasOverridePriceChanged(siteItem, fuelDisplayItem.FuelTypeId)
                        }

                        // remove all Price overrides for Fuel ?
                        if (removeOverrideFuelTypeId && fuelDisplayItem.FuelTypeId == removeOverrideFuelTypeId) {
                            removeThisPrice = true;
                        } else
                            removeThisPrice = false;

                        if (priceHasChanged || removeThisPrice) {
                            var ovrPrice = fuelDisplayItem.FuelPrice.OverridePrice() + ''; // convert price back to string
                            ovrPrice = ovrPrice.replace('   ', '');
                            ovrPrice = ovrPrice.trim();
                            if (ovrPrice == '') ovrPrice = 0.0;
                            if (removeThisPrice) ovrPrice = -1.0;
                            var postbackItem = {
                                SiteId: siteItem.SiteId,
                                FuelTypeId: fuelDisplayItem.FuelTypeId,
                                OverridePrice: ovrPrice
                            };
                            retval.push(postbackItem);
                        }
                    });
                });
                return retval;
            }

            // Serialize the data back to server from dataModel
            // ### POST SitePricing Form data back to server.
            self.savePetrolPricingForm = function (fuelTypeId) {
                // Simpler VM - SiteId, FuelId, OverridePrice (only ones which are non blank), for faster updates and avoid unnecessary ones
                var postbackData = serialisePostbackData(0, fuelTypeId);

                var messages = getMessages("Save");
                $.ajax({
                    url: common.getSiteRoot() + "Sites/SavePriceOverrides", // Put method for Update - Placeholder created
                    method: "POST",
                    data: {
                        postbackKey1: postbackData
                    }, // Postback a List<OverridePricePostViewModel>, {postbackKey1: etc must match Controller method param name}
                    contentType: 'application/x-www-form-urlencoded; charset=utf-8', // 'application/json' ,
                })
                    .done(function (response, textStatus, jqXhr) {
                        if (response.JsonStatusCode.CustomStatusCode == "ApiSuccess") {
                            $('#msgSuccess').html("<strong>" + messages.success + "</strong>");
                            self.HasSuccessMessage(true);
                            self.loadPageWithParams();
                        } else if (response.JsonStatusCode.CustomStatusCode == "ApiFail") {
                            window.alert(messages.failure);
                            $('#msgError').html("<strong>" + messages.failure + ": " + response.ErrorSummaryString + "</strong>");
                            self.HasErrorMessage(true);
                        } else { // UI validation errors
                            displayErrors(response.ModelErrors);
                        }
                        // msg success and redirect to another page if needed
                        // Define a standard message format for Post(aka Create) Response returned,
                    })
                    .fail(function (jqXhr, textStatus, errorThrown) {
                        // msg failure
                        notify.error(messages.failure);
                        //window.alert(messages.failure);
                        $('#msgError').html("<strong>" + messages.failure + ": " + errorThrown + "</strong>");
                        self.HasErrorMessage(true);
                    });
                return false;
            }

            // Parses response.ModelErrors dictionary
            var displayErrors = function (errors) {
                var errorsList = "";
                for (var i = 0; i < errors.length; i++) {
                    errorsList = errorsList + "<li>" + errors[i].Value[0] + "</li>";
                }
                $("#msgError").html("<ul class='pageErrorMsg' style='color: red'>" + errorsList + "</ul>");
                window.alert("VALIDATION ERRORS, please see top of screen for remedial action.");
            }

            self.crudMode = "";

            function loadedSitePricingSettings(settings) {
                var superUnleaded = fuelOverrideLimits["1"],
                    unleaded = fuelOverrideLimits["2"],
                    diesel = fuelOverrideLimits["6"];

                superUnleaded.change.min = settings.MinSuperUnleadedPriceChange;
                superUnleaded.change.max = settings.MaxSuperUnleadedPriceChange;
                superUnleaded.absolute.min = settings.MinSuperUnleadedPrice;
                superUnleaded.absolute.max = settings.MaxSuperUnleadedPrice;

                unleaded.change.min = settings.MinUnleadedPriceChange;
                unleaded.change.max = settings.MaxUnleadedPriceChange;
                unleaded.absolute.min = settings.MinUnleadedPrice;
                unleaded.absolute.max = settings.MaxUnleadedPrice;

                diesel.change.min = settings.MinDieselPriceChange;
                diesel.change.max = settings.MaxDieselPriceChange;
                diesel.absolute.min = settings.MinDieselPrice;
                diesel.absolute.max = settings.MaxDieselPrice;

                // from SitePriceSettings...

                pricingSettings.priceChangeVarianceThreshold = settings.PriceChangeVarianceThreshold;
                pricingSettings.maxGrocerDriveTimeMinutes = settings.MaxGrocerDriveTimeMinutes;
                pricingSettings.superUnleadedMarkupPrice = settings.SuperUnleadedMarkupPrice;

                self.maxGrocerDriveTimeMinutes(settings.MaxGrocerDriveTimeMinutes);

                self.populateOverrideExceedMessages();
            };

            function failureSitePricingSettings() {
                notify.error("Unable to load Site Pricing Settings");
            };

            self.loadSitePricingSettingsData = function () {
                sitePricingSettings.load({
                    success: loadedSitePricingSettings,
                    failure: failureSitePricingSettings
                });
            };

            self.getDataForPage = function () {
                //Check CRUD mode
                self.crudMode = "Edit";

                var yyyymmdd = dmyStringToYmdString(self.InitDate());

                var url = "Sites/GetSitesWithPricesJson?date=" + yyyymmdd
                + "&storeName=" + (self.StoreName() ? escape(self.StoreName()) : "")
                + "&catNo=" + (self.CatNo() ? escape(self.CatNo()) : "0")
                + "&storeNo=" + (self.StoreNo() ? escape(self.StoreNo()) : "0")
                + "&storeTown=" + (self.StoreTown() ? escape(self.StoreTown()) : "");

                var $promise = common.callService("get", url, null); // args - maybe page no. (assuming no paging for now)

                self.loadPageData($promise);
            };

            self.getCompetitorDataForSite = function (site, callback) {
                //Check CRUD mode
                self.crudMode = "Edit";
                if (site.SiteId == "") site.SiteId = 0; // default to all sites

                var yyyymmdd = dmyStringToYmdString(self.InitDate());

                var filter = "date=" + yyyymmdd + "&siteId=" + site.SiteId;

                var url = "Sites/GetSitesWithPricesJson?getCompetitor=1&" + filter; // ScriptMethod GetCompetitors

                var $promise = common.callService("get", url, null); // args - maybe page no. (assuming no paging for now)
                self.loadCompetitorDataForSite(site, $promise, callback);
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

            function recalculateSainsburysStats() {
                var sitesVm = ko.utils.unwrapObservable(self.dataModel);
                var sites = sitesVm.sites;
                calcSainsburysStats(sites);
            };

            //
            // Recalculate all the fuel prices and price change stats
            //
            function calcSainsburysStats(sites) {

                var formatted = {
                    count: '-',
                    total: '-',
                    min: '-',
                    max: '-'
                };

                var stats = {
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

                //
                // update KO (Knockout) stats
                //
                self.stats_Sites_Count(stats.sites.count);
                self.stats_Sites_Active(stats.sites.active);
                self.stats_Sites_WithEmails(stats.sites.withEmails);
                self.stats_Sites_WithNoEmails(stats.sites.withNoEmails);

                // unleaded tomorrow prices
                formatted = formatStatsValues(stats.unleaded.tomorrow.price);

                self.stats_Unleaded_Tomorrow_Price_Average(formatted.average);
                self.stats_Unleaded_Tomorrow_Price_Count(formatted.count);
                self.stats_Unleaded_Tomorrow_Price_Max(formatted.max);
                self.stats_Unleaded_Tomorrow_Price_Min(formatted.min);

                // unleaded tomorrow price changes
                formatted = formatStatsValues(stats.unleaded.tomorrow.priceChanges);

                self.stats_Unleaded_Tomorrow_PriceChanges_Average(formatted.average);
                self.stats_Unleaded_Tomorrow_PriceChanges_Count(formatted.count);
                self.stats_Unleaded_Tomorrow_PriceChanges_Max(formatted.max);
                self.stats_Unleaded_Tomorrow_PriceChanges_Min(formatted.min);

                // unleaded today prices
                formatted = formatStatsValues(stats.unleaded.today.price);

                self.stats_Unleaded_Today_Price_Average(formatted.average);
                self.stats_Unleaded_Today_Price_Count(formatted.count);
                self.stats_Unleaded_Today_Price_Max(formatted.max);
                self.stats_Unleaded_Today_Price_Min(formatted.min);

                // diesel tomorrow prices
                formatted = formatStatsValues(stats.diesel.tomorrow.price);

                self.stats_Diesel_Tomorrow_Price_Average(formatted.average);
                self.stats_Diesel_Tomorrow_Price_Count(formatted.count);
                self.stats_Diesel_Tomorrow_Price_Max(formatted.max);
                self.stats_Diesel_Tomorrow_Price_Min(formatted.min);

                // diesel tomorrow price changes
                formatted = formatStatsValues(stats.diesel.tomorrow.priceChanges);

                self.stats_Diesel_Tomorrow_PriceChanges_Average(formatted.average);
                self.stats_Diesel_Tomorrow_PriceChanges_Count(formatted.count);
                self.stats_Diesel_Tomorrow_PriceChanges_Max(formatted.max);
                self.stats_Diesel_Tomorrow_PriceChanges_Min(formatted.min);

                // diesel today prices
                formatted = formatStatsValues(stats.diesel.today.price);

                self.stats_Diesel_Today_Price_Average(formatted.average);
                self.stats_Diesel_Today_Price_Count(formatted.count);
                self.stats_Diesel_Today_Price_Max(formatted.max);
                self.stats_Diesel_Today_Price_Min(formatted.min);

                // super-unleaded tomorrow prices
                formatted = formatStatsValues(stats.superUnleaded.tomorrow.price);

                self.stats_SuperUnleaded_Tomorrow_Price_Average(formatted.average);
                self.stats_SuperUnleaded_Tomorrow_Price_Count(formatted.count);
                self.stats_SuperUnleaded_Tomorrow_Price_Max(formatted.max);
                self.stats_SuperUnleaded_Tomorrow_Price_Min(formatted.min);

                // super-unleaded tomorrow price changes
                formatted = formatStatsValues(stats.superUnleaded.tomorrow.priceChanges);

                self.stats_SuperUnleaded_Tomorrow_PriceChanges_Average(formatted.average);
                self.stats_SuperUnleaded_Tomorrow_PriceChanges_Count(formatted.count);
                self.stats_SuperUnleaded_Tomorrow_PriceChanges_Max(formatted.max);
                self.stats_SuperUnleaded_Tomorrow_PriceChanges_Min(formatted.min);
                
                // super-unleaded today prices
                formatted = formatStatsValues(stats.superUnleaded.today.price);

                self.stats_SuperUnleaded_Today_Price_Average(formatted.average);
                self.stats_SuperUnleaded_Today_Price_Count(formatted.count);
                self.stats_SuperUnleaded_Today_Price_Max(formatted.max);
                self.stats_SuperUnleaded_Today_Price_Min(formatted.min);

                // combined tomorrow prices
                formatted = formatStatsValues(stats.combined.tomorrow.price);

                self.stats_Combined_Tomorrow_Prices_Average(formatted.average);
                self.stats_Combined_Tomorrow_Prices_Count(formatted.count);
                self.stats_Combined_Tomorrow_Prices_Max(formatted.max);
                self.stats_Combined_Tomorrow_Prices_Min(formatted.min);

                // combined tomorrow price changes
                formatted = formatStatsValues(stats.combined.tomorrow.priceChanges);

                self.stats_Combined_Tomorrow_PriceChanges_Average(formatted.average);
                self.stats_Combined_Tomorrow_PriceChanges_Count(formatted.count);
                self.stats_Combined_Tomorrow_PriceChanges_Max(formatted.max);
                self.stats_Combined_Tomorrow_PriceChanges_Min(formatted.min);

                // combined today prices
                formatted = formatStatsValues(stats.combined.today.price);

                self.stats_Combined_Today_Prices_Average(formatted.average);
                self.stats_Combined_Today_Prices_Count(formatted.count);
                self.stats_Combined_Today_Prices_Max(formatted.max);
                self.stats_Combined_Today_Prices_Min(formatted.min);
            };
        };

        function createStat() {
            return {
                count: 0,
                total: 0,
                min: undefined,
                max: undefined,
                average: undefined
            };
        };

        function updateStatValues(obj, value) {
            obj.count++;
            obj.total += value;
            obj.min = obj.min == undefined ? value : Math.min(obj.min, value);
            obj.max = obj.max == undefined ? value : Math.max(obj.max, value);
        };

        function combineStatFuelPrices(obj, fuel1, fuel2, fuel3) {
            obj.count = fuel1.count + fuel2.count + fuel3.count;
            obj.total = fuel1.total + fuel2.total + fuel3.total;
            obj.min = Math.min(fuel1.min, fuel2.min, fuel3.min);
            obj.max = Math.max(fuel1.max, fuel2.max, fuel3.max);
        };

        function populateStatAverages(items) {
            var i,
                item;
            for (i = 0; i < items.length; i++) {
                item = items[i];
                item.average = item.count == 0 ? 0 : item.total / item.count;
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

        //numeric extension
        ko.bindingHandlers.numeric = {
            init: function (element, valueAccessor) {
                $(element).on("keydown", function (event) {
                    // Allow: backspace, delete, tab, escape, and enter
                    if (event.keyCode == 46 || event.keyCode == 8 || event.keyCode == 9 || event.keyCode == 27 || event.keyCode == 13 ||
                        // Allow: Ctrl+A
                        (event.keyCode == 65 && event.ctrlKey === true) ||
                        // Allow: . ,
                        (event.keyCode == 188 || event.keyCode == 190 || event.keyCode == 110) ||
                        // Allow: home, end, left, right
                        (event.keyCode >= 35 && event.keyCode <= 39)) {
                        // let it happen, don't do anything
                        return;
                    }
                    else {
                        // Ensure that it is a number and stop the keypress
                        if (event.shiftKey || (event.keyCode < 48 || event.keyCode > 57) && (event.keyCode < 96 || event.keyCode > 105)) {
                            event.preventDefault();
                        }
                    }
                });
            }
        };

        // custom binding
        ko.bindingHandlers.formatSiteNote = {
            init: function (element, valueAccessor, allBindings) {
                var value = valueAccessor(),
                    valueUnwrapped = ko.unwrap(value);
                var html = valueUnwrapped == null ? '' : common.htmlEncode(valueUnwrapped).replace(/\n/g, '<br />');;
                $(element).html(html);
            },
            update: function (element, valueAccessor, allBindings) {
            }
        };

        function applyHighlightCookieSettings(vm) {
            var trailPrices = cookieSettings.readBoolean('pricing.highlightTrialPrices', false),
                matchCompetitors = cookieSettings.readBoolean('pricing.highlightMatchCompetitors', false),
                soloPrices = cookieSettings.readBoolean('pricing.highlightSoloPrices', false),
                noNearbyGrocerPrices = cookieSettings.readBoolean('pricing.highlightNoNearbyGrocerPrices', false),
                hasNearbyGrocerPrices = cookieSettings.readBoolean('pricing.highlightHasNearbyGrocerPrices', false),
                hasNearbyGrocerWithOutPrices = cookieSettings.readBoolean('pricing.highlightHasNearbyGrocerWithOutPrices', false);

            vm.highlightTrialPrices(trailPrices);
            vm.highlightMatchCompetitors(matchCompetitors);
            vm.highlightSoloPrices(soloPrices);
            vm.highlightNoNearbyGrocerPrices(noNearbyGrocerPrices);
            vm.highlightHasNearbyGrocerPrices(hasNearbyGrocerPrices);
            vm.highlightHasNearbyGrocerWithOutPrices(hasNearbyGrocerPrices);
            vm.redrawGridHighlights();
        };

        function applyPriceChangeFilterCookieSettings(vm) {
            var up = cookieSettings.readBoolean('pricing.priceChangeFilterUp', true),
                none = cookieSettings.readBoolean('pricing.priceChangeFilterNone', true),
                down = cookieSettings.readBoolean('pricing.priceChangeFilterDown', true);

            vm.priceChangeFilterDown(down);
            vm.priceChangeFilterNone(none);
            vm.priceChangeFilterUp(up);
        };

        function restoreFromCookieSettings(vm) {
            applyHighlightCookieSettings(vm);
            applyPriceChangeFilterCookieSettings(vm);
        };

        function bindEvents() {
            $('#txtOverrideUnleaded').on('keyup', function (ev) {
                if (ev.keyCode == 13) {
                    ev.preventDefault();
                    $('#btnGoOverrideUnleaded').trigger('click');
                }
            });

            $('#txtOverrideDiesel').on('keyup', function (ev) {
                if (ev.keyCode == 13) {
                    ev.preventDefault();
                    $('#btnGoOverrideDiesel').trigger('click');
                }
            });

            $('#txtOverrideSuperUnleaded').on('keyup', function (ev) {
                if (ev.keyCode == 13) {
                    ev.preventDefault();
                    $('#btnGoSuperUnleaded').trigger('click');
                }
            });
        };

        function loadDriveTimeMarkups() {
            function failure() {
                notify.error("Unable to load Drive Time Markup data");
            };
            function success(data) {
                if (!data || !data.Status || data.Status.ErrorMessage)
                    failure();
                else
                    driveTimeMarkups = data;
            };

            driveTimeMarkupService.loadDriveTimeMarkups(success, failure);
        };

        function getNumberOrZero(value) {
            return value != '' && !isNaN(value) ? Number(value) : 0;
        };


        var go = function () {
            // competitor price grid popup
            competitorPopup.bindPopup();

            // competitor note popup
            competitorPopup.bindNotePopup({
                events: {
                    afterNoteUpdate: competitorPopup.afterNoteUpdate,
                    afterNoteDelete: competitorPopup.afterNoteDelete,
                    afterNoteHide: competitorPopup.afterNoteHide
                }
            });

            moment.locale("en-gb");

            var vm = new PageViewModel();

            var binder = function () {
                ko.applyBindings(vm, $("#petrolpricingpage")[0]);
            };
            vm.bind = binder;

            restoreFromCookieSettings(vm);

            bindEvents();

            var pageQueryParams = helpers.queryStringHelpers.getQueryParams(window.location.search);

            var forDatefromQryStr = pageQueryParams["date"];
            var forStoreNameQryStr = pageQueryParams["storeName"];
            var forCatNoQryStr = pageQueryParams["catNo"];
            var forStoreNoQryStr = pageQueryParams["storeNo"];
            var forStoreTownQryStr = pageQueryParams["storeTown"];

            var ukDateString;
            if (moment(forDatefromQryStr, dmyFormatString).isValid())
                ukDateString = moment(forDatefromQryStr).format(dmyFormatString);
            else if (moment(forDatefromQryStr, ymdFormatString).isValid())
                ukDateString = moment(forDatefromQryStr, ymdFormatString).format(dmyFormatString);
            else ukDateString = todaysDateUkformat;
            // Show sitepricing data once loaded from GET
            vm.setupSearchFields(ukDateString, forStoreNameQryStr, forCatNoQryStr, forStoreNoQryStr, forStoreTownQryStr);
            vm.getDataForPage();
            vm.loadSitePricingSettingsData();

            loadDriveTimeMarkups();
        };
        return { go: go };
    });