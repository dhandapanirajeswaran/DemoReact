/// <reference path="App/Services/PetrolPricingService.js" />
/// <reference path="App/ViewScripts/SitePricing.js" />
//Must omit extension on right side (in values)

(function () {
    var scriptVersion = JSPLC.PetrolPricing.scriptVersion,
        config = {
        waitSeconds: 200,
        baseUrl: "Scripts",
        urlArgs: 'v=' + scriptVersion,
        paths: {
            "modernizr": "modernizr-2.6.2",
            "jquery": "jquery-1.10.2",
            "jqueryval": "jquery.validate.min",
            "knockout": "knockout-3.4.0.debug",
            "moment": 'moment-with-locales',
            "bootstrap": "bootstrap.min",
            "bootstrap-datepicker": "bootstrap-datepicker",
            "bootstrap-datepickerGB": "locales/bootstrap-datepicker.en-GB.min",
            "URI": "UriJs/Uri",
            "underscore": "underscore",
            "chosen": "chosen/chosen.jquery",

            // LINK Specific js files.
            "helpers": "Utils/helpers",
            "petrolpricingDatePickers": "utils/petrolpricingDatePickers",
            "text": "text.min",
            //komoment: 'path/to/komoment', // KoMoment potentially useful
            //"datatables": "DataTables/jquery.dataTables",

            //App modules
            "common": "App/common",
            "busyloader": "App/busyloader",
            "etaCountdown": "App/etaCountdown",
            "notify": "App/notify",
            "cookie": "App/cookie",
            "downloader": "App/downloader",
            "layout": "App/layout",
            "scrollToTop": "App/scrollToTop",
            "scrollToElement": "App/scrollToElement",
            "infotips": "App/infotips",
            "cookieSettings": "App/cookieSettings",
            "bootbox": "App/bootbox.min",
            "sitePricingSettings": "App/sitePricingSettings",
            "buildDetector": "App/buildDetector",
            "validation": "App/validation",
            "EasyTemplate": "App/EasyTemplate",
            "PriceStats": "App/pricestats",
            "PriceChangesTab": "App/priceChangesTab",
            "EditInPlace": "App/EditInPlace/edit-in-place",
            "driveTimeMarkup": "App/driveTimeMarkup",
            "DriveTimeChart": "App/DriveTimeChart",
            "SitePricingSorting": "App/SitePricingSorting",
            "waiter": "App/Waiter",
            "chooseOneGroup": "App/chooseOneGroup",
            "HistoricSitePrices": "App/HistoricSitePrices",
            "UnsavedChanges": "App/UnsavedChanges",
            "PriceReasons": "App/PriceReasons",
            "PriceFreezeEvents": "App/PriceFreezeEvents",
            "DateUtils": "App/DateUtils",
            "Help": "App/Help",
            "ErrorCatcher": "App/ErrorCatcher",
            "JsPriceOverrides": "App/JsPriceOverrides",

            // popups
            "SiteEmailPopup" : "App/Popups/SiteEmailPopup",

            //Services
            "PetrolPricingService": "App/Services/PetrolPricingService",
            "EmailTemplateService": "App/Services/EmailTemplateService",
            "DriveTimeMarkupService": "App/Services/DriveTimeMarkupService",
            "EmailScheduleService": "App/Services/EmailScheduleService",
            "PriceFreezeEventService": "App/Services/PriceFreezeEventService",
            "ServiceUtils": "App/Services/ServiceUtils",

            //View scripts
            "SitePricing": "App/ViewScripts/SitePricing",
            "competitorPricePopup": "App/ViewScripts/CompetitorPricePopup",
            "competitorPriceNotePopup": "App/ViewScripts/CompetitorPriceNotePopup",
            "UploadCountdown": "App/ViewScripts/UploadCountdown",
            "FileUpload": "App/ViewScripts/FileUpload",
            "Diagnostics": "App/ViewScripts/Diagnostics",
            "SiteMaintenance": "App/ViewScripts/SiteMaintenance",
            "SiteEmails": "App/ViewScripts/SiteEmails",
            "DriveTime": "App/ViewScripts/DriveTime",
            "EmailSchedule": "App/ViewScripts/EmailSchedule",
            "SystemSettings": "App/ViewScripts/SystemSettings",

            // Page scripts
            "Prices": "App/Prices"
        },
        shim: {
            "bootstrap-datepicker": { deps: ["jquery", "bootstrap"] },
            "bootstrap-datepickerGB": { deps: ["jquery", "bootstrap", "bootstrap-datepicker"] },
            "jqueryval": { deps: ["jquery"] },
            "knockout": { deps: ["jquery"] },
            "bootstrap": { deps: ["jquery"] },
            "chosen": { deps: ["jquery"] },
            "SitePricing": { deps: ["jquery", "bootstrap-datepickerGB"] },
            'bootbox': {
                deps: ['jquery']
            }
            //"ko-binding-handlers": { deps: ["jquery"] }
            //"tableedit": { deps: ["tabletools"] }
        },
        map: {
            //typeahead: "typeahead-helper!typeahead.bundle"
            "URI": {
                "IPv6": "URIjs/punycode",
                "punycode": "URIjs/punycode",
                "SecondLevelDomains": "URIjs/punycode"
            }
        }
    };
    require.config(config);
})();

//To load essential modules first
//require(["RegisterKoComponents"]);
//require(['jquery', 'knockout', 'ko-binding-handlers', 'text'], function ($, ko) { });//Module end
require(['jquery', 'knockout'], function ($, ko) { });//Module end
