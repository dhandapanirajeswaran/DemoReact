$("document").ready(function () {

    var rootFolder = /\/petrolpricing\//i.test(window.location.href) ? "/petrolpricing" : "";

    var forDp = $('.datepicker').datepicker({
        language: "en-GB",
        autoClose: true,
        format: 'd-M-yyyy',
        todayBtn: "linked",
        todayHighlight:true
    });

    $("#btnViewReport").click(function () {
        var dt = forDp.val();
        window.location.href = rootFolder + '/PriceReports/PricePoints?For=' + dt;
    });
    $("#btnExportReport").click(function () {
        var dt = forDp.val();
        window.location.href = rootFolder + '/PriceReports/ExportPricePoints?For=' + dt;
    });
});

