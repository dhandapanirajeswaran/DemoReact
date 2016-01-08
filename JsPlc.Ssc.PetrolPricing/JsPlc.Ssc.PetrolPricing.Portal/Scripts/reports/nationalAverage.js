var _forDp
$(function () {
    _forDp = $('.datepicker').datepicker({
        language: "en-GB",
        autoClose: true,
        format: 'd-M-yyyy',
        todayBtn: "linked",
        todayHighlight:true
    });

    $("#btnViewReport").click(function () {
        var dt = _forDp.val();
        window.location.href = '/reports/nationalAverage?For=' + dt;
    });
});

