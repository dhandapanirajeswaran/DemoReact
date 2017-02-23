/// <reference path="App/Services/PetrolPricingService.js" />
/// <reference path="App/ViewScripts/SitePricing.js" />
//Must omit extension on right side (in values)
require.config({
    waitSeconds: 200,
    baseUrl: "Scripts",
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

        // LINK Specific js files.
        "helpers": "Utils/helpers",
        "petrolpricingDatePickers": "utils/petrolpricingDatePickers",
        "text": "text.min",
        //komoment: 'path/to/komoment', // KoMoment potentially useful
        //"datatables": "DataTables/jquery.dataTables",

        //App modules
        "common": "App/common",
        "busyloader": "App/busyloader",
        "notify": "App/notify",

        //"ko-binding-handlers": "App/ko-binding-handlers",

        //"RegisterKoComponents": "App/kocomponents/RegisterKoComponents",

        //Services
        "PetrolPricingService": "App/Services/PetrolPricingService",

        //View scripts
        "SitePricing": "App/ViewScripts/SitePricing",
        "competitorPricePopup": "App/ViewScripts/CompetitorPricePopup",
        "competitorPriceNotePopup": "App/ViewScripts/CompetitorPriceNotePopup",
        "UploadCountdown": "App/ViewScripts/UploadCountdown"

    },
    shim: {
        "bootstrap-datepicker": { deps: ["jquery", "bootstrap"] },
        "bootstrap-datepickerGB": { deps: ["jquery", "bootstrap", "bootstrap-datepicker"] },
        "jqueryval": { deps: ["jquery"] },
        "knockout": { deps: ["jquery"] },
        "bootstrap": { deps: ["jquery"] },
        "SitePricing": { deps: ["jquery", "bootstrap-datepickerGB"] }
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
});

//To load essential modules first
//require(["RegisterKoComponents"]);
//require(['jquery', 'knockout', 'ko-binding-handlers', 'text'], function ($, ko) { });//Module end
require(['jquery', 'knockout'], function ($, ko) { });//Module end
