$(function () {
    var forDp;
    forDp = $('.datepicker').datepicker({
        language: "en-GB",
        autoClose: true,
        format: 'd-M-yyyy',
        todayBtn: "linked",
        todayHighlight:true
    });

    $("#btnViewReport").click(function () {
        var dt = forDp.val();
        window.location.href = '/reports/Compliance?For=' + dt;
    });
    //$("#btnExportReport").click(function () {
    //    var dt = forDp.val();
    //    window.location.href = '/reports/ExportCompliance?For=' + dt;
    //});
});

