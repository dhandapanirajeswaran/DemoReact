define(["jquery", "knockout", "moment", "bootstrap-datepicker", "bootstrap-datepickerGB", "underscore", "common", "helpers", "URI", "competitorPricePopup", "notify", "busyloader"],
    function ($, ko, moment, datepicker, datePickerGb, _, common, helpers, URI, competitorPopup, notify, busyloader) {
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
            self.PriceDifferences =  ko.observableArray([]);

            self.HasUnsavedChanges = ko.observable(false); // default to false

            self.ViewingHistorical = ko.observable(false); // TODO useful to have for disabling few things 

            self.unleadedOffset = ko.observable('');
            self.dieselOffset = ko.observable('');
            self.superOffset = ko.observable('');
            self.changeTypeValues = ["(+/-n)", "Override all"];
            self.changeTypeValuePlusMinus = self.changeTypeValues[0];
            self.changeTypeValueOverrideAll = self.changeTypeValues[1];
            self.changeType = ko.observable(self.changeTypeValuePlusMinus);

            self.HasValidationErrors = ko.observable(false);

            self.HasSuccessMessage = ko.observable(false);
            self.HasErrorMessage = ko.observable(false);

            self.emailPopupSiteItemModel = ko.observable(); // List<SitePriceViewModel> object to be bound to main list
            self.showEmailPopup = ko.observable(false);

            self.showEmailAllPopup = ko.observable(false);

            self.busyLoadingData = ko.observable(true);
            self.firstPageLoad = ko.observable(true);
            self.hasBadPriceChangeValue = ko.observable(false);
            self.hasAnyFieldErrors = ko.observable(false);
            self.fieldErrorCount = ko.observable(0);

            self.showCompetitorPricesPopup = ko.observable(true);

            self.competitorPricesPopupSiteItemModel = ko.observable({});

            self.emailAllPopupSiteItemModel = ko.observable({
                pageHasUnsavedChanges: ko.computed(function () { return self.HasUnsavedChanges(); }),
                emailAllClick: function () {
                    self.showEmailAllPopup(true);
                    self.sendEmailToSite(null, null);
                },
                emailAllCloseClick: function () {
                    self.showEmailAllPopup(false);
                }
            });

            self.showCompetitorNotePopup = ko.observable(false);

            self.priceChangeFilter = ko.observable('All');
            self.priceChangeFilterUp = ko.observable(true);
            self.priceChangeFilterNone = ko.observable(true);
            self.priceChangeFilterDown = ko.observable(true);

            self.shouldShowSorryResults = ko.computed(function () {
                return self.hasAllSitesHidden()
                    || (!self.hasSites() && !self.busyLoadingData());
            });

            self.InitDateInUsFormat = function () {
                return moment(self.InitDate(), dmyFormatString).format(ymdFormatString);
            }

            // Ajax Call to SendEmailToSite (both params could be null)
            self.sendEmailToSite = function (popupObj, siteItem) {
                var url = "";
                if (siteItem == null) {
                    url = "Sites/SendEmailToSite?siteId=0"; // Send to all site
                } else {
                    url = "Sites/SendEmailToSite?siteId=" + siteItem.SiteId; // Send to single site
                };
                var $promise = common.callService("get", url, null); // args - maybe page no. (assuming no paging for now)
                self.sendEmail($promise, popupObj, siteItem);
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
                })
                    .fail(function () {
                        $('#msgError').html(messages.failure + ": " + response.ErrorSummaryString);
                        self.HasErrorMessage(true);
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

            self.bind = function () { };

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
            };

            self.setOverrideModeOverrideAll = function () {
                self.changeType(self.changeTypeValueOverrideAll);
                self.switchChangeType();
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


            var commonChangePricePerLitre = function (pplVal, fuelTypeId) {
                var message = (pplVal == '')
                    ? 'Removing Price Override for ' + fuelTypeNames[fuelTypeId]
                    : 'Appyling Price Override: ' + pplVal + ' to ' + fuelTypeNames[fuelTypeId];

                    applier = function () {
                    fieldErrors.clearForFuel(fuelTypeId);
                    self.fieldErrorCount(fieldErrors.count());
                    validateAndApplyPriceChange(pplVal, fuelTypeId);
                    busyloader.hide();
                };
                busyloader.show({
                    message: message,
                    dull: true
                });

                setTimeout(applier, 1500);
            };

            self.changePricePerLitreUnleaded = function () {
                var pplVal = ko.utils.unwrapObservable(self.unleadedOffset);
                commonChangePricePerLitre(pplVal, 2);
            }

            self.changePricePerLitreDiesel = function () {
                var pplVal = ko.utils.unwrapObservable(self.dieselOffset);
                commonChangePricePerLitre(pplVal, 6);
            };

            self.changePricePerLitreSuper = function () {
                var pplVal = ko.utils.unwrapObservable(self.superOffset);
                commonChangePricePerLitre(pplVal, 1);
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

            var setBadPriceChangeForFuelError = function(fuelTypeId) {
                $('#msgFuelTypeError').html(fuelTypeNames[fuelTypeId]);
                self.hasBadPriceChangeValue(true);
            };

            var isValidOffsetNumber = function (val) {
                return /^\-?\d{1,4}(\.\d{1})?$/.test(val);
            };

            var isValidOverrideNumber = function (val) {
                return /^\d{1,4}(\.\d{1})?$/.test(val) && parseFloat(val,10) > 0.00;
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

                self.hasSites(sitePricingView.sites.length != 0);
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
                    siteCount,
                    totalSiteCount = 0,
                    css,
                    barHeight,
                    lowerSiteCount = 0,
                    sameSiteCount = 0,
                    higherSiteCount = 0,
                    naSiteCount = 0;

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

                    css = diff == 'n/a' ? 'na-price' : diff < 0 ? 'lower-price' : diff > 0 ? 'higher-price' : 'same-price';
                    row1.push('<td style="width:' + cellWidthPixels + 'px;">');
                    row1.push('<div class="bar ' + css + '" style="border-bottom-width: ' + barHeight + 'px">' + siteCount + '</span>');
                    row1.push('</td>');
                    row2.push('<td class="diff ' + css + '">' + diff + '</td>');
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
                                enListOnlyFuelsToDisplay(competitorsForSite);
                                siteItem.competitors(competitorsForSite);
                            }
                        } else {
                            siteItem.hasCompetitors(false);
                        }

                        callback(siteItem);
                    });
                }
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
                            siteItem.EmailSendLogEntry(emailSendLogEntryForSite);
                            siteItem.hasEmailSendLogEntry(true);
                        }
                    });
                };
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
                _.each(sites, function (siteItem) {
                    siteItem.FuelPricesToDisplay = getFuelPricesToDisplay(siteItem, siteItem.FuelPrices);

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

                    siteItem.siteRowCss = ko.computed(function () {
                        if (siteItem.isEditing())
                            return 'focused';
                        return siteItem.hasRowErrors() ? 'row-errors' : '';
                    });
                });
            };

            var setEmailLogEntryFields = function (sites) {
                _.each(sites, function (siteItem) {
                    siteItem.EmailSendLogEntry = ko.observable();
                    siteItem.hasEmailSendLogEntry = ko.observable(false);
                });
            };

            var setCompetitorFields = function (sites) {
                _.each(sites, function (siteItem) {
                    siteItem.hasCompetitors = ko.observable(false);
                    siteItem.competitors = ko.observableArray([]);
                    siteItem.showingComps = ko.observable(false);
                    siteItem.loadingCompetitors = ko.observable(false);
                    siteItem.isEditing = ko.observable(false);

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

                        for (var i = 0; i < siteItem.FuelPrices.length; i++)
                        {
                            var currentFuel = siteItem.FuelPrices[i];                            

                            if(currentFuel.OverridePrice > 0 && currentFuel.TodayPrice - currentFuel.OverridePrice != 0) {
                                result = true;
                                break;
                            }
                            else if (currentFuel.OverridePrice ==0  && currentFuel.TodayPrice - currentFuel.AutoPrice != 0)
                            {
                                result = true;
                                break;
                            }
                        }
                        return result;
                    };

                    //siteItem.hasEmails = function () {
                    //    return 
                    //    return true;
                    //};


                    siteItem.bindEmailTemplate = function () {
                        
                        var dmonyDateString = dmyStringToDMonYString(self.InitDate());
                        var unleadedPrice = getFuelDisplayById(siteItem.FuelPricesToDisplay, 2); // unleaded = 2
                        var dieselprice = getFuelDisplayById(siteItem.FuelPricesToDisplay, 6); // diesel = 6
                        var superprice = getFuelDisplayById(siteItem.FuelPricesToDisplay, 1); // superunleaded = 1 , lpg = 7

                        var p1 = (!unleadedPrice.Fuel) ? 'n/a' :
                            getOverrideOrAutoPrice(unleadedPrice.Fuel.FuelPrice.OverridePrice(), unleadedPrice.Fuel.FuelPrice.AutoPrice);
                        var p2 = (!dieselprice.Fuel) ? 'n/a' :
                            getOverrideOrAutoPrice(dieselprice.Fuel.FuelPrice.OverridePrice(), dieselprice.Fuel.FuelPrice.AutoPrice);
                        var p3 = (!superprice.Fuel) ? 'n/a' :
                            getOverrideOrAutoPrice(superprice.Fuel.FuelPrice.OverridePrice(), superprice.Fuel.FuelPrice.AutoPrice);

                        // Knockout: New approach to show popup and click handling with knockout binding..
                        var popupModel = {
                            kSiteItem: siteItem,
                            kSiteName: siteItem.StoreName,
                            kStartDateMonthYear: dmonyDateString,
                            kUnleadedPrice: p1,
                            kDieselPrice: p2,
                            kSuperPrice: p3,
                            showEmailSiteButton: !self.ViewingHistorical(),
                            pageHasUnsavedChanges: self.HasUnsavedChanges(),
                            emailSendClick: function () {
                                var site = siteItem;
                                var popupObj = this;
                                self.sendEmailToSite(popupObj, site);
                            },
                            hideEmailPopup: function () {
                                self.showEmailPopup(false);
                            }
                        };
                        self.showEmailPopup(true);

                        // The pop now observes this popupModel..
                        self.emailPopupSiteItemModel(popupModel);
                        // END New approach

                        return true;
                    }
                });
            }


            var buildPriceDifferences=function () {
                
                var sitesVm = ko.utils.unwrapObservable(self.dataModel);
                var sites = sitesVm.sites;
                _.each(sites, function (siteItem) {
                    
                    for (var i = 0; i < siteItem.FuelPrices.length; i++) {
                        var currentFuel = siteItem.FuelPrices[i];

                        if (currentFuel.FuelTypeId == 2) {
                            var difference = (currentFuel.AutoPrice - currentFuel.TodayPrice)/10;
                            if (currentFuel.AutoPrice == 0 || currentFuel.TodayPrice == 0) difference = "n/a";
                            //if (difference != 999) {
                            var _item = ko.utils.arrayFirst(self.PriceDifferences(), function (item) {
                                return item.key == difference;
                            });

                            if (_item == null) {
                                self.PriceDifferences.push({ key: difference, value: 1 });
                            }
                            else {
                                _item = ko.utils.arrayFirst(self.PriceDifferences(), function (item) {
                                    return item.key == difference;
                                });
                                    
                                _item.value++;
                                self.PriceDifferences.remove(function (item) {
                                    return item.key == difference;
                                });
                                self.PriceDifferences.push({ key: difference, value: _item.value
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
                var siteFuels = siteFuelPrices;
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
                            hasValidValue: ko.observable(true),
                            isEditingFuel: ko.observable(false),
                            focused: function (obj, newValue) {
                                var siteId = obj.siteItem.SiteId;
                                delayedBlurs.focusSite(siteId);
                                obj.siteItem.isEditing(true);
                                f.isEditingFuel(true);
                            },
                            blurred: function (obj, newValue) {
                                var siteId = obj.siteItem.SiteId,
                                    fn = function () {
                                        obj.siteItem.isEditing(false);
                                    };
                                f.isEditingFuel(false);
                                delayedBlurs.blurSite(siteId, fn);
                            },
                            // Up = 1, down=-1, same=0, today or calc missing ="": returns computed values(1, -1, 0, "") 
                            changeProp: function (obj, newValue) {
                                var newPrice = $(newValue.target).val();
                                obj.hasValidValue(self.validateOverridePrice(newPrice, siteItem.SiteId, fuelToDisplay.FuelTypeId));
                                if (obj.hasValidValue()) {
                                    var fuelPrice = obj.FuelPrice;

                                    obj.FuelPrice.OverridePrice(newPrice);

                                    finalisePriceChange(obj, fuelPrice.TodayPrice, newPrice, fuelPrice.AutoPrice, 1);

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
                                var calculatedPrice = 0;
                                if (todayPrice > 0) {
                                    calculatedPrice = +todayPrice + +pplValue;
                                    // limit to 1 decimal place
                                    calculatedPrice = parseFloat(Math.round(calculatedPrice * 10) / 10).toFixed(1);

                                    fuelObj.FuelPrice.OverridePrice(calculatedPrice);
                                    finalisePriceChange(fuelObj, fuelPrice.TodayPrice, calculatedPrice, fuelPrice.AutoPrice, 1);

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
                            }, // end setPlusMinusPpl
                            clearOverridePpl: function (fuelObj) {
                                var fuelPrice = fuelObj.FuelPrice;
                                fuelObj.FuelPrice.OverridePrice('');

                                finalisePriceChange(fuelObj, fuelPrice.TodayPrice, 0, fuelPrice.AutoPrice, 1);
                                fuelObj.hasValidValue(true);
                            }, // end clearOverridePpl
                            OverrideInputCss: function () {
                                return f.hasValidValue() ? f.FuelPrice.OverridePrice() == '' ? 'override-empty' : 'override-valid' : 'override-invalid';
                            }
                        };

                        f.UpDownValue = ko.observable(getUpDownValue(f.FuelPrice.TodayPrice, f.FuelPrice.OverridePrice(), f.FuelPrice.AutoPrice, 1)); // this works fine

                        f.UpDownIconCss = ko.observable(getUpDownIconCss(f.UpDownValue()));

                        f.UpDownText = ko.observable(getFormattedUpDownValue(f.UpDownValue(), '-'));

                        f.UpDownSignCss = ko.observable(getUpDownSignCss(f.UpDownValue()));

                        f.FuelPrice.ForecourtAutoPriceHtml = ko.observable(getFormattedForecourtPriceHtml(f.FuelPrice.AutoPrice));

                        f.FuelPrice.ForecourtTodayPriceHtml = ko.observable(getFormattedForecourtPriceHtml(f.FuelPrice.TodayPrice));

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
                return returnValue; // always contains 3 items only (as per length of fuelsToDisplay array
            };

            self.validateOverridePrice = function (priceValue, siteId, fuelId) {
                if (priceValue < 0 || priceValue > 400 || isNaN(priceValue)) {
                    fieldErrors.add(siteId, fuelId);
                    self.hasAnyFieldErrors(fieldErrors.hasAny());
                    self.fieldErrorCount(fieldErrors.count());
                    $("#msgError").html("Invalid price value: " + priceValue);
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
                    return { Fuel: fuelToFind };
                } else {
                    return { fuelToFind: null };
                }
            };

            var getUpDownValue = function (todayPrice, overridePrice, autoPrice, divisor) {
                var chg = 0;
                if (isNaN(todayPrice) || isNaN(autoPrice) || isNaN(overridePrice)) return 0; // isNaN('') = false -- so wierd !!

                var tomorrowPrice = (overridePrice && overridePrice > 0) ? overridePrice : autoPrice;
                chg = (tomorrowPrice - todayPrice) * 10;
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
                        return '-' + Math.abs(upDownValue) / 10;
                    case 0:
                        return zeroString;
                    case +1:
                        return '+' + Math.abs(upDownValue) / 10;
                }
            };

            var getFormattedForecourtPriceHtml = function (autoPrice) {
                var parts = (autoPrice.toString() + '.').split('.');
                return '<span class="price-pence">' + parts[0] + '</span><span class="price-fraction">.' + parts[1] + '</span>';
            };

            var getUpDownSignCss = function (upDownValue) {
                var sign = getUpDownValueSign(upDownValue);
                return sign == 0 ? 'none' : sign < 0 ? 'down' : 'up';
            };

            var finalisePriceChange = function (fuelObj, todayPrice, overridePrice, autoPrice, divisor) {
                var updown = getUpDownValue(todayPrice, overridePrice, autoPrice, divisor);

                fuelObj.UpDownValue(updown);

                self.HasUnsavedChanges(false);

                var hasChanges = (serialisePostbackData(0) != null && serialisePostbackData(0).length > 0);

                self.HasUnsavedChanges(hasChanges);

                self.HasValidationErrors(false);
            }

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
                }
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

            self.commonTogglePriceChangeFilter = function (obj) {
                var toggled = !(obj.observer()),
                    message = toggled ? obj.showing : obj.hiding;
                obj.observer(toggled);
                self.redrawFuelPriceChanges();
                filterPriceChangeSiteRow();
                notify.info(message);
            };

            self.togglePriceChangeFilterUp = function () {
                self.commonTogglePriceChangeFilter({
                    observer: self.priceChangeFilterUp,
                    showing: 'Showing upward price changes',
                    hiding: 'Hiding upward price changes'
                });
            };

            self.togglePriceChangeFilterNone = function () {
                self.commonTogglePriceChangeFilter({
                    observer: self.priceChangeFilterNone,
                    showing: 'Showing no price changes',
                    hiding: 'Hiding no price changes'
                });
            };

            self.togglePriceChangeFilterDown = function () {
                self.commonTogglePriceChangeFilter({
                    observer: self.priceChangeFilterDown,
                    showing: 'Showing downward price changes',
                    hiding: 'Hiding downward price changes'
                });
            };

            self.resetPriceChangeFilters = function () {
                self.priceChangeFilterDown(true);
                self.priceChangeFilterNone(true);
                self.priceChangeFilterUp(true);
                self.redrawFuelPriceChanges();
                filterPriceChangeSiteRow();
                notify.info('Price Change filters have been reset.')
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
                    showNone = self.priceChangeFilterNone();
                rows.each(function () {
                    var row = $(this),
                        signs = row.data('price-change-signs'),
                        visible,
                        hasRowErrors = row.hasClass('row-errors') ;
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
                    success: function (data) 
                    {
                        var yyyymmdd = dmyStringToYmdString(self.InitDate());

                        window.location.href = common.getSiteRoot() + "Sites/Prices?date=" + yyyymmdd
                        + "&storeName=" + escape(self.StoreName())
                        + "&catNo=" + escape(self.CatNo())
                        + "&storeNo=" + escape(self.StoreNo())
                        + "&storeTown=" + escape(self.StoreTown())
                        + "&priceChanges=" + escape(self.priceChangeFilter());
                    }, 
                    error: function (err) 
                    { 
                        var yyyymmdd = dmyStringToYmdString(self.InitDate());

                        window.location.href = common.getSiteRoot() + "Sites/Prices?date=" + yyyymmdd
                        + "&storeName=" + escape(self.StoreName())
                        + "&catNo=" + escape(self.CatNo())
                        + "&storeNo=" + escape(self.StoreNo())
                        + "&storeTown=" + escape(self.StoreTown())
                        + "&priceChanges=" + escape(self.priceChangeFilter());
                    } });
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

            var serialisePostbackData = function (siteId) {
                var sitesVm = ko.utils.unwrapObservable(self.dataModel);
                var sites = sitesVm.sites;
                var retval = [];
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
                            priceHasChanged = hasOverridePriceChanged(siteItem, fuelDisplayItem.FuelTypeId);
                        }

                        if (priceHasChanged) {
                            var ovrPrice = fuelDisplayItem.FuelPrice.OverridePrice() + ''; // convert price back to string
                            ovrPrice = ovrPrice.replace('   ', '');
                            ovrPrice = ovrPrice.trim();
                            if (ovrPrice == '') ovrPrice = 0.0;
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
            self.savePetrolPricingForm = function () {
                // Simpler VM - SiteId, FuelId, OverridePrice (only ones which are non blank), for faster updates and avoid unnecessary ones
                var postbackData = serialisePostbackData(0);

                var messages = getMessages("Save");
                $.ajax({
                    url: common.getSiteRoot() + "Sites/SavePriceOverrides", // Put method for Update - Placeholder created
                    method: "POST",
                    data: { postbackKey1: postbackData }, // Postback a List<OverridePricePostViewModel>, {postbackKey1: etc must match Controller method param name}
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
        };
        return { go: go };
    });



