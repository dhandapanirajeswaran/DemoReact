﻿$("document").ready(function () {

    var rootFolder = /\/petrolpricing\//i.test(window.location.href) ? "/petrolpricing" : "";

    var forDp = $('.datepicker').datepicker({
        language: "en-GB",
        autoClose: true,
        format: 'd-M-yyyy',
        todayBtn: "linked",
        todayHighlight: true,
        orientation: 'auto top',
        endDate: '1d'
    });

    $("#btnViewReport").click(function () {
        var dt = forDp.val();
        window.location.href = rootFolder + '/PriceReports/nationalAverage3?For=' + dt;
    });
    $("#btnExportReport").click(function () {
        var dt = forDp.val();
        window.location.href = rootFolder + '/PriceReports/ExportNationalAverage2?For=' + dt + "&viewAllCompitetors=True";
    });
});
