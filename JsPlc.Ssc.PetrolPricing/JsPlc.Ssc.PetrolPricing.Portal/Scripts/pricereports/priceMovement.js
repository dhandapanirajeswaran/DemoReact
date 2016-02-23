$(function () {
    var forDp, toDp;
    forDp = $('#DateFrom').datepicker({
        language: "en-GB",
        autoClose: true,
        format: 'd-M-yyyy',
        todayBtn: "linked",
        todayHighlight:true
    });
    toDp = $('#DateTo').datepicker({
        language: "en-GB",
        autoClose: true,
        format: 'd-M-yyyy',
        todayBtn: "linked",
        todayHighlight: true
    });

    $("#btnViewReport").click(function () {
        $('#errorMsgs').html("");
        $('#msgs').html("");

        var id = $('#fuelTypes').val();
        var brandName = $('#brands').val();
        var dt1 = forDp ? forDp.val() : $('#DateFrom').val();
        var dt2 = toDp ? toDp.val() : $('#DateTo').val();
        if (id == 0) {
            $('#errorMsgs').html("Please select a fuel");
            $('#fuelTypes').focus();
            return false;
        }
        
        window.location.href = '/pricereports/PriceMovement?DateFrom=' + dt1 + "&DateTo=" + dt2 + "&Id=" + id + "&BrandName=" + brandName;
        return true;
    });
    $("#btnExportReport").click(function () {
        $('#errorMsgs').html("");
        $('#msgs').html("");

        var id = $('#fuelTypes').val();
        var brandName = $('#brands').val();
        var dt1 = forDp ? forDp.val() : $('#DateFrom').val();
        var dt2 = toDp ? toDp.val() : $('#DateTo').val();
        if (id == 0) {
            $('#errorMsgs').html("Please select a fuel");
            $('#fuelTypes').focus();
            return false;
        }

        window.location.href = '/pricereports/ExportPriceMovement?DateFrom=' + dt1 + "&DateTo=" + dt2 + "&Id=" + id + "&BrandName=" + brandName;
        return true;
    });

});

