﻿$("document").ready(function () {
    var forDp = $('.datepicker').datepicker({
        language: "en-GB",
        autoClose: true,
        format: 'd-M-yyyy',
        todayBtn: "linked",
        todayHighlight:true
    });

    $("#btnViewReport").click(function () {
        var dt = forDp.val();
        window.location.href = '/PriceReports/nationalAverage2?For=' + dt;
    });
    $("#btnExportReport").click(function () {
        var dt = forDp.val();
        window.location.href = '/PriceReports/ExportNationalAverage2?For=' + dt;
    });
});

