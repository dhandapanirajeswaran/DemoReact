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
    var ymdFormatString = "YYYY-MM-DD";

    var ukDateSample = moment("16/12/2015", dmyFormatString).format(dmyFormatString);
    var usDateSample = moment("16/12/2015", dmyFormatString).format(ymdFormatString);
    var todayDateStringUkFormat = function() {
        var currentDate = new Date();
        var day = currentDate.getDate();
        var month = currentDate.getMonth() + 1;
        var year = currentDate.getFullYear();
        return (day + "/" + month + "/" + year);
    };
    var todaysDateUkformat = todayDateStringUkFormat();

    var dmyStringToYmdString = function (ukDateString) {
        if (ukDateString == "") ukDateString = todaysDateUkformat;

        var ymdDateString = moment(ukDateString, dmyFormatString).isValid() ?
            moment(ukDateString, dmyFormatString).format(ymdFormatString) : "";
        return ymdDateString; // returns blank is invalid ukdate
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

        self.ShowSaveButton = ko.observable('');
        
        // only run when new data is loaded
        self.ShowOrHideSaveButton = function () {
            if (self.InitDate() == todaysDateUkformat)
                self.ShowSaveButton(true);
            else
                self.ShowSaveButton(false);
        }

        self.bind = function () { };

        // End define

        var fuelsToDisplay = [2, 6, 7]; // fuel columns

        var sitePricingView;

        self.setupDatePicker = function (ukDate) {
            moment.locale("en-gb"); // Set Locale for moment (aka moment.locale("en-gb"))
            //var forDate = moment(IsoDate).format("L"); // we get dd/mm/yyyy
            self.InitDate(ukDate);

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

            //enListOnlyFuelsToDisplayForCompetitors(sitePricingView.sites);
            debugger;
            self.dataModel(sitePricingView);
        };

        // map all competitors from ajax call to inidividual site
        var attachCompetitorsToSite = function (site, serverCompetitors) {
            // extract the competitors and attach them to their relevant site in the model
            debugger;
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
                siteItem.FuelPricesToDisplay = getFuelPricesToDisplay(siteItem.FuelPrices);
            });
        }

        // format fuels for comps
        var enListOnlyFuelsToDisplayForCompetitors = function(sites) {
            _.each(sites, function(siteItem) {
                if (siteItem.hasCompetitors() && _.any(siteItem.competitors())) {
                    enListOnlyFuelsToDisplay(siteItem.competitors());
                };
            });
        };

        var setCompetitorFields = function (sites) {
            _.each(sites, function (siteItem) {
                siteItem.hasCompetitors = ko.observable(false);
                siteItem.competitors = ko.observableArray([]);
                siteItem.showingComps = ko.observable(false);
                siteItem.loadingCompetitors = ko.observable(false);

                siteItem.getCompetitorDataClick = function () {
                    toggleCompDisplay(siteItem);
                    siteItem.loadingCompetitors(true);
                    self.getCompetitorDataForSite(siteItem);
                    siteItem.loadingCompetitors(false);
                    return true;
                }
            });
        }
        var getFuelPricesToDisplay = function (siteFuelPrices) {
            var returnValue = [];
            var siteFuels = siteFuelPrices;
            _.each(fuelsToDisplay, function (id) {
                var fuelToDisplay = _.find(siteFuels, function (item) {
                    return (item.FuelTypeId == id);
                });
                if (fuelToDisplay) {
                    returnValue.push({ siteSupportsFuel: true, FuelPrice: self.getFormattedObservableFuelPrice(fuelToDisplay) });
                }
                else {
                    returnValue.push({ siteSupportsFuel: false, FuelPrice: null });
                }
            });
            
            return returnValue; // always contains 3 items only (as per length of fuelsToDisplay array
        }

        self.getFormattedObservableFuelPrice = function (fuelPriceToDisplay) {
            return {
                FuelTypeId: fuelPriceToDisplay.FuelTypeId,
                YestPrice: self.formatValueTo1DP(fuelPriceToDisplay.YestPrice),
                TodayPrice: self.formatValueTo1DP(fuelPriceToDisplay.TodayPrice),
                AutoPrice: self.formatValueTo1DP(fuelPriceToDisplay.AutoPrice),
                // OBSERVABLE as its tied to user input
                OverridePrice: ko.observable(self.formatValueTo1DP(fuelPriceToDisplay.OverridePrice, '')),
                changeProp: function (obj, newValue) {
                    var newPrice = $(newValue.target).val();
                    if (newPrice != '') self.HasUnsavedChanges(true);
                }
            }
        }

        self.formatValueTo1DP = function(priceValue, replacementForZero) {
            if (priceValue <= 0.0001) return replacementForZero;
            else return parseFloat(Math.round((priceValue/10) * 100) / 100).toFixed(1); // Number formatting to 1dp
        }

        self.loadNewDateData = function () {
            // TODO check for unsaved changes
            //self.getDataForPage();
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
                    if (serverData == "Error") {
                        $('#msgs').html("Error - no data");
                        site.hasCompetitors(false);
                        self.dataAvailable(false);
                    } else {
                        // $('#msgs').html("Fetch success");
                        attachCompetitorsToSite(site, serverData); // attach the right competitor list to the + site clicked
                    }
                })
                .fail(function() {
                    self.dataAvailable(false);
                    site.hasCompetitors(false);
                    $('#msgs').html("Error occured");
                });
            //self.bind(); // Call a diff binder (or do we even need to, if we just populated the competitors array to the right site)
        }

        // ### POST SitePricing Form data back to server. // TODO serialize the data back to server from dataModel
        self.savePetrolPricingForm = function () {
            console.log("Form data:" + $('#myform').serialize());
            console.log("Form data(json):" + ko.toJSON(self.dataModel));
            console.log("Self.DataModel:" + self.dataModel());

            var data = self.dataModel();

            // TODO copy all Fuels info to postBackdata
            // TODO Simpler VM - SiteId, FuelId, OverrideValue (only ones which are non blank), for faster updates and avoid unnecessary ones
            // End copy Fuels.

            // TODO refer to picker forDate
            var yyyymmdd = dmyStringToYmdString(self.InitDate());
            console.log("Proposed postback forDate (yyyy-mm-ddThh:mm:ss.xxxZ):" + yyyymmdd);
            var forDatePostback = yyyymmdd; // not used yet, we postback for today's prices only until simulation is in place

            var messages = getMessages(self.crudMode);
            return; // TODO - enable this when MVC, API server code is ready to update backend

            $.ajax({
                url: common.getSiteRoot() + "Sites/PutPriceOverride", // Put method for Update - Placeholder created
                method: "POST",
                data: data, // Postback a List<SitePriceViewModel>
                contentType: 'application/x-www-form-urlencoded; charset=utf-8', // 'application/json'
            })
                .done(function (response, textStatus, jqXhr) {

                    if (response.JsonStatusCode.CustomStatusCode == "ApiSuccess") {
                        window.alert(messages.success);
                    }
                    else if (response.JsonStatusCode.CustomStatusCode == "ApiFail") {
                        window.alert(messages.failure);
                        $('#msgs').html("<strong>" + messages.failure + " : " + response + "</strong>");
                    }
                    else { // UI validation errors
                        displayErrors(response.ModelErrors);
                    }
                    // msg success and redirect to another page if needed
                    // Define a standard message format for Post(aka Create) Response returned,
                })
                .fail(function (jqXhr, textStatus, errorThrown) {
                    // msg failure
                    window.alert(messages.failure);
                    $('#msgs').html("<strong>" + messages.failure + ":" + errorThrown + "</strong>");
                });
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
            debugger;
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



