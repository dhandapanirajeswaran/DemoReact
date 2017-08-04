define(["jquery", "knockout", "moment", "bootstrap-datepicker", "bootstrap-datepickerGB", "underscore", "common", "helpers", "URI", "competitorPricePopup", "notify", "busyloader", "cookieSettings", "bootbox", "sitePricingSettings", "EmailTemplateService", "SiteEmailPopup", "DriveTimeMarkupService", "validation", "PriceStats", "SitePricingSorting", "PetrolPricingService", "PriceChangesTab", "etaCountdown", "waiter"],
    function ($, ko, moment, datepicker, datePickerGb, _, common, helpers, URI, competitorPopup, notify, busyloader, cookieSettings, bootbox, sitePricingSettings, emailTemplateService, siteEmailPopup, driveTimeMarkupService, validation, priceStats, sitePricingSorting, petrolPricingService, priceChangesTab, etaCountdown, waiter) {
        // constants
        var FUEL_SUPER_UNLEADED = 1,
            FUEL_UNLEADED = 2,
            FUEL_DIESEL = 6;

        // loaded via 'init' from page
        var pagedata = {};

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
            priceChangeVarianceThreshold: 0.3,
            decimalRounding: -1,
            superUnleadedMarkupPrice: 5.0,
            enableSiteEmails: false,
            siteEmailTestAddresses: ''
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

        var isNonZeroPrice = function (value) {
            return value != '' && !isNaN(value) && value > 0;
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

        var formatIgnoredPriceVarianceInfotip = function (value) {
            if (!isNumber(value))
                return '';

            if (Math.abs(value / 10) <= pricingSettings.priceChangeVarianceThreshold)
                return '[br /] [em]Ignored[/em] within [b]+/- ' + pricingSettings.priceChangeVarianceThreshold + '[/b] variance';
            return '';
        };

        function getIndexForFuelType(fuelTypeId) {
            switch (fuelTypeId) {
                case FUEL_SUPER_UNLEADED:
                    return 2;
                case FUEL_UNLEADED:
                    return 0;
                case FUEL_DIESEL:
                    return 1;
                default:
                    console.log('Unable to find index for Fuel: ' + fuelTypeId);
                    break;
            }
        };

        var messageFormats = {
            priceIsNotAvailableForTrialPrice: '[i class="fa fa-flask"][/i] [b]Trial Price[/b] not available',
            priceIsNotAvailableForMatchCompetitor: '[i class="fa fa-clone"][/i] [b]Match Competitor[/b] Price not available [br /] for Competitor [b]{NAME}[/b]',
            priceIsNotAvailableForStandardPrice: '[i class="fa fa-dot-circle-o"][/i] [b]Standard Price[/b] not available',
            trialPriceInfoTip: '[q][i class="fa fa-flask"][/i] Trial Price:[/q] [b]{NAME}[/b] - [q]Markup:[/q] [b]{MARKUP}[/b] [br /]Distance: [b]{DISTANCE}[/b] miles = [b]{DRIVETIME}[/b] minutes [br /] Price Source: [b]{PRICESOURCE}[/b] on [b]{PRICESOURCEDATETIME}[/b]',
            matchCompetitorInfoTip: '[q][i class="fa fa-clone"][/i] Match Competitor[/q] : [b]{NAME}[/b] -[q] MarkUp:[/q] [b]{MARKUP}[/b] [br /]Distance: [b]{DISTANCE}[/b] miles = [b]{DRIVETIME}[/b] minutes [br /] Price Source: [b]{PRICESOURCE}[/b] on [b]{PRICESOURCEDATETIME}[/b]',
            standardPriceInfoTip: '[q][i class="fa fa-dot-circle-o"][/i] Standard Price[/q]',
            standardPriceBasedOnCompetitorInfoTip: '[q][i class="fa fa-dot-circle-o"][/i] Standard Price[/q] based on Competitor: [b]{COMPETITOR}[/b][br /] Distance: [b]{DISTANCE}[/b] miles = [b]{DRIVETIME}[/b] minutes[br /] Price Source: [b]{PRICESOURCE}[/b] on [b]{PRICESOURCEDATETIME}[/b]',
            currentPriceInfoTip: 'Current Price for [b]{NAME}[/b]',
            priceOverrideInfoTip: 'Override Price for [b]{NAME}[/b]',
            priceOverrideIncreaseExceedsInfoTip: 'Override Price for [b]{NAME}[/b][br /] [b][i class="fa fa-warning"][/i] Warning:[/b] Increase exceeds [b]{INCMIN}[/b] to [b]{INCMAX}[/b]',
            priceOverrideInvalidValueInfoTip: 'Override Price for [b]{NAME}[/b][br /][em][i class="fa fa-bug"][/i]Invalid Price[/em] (range [b]{ABSMIN}[/b] to [b]{ABSMAX}[/b])',
            withinPriceChangeVarianceInfoTip: 'Override Price [em]Ignored[/em] [br /]( within [b]Price Change Variance[/b] of [b]+/- {VARIANCE}[/b] )',
            yestPriceInfoTip: '[b]{NAME}[/b][br /]Yesterday Price of [b]{VALUE}[/b][br /]Drive Time Markup: [b]{DRIVETIMEMARKUP}[/b][br /]Yesterday + Drive-Time [b]{INCPRICE}[/b]',
            compTodayPriceInfotip: '[b]{NAME}[/b][br /]Today Price of [b]{VALUE}[/b][br /]Drive Time Markup: [b]{DRIVETIMEMARKUP}[/b][br /]Today + Drive-Time [b]{INCPRICE}[/b]',
            updownInfoTips: {
                '-1': '[u][i class="fa fa-arrow-down"][/i] Price Decrease[/u] of [b]-{VALUE}[/b] for [b]{NAME}[/b]{VARIANCE}',
                '0': '[u][i class="fa fa-minus"][/i] Incomplete Price Change[/u] for [b]{NAME}[/b]{VARIANCE}',
                '1': '[u][i class="fa fa-arrow-up"][/i] Price Increase[/u] of [b]+{VALUE}[/b] for [b]{NAME}[/b]{VARIANCE}'
            },
            barChartCellsNAInfoTips: 'There are [b]{COUNT}[/b] Sites with a [br /] [b]N/A[/b] Price Difference',
            barChartCellsInfoTips: {
                '-1': 'There are [b]{COUNT}[/b] Sites with a [br /][i class=&quot;fa fa-arrow-down&quot;][/i] Price Decrease of [b]{VALUE}[/b]',
                '0': 'There are [b]{COUNT}[/b] Sites with [br /][i class=&quot;fa fa-minus&quot;][/i] Incomplete Price Change',
                '1': 'There are [b]{COUNT}[/b] Sites with a [br /][i class=&quot;fa fa-arrow-up&quot;][/i] Price Increase of [b]{VALUE}[/b]',
            },
            noNearbyGrocersInfotip: 'There are [b]No Grocers[/b] [br /]within [b]{DRIVETIME} Minutes[/b] Drive Time',
            nearbyGrocersWithoutPriceInfoTip: '[b]Grocers[/b] found but [b]Incomplete Price Data[/b] [br /]within {DRIVETIME} Minutes for Date',
            nearbyGrocersWithPriceInfoTip: '[b]Grocers[/b] and Price are [b]Available[/b] [br /]within {DRIVETIME} Minutes for Date',
            loadingETACached: '<span class="text-success"><i class="fa fa-check"></i> ETA less than <strong class="eta-countdown""></strong></span>',
            loadingETARecalculate: '<span class="text-warning"><strong><i class="fa fa-warning"></i> Recalculating</strong> &mdash; ETA <strong class="eta-countdown"></strong></span>',
            loadingETAProcessing: '<span class="text-danger"><strong><i class="fa fa-warning"></i> Processing</strong> &mdash; ETA <strong class="eta-countdown"></strong></span>',
            competitorDataPercentAllSummary: '[u]Showing ALL Competitors % [i class="fa fa-credit-card"][/i][/u][br /]Drive-Time: [em]25 Minutes[/em][br /]All Competitors: [b]{COMPETITORPRICECOUNT} of {COMPETITORCOUNT}[/b] = [u]{COMPETITORPERCENT} %[/u][br /] Grocers Only: [b]{GROCERPRICECOUNT} of {GROCERCOUNT}[/b] = [u]{GROCERPERCENT} %[/u]',
            competitorDataPercentGrocerSummary: '[u]Showing Grocer % [i class="fa fa-shopping-cart"][/i][/u][br /]Drive-Time: [em]25 Minutes[/em][br /]All Competitors: [b]{COMPETITORPRICECOUNT} of {COMPETITORCOUNT}[/b] = [u]{COMPETITORPERCENT} %[/u][br /] Grocers Only: [b]{GROCERPRICECOUNT} of {GROCERCOUNT}[/b] = [u]{GROCERPERCENT} %[/u]'
        };

        var markupFormats = {
            noNearbyGrocers: '<span class="no-nearby-grocer-price" data-infotip="There are [i]No Grocers[/i] [br /]within [b]{DRIVETIME} Minutes[/b] Drive Time"><i class="fa fa-times"></i></span>',
            nearbyGrocersWithoutPrices: '<span class="has-nearby-grocer-with-out-price" data-infotip="[u]Grocers[/u] found but [em]Incomplete Price Data[/em] [br /]within [b]{DRIVETIME} Minutes[/b] for Date"><i class="fa fa-question"></i></span>',
            nearbyGrocersAndPrices: '<span class="has-nearby-grocer-price" data-infotip="[b]Grocers Prices[/b] are [u]Available[/u] [br /]within [b]{DRIVETIME} Minutes[/b] for Date"><i class="fa fa-gbp"></i></span>'
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

            // dictionary of {key, ref} to Sainsburys sites
            self.sainsurysSitesLookups = {};

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
            self.isEditing = ko.observable(false);

            self.ViewingHistorical = ko.observable(false); // TODO useful to have for disabling few things
            self.hasDailyPriceFile = ko.observable(false);
            self.isDailyPriceFileOutdated = ko.observable(true);
            self.hasLatestJsPriceDataForToday = ko.observable(false);

            self.hasPriceSnapshotIsActive = ko.observable(false);
            self.hasPriceSnapshotIsOutdated = ko.observable(false);

            if (pagedata && pagedata.page == 'sitepricing') {
                // detect if a Daily Price Data file exists for today
                self.hasDailyPriceFile(!pagedata.DailyPriceData.IsMissing);
                self.isDailyPriceFileOutdated(pagedata.DailyPriceData.IsOutdated);
                // detect if a Latest JS Price Data file exists for today
                //self.hasLatestJsPriceDataForToday(!pagedata.LatestPriceData.IsMissing);
                self.hasPriceSnapshotIsActive(pagedata.PriceSnapshot.IsActive);
                self.hasPriceSnapshotIsOutdated(pagedata.PriceSnapshot.IsOutdated);
            }

            self.PriceChangeVariance = ko.observable(0.3);
            self.MaxGrocerDriveTime = ko.observable(5.0);
            self.SuperUnleadedMarkup = ko.observable(5.0);
            self.DecimalRounding = ko.observable(-1);
            self.EnableSiteEmails = ko.observable(false);
            self.SiteEmailTestAddresses = ko.observable('');

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

            self.busyLoadingData = ko.observable(false);

            self.startBusyLoadingData = function () {
                etaCountdown.start('.eta-countdown', self.etaLoadingDownSeconds)
                self.busyLoadingData(true);
            };
            self.stopBusyLoadingData = function () {
                etaCountdown.stop();
                self.busyLoadingData(false);
            };

            self.firstPageLoad = ko.observable(true);
            self.hasBadPriceChangeValue = ko.observable(false);
            self.hasAnyFieldErrors = ko.observable(false);
            self.fieldErrorCount = ko.observable(0);

            self.showCompetitorPricesPopup = ko.observable(true);

            self.competitorPricesPopupSiteItemModel = ko.observable({});

            self.checkedEmailCount = ko.observable(0);

            self.showCompetitorNotePopup = ko.observable(false);

            self.isShowingCompetitorNoPriceData = ko.observable(true);


            self.setCompetitorNoPriceDataShow = function (show) {
                var table = $('table.competitorTable');
                self.isShowingCompetitorNoPriceData(show);
                show 
                ? table.removeClass('hide-competitor-no-price-data').addClass('show-competitor-no-price-data')
                : table.removeClass('show-competitor-no-price-data').addClass('hide-competitor-no-price-data');
            };

            self.showCompetitorNoPriceData = function () {
                self.setCompetitorNoPriceDataShow(true);
                notify.info('Showing Competitors Prices with no data');
            };
            self.hideCompetitorNoPriceData = function () {
                self.setCompetitorNoPriceDataShow(false);
                notify.info('Hiding Competitors Prices with no data');
            };

            self.isAutoHideShowOverrides = ko.observable(false);

            self.enableAutoHideOverrides = function () {
                notify.info('Auto hide empty Overrides is ON');
                cookieSettings.writeBoolean('pricing.autoHideOverrides', true);
                setTimeout(function () {
                    self.isAutoHideShowOverrides(true);
                }, 1000);
            };

            self.disableAutoHideOverrides = function () {
                notify.info('Auto hide empty Overrides is OFF');
                cookieSettings.writeBoolean('pricing.autoHideOverrides', false);
                setTimeout(function () {
                    self.isAutoHideShowOverrides(false);
                }, 1000);
            };

            self.ShowingDataPercent = ko.observable('all');

            self.showDataPercentAll = function () {
                var wasPaused = sitePricingSorting.pauseForColumns([3, 7, 11]);
                self.ShowingDataPercent('all');
                sitePricingSorting.setDataPercentMode('all');
                notify.info('Showing all Data % for ALL Competitors');

                if (wasPaused)
                    self.sortHasBeenPaused(true);
            };

            self.showDataPercentGrocer = function () {
                var wasPaused = sitePricingSorting.pauseForColumns([3, 7, 11]);
                self.ShowingDataPercent('grocer');
                sitePricingSorting.setDataPercentMode('grocer');
                notify.info('Showing Data % for Grocers only');

                if (wasPaused)
                    self.sortHasBeenPaused(true);
            };


            self.setCompetitorSiteNote = function (siteId, note) {
                var sites = self.dataModel().sites,
                    site,
                    compsites,
                    compsite,
                    compIndex,
                    siteIndex;
                for (siteIndex = 0; siteIndex < sites.length; siteIndex++) {
                    site = sites[siteIndex];
                    if (site.hasCompetitors) {
                        compsites = site.competitors();
                        for (compIndex = 0; compIndex < compsites.length; compIndex++) {
                            compsite = compsites[compIndex];
                            if (compsite.SiteId == siteId) {
                                compsite.Notes = note;
                                compsite.hasNotes = note != '';
                                return;
                            }
                        }
                    }
                }
            };

            self.confirmGotoSettings = function () {
                bootbox.confirm({
                    title: 'Navigation Confirmation',
                    message: '<i class="fa fa-question fa-2x"></i> Are you sure you wish to open the the Settings page?',
                    buttons: {
                        cancel: {
                            label: '<i class="fa fa-times"></i> No',
                            className: 'btn-default'
                        },
                        confirm: {
                            label: '<i class="fa fa-check"></i> Yes',
                            className: 'btn-success'
                        }
                    },
                    callback: function (result) {
                        if (result) {
                            busyloader.show({
                                message: 'Opening the Settings page',
                                showtime: 1000,
                                dull: true
                            });
                            window.location = $('[data-menu-item="nav-settings"] a').attr('href');
                        }
                    }
                })
            };

            self.tableGridCss = function () {
                return !self.ViewingHistorical() && !pagedata.DailyPriceData.IsOutdated
                    ? ' table-show-edit-overrides'
                    : '';
            };

            self.etaLoadingDownSeconds = 15;

            self.loadingETAMessage = ko.computed(function () {
                var active = self.hasPriceSnapshotIsActive(),
                    outdated = self.hasPriceSnapshotIsOutdated(),
                    message,
                    seconds;

                if (active && !outdated) {
                    message = messageFormats.loadingETACached;
                    seconds = 15;
                } else if (active && outdated) {
                    message = messageFormats.loadingETARecalculate;
                    seconds = 60;
                } else {
                    message = messageFormats.loadingETAProcessing;
                    seconds = 60;
                }

                self.etaLoadingDownSeconds = seconds;
                return replaceTokens(message, {
                    '{SECONDS}': seconds
                });
            });

            // excluded brands
            self.excludedBrands = {
                value: '',
                unsaved: false,
                isSaving: false,
                countdown: 15,
                ticker: 15,
                messages: {
                    none: {
                        css: '',
                        message: ''
                    },
                    saved: {
                        css: 'status-saved',
                        message: '<i class="fa fa-check"></i> Saved'
                    },
                    saving: {
                        css: 'status-saving',
                        message: '<i class="fa fa-save"></i> Auto saving...'
                    },
                    unsaved:
                    {
                        css: 'status-unsaved',
                        message: '<i class="fa fa-warning"></i> Unsaved changes &mdash; (auto saves in a few moments...)'
                    },
                    error: {
                        css: 'status-error',
                        message: '<i class="fa fa-times"></i> Error - unable to save'
                    }
                }
            };
            self.savingExcludedBrandsMessage = ko.observable('');
            self.savingExcludedBrandsCss = ko.observable('');

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

            // sorting:start

            self.sortByColumn0 = function () { setSortColumn(0) };
            self.sortByColumn1 = function () { setSortColumn(1) };
            self.sortByColumn2 = function () { setSortColumn(2) };
            self.sortByColumn3 = function () { setSortColumn(3) };
            self.sortByColumn4 = function () { setSortColumn(4) };
            self.sortByColumn5 = function () { setSortColumn(5) };
            self.sortByColumn6 = function () { setSortColumn(6) };
            self.sortByColumn7 = function () { setSortColumn(7) };
            self.sortByColumn8 = function () { setSortColumn(8) };
            self.sortByColumn9 = function () { setSortColumn(9) };
            self.sortByColumn10 = function () { setSortColumn(10) };
            self.sortByColumn11 = function () { setSortColumn(11) };
            self.sortByColumn12 = function () { setSortColumn(12) };
            self.sortByColumn13 = function () { setSortColumn(13) };
            self.sortByColumn14 = function () { setSortColumn(14) };

            self.sortHasBeenPaused = ko.observable(false);

            self.sortForceUpdate = ko.observable(false);

            function resortPricingGrid() {
                // this forces KO to re-sort the pricing table grid
                self.sortForceUpdate(!self.sortForceUpdate());
            };

            self.pricingColumnSorter = function (site1, site2) {
                // dummy KO variable used to force KO to redraw/resort grid
                var dummyRead = self.sortForceUpdate();
                return sitePricingSorting.sort(site1, site2);
            };
            
            self.resumeSorting = function () {
                sitePricingSorting.resume();
                self.sortHasBeenPaused(false);
                resortPricingGrid();
            };

            function setSortColumn(index) {
                self.sortHasBeenPaused(false);
                sitePricingSorting.setSortColumn(index);
                // update KO
                resortPricingGrid();
                notify.info(sitePricingSorting.getSortMessage());
                $('#PricingPanelScroller').scrollTop(0);
            };
            // sorting:end

            // price summary tabs
            self.showingPriceChangesTab = ko.observable(true);
            self.showingEmptyTab = ko.observable(true);
            self.showingPriceSummaryTab = ko.observable(true);
            self.showingCompetitorDifferencesTab = ko.observable(true);

            var pricingTabs = [
                { message: 'Hiding summary tab' },
                { message: 'Showing Our Price Changes' },
                { message: 'Showing Our Price Stats' }
            ];

            self.setPricingTab = function(index, quiet) {
                self.showingPriceChangesTab(index == 1);
                self.showingPriceSummaryTab(index == 2);
                self.showingEmptyTab(index == 0);
                cookieSettings.writeInteger('pricing.priceSummaryTabIndex', index);
                if (!quiet)
                    notify.info(pricingTabs[index].message);
            };

            self.showEmptySummaryTab = function () {
                self.setPricingTab(0);
            };

            self.showPriceChangesTab = function () {
                self.setPricingTab(1);
            };

            self.showPriceSummaryTab = function () {
                self.setPricingTab(2);
            };

            self.showCompetitorDifferencesTab = function () {
                self.setPricingTab(3);
            };

            // price changes tab

            self.priceChangeTabView = ko.observable(0);

            self.showPriceChangesAsBarChart = function () {
                self.priceChangeTabView(0);
                notify.info('Showing our Price Changes as a Bar Chart');
                self.renderPriceChangeGraph();
            };
            self.showPriceChangesAsTable = function () {
                self.priceChangeTabView(1);
                notify.info('Showing our Price Changes as a Table');
                self.renderPriceChangeGraph();
            };

            self.showPriceChangesAsTags = function () {
                self.priceChangeTabView(2);
                notify.info('Showing Our Price Changes as Tags');
                self.renderPriceChangeGraph();
            };

            self.priceChangesTabShowUnleaded = ko.observable(true);
            self.priceChangesTabShowDiesel = ko.observable(true);
            self.priceChangesTabShowSuperUnleaded = ko.observable(true);

            self.priceChangesTabShowUnleaded.subscribe(function (newValue) {
                self.priceChangesTabShowUnleaded(newValue);
                self.renderPriceChangeGraph();
            });
            self.priceChangesTabShowDiesel.subscribe(function (newValue) {
                self.priceChangesTabShowDiesel(newValue);
                self.renderPriceChangeGraph();
            });
            self.priceChangesTabShowSuperUnleaded.subscribe(function (newValue) {
                self.priceChangesTabShowSuperUnleaded();
                self.renderPriceChangeGraph();
            });

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
            self.highlightStandardPrices = ko.observable(false);
            self.highlightNoNearbyGrocerPrices = ko.observable(false);
            self.highlightHasNearbyGrocerPrices = ko.observable(false);

            self.highlightHasNearbyGrocerWithOutPrices = ko.observable(false);

            self.maxGrocerDriveTimeMinutes = ko.observable(5);

            self.shouldShouldHighlightReset = function () {
                return self.highlightTrialPrices()
                    || self.highlightMatchCompetitors()
                    || self.highlightStandardPrices()
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
                    "No Overrides": "Showing Sites with No Price Overrides",
                    "Exceeds Absolute Min/Max": "Showing Sites which Exceed the Absolute min/max limits",
                    "Exceeds +/- Warnings" : "Showings Sites which Exceed the Warning +/- limits",
                    "Errors": "Showing Sites with Errors",
                    "Inside Variance": "Showing Sites with price changes inside Variance",
                    "Outside Variance": "Showing Sites with price changes outside Variance",
                    "Missing CatNo": "Showing Sites with a Missing CatNo",
                    "Missing PfsNo": "Showing Sites with a Missing PfsNo",
                    "Missing StoreNo": "Showing Sites with a Missing StoreNo"
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
                    if (siteItem.HasEmails && siteItem.hasEmailPricesChanges()) {
                        siteItem.checkedEmail(true);
                        count++;
                    }
                });

                if (count == 0) {
                    notify.warning('There are 0 Sites with an Email address and Price Changes');
                } else {
                    notify.success('Selected ' + count + ' Sites with an Email Address and Price Changes');
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
                    if (siteItem.HasEmails && siteItem.hasEmailPricesChanges()) {
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
            self.StandardPricesCount = ko.observable(0);

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

                    if (unleadedOverridePrice > 0)
                        setOverridePriceForSiteFuel(superUnleadedOverridePrice, siteItem, FUEL_SUPER_UNLEADED);
                })
            };

            var commonChangePricePerLitre = function (pplVal, fuelTypeId) {
                var isClearing = pplVal == '-',
                    message = 'Applying Override to ' + fuelTypeNames[fuelTypeId];

                if (!isClearing && fuelTypeId == FUEL_UNLEADED)
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
                var isClearing = pplVal == '-';
                isApplyingMassFuelPriceOverride = true;
                validateAndApplyPriceChange(pplVal, fuelTypeId);

                if (!isClearing) {
                    // handle SuperUnleaded = Unleaded + Markup
                    if (fuelTypeId == FUEL_UNLEADED) {
                        if (isNumber(pplVal)) {
                            setSuperUnleadedToUnleadedPlusMarkup();
                        }
                    }
                }

                isApplyingMassFuelPriceOverride = false;

                self.sortHasBeenPaused(false);
                sitePricingSorting.resume();
                resortPricingGrid();

                detectUnsavedChanges();
                recalculateSainsburysStats();
            };

            self.applyOverrideAllRounding = function (value) {
                if (self.changeType() == self.changeTypeValueOverrideAll) {
                    //value = applyDecimalRounding(value);
                    return isNumber(value) ? Number(value).toFixed(1) : value;
                } else {
                    //value = applyDecimalRounding(value);
                    return isNumber(value) ? Number(value).toFixed(1) : value;
                }
            };

            function applyDecimalRounding(value) {
                if (!isNumber(value) || pricingSettings.decimalRounding == -1)
                    return value;
                return Math.floor(value / 10) + '' + pricingSettings.decimalRounding;
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

                if (opts.value == '' || !isNumber(opts.value)) {
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
                    isClearing = opts.value == '',
                    isPlusMinus = self.changeType() == self.changeTypeValuePlusMinus,
                    message;

                if (isClearing) {
                    message = 'Are you sure you wish to clear the Override Prices?<br />'
                        + '<br />'
                        + 'All Override values for ' + opts.fuelName + ' will be cleared.';
                } else {
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

                setCompetitorFields(sitePricingView.sites); // sets up additional fields per site hasCompetitors and competitors = [];
                setEmailLogEntryFields(sitePricingView.sites);
                enListOnlyFuelsToDisplay(sitePricingView.sites); // adds another prop to siteItem - FuelPricesToDisplay

                self.dataModel(sitePricingView);

                // load this later once page is loaded..
                self.loadEmailSendLogForDate(null);

                buildPriceDifferences();
                
                $('#PriceDifferencePanel').trigger('data-loaded');

                self.redrawFuelPriceChanges();
                filterPriceChangeSiteRow();

                applyPriceDifferenceToolTipFixToAllSites(sitePricingView.sites);

                applyInitStateToAllSites(sitePricingView);

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

            // Apply the inital state of the view model
            function applyInitStateToAllSites(vm) {
                _.each(vm.sites, function (siteItem) {

                    var unleaded = siteItem.FuelPricesToDisplay[0],
                        diesel = siteItem.FuelPricesToDisplay[1],
                        superUnleaded = siteItem.FuelPricesToDisplay[2];

                    self.validateOverridePrice(unleaded.FuelPrice.OverridePrice(), siteItem.SiteId, unleaded.FuelTypeId, unleaded.FuelPrice);
                    self.validateOverridePrice(diesel.FuelPrice.OverridePrice(), siteItem.SiteId, diesel.FuelTypeId, diesel.FuelPrice);
                    self.validateOverridePrice(superUnleaded.FuelPrice.OverridePrice(), siteItem.SiteId, superUnleaded.FuelTypeId, superUnleaded.FuelPrice);

                    siteItem.canSendEmail(siteItem.hasEmailPricesChanges());
                });
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
                                linkCompetitorsToSainsburysSite(competitorsForSite, siteItem);
                                siteItem.hasCompetitors(true);
                                markCompetitorDrivetimeBoundaries(competitorsForSite);
                                markCompetitorFuelPriceMatches(competitorsForSite, siteItem);
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

            // Mark competitor Fuel Price matches
            function markCompetitorFuelPriceMatches(competitorsForSite, siteItem) {

                var tomorrow = {
                    unleaded: {
                        auto: siteItem.FuelPricesToDisplay[1].FuelPrice.AutoPrice,
                        override: siteItem.FuelPricesToDisplay[1].FuelPrice.OverridePrice(),
                        price: undefined
                    },
                    diesel: {
                        auto: siteItem.FuelPricesToDisplay[2].FuelPrice.AutoPrice,
                        override: siteItem.FuelPricesToDisplay[2].FuelPrice.OverridePrice(),
                        price: undefined
                    },
                    superUnleaded: {
                        auto: siteItem.FuelPricesToDisplay[0].FuelPrice.AutoPrice,
                        override: siteItem.FuelPricesToDisplay[0].FuelPrice.OverridePrice(),
                        price: undefined
                    }
                };

                tomorrow.unleaded.price = isNonZeroPrice(tomorrow.unleaded.override)
                    ? tomorrow.unleaded.override
                    : tomorrow.unleaded.auto;

                tomorrow.diesel.price = isNonZeroPrice(tomorrow.diesel.override)
                    ? tomorrow.diesel.override
                    : tomorrow.diesel.auto;

                tomorrow.superUnleaded = isNonZeroPrice(tomorrow.superUnleaded.override)
                    ? tomorrow.superUnleaded.override
                    : tomorrow.superUnleaded.auto;

                _.each(competitorsForSite, function (compsite) {
                    var compname = compsite.Brand + '/' + compsite.StoreName;
                    compsite.isCompetitorForSuperUnleaded = siteItem.FuelPrices[1].CompetitorName == compname;
                    compsite.isCompetitorForUnleaded = siteItem.FuelPrices[1].CompetitorName == compname;
                    compsite.isCompetitorForDiesel = siteItem.FuelPrices[2].CompetitorName == compname;

                    compsite.tomorrowUnleadedPrice = tomorrow.unleaded.price;
                    compsite.tomorrowDieselPrice = tomorrow.diesel.price;
                    compsite.tomorrowSuperUnleadedPrice = tomorrow.superUnleaded.price;
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

            function linkCompetitorsToSainsburysSite(competitors, jsSite) {
                _.each(competitors, function (compsite) {
                    compsite.jsSiteItem = jsSite;
                });
            };

            var enListOnlyFuelsToDisplay = function (sites) {

                var totals_standardPrices = 0,
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
                                totals_standardPrices++;
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
                    siteItem.hasRowWarnings = ko.computed(function () {
                        var i,
                            fuel,
                            fuels = siteItem.FuelPricesToDisplay;
                        for (i = 0; i < fuels.length; i++) {
                            fuel = fuels[i];
                            if (!fuel.hasWarning())
                                return true;
                        }
                        return false;
                    });

                    siteItem.hasOverrideOutsideWarningLimits = ko.computed(function () {
                        var i,
                            fuel,
                            fuels = siteItem.FuelPricesToDisplay;
                        for (i = 0; i < fuels.length; i++) {
                            fuel = fuels[i];
                            if (fuel.FuelPrice.isOutsideWarningLimits())
                                return true;
                        }
                        return false;
                    });

                    siteItem.hasPriceMovementOutsideVariance = ko.computed(function () {
                        var i,
                            fuel,
                            fuels = siteItem.FuelPricesToDisplay;
                        for (i = 0; i < fuels.length; i++) {
                            fuel = fuels[i];
                            if (fuel.FuelPrice.isOutsidePriceChangeVariance())
                                return true;
                        }
                        return false;
                    });

                    siteItem.hasPriceMovementInsideVariance = ko.computed(function () {
                        var i,
                            fuel,
                            fuels = siteItem.FuelPricesToDisplay;
                        for (i = 0; i < fuels.length; i++) {
                            fuel = fuels[i];
                            if (fuel.FuelPrice.isWithinPriceChangeVariance())
                                return true;
                        }
                        return false;
                    });

                    siteItem.hasOverrideExceedAbsoluteMinMax = ko.computed(function () {
                        var i,
                            fuel,
                            fuels = siteItem.FuelPricesToDisplay;
                        for (i = 0; i < fuels.length; i++) {
                            fuel = fuels[i];
                            if (fuel.FuelPrice.isOutsideAbsoluteMinMax())
                                return true;
                        }
                        return false;
                    });

                    siteItem.siteRowCss = ko.computed(function () {
                        var css = '',
                            emailFilter = self.siteEmailFilter(),
                            visible = true;

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

                        if ($.isFunction(siteItem.isEditing) && siteItem.isEditing())
                            css += ' focused';
                        else {
                            // perform site row filtering
                            visible = (emailFilter == 'All Sites')
                                || (emailFilter == "No Emails" && !siteItem.HasEmails)
                                || (emailFilter == "With Emails" && siteItem.HasEmails)
                                || (emailFilter == "Selected" && siteItem.HasEmails && siteItem.checkedEmail())
                                || (emailFilter == "Not Selected" && siteItem.HasEmails && !siteItem.checkedEmail())
                                || (emailFilter == "With Overrides" && siteItem.HasOverrides())
                                || (emailFilter == "No Overrides" && !siteItem.HasOverrides())
                                || (emailFilter == "Exceeds Absolute Min/Max" && siteItem.hasOverrideExceedAbsoluteMinMax())
                                || (emailFilter == "Exceeds +/- Warnings" && siteItem.hasOverrideOutsideWarningLimits())
                                || (emailFilter == "Errors" && siteItem.hasRowErrors())
                                || (emailFilter == "Inside Variance" && siteItem.hasPriceMovementInsideVariance())
                                || (emailFilter == "Outside Variance" && siteItem.hasPriceMovementOutsideVariance())
                                || (emailFilter == "Missing CatNo" && siteItem.hasMissingCatNo)
                                || (emailFilter == "Missing PfsNo" && siteItem.hasMissingPfsNo)
                                || (emailFilter == "Missing StoreNo" && siteItem.hasMissingStoreNo);

                            css += (siteItem.hasRowErrors() ? 'row-errors' : '')
                                + ('checkedEmail' in siteItem && siteItem.checkedEmail() ? ' has-checked-email' : ' no-checked-email');
                        }

                        css += (visible ? '' : ' hide')
                            + (siteItem.hasOverrideOutsideWarningLimits() ? ' row-override-exceeds-warning-limits' : '')
                            + (siteItem.hasPriceMovementInsideVariance() ? ' row-inside-variance' : '')
                            + (siteItem.hasPriceMovementOutsideVariance() ? ' row-outside-variance' : '')
                            + (siteItem.hasOverrideExceedAbsoluteMinMax() ? ' row-exceeds-absolutes' : '');

                        return css;
                    });
                });

                // set totals
                self.TrialPricesCount(totals_trialPrices);
                self.MatchCompetitorsCount(totals_matchCompetitors);
                self.StandardPricesCount(totals_standardPrices);

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

                    siteItem.canSendEmail = ko.observable(false);

                    siteItem.HasOverrides = ko.observable(false);

                    siteItem.hasEmailPricesChanges = function () {
                        for (var i = 0; i < siteItem.FuelPrices.length; i++) {
                            var currentFuel = siteItem.FuelPricesToDisplay[i].FuelPrice,
                                override = currentFuel.OverridePrice(),
                                today = currentFuel.TodayPrice,
                                auto = currentFuel.AutoPrice;

                            if (isNonZeroPrice(override)) {
                                // found 1 Override price ?
                                return true;
                            } else {
                                if (isNonZeroPrice(auto)) {
                                    if (isNonZeroPrice(today)) {
                                        var diff = auto - today;
                                        if (diff > pricingSettings.priceChangeVarianceThreshold)
                                            // found 1 price outside Variance ?
                                            return true;
                                    } else {
                                        return true;
                                    }
                                }
                            }
                        }
                        return false;
                    };

                    siteItem.bindEmailTemplate = function () {
                        var siteIds = [siteItem.SiteId];
                        commonShowEmailModal(siteIds);
                        return true;
                    };

                    siteItem.emailsInfoTip = function () {
                        var text = 'Open Email Popup',
                            i;
                        for (i = 0; i < siteItem.Emails.length; i++) {
                            text += '[br /][u]' + siteItem.Emails[i] + '[/u]';
                        }
                        return text;
                    };

                    siteItem.hasMissingCatNo = siteItem.CatNo < 1;
                    siteItem.hasMissingPfsNo = siteItem.PfsNo < 1;
                    siteItem.hasMissingStoreNo = siteItem.StoreNo < 1;
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
                var dieselPrice = getFuelDisplayById(siteItem.FuelPricesToDisplay, FUEL_DIESEL); // diesel = 6
                var superPrice = getFuelDisplayById(siteItem.FuelPricesToDisplay, FUEL_SUPER_UNLEADED); // superunleaded = 1 , lpg = 7

                var params = {
                    siteIdList: siteIds,
                    siteName: siteItem.StoreName,
                    siteId: siteItem.SiteId,
                    dayMonthYear: dmyStringToDMonYString(self.InitDate()),
                    prices: {
                        unleaded: (!unleadedPrice.Fuel) ? '----' : getEmailPreviewPrice(unleadedPrice.Fuel.FuelPrice),
                        diesel: (!dieselPrice.Fuel) ? '----' : getEmailPreviewPrice(dieselPrice.Fuel.FuelPrice),
                        superUnleaded: (!superPrice.Fuel) ? '----' : getEmailPreviewPrice(superPrice.Fuel.FuelPrice)
                    },
                    isViewingHistorical: self.ViewingHistorical(),
                    hasUnsavedChanges: self.HasUnsavedChanges(),
                    send: function (emailTemplateId, siteIds) {
                        var url = 'Sites/SendEmailToSite?emailTemplateId=' + emailTemplateId + '&siteIdsList=' + siteIds;
                        var $promise = common.callService("get", url, null);
                        self.sendEmail($promise, null, null);
                    },
                    enableSiteEmails: self.EnableSiteEmails(),
                    siteEmailTestAddresses: self.SiteEmailTestAddresses(),
                    settingsPageUrl: $('[data-menu-item="nav-settings"] a').attr('href')
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

            var getEmailPreviewPrice = function (fuelPrice) {
                var tomorrow = undefined,
                    override = fuelPrice.OverridePrice(),
                    auto = fuelPrice.AutoPrice,
                    today = fuelPrice.TodayPrice;

                if (isNonZeroPrice(override))
                    // Override ? --> Override
                    tomorrow = fuelPrice.OverridePrice()
                else {
                    if (isNonZeroPrice(auto)) {
                        tomorrow = auto;

                        if (isNonZeroPrice(today)) {
                            var diff = auto - today;
                            if (Math.abs(diff) <= pricingSettings.priceChangeVarianceThreshold)
                                tomorrow = undefined;
                        }
                    }
                }

                return tomorrow == undefined
                    ? '----'
                    : formatNumberTo1DecimalPlace(tomorrow);
            };

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
                            hasWarning: ko.observable(false),
                            isEditingFuel: ko.observable(false),
                            isMatchedByPriceMovementFilter: ko.observable(false),
                            DriveTime: fuelToDisplay.DriveTime.toFixed(2),
                            DriveTimePence: fuelsToDisplay.DriveTimePence,
                            Distance: fuelToDisplay.Distance.toFixed(2),
                            PriceSource: fuelToDisplay.PriceSource,
                            PriceSourceDateTime: fuelToDisplay.PriceSourceDateTime,
                            showToEditOverride: function () {
                                if (self.ViewingHistorical() || pagedata.DailyPriceData.IsOutdated)
                                    return;

                                f.isEditingFuel(true);
                                self.isEditing(true);
                                siteItem.isEditing(true);
                                var ele = $('#OverridePrice_' + siteItem.SiteId + '_' + f.FuelTypeId);
                                setTimeout(function () {
                                    ele.focus();
                                }, 100);
                            },
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
                                self.isEditing(true);
                            },
                            blurred: function (obj, newValue) {
                                var input = $(newValue.currentTarget),
                                    value = input.val(),
                                    formatted;

                                if (isNumber(value)) {
                                    //value = applyDecimalRounding(value * 10) / 10;
                                    formatted = formatNumberTo1DecimalPlace(value);
                                    input.val(formatted);
                                }

                                var siteId = obj.siteItem.SiteId,
                                    fn = function () {
                                        obj.siteItem.isEditing(false);
                                    };
                                f.isEditingFuel(false);
                                self.isEditing(false);

                                // deal with SuperUnleaded = Unleaded + Markup
                                if (obj.FuelTypeId == FUEL_UNLEADED) {
                                    if (isNumber(value)) {
                                        if (obj.FuelPrice.isOutsidePriceChangeVariance()) {
                                            var superUnleaded = obj.siteItem.FuelPricesToDisplay[2],
                                                superUnleadedPrice = formatNumberTo1DecimalPlace(Number(value) + pricingSettings.superUnleadedMarkupPrice);
                                            if (value >= fuelOverrideLimits[FUEL_UNLEADED].absolute.min) {
                                                setOverridePriceForSiteFuel(superUnleadedPrice, f.siteItem, FUEL_SUPER_UNLEADED);
                                                notify.info('Updated Super-Unleaded Price to Unleaded +' + pricingSettings.superUnleadedMarkupPrice);
                                            }
                                        }
                                    }
                                }

                                recalculateSainsburysStats();

                                delayedBlurs.blurSite(siteId, fn);
                            },
                            // Up = 1, down=-1, same=0, today or calc missing ="": returns computed values(1, -1, 0, "")
                            changeProp: function (obj, newValue) {
                                var wasPaused;
                                // prevent KO from sorting while user is typing Override Prices
                                switch (obj.FuelTypeId) {
                                    case FUEL_UNLEADED:
                                        wasPaused = sitePricingSorting.pauseForColumns([5, 6]);
                                        break;
                                    case FUEL_DIESEL:
                                        wasPaused = sitePricingSorting.pauseForColumns([9, 10]);
                                        break;
                                    case FUEL_SUPER_UNLEADED:
                                        wasPaused = sitePricingSorting.pauseForColumns([13, 14]);
                                        break;
                                }

                                if (wasPaused)
                                    self.sortHasBeenPaused(true);

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

                                    // do more validation, mark errors, etc...
                                    fuelObj.hasValidValue(self.validateOverridePrice(calculatedPrice, fuelObj.siteItem.SiteId, fuelObj.FuelTypeId, fuelObj.FuelPrice));

                                    fuelObj.FuelPrice.ValidateOverride();

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

                                // do more validation, mark errors, etc...
                                fuelObj.hasValidValue(self.validateOverridePrice(calculatedPrice, fuelObj.siteItem.SiteId, fuelObj.FuelTypeId, fuelObj.FuelPrice));

                                fuelObj.FuelPrice.ValidateOverride();

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
                                var limits = fuelOverrideLimits[f.FuelTypeId],
                                    css = '',
                                    isValid = f.hasValidValue(),
                                    isEmpty = f.FuelPrice.OverridePrice() == '',
                                    isWithinPriceChangeVariance = f.FuelPrice.isWithinPriceChangeVariance(),
                                    overridePrice = f.FuelPrice.OverridePrice(),
                                    isOutsideAbsoluteMinMax = isNonZeroPrice(overridePrice) && (overridePrice < limits.absolute.min || overridePrice > limits.absolute.max),
                                    isOutsideWarningLimits = f.FuelPrice.isOutsideWarningLimits();

                                if (isEmpty)
                                    return 'override-empty';
                                else if (!isValid || isOutsideAbsoluteMinMax)
                                    return 'override-invalid';

                                css = (isOutsideAbsoluteMinMax ? 'outside-absolute-limits' : 'inside-absolute-limits');

                                if (isWithinPriceChangeVariance)
                                    return css + ' override-valid override-within-price-change-variance';
                                else if (isOutsideWarningLimits)
                                    return css + ' override-exceeds-warnings-limits';
                                else
                                    return css + 'override-valid';
                            },
                            getOverridePrice: function () {
                                return f.FuelPrice.OverridePrice();
                            }
                        };

                        if (isNumber(f.FuelPrice.OverridePrice())) {
                            hasOverrides = true;
                        }

                        f.FuelPrice.isOverrideHidden = ko.computed(function () {
                            return self.isAutoHideShowOverrides()
                                && (f.siteItem.isEditing && !f.siteItem.isEditing())
                                && f.FuelPrice.OverridePrice() == '';
                        });

                        f.competitorDay1UnleadedCellCss = function () {
                            var css = (f.isDriveTimeBoundary() ? 'drive-time-boundary' : ''),
                                todayprice = f.FuelPrice.TodayPrice;

                            if (isNonZeroPrice(todayprice)) {
                                switch (f.FuelTypeId) {
                                    case FUEL_UNLEADED:
                                        css += (f.siteItem.isCompetitorForUnleaded ? ' competitor-fuel-unleaded' : '');
                                        break;
                                    case FUEL_DIESEL:
                                        css += (f.siteItem.isCompetitorForDiesel ? ' competitor-fuel-diesel' : '');
                                        break;
                                    case FUEL_SUPER_UNLEADED:
                                        css += (f.siteItem.isCompetitorForSuperUnleaded ? ' competitor-fuel-super-unleaded' : '');
                                        break;
                                }
                            } else {
                                css += ' competitor-price-na';
                            }
                            return css;
                        };

                        f.competitorPriceDiffHtml = function () {
                            var diff = undefined,
                                diffWithMarkup = undefined,
                                html = '',
                                fuelIndex = getIndexForFuelType(f.FuelTypeId),
                                jsSite = f.siteItem.jsSiteItem,
                                todayprice = f.FuelPrice.TodayPrice,
                                todaypriceWithMarkup = todayprice,
                                tomorrow = {
                                    override: jsSite.FuelPricesToDisplay[fuelIndex].FuelPrice.OverridePrice(),
                                    autoprice: jsSite.FuelPricesToDisplay[fuelIndex].FuelPrice.AutoPrice
                                },
                                driveTimePence = 0,
                                todayPriceWithMarkup = todayprice;

                            switch (f.FuelTypeId) {
                                case FUEL_UNLEADED:
                                    driveTimePence = f.siteItem.DrivePenceForUnleaded;
                                    break;
                                case FUEL_DIESEL:
                                    driveTimePence = f.siteItem.DrivePenceForDiesel;
                                    break;
                                case FUEL_SUPER_UNLEADED:
                                    driveTimePence = f.siteItem.DrivePenceForSuperUnleaded;
                                    break;
                            }

                            if (isNonZeroPrice(todayprice))
                                todayPriceWithMarkup = todayprice + driveTimePence;

                            if (isNonZeroPrice(tomorrow.override) && isNonZeroPrice(todayprice)) {
                                diff = tomorrow.override - todayprice;
                                diffWithMarkup = tomorrow.override - todaypriceWithMarkup;
                            }
                            else if (isNonZeroPrice(tomorrow.autoprice) && isNonZeroPrice(todayprice)) {
                                diff = tomorrow.autoprice - todayprice;
                                diffWithMarkup = tomorrow.autoprice - todaypriceWithMarkup;
                            }

                            if (diff == undefined) {
                                html = '<div class="comp-js-price-diff-na"><span class="comp-js-price-diff comp-diff-na">n/a</span></div>';
                            } else {
                                if (diff < 0) {
                                    html = '<div class="comp-js-price-diff comp-diff-down"><span>' + diff.toFixed(1) + '</span>';
                                    html += ' <i class="fa fa-arrow-down"></i>';
                                    html += '</div>';
                                }
                                else if (diff == 0) {
                                    html = '<div class="comp-js-price-diff comp-diff-none"><span>' + diff.toFixed(1) + '</span>';
                                    html += ' <i class="fa fa-arrow-right"></i>';
                                    html += '</div>';
                                }
                                else if (diff > 0) {
                                    html = '<div class="comp-js-price-diff comp-diff-up"><span>' + diff.toFixed(1) + '</span>';
                                    html += ' <i class="fa fa-arrow-up"></i>';
                                    html += '</div>';
                                }
                            }
                            return html;
                        };

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
                            else if (f.FuelPrice.isOutsideAbsoluteMinMax())
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
                                '{MARKUP}': formatNumber1DecimalWithPlusMinus(f.FuelPrice.Markup),
                                '{DISTANCE}': f.Distance,
                                '{DRIVETIME}': f.DriveTime,
                                '{PRICESOURCE}': formatPriceSource(f.PriceSource),
                                '{PRICESOURCEDATETIME}': formatPriceSourceDateTime(f.PriceSourceDateTime)
                            };
                            return f.FuelPrice.AutoPrice == 'n/a'
                                ? replaceTokens(messageFormats.priceIsNotAvailableForMatchCompetitor, tokens)
                                : replaceTokens(messageFormats.matchCompetitorInfoTip, tokens);
                        };

                        f.FuelPrice.StandardPriceInfoTip = function () {
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
                                replaceTokens(messageFormats.priceIsNotAvailableForStandardPrice, tokens)

                            if (competitorName == '')
                                return replaceTokens(messageFormats.standardPriceInfoTip, tokens);
                            else
                                return replaceTokens(messageFormats.standardPriceBasedOnCompetitorInfoTip, tokens);
                        };

                        f.FuelPrice.CompetitorPricePercentInfoTip = function () {
                            var summary = getCompetitorPriceSummary(f),
                                tokens = {
                                    '{COMPETITORCOUNT}': summary.CompetitorCount,
                                    '{COMPETITORPRICECOUNT}': summary.CompetitorPriceCount,
                                    '{COMPETITORPERCENT}': summary.CompetitorPricePercent,
                                    '{GROCERCOUNT}': summary.GrocerCount,
                                    '{GROCERPRICECOUNT}': summary.GrocerPriceCount,
                                    '{GROCERPERCENT}': summary.GrocerPricePercent
                                },
                            message = self.ShowingDataPercent() == 'all'
                                ? messageFormats.competitorDataPercentAllSummary
                                : messageFormats.competitorDataPercentGrocerSummary;

                            return replaceTokens(message, tokens);
                        };

                        f.FuelPrice.CompetitorPricePercentHtml = function () {
                            var summary = getCompetitorPriceSummary(f),
                                percent = self.ShowingDataPercent() == 'all' ? summary.CompetitorPricePercent : summary.GrocerPricePercent,
                                text = percent + '%';

                            if (percent == 100)
                                return '<div class="range-100">' + text + '</div>';
                            if (percent >= 75) 
                                return '<div class="range-75-to-99">' + text + '</div>';
                            if (percent >= 50)
                                return '<div class="range-50-to-74">' + text + '</div>';
                            if (percent >= 25)
                                return '<div class="range-25-to-49">' + text + '</div>';
                            return '<div class="range-0-to-24">' + text + '</div>';
                        };

                        f.FuelPrice.TodayPriceCss = ko.observable(function () {
                            switch (siteItem.PriceMatchType) {
                                case 1:
                                    return 'standard-price';
                                case 2:
                                    return 'trial-price';
                                case 3:
                                    return 'match-competitor-price';
                                default:
                                    return 'standard-price';
                            }
                        });

                        f.FuelPrice.TodayPriceTitle = ko.observable(function () {
                            switch (siteItem.PriceMatchType) {
                                case 1:
                                    return 'Standard Price ';
                                case 2:
                                    return 'Trial price: ' + f.FuelPrice.CompetitorName;
                                case 3:
                                    return 'Competitor: ' + f.FuelPrice.CompetitorName + '; Markup: ' + f.FuelPrice.Markup
                            }
                        });

                        f.FuelPrice.CompTodayPriceInfotip = function () {
                            var driveTimeMarkup = findDriveTimeMarkup(f),
                                incPrice = f.FuelPrice.TodayPrice > 0 ? Number(f.FuelPrice.TodayPrice) + driveTimeMarkup : f.FuelPrice.TodayPrice,
                                tokens = {
                                    '{NAME}': f.FuelTypeName,
                                    '{VALUE}': f.FuelPrice.TodayPrice,
                                    '{DRIVETIMEMARKUP}': driveTimeMarkup,
                                    '{INCPRICE}': incPrice
                                };
                            return replaceTokens(messageFormats.compTodayPriceInfotip, tokens);
                        };

                        f.FuelPrice.CompDay1PriceIncDriveTimePriceHtml = function () {
                            var price = f.FuelPrice.TodayPrice,
                                driveTimeMarkup = 0;
                            if (isNonZeroPrice(price)) {
                                driveTimeMarkup = findDriveTimeMarkup(f);
                                price = (Number(price) + driveTimeMarkup).toFixed(1);
                            }
                            return getFormattedForecourtPriceHtml('' + price);
                        };

                        f.FuelPrice.CompDay2YestPriceIncDriveTimePriceHtml = function () {
                            var price = f.FuelPrice.YestPrice,
                                driveTimeMarkup = 0;
                            if (isNonZeroPrice(price)) {
                                driveTimeMarkup = findDriveTimeMarkup(f);
                                price = (Number(price) + driveTimeMarkup).toFixed(1);
                            }
                            return getFormattedForecourtPriceHtml('' + price);
                        };

                        f.FuelPrice.YestPriceInfotip = function () {
                            var driveTimeMarkup = findDriveTimeMarkup(f),
                                incPrice = f.FuelPrice.YestPrice > 0 ? Number(f.FuelPrice.YestPrice) + driveTimeMarkup : f.FuelPrice.YestPrice,
                                tokens = {
                                    '{VALUE}': f.FuelPrice.YestPrice,
                                    '{NAME}': f.FuelTypeName,
                                    '{DRIVETIMEMARKUP}': driveTimeMarkup,
                                    '{INCPRICE}': incPrice
                                };
                            return replaceTokens(messageFormats.yestPriceInfoTip, tokens);
                        };

                        f.FuelPrice.ValidateOverride = function () {
                            var limits = fuelOverrideLimits[f.FuelTypeId],
                                overridePrice = f.FuelPrice.OverridePrice(),
                                valid = true;

                            if (isNumber(overridePrice)) {
                                valid = overridePrice >= limits.absolute.min && overridePrice <= limits.absolute.max;

                                if (!valid) {
                                    self.HasValidationErrors(true);
                                    fieldErrors.add(siteItem.SiteId, f.FuelTypeId);
                                    $('#msgError').text('One or more Fuel Price Overrides exceed their absolute price limits');
                                }
                            }

                            f.hasValidValue(valid);
                        };

                        f.TomorrowPriceCellCss = function () {
                            var siteFilter = self.siteEmailFilter(),
                                css = 'col-today'
                                + (isNonZeroPrice(f.FuelPrice.OverridePrice()) ? ' col-has-override' : ' col-has-no-override' )
                                + (f.isEditingFuel() ? ' col-editing' : '')
                                + (f.hasValidValue() ? ' col-valid' : ' col-invalid')
                                + (f.isMatchedByPriceMovementFilter() ? ' price-movement-filter-match' : '')
                                + (!f.siteItem.isEditing() && f.FuelPrice.isOverrideHidden() && !self.isDailyPriceFileOutdated() ? ' hide-empty-override' : '')
                                + (siteFilter == 'Inside Variance' && f.FuelPrice.isWithinPriceChangeVariance() ? ' inside-price-variance' : ' not-inside-price-variance')
                                + (siteFilter == 'Outside Variance' && f.FuelPrice.isOutsidePriceChangeVariance() ? ' outside-price-variance' : ' not-outside-price-variance');

                            return css;

                            //  ('col-today' + (isEditingFuel() ? ' col-editing' : '') + (hasValidValue() ? ' col-valid' : ' col-invalid') + (isMatchedByPriceMovementFilter() ? ' price-movement-filter-match' : '') + (!$parent.isEditing() && FuelPrice.isOverrideHidden() && !$root.isDailyPriceFileOutdated() ? ' hide-empty-override' : '')
                        };

                        f.UpDownCellCss = function () {
                            var siteFilter = self.siteEmailFilter(),
                                css = f.isEditingFuel() ? ' col-editing' : ''
                                + f.hasValidValue() ? ' col-valid' : ' col-invalid'
                                + f.FuelPrice.isMatchedByPriceMovementFilter() ? ' price-movement-filter-match' : '';

                            if (f.FuelPrice.isOutsideWarningLimits())
                                css += ' outside-prices-warning-limits';

                            switch (siteFilter) {
                                case 'Inside Variance':
                                    css += f.FuelPrice.isWithinPriceChangeVariance()
                                        ? ' inside-price-variance'
                                        : ' not-inside-price-variance'
                                    break;

                                case 'Outside Variance':
                                    css += f.FuelPrice.isOutsidePriceChangeVariance()
                                        ? ' outside-price-variance'
                                        : ' not-outside-price-variance';
                                    break;
                            }

                            return css;
                        };

                        f.UpDownTextCss = function () {
                            return f.FuelPrice.isWithinPriceChangeVariance()
                                ? ' price-diff-ignored'
                                : '';
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

            function getCompetitorPriceSummary(fuelObj) {
                var index = getIndexForFuelType(fuelObj.FuelTypeId),
                    summary = fuelObj.siteItem.SiteCompetitorsInfo.PriceSummaries[index];
                return summary;
            };

            self.getUpDownInfoTip = function (f) {
                var value = f.UpDownValue(),
                    sign = getUpDownValueSign(value),
                    isInsideVariance = isNumber(value) && Math.abs(value / 10) <= pricingSettings.priceChangeVarianceThreshold,
                    tokens = {
                        '{VALUE}': (Math.abs(value) / 10).toFixed(1),
                        '{NAME}': f.FuelTypeName,
                        '{VARIANCE}': ''
                    };
                return replaceTokens(messageFormats.updownInfoTips[sign], tokens);
            };

            self.validateOverridePrice = function (overridePrice, siteId, fuelId, fuelPrice) {
                var limits = fuelOverrideLimits[fuelId],
                    hasAutoPrice = isNonZeroPrice(fuelPrice.AutoPrice),
                    hasTodayPrice = isNonZeroPrice(fuelPrice.TodayPrice),
                    hasOverridePrice = overridePrice != '',
                    exceedsAbsoluteMinMax = isNonZeroPrice(overridePrice) && (overridePrice < limits.absolute.min || overridePrice > limits.absolute.max),
                    diff = 0,
                    isWithinPriceChangeVariance = false,
                    isOutsidePriceChangeVariance = false,
                    isOutsideWarningLimits = false,
                    limits = fuelOverrideLimits[fuelId],
                    hasError = false;

                // first check Override price (within Min/Max limits) vs TodayPrice (if any)
                if (hasTodayPrice && hasOverridePrice && !exceedsAbsoluteMinMax) {
                    diff = overridePrice - fuelPrice.TodayPrice;

                    isWithinPriceChangeVariance = Math.abs(diff) <= pricingSettings.priceChangeVarianceThreshold;
                    isOutsidePriceChangeVariance = !isWithinPriceChangeVariance;
                } else if (hasTodayPrice && hasAutoPrice) {
                    // then check today price vs AutoPrice (if any)
                    diff = fuelPrice.AutoPrice - fuelPrice.TodayPrice;
                    if (fuelPrice.AutoPrice < limits.absolute.min|| fuelPrice.AutoPrice > limits.absolute.max)
                        exceedsAbsoluteMinMax = true;
                    isWithinPriceChangeVariance = Math.abs(diff) <= pricingSettings.priceChangeVarianceThreshold;
                    isOutsidePriceChangeVariance = !isWithinPriceChangeVariance;
                };

                if (diff != 0 && (diff < limits.change.min) || diff > (limits.change.max))
                    isOutsideWarningLimits = true;

                fuelPrice.isOutsideAbsoluteMinMax(exceedsAbsoluteMinMax);
                fuelPrice.isWithinPriceChangeVariance(isWithinPriceChangeVariance);
                fuelPrice.isOutsidePriceChangeVariance(isOutsidePriceChangeVariance);
                fuelPrice.isOutsideWarningLimits(isOutsideWarningLimits);

                if (hasOverridePrice && !isNumber(overridePrice) || exceedsAbsoluteMinMax) {
                    hasError = true;
                }

                if (hasError) {
                    fieldErrors.add(siteId, fuelId);
                    self.hasAnyFieldErrors(fieldErrors.hasAny());
                    self.fieldErrorCount(fieldErrors.count());
                    $("#msgError").html("Invalid price value: " + overridePrice + ' - Please enter a value between ' + formatNumber1DecimalWithPlusMinus(limits.absolute.min) + ' and ' + formatNumber1DecimalWithPlusMinus(limits.absolute.max));
                    return false;
                } else {
                    fieldErrors.remove(siteId, fuelId);
                    self.hasAnyFieldErrors(fieldErrors.hasAny());
                    self.fieldErrorCount(fieldErrors.count());
                    $("#msgError").html("");
                    return true;
                }
            }

            var findDriveTimeMarkup = function(fuelObj) {
                switch (fuelObj.FuelPrice.FuelTypeId) {
                    case FUEL_UNLEADED:
                        return Number(fuelObj.siteItem.DrivePenceForUnleaded);
                    case FUEL_DIESEL:
                        return Number(fuelObj.siteItem.DrivePenceForDiesel);
                    case FUEL_SUPER_UNLEADED:
                        return Number(fuelObj.siteItem.DrivePenceForSuperUnleaded);
                }
                return 0;
            };

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
                if (!isNumber(todayPrice) || (!isNumber(autoPrice) && !isNumber(overridePrice)))
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
                var isInsideVariance = isNumber(upDownValue) && Math.abs(upDownValue / 10) <= pricingSettings.priceChangeVarianceThreshold,
                    ignoredCss = isInsideVariance ? ' price-diff-ignored' : '';

                switch (getUpDownValueSign(upDownValue)) {
                    case -1:
                        return 'fa fa-arrow-down price-diff-down' + ignoredCss;
                    case 0:
                        return 'fa fa-arrow-right price-diff-none' + ignoredCss;
                    case 1:
                        return 'fa fa-arrow-up price-diff-up' + ignoredCss;
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
                        '{NAME}': fuelTypeName,
                        '{VARIANCE}': formatIgnoredPriceVarianceInfotip(value)
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
                        '{NAME}': fuelObj.FuelTypeName,
                        '{VARIANCE}': ''
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
                    exceedAbsolutesMinMax = isNonZeroPrice(overrideValue) && (overrideValue < limits.absolute.min || overrideValue > limits.absolute.max),
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

                fuelObj.FuelPrice.isOutsideAbsoluteMinMax(exceedAbsolutesMinMax);
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


                fuelObj.siteItem.canSendEmail(fuelObj.siteItem.hasEmailPricesChanges());
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
                    isOutsideAbsoluteMinMax: ko.observable(false),
                    isWithinPriceChangeVariance: ko.observable(false),
                    isOutsidePriceChangeVariance: ko.observable(false),
                    isOutsideWarningLimits: ko.observable(false)
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
                self.highlightStandardPrices(false);
                self.highlightNoNearbyGrocerPrices(false);
                self.highlightHasNearbyGrocerPrices(false);
                self.highlightHasNearbyGrocerWithOutPrices(false);
                self.redrawGridHighlights();
                notify.info('Removed Price Highlighting');
                cookieSettings.writeBoolean('pricing.highlightTrialPrices', false);
                cookieSettings.writeBoolean('pricing.highlightMatchCompetitors', false);
                cookieSettings.writeBoolean('pricing.highlightStandardPrices', false);
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

            self.toggleHighlightStandardPrices = function () {
                var enabled = !self.highlightStandardPrices(),
                    message = enabled ? 'Highlighting ' + self.StandardPricesCount() + ' Standard Prices' : '';
                self.highlightStandardPrices(enabled);
                self.redrawGridHighlights();
                notify.info(message);
                cookieSettings.writeBoolean('pricing.highlightStandardPrices', enabled);
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
                    message = enabled ? 'Highlighting ' + self.NearbyGrocersWithoutPricesCount() + ' Sites with Nearby Grocer Incomplete Prices' : '';
                self.highlightHasNearbyGrocerWithOutPrices(enabled);
                self.redrawGridHighlights();
                notify.info(message);
                cookieSettings.writeBoolean('pricing.highlightHasNearbyGrocerWithOutPrices', enabled);
            };

            self.redrawGridHighlights = function () {
                var grid = $('.pricing-grid'),
                    showTrialPrices = self.highlightTrialPrices(),
                    showStandardPrices = self.highlightStandardPrices(),
                    showMatchCompetitorPrices = self.highlightMatchCompetitors(),
                    showNoNearbyGrocerPrices = self.highlightNoNearbyGrocerPrices(),
                    showHasNearbyGrocerPrices = self.highlightHasNearbyGrocerPrices(),
                    showHasNearbyGrocerWithOutPrices = self.highlightHasNearbyGrocerWithOutPrices();
                grid[showTrialPrices ? 'addClass' : 'removeClass']('highlight-trial-prices');
                grid[showMatchCompetitorPrices ? 'addClass' : 'removeClass']('highlight-match-competitor-prices');
                grid[showStandardPrices ? 'addClass' : 'removeClass']('highlight-standard-prices');
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
                self.startBusyLoadingData();
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
                        self.redrawGridHighlights();
                        self.dataLoading(false);
                    } else {
                        //$('#msgError').html("No data found");
                        self.HasErrorMessage(false);
                        self.dataAvailable(false);
                    }
                    self.stopBusyLoadingData();
                    $('.hide-first-load').removeClass('hide-first-load');
                    self.clearFirstPageLoad();
                    fieldErrors.removeAll();
                    self.fieldErrorCount(0);
                })
                .fail(function () {
                    $('#msgError').html("Sorry, we are unable to loaded the data &mdash; try reloading the page.");
                    self.HasErrorMessage(true);
                    self.dataAvailable(false);
                    self.clearFirstPageLoad();
                    self.dataLoading(false);
                    self.stopBusyLoadingData();
                });
                self.bind();
            };

            self.clearFirstPageLoad = function () {
                $('#SorryNoResultsPanel');
                self.firstPageLoad(false);
            };

            self.loadCompetitorDataForSite = function (site, promise, callback) {

                waiter.show({
                    title: 'Competitor Sites',
                    message: 'Loading Competitor Data...'
                });

                //self.busyLoadingData(true);
                promise.done(function (serverData, textStatus, jqXhr) {
                    waiter.hide();
                    site.loadingCompetitors(false);
                    if (serverData == "Error") {
                        $('#msgError').html("Error - no data");
                        self.HasErrorMessage(true);
                        site.hasCompetitors(false);
                    } else {
                        attachCompetitorsToSite(site, serverData, callback); // attach the right competitor list to the + site clicked
                    }
                    self.busyLoadingData(false);
                })
                    .fail(function () {
                        waiter.hide();
                        site.hasCompetitors(false);
                        site.loadingCompetitors(false);
                        $('#msgError').html("Error occured");
                        self.HasErrorMessage(true);
                        self.busyLoadingData(false);
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

                waiter.show({
                    title: 'Petrol Pricing',
                    message: 'Saving Petrol Pricing Overrides.'
                });


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
                        waiter.hide();
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
                        waiter.hide();
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
                pricingSettings.decimalRounding = settings.DecimalRounding;
                pricingSettings.superUnleadedMarkupPrice = settings.SuperUnleadedMarkupPrice;
                pricingSettings.enableSiteEmails = settings.EnableSiteEmails;
                pricingSettings.siteEmailTestAddresses = settings.SiteEmailTestAddresses;

                self.PriceChangeVariance(settings.PriceChangeVarianceThreshold);
                self.MaxGrocerDriveTime(settings.MaxGrocerDriveTimeMinutes);
                self.DecimalRounding(settings.DecimalRounding);
                self.SuperUnleadedMarkup(settings.SuperUnleadedMarkupPrice);
                self.EnableSiteEmails(settings.EnableSiteEmails);
                self.SiteEmailTestAddresses(settings.SiteEmailTestAddresses);

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


            // render the Price Change graph
            self.renderPriceChangeGraph = function () {
                var statsData = priceStats.getData(),
                    priceChangeOptions = {
                        show: {
                            view: self.priceChangeTabView(),
                            unleaded: self.priceChangesTabShowUnleaded(),
                            diesel: self.priceChangesTabShowDiesel(),
                            superUnleaded: self.priceChangesTabShowSuperUnleaded()
                        }
                    };

                // render the Price Changes tab
                priceChangesTab.render(priceChangeOptions, statsData);
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

                priceStats.gatherSiteFuelStats(sites);

                var formatted = priceStats.format();

                //
                // update KO (Knockout) stats
                //
                self.stats_Sites_Count(formatted.sites.count);
                self.stats_Sites_Active(formatted.sites.active);
                self.stats_Sites_WithEmails(formatted.sites.withEmails);
                self.stats_Sites_WithNoEmails(formatted.sites.withNoEmails);

                // unleaded tomorrow prices
                self.stats_Unleaded_Tomorrow_Price_Average(formatted.unleaded.tomorrow.price.average);
                self.stats_Unleaded_Tomorrow_Price_Count(formatted.unleaded.tomorrow.price.count);
                self.stats_Unleaded_Tomorrow_Price_Max(formatted.unleaded.tomorrow.price.max);
                self.stats_Unleaded_Tomorrow_Price_Min(formatted.unleaded.tomorrow.price.min);

                // unleaded tomorrow price changes
                self.stats_Unleaded_Tomorrow_PriceChanges_Average(formatted.unleaded.tomorrow.priceChanges.average);
                self.stats_Unleaded_Tomorrow_PriceChanges_Count(formatted.unleaded.tomorrow.priceChanges.count);
                self.stats_Unleaded_Tomorrow_PriceChanges_Max(formatted.unleaded.tomorrow.priceChanges.max);
                self.stats_Unleaded_Tomorrow_PriceChanges_Min(formatted.unleaded.tomorrow.priceChanges.min);

                // unleaded today prices
                self.stats_Unleaded_Today_Price_Average(formatted.unleaded.today.price.average);
                self.stats_Unleaded_Today_Price_Count(formatted.unleaded.today.price.count);
                self.stats_Unleaded_Today_Price_Max(formatted.unleaded.today.price.max);
                self.stats_Unleaded_Today_Price_Min(formatted.unleaded.today.price.min);

                // diesel tomorrow prices
                self.stats_Diesel_Tomorrow_Price_Average(formatted.diesel.tomorrow.price.average);
                self.stats_Diesel_Tomorrow_Price_Count(formatted.diesel.tomorrow.price.count);
                self.stats_Diesel_Tomorrow_Price_Max(formatted.diesel.tomorrow.price.max);
                self.stats_Diesel_Tomorrow_Price_Min(formatted.diesel.tomorrow.price.min);

                // diesel tomorrow price changes
                self.stats_Diesel_Tomorrow_PriceChanges_Average(formatted.diesel.tomorrow.priceChanges.average);
                self.stats_Diesel_Tomorrow_PriceChanges_Count(formatted.diesel.tomorrow.priceChanges.count);
                self.stats_Diesel_Tomorrow_PriceChanges_Max(formatted.diesel.tomorrow.priceChanges.max);
                self.stats_Diesel_Tomorrow_PriceChanges_Min(formatted.diesel.tomorrow.priceChanges.min);

                // diesel today prices
                self.stats_Diesel_Today_Price_Average(formatted.diesel.today.price.average);
                self.stats_Diesel_Today_Price_Count(formatted.diesel.today.price.count);
                self.stats_Diesel_Today_Price_Max(formatted.diesel.today.price.max);
                self.stats_Diesel_Today_Price_Min(formatted.diesel.today.price.min);

                // super-unleaded tomorrow prices
                self.stats_SuperUnleaded_Tomorrow_Price_Average(formatted.superUnleaded.tomorrow.price.average);
                self.stats_SuperUnleaded_Tomorrow_Price_Count(formatted.superUnleaded.tomorrow.price.count);
                self.stats_SuperUnleaded_Tomorrow_Price_Max(formatted.superUnleaded.tomorrow.price.max);
                self.stats_SuperUnleaded_Tomorrow_Price_Min(formatted.superUnleaded.tomorrow.price.min);

                // super-unleaded tomorrow price changes
                self.stats_SuperUnleaded_Tomorrow_PriceChanges_Average(formatted.superUnleaded.tomorrow.priceChanges.average);
                self.stats_SuperUnleaded_Tomorrow_PriceChanges_Count(formatted.superUnleaded.tomorrow.priceChanges.count);
                self.stats_SuperUnleaded_Tomorrow_PriceChanges_Max(formatted.superUnleaded.tomorrow.priceChanges.max);
                self.stats_SuperUnleaded_Tomorrow_PriceChanges_Min(formatted.superUnleaded.tomorrow.priceChanges.min);
                
                // super-unleaded today prices
                self.stats_SuperUnleaded_Today_Price_Average(formatted.superUnleaded.today.price.average);
                self.stats_SuperUnleaded_Today_Price_Count(formatted.superUnleaded.today.price.count);
                self.stats_SuperUnleaded_Today_Price_Max(formatted.superUnleaded.today.price.max);
                self.stats_SuperUnleaded_Today_Price_Min(formatted.superUnleaded.today.price.min);

                // combined tomorrow prices
                self.stats_Combined_Tomorrow_Prices_Average(formatted.combined.tomorrow.price.average);
                self.stats_Combined_Tomorrow_Prices_Count(formatted.combined.tomorrow.price.count);
                self.stats_Combined_Tomorrow_Prices_Max(formatted.combined.tomorrow.price.max);
                self.stats_Combined_Tomorrow_Prices_Min(formatted.combined.tomorrow.price.min);

                // combined tomorrow price changes
                self.stats_Combined_Tomorrow_PriceChanges_Average(formatted.combined.tomorrow.priceChanges.average);
                self.stats_Combined_Tomorrow_PriceChanges_Count(formatted.combined.tomorrow.priceChanges.count);
                self.stats_Combined_Tomorrow_PriceChanges_Max(formatted.combined.tomorrow.priceChanges.max);
                self.stats_Combined_Tomorrow_PriceChanges_Min(formatted.combined.tomorrow.priceChanges.min);

                // combined today prices
                self.stats_Combined_Today_Prices_Average(formatted.combined.today.price.average);
                self.stats_Combined_Today_Prices_Count(formatted.combined.today.price.count);
                self.stats_Combined_Today_Prices_Max(formatted.combined.today.price.max);
                self.stats_Combined_Today_Prices_Min(formatted.combined.today.price.min);

                self.renderPriceChangeGraph();
            };
        };

        //numeric extension
        ko.bindingHandlers.numeric = {
            init: function (element, valueAccessor) {
                $(element).on("keydown", function (event) {
                    console.log(event.keyCode, event.ctrlKey);
                    // Allow: backspace, delete, tab, escape, and enter
                    if (event.keyCode == 46 || event.keyCode == 8 || event.keyCode == 9 || event.keyCode == 27 || event.keyCode == 13 ||
                        // Allow: Ctrl-C (copy)
                        (event.keyCode == 67 && event.ctrlKey === true) ||
                        // Allow: Ctrl-V (paste)
                        (event.keyCode == 86 && event.ctrlKey === true) ||
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
                standardPrices = cookieSettings.readBoolean('pricing.highlightStandardPrices', false),
                noNearbyGrocerPrices = cookieSettings.readBoolean('pricing.highlightNoNearbyGrocerPrices', false),
                hasNearbyGrocerPrices = cookieSettings.readBoolean('pricing.highlightHasNearbyGrocerPrices', false),
                hasNearbyGrocerWithOutPrices = cookieSettings.readBoolean('pricing.highlightHasNearbyGrocerWithOutPrices', false);

            vm.highlightTrialPrices(trailPrices);
            vm.highlightMatchCompetitors(matchCompetitors);
            vm.highlightStandardPrices(standardPrices);
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

        function applySummaryTabCookieSettings(vm) {
            var tabIndex = cookieSettings.readInteger('pricing.priceSummaryTabIndex', 1);
            vm.setPricingTab(tabIndex, true);
        };

        function applyAutoHideOverrideCookieSettings(vm) {
            var enabled = cookieSettings.readBoolean('pricing.autoHideOverrides', false);
            vm.isAutoHideShowOverrides(enabled);
        }

        function restoreFromCookieSettings(vm) {
            applyHighlightCookieSettings(vm);
            applyPriceChangeFilterCookieSettings(vm);
            applySummaryTabCookieSettings(vm);
            applyAutoHideOverrideCookieSettings(vm);
        };

        function bindEvents(vm) {
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

            initExcludedBrands(vm);
        };

        /* excluded brands */
        function initExcludedBrands(vm) {
            // get initial value (page load)
            vm.excludedBrands.value = getExcludedBrands();

            // Note: onchange event fails, so done like this!
            setTimeout(function () { monitorExcludedBrands(vm); }, 5000);
        };

        function getExcludedBrands() {
            var val = $('#lstbxexcludebrands').val() || [];
            return val.join(',');
        };

        function setExcludedBrandsStatusAndMessage(vm, status) {
            var msg = vm.excludedBrands.messages[status];
            vm.savingExcludedBrandsMessage(msg.message)
            vm.savingExcludedBrandsCss(msg.css);
        };

        function saveExcludedBrands(vm) {
            vm.excludedBrands.isSaving = true;
            setExcludedBrandsStatusAndMessage(vm, 'saving');

            function clear() {
                if (vm.excludedBrands.unsaved)
                    setExcludedBrandsStatusAndMessage(vm, 'unsaved');
                else
                    setExcludedBrandsStatusAndMessage(vm, 'none');
                vm.excludedBrands.isSaving = false;
            };

            function failure() {
                setTimeout(function () {
                    setExcludedBrandsStatusAndMessage(vm, 'error');
                    setTimeout(clear, 5000);
                }, 1000);
            };
            function success() {
                setTimeout(function () {
                    vm.excludedBrands.unsaved = false;
                    setExcludedBrandsStatusAndMessage(vm, 'saved');
                    setTimeout(clear, 3000);
                }, 1000);
            };

            petrolPricingService.saveExcludedBrands(success, failure, vm.excludedBrands.value);
        };

        function monitorExcludedBrands(vm) {
            if (!vm.dataLoading() && !vm.excludedBrands.isSaving) {
                var excludedBrands = getExcludedBrands();
                if (excludedBrands != vm.excludedBrands.value) {
                    setExcludedBrandsStatusAndMessage(vm, 'unsaved');
                    vm.excludedBrands.ticker = vm.excludedBrands.countdown;
                    vm.excludedBrands.value = excludedBrands;
                    vm.excludedBrands.unsaved = true;
                }
            }
            if (!vm.excludedBrands.isSaving && vm.excludedBrands.unsaved) {
                if (vm.excludedBrands.ticker == 0)
                    saveExcludedBrands(vm);
                else if (vm.excludedBrands.ticker > 0)
                    vm.excludedBrands.ticker--;
            }
            setTimeout(function () { monitorExcludedBrands(vm); }, 300);
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

        var go = function () {

            moment.locale("en-gb");

            var vm = new PageViewModel();

            function afterNoteUpdate(model) {
                competitorPopup.afterNoteUpdate(model);
                vm.setCompetitorSiteNote(model.SiteId, model.Note);
            };

            function afterNoteDelete(siteModel) {
                competitorPopup.afterNoteDelete(siteModel);
                vm.setCompetitorSiteNote(siteModel.SiteId, '');
            };

            function afterNoteHide() {
                competitorPopup.afterNoteHide();
            };

            // competitor price grid popup
            competitorPopup.bindPopup();

            // competitor note popup
            competitorPopup.bindNotePopup({
                events: {
                    afterNoteUpdate: afterNoteUpdate,
                    afterNoteDelete: afterNoteDelete,
                    afterNoteHide: afterNoteHide
                }
            });

            var binder = function () {
                ko.applyBindings(vm, $("#petrolpricingpage")[0]);
            };
            vm.bind = binder;

            restoreFromCookieSettings(vm);

            bindEvents(vm);

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
            vm.startBusyLoadingData();
        };

        function initPricingPage(pagedataJSON) {
            pagedata = JSON.parse(pagedataJSON);
            pagedata.page = 'sitepricing';

            priceChangesTab.init('#canvasPriceChangesTab');
            go();
        };

        // API
        return {
            initPricingPage: initPricingPage,
            go: go
        };
    });