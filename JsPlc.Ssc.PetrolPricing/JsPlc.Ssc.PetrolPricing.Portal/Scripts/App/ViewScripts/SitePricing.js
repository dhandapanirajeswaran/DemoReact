define(["jquery", "knockout", "moment", "bootstrap-datepicker", "bootstrap-datepickerGB", "underscore", "common", "helpers", "URI"],
function ($, ko, moment, datepicker, datePickerGb, _, common, helpers, URI) {
    var getMessages = function (crudMode) {
        switch (crudMode) {
            case "Edit":
                return {
                    success: "Site prices updated",
                    failure: "Site price update failed"
                };
            default:
                return "Undefined page mode";
        };
    };
    //var getDatePickerValue = function () {
    //    var txtBoxDate = $('#viewingDate').val();
    //    return txtBoxDate;
    //}

    var dmyFormatString = "DD/MM/YYYY";
    var dmonyFormatString = "DD MMM YYYY";
    var ymdFormatString = "YYYY-MM-DD";

    var ukDateSample = moment("16/12/2015", dmyFormatString).format(dmyFormatString);
    var usDateSample = moment("16/12/2015", dmyFormatString).format(ymdFormatString);
    var todayDateStringUkFormat = function() {
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

    // VM for Page
    function PageViewModel() {

        var self = this;

        // Define fields that need to be bound to the page

        self.dataModel = ko.observable(""); // List<SitePriceViewModel> object to be bound to main list
        self.dataAvailable = ko.observable('');
        //self.PickedDate = function () {
        //    return getDatePickerValue();
        //};

        self.InitDate = ko.observable('');
        self.HasUnsavedChanges = ko.observable(false); // default to false

        self.ViewingHistorical = ko.observable(false); // TODO useful to have for disabling few things 

        // only run when new data is loaded
        self.ShowOrHideSaveButton = function () {
            // Useful checking can be done here
            if (self.InitDate() == todaysDateUkformat) {
            } else {
            }
        }

        self.HasValidationErrors = ko.observable(false);

        self.EmailPopupForSiteId = ko.observable('');
        self.sendEmail = function (siteItem) {
            self.EmailPopupForSiteId(siteItem.SiteId); // sets up the obervable
        }

        self.bind = function () { };

        // End define

        var fuelsToDisplay = [2, 6, 1]; // fuel columns (Unleaded, Diesel, SuperUnleaded)

        var sitePricingView;

        self.setupDatePicker = function (ukDate) {
            moment.locale("en-gb"); // Set Locale for moment (aka moment.locale("en-gb"))
            //var forDate = moment(IsoDate).format("L"); // we get dd/mm/yyyy
            debugger;
            self.InitDate(ukDate);

            self.ViewingHistorical(true);
            if (self.InitDate() == todaysDateUkformat) {
                self.ViewingHistorical(false);
            }
            //Init the calendar to the forDate 
            //if (moment(forDate, dmyFormatString).isValid()) {
            //    $('.datepicker').datepicker("setDate", initDate);
            //} else {
            //    $('.datepicker').datepicker("setDate", new Date());
            //}
        }

        var buildViewModels = function (serverData) {

            // Page will have data items: List of items, pageNo, Date
            sitePricingView = {
                // We don't know how many pages we have until we have a ajax call for count of pages 
                // or return that within this get request
                pageNo: 1, //pageNo,
                sites: serverData, // array/list of sitePriceViewModels
            };

            setCompetitorFields(sitePricingView.sites); // sets up 2 additional fields per site hasCompetitors and competitors = [];
            enListOnlyFuelsToDisplay(sitePricingView.sites); // adds another prop to siteItem - FuelPricesToDisplay

            self.dataModel(sitePricingView);
        };

        // map all competitors from ajax call to inidividual site
        var attachCompetitorsToSite = function (site, serverCompetitors) {
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
                        var competitorsForSite = _.filter(serverCompetitors, function(compItem) {
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
                });
            }
        }

        var toggleCompDisplay = function (siteItem) {
            if (siteItem.SiteId != 0) {
                if (siteItem.showingComps()) {
                    $('#AjaxCollapseCompetitorsforsite' + siteItem.SiteId).hide();
                    siteItem.showingComps(false);
                } else {
                    $('#AjaxCollapseCompetitorsforsite' + siteItem.SiteId).show();
                    siteItem.showingComps(true);
                }
            }
        }
        var enListOnlyFuelsToDisplay = function (sites) {
            _.each(sites, function (siteItem) {
                siteItem.FuelPricesToDisplay = getFuelPricesToDisplay(siteItem, siteItem.FuelPrices);
            });
        }

        var setCompetitorFields = function (sites) {
            _.each(sites, function (siteItem) {
                siteItem.hasCompetitors = ko.observable(false);
                siteItem.competitors = ko.observableArray([]);
                siteItem.showingComps = ko.observable(false);
                siteItem.loadingCompetitors = ko.observable(false);

                siteItem.getCompetitorDataClick = function() {
                    toggleCompDisplay(siteItem);
                    siteItem.loadingCompetitors(true);
                    self.getCompetitorDataForSite(siteItem);
                    return true;
                };
                // TODO - disable send button in template, if its not today
                siteItem.bindEmailTemplate = function () {
                    // TODO - IMPORTANT Email Site button should save that site's price and then send email, since we're showing UI entries in email popup, not DB entries
                    // OR we could ajax the price back to save to sitePrice, before showing popup. Note, we don't need to refresh the site object for which ajax save was called

                    var dmonyDateString = dmyStringToDMonYString(self.InitDate());
                    var unleadedPrice = getFuelDisplayById(siteItem.FuelPricesToDisplay, 2); // unleaded = 2
                    var dieselprice = getFuelDisplayById(siteItem.FuelPricesToDisplay, 6); // diesel = 6
                    var superprice = getFuelDisplayById(siteItem.FuelPricesToDisplay, 1); // superunleaded = 1 , lpg = 7

                    replaceTemplateValue("kStartDateMonthYear", dmonyDateString);
                    replaceTemplateValue("kSiteName", siteItem.StoreName);
                    replaceTemplateValue("kUnleadedPrice", "n/a");
                    replaceTemplateValue("kDieselPrice", "n/a");
                    replaceTemplateValue("kSuperPrice", "n/a");

                    $("#EmailSiteButton").prop('onclick', '').click(self.sendEmail(siteItem)); // sets up the siteId 

                    var p1 = getOverrideOrAutoPrice(unleadedPrice.Fuel.FuelPrice.OverridePrice(), unleadedPrice.Fuel.FuelPrice.AutoPrice);
                    var p2 = getOverrideOrAutoPrice(dieselprice.Fuel.FuelPrice.OverridePrice(), dieselprice.Fuel.FuelPrice.AutoPrice);
                    var p3 = getOverrideOrAutoPrice(superprice.Fuel.FuelPrice.OverridePrice(), superprice.Fuel.FuelPrice.AutoPrice);

                    if (unleadedPrice) replaceTemplateValue("kUnleadedPrice", p1);
                    if (dieselprice) replaceTemplateValue("kDieselPrice", p2);
                    if (superprice) replaceTemplateValue("kSuperPrice", p3);

                    if (self.ViewingHistorical()) $('#EmailSiteButton').hide();
                    else $('#EmailSiteButton').show();

                    return true;
                }
            });
        }
        var getOverrideOrAutoPrice = function (overridePrice, autoPrice) {
            if (isNaN(overridePrice) || overridePrice=='') {
                if (isNaN(autoPrice) || autoPrice == '') {
                    return 0;
                } else {
                    return autoPrice;
                }
            } else {
                return overridePrice;
            }
        }


        var replaceTemplateValue = function(idToFind, valueToSet) {
            var placeHolder = $("#" + idToFind);
            if (placeHolder) {
                placeHolder.html(valueToSet);
            }
        };

        var getFuelPricesToDisplay = function(siteItem, siteFuelPrices) {
            var returnValue = [];
            var siteFuels = siteFuelPrices;
            _.each(fuelsToDisplay, function(id) {
                var fuelToDisplay = _.find(siteFuels, function(item) {
                    return (item.FuelTypeId == id);
                });
                if (fuelToDisplay) {
                    var f = {
                        siteSupportsFuel: true,
                        FuelPrice: self.getFormattedObservableFuelPrice(fuelToDisplay), // FuelPrice: observable changes on OverridePrice input
                        FuelTypeId: fuelToDisplay.FuelTypeId,
                        // Up = 1, down=-1, same=0, today or calc missing ="": returns computed values(1, -1, 0, "") 
                        changeProp: function(obj, newValue) {
                            var newPrice = $(newValue.target).val();

                            var valueIsValid = self.validateOverridePrice(newPrice, siteItem.SiteId, fuelToDisplay.FuelTypeId);
                            if (valueIsValid) {
                                var fuelPrice = obj.FuelPrice;

                                obj.FuelPrice.OverridePrice(newPrice);
                                console.log("TodayPrice: " + fuelPrice.TodayPrice);
                                console.log("OverridePrice: " + newPrice);
                                console.log("AutoPrice: " + fuelPrice.AutoPrice);

                                var updown = getUpDownValue(fuelPrice.TodayPrice, newPrice, fuelPrice.AutoPrice, 1);
                                console.log("Chg: " + updown);
                                obj.UpDownValue(updown);

                                self.HasUnsavedChanges(false);
                                var hasChanges = (serialisePostbackData(0) != null && serialisePostbackData(0).length > 0);
                                self.HasUnsavedChanges(hasChanges);

                                self.HasValidationErrors(false);

                                return true;
                            } else {
                                obj.UpDownValue('0');
                                self.HasValidationErrors(true);
                                self.HasUnsavedChanges(false);
                                return true; //false;
                            }
                        },
                    };

                    f.UpDownValue = ko.observable(getUpDownValue(f.FuelPrice.TodayPrice, f.FuelPrice.OverridePrice(), f.FuelPrice.AutoPrice, 1)); // this works fine
                    returnValue.push(f);
                } else {
                    returnValue.push({
                        siteSupportsFuel: false,
                        FuelPrice: null,
                        FuelTypeId: -1,
                        UpDownValue: ko.observable(''),
                });
                }
            });
            return returnValue; // always contains 3 items only (as per length of fuelsToDisplay array
        };

        self.validateOverridePrice = function (priceValue, siteId, fuelId) {
            var selector = '#OverridePriceHighlight_' + siteId + '_' + fuelId;
            console.log("Field marker selector: " + selector);

            var overRideErrorMarker = $(selector);
            debugger;
            if (priceValue < 0 || priceValue > 400 || isNaN(priceValue)) {
                overRideErrorMarker.html('**');
                $("#msgs").html("<ul class='pageErrorMsg' style='color: red'>Invalid price value:" + priceValue + "</ul>");
                return false;
            } else {
                overRideErrorMarker.html('');
                $("#msgs").html("");
                return true;
            }
        }

        var findFuelPriceById = function(fuelPrices, id) {
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

        var getFuelDisplayById = function(siteFuelPricesToDisplay, fuelTypeId) {
            var fuelToFind = _.find(siteFuelPricesToDisplay, function(item) {
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

        self.getFormattedObservableFuelPrice = function(fuelPriceToDisplay) {
            return {
                FuelTypeId: fuelPriceToDisplay.FuelTypeId,
                YestPrice: self.formatValueTo1DP(fuelPriceToDisplay.YestPrice, "n/a"),
                TodayPrice: self.formatValueTo1DP(fuelPriceToDisplay.TodayPrice, "n/a"),
                AutoPrice: self.formatValueTo1DP(fuelPriceToDisplay.AutoPrice, "n/a"),
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

        self.reloadPage = function () {
            window.location.href = common.getSiteRoot() + "Sites/Prices";
        }

        self.loadNewDateData = function () {
            // TODO check for unsaved changes, if hasChanges, prompt user, if Cancel clicked, return false, else redirect as below.. 
            var yyyymmdd = dmyStringToYmdString(self.InitDate());
            window.location.href = common.getSiteRoot() + "Sites/Prices?date=" + yyyymmdd;
        }
        // ### GET SitePricing Data
        self.loadPageData = function (promise) {
            //$.ajax({
            //    url: common.getSiteRoot() + "Sites/GetSitesWithPricesJson/?" + "forDate" + "=" + forDate
            //    method: "GET"
            //})
            promise.done(function (serverData, textStatus, jqXhr) {
                if (serverData == "Error") {
                    $('#msgs').html("Error - no data");
                    self.dataAvailable(false);
                }
                else {
                    // $('#msgs').html("Fetch success");
                    self.dataAvailable(true);
                    buildViewModels(serverData);
                    self.ShowOrHideSaveButton();
                }
            })
            .fail(function () {
                self.dataAvailable(false);
                $('#msgs').html("Error occured");
            });

            self.bind();
        }

        self.loadCompetitorDataForSite = function(site, promise) {
            promise.done(function(serverData, textStatus, jqXhr) {
                site.loadingCompetitors(false);
                if (serverData == "Error") {
                        $('#msgs').html("Error - no data");
                        site.hasCompetitors(false);
                    } else {
                        // $('#msgs').html("Fetch success");
                        attachCompetitorsToSite(site, serverData); // attach the right competitor list to the + site clicked
                    }
                })
                .fail(function() {
                    site.hasCompetitors(false);
                    site.loadingCompetitors(false);
                    $('#msgs').html("Error occured");
                });
            //self.bind(); // Call a diff binder (or do we even need to, if we just populated the competitors array to the right site)
        }

        var serialisePostbackData = function (siteId) {
            var sitesVm = ko.utils.unwrapObservable(self.dataModel);
            var sites = sitesVm.sites;
            var retval = [];
            debugger;
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
                    //if (fuelDisplayItem.FuelPrice.OverridePrice() != ''
                    // && !isNaN(fuelDisplayItem.FuelPrice.OverridePrice()
                    // && fuelDisplayItem.FuelPrice.OverridePrice() != '0')) 

                    if (priceHasChanged) {
                        var ovrPrice = fuelDisplayItem.FuelPrice.OverridePrice();
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
            debugger;

            //console.log("Form data:" + $('#myform').serialize());
            //console.log("Self.DataModel:" + self.dataModel());

            // Simpler VM - SiteId, FuelId, OverridePrice (only ones which are non blank), for faster updates and avoid unnecessary ones
            var postbackData = serialisePostbackData(0);
            console.log("Form data(json):" + ko.toJSON(postbackData));
            //console.log("Form data(json):" + ko.toJSON(self.dataModel)); // good alternative for posting all data (changed/unchanged)

            //return true; // enable postback
            //return false; // disable postback
            var messages = getMessages(self.crudMode);
            $.ajax({
                url: common.getSiteRoot() + "Sites/SavePriceOverrides", // Put method for Update - Placeholder created
                method: "POST",
                data: { postbackKey1: postbackData }, // Postback a List<OverridePricePostViewModel>, {postbackKey1: etc must match Controller method param name}
                contentType: 'application/x-www-form-urlencoded; charset=utf-8', // 'application/json' , 
            })
                .done(function (response, textStatus, jqXhr) {
                    debugger;
                    if (response.JsonStatusCode.CustomStatusCode == "ApiSuccess") {
                        window.alert(messages.success);
                        self.reloadPage();
                    }
                    else if (response.JsonStatusCode.CustomStatusCode == "ApiFail") {
                        debugger;
                        window.alert(messages.failure);
                        //var errors = JSON.parse(response.ErrorSummaryString); // errors.ExceptionMessage
                        $('#msgs').html("<strong>" + messages.failure + " : " + response.ErrorSummaryString + "</strong>");
                    }
                    else { // UI validation errors
                        displayErrors(response.ModelErrors);
                    }
                    // msg success and redirect to another page if needed
                    // Define a standard message format for Post(aka Create) Response returned,
                })
                .fail(function (jqXhr, textStatus, errorThrown) {
                    debugger;
                    // msg failure
                    window.alert(messages.failure);
                    $('#msgs').html("<strong>" + messages.failure + ":" + errorThrown + "</strong>");
                });
            return false;
        }

        ///
        //self.displayPriceViewAsEmail = function () {
        //    var PriceView = self.dataModel();//Not quite the same
        //    var param = { PriceData: PriceView };
        //    //var $promise = common.callService("post", "Email/MakeFromJson", param);
        //    $.ajax({
        //        url: common.getSiteRoot() + "Email/MakeFromJson",
        //        method: "POST",
        //        data: param
        //    })
        //        .done(function (data) {
        //            if (data.success) {
        //                window.location.href = common.getSiteRoot() + "Email/DownloadEmail/" + "?fName=" + data.fName;
        //            }
        //        });
        //};

        // Parses response.ModelErrors dictionary
        var displayErrors = function (errors) {
            var errorsList = "";
            for (var i = 0; i < errors.length; i++) {
                errorsList = errorsList + "<li>" + errors[i].Value[0] + "</li>";
            }
            $("#msgs").html("<ul class='pageErrorMsg' style='color: red'>" + errorsList + "</ul>");
            window.alert("VALIDATION ERRORS, please see top of screen for remedial action.");
        }

        //self.getPriceView = function () {
        //    return self.dataModel();
        //}

        //
        self.crudMode = "";

        self.getDataForPage = function () {
            //Check CRUD mode
            self.crudMode = "Edit";

            var yyyymmdd = dmyStringToYmdString(self.InitDate());

            var url = "Sites/GetSitesWithPricesJson?date=" + yyyymmdd; // ScriptMethod 
            //window.alert("Get:" + url);
            //var jsonArgs = { forDate: ko.utils.unwrapObservable(PickedDate) };

            var $promise = common.callService("get", url, null); // args - maybe page no. (assuming no paging for now)
            self.loadPageData($promise);
        };

        self.getCompetitorDataForSite = function (site) {
            //Check CRUD mode
            self.crudMode = "Edit";
            if (site.SiteId == "") site.SiteId = 0; // default to all sites

            var yyyymmdd = dmyStringToYmdString(self.InitDate());

            var filter = "date=" + yyyymmdd + "&siteId=" + site.SiteId;

            var url = "Sites/GetSitesWithPricesJson?getCompetitor=1&" + filter; // ScriptMethod GetCompetitors
            //window.alert("Get:" + url);
            //var jsonArgs = { forDate: ko.utils.unwrapObservable(PickedDate) };

            var $promise = common.callService("get", url, null); // args - maybe page no. (assuming no paging for now)
            self.loadCompetitorDataForSite(site, $promise);
        };

        // Recode for unsaved changes prompting
        // TODO : prompt for unsaved Changes as soon as any override done - #unSavedChangesMsg

        //self.confirmCheckbox = function (data, event) {
        //    debugger;
        //    if (event.currentTarget.checked === true) {
        //        var box = confirm("Are you sure you want to complete this form?");
        //        if (box == true)
        //            return true;
        //        else
        //            event.currentTarget.checked = false;
        //        return true;
        //    } else {
        //        event.currentTarget.checked = false;
        //    }
        //    return true;
        //}
    };


    $(document).ready(function () {
        moment.locale("en-gb"); // Set Locale for moment (aka moment.locale("en-gb"))

        // http://stackoverflow.com/questions/26487765/bootstrap-datepicker-set-language-globally
        // set locales
        try {
            $.fn.datepicker.defaults.language = 'en-GB';
            $('.datepicker').datepicker({ language: "en-GB", dateFormat: 'dd/mm/yyyy', orientation: 'auto top', autoclose: true });
        } catch (ex) {

        }

        // knockout locale based date formatting - ko.observable(dateFormat(date, "dd/mm/yyyy"));
        // bootstrap datepicker formatting =  $("#pricingDatePicker").datepicker({dateFormat: 'dd/mm/yy'});

        var vm = new PageViewModel();

        var binder = function () {
            ko.applyBindings(vm, $("#petrolpricingpage")[0]);
            // set locales
            try {
                $('.datepicker').datepicker({ language: "en-GB", dateFormat: 'dd/mm/yyyy' });
            } catch (ex1) {

            }
        };
        vm.bind = binder;

        var pageQueryParams = helpers.queryStringHelpers.getQueryParams(window.location.search);
        var forDatefromQryStr = pageQueryParams["date"];

        debugger;
        var ukDateString;
        if (moment(forDatefromQryStr, dmyFormatString).isValid())
            ukDateString = moment(forDatefromQryStr).format(dmyFormatString);
        else if (moment(forDatefromQryStr, ymdFormatString).isValid())
            ukDateString = moment(forDatefromQryStr, ymdFormatString).format(dmyFormatString);
        else ukDateString = todaysDateUkformat;
        //// Show sitepricing data once loaded from GET
        vm.setupDatePicker(ukDateString);
        vm.getDataForPage();

        //var PriceViewJsonForEmail = vm.getPriceView();
    });

});



