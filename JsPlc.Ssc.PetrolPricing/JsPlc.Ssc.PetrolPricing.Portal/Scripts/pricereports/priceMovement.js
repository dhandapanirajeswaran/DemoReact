$("document").ready(function () {
    var forDp = $('#DateFrom').datepicker({
        language: "en-GB",
        autoClose: true,
        format: 'd-M-yyyy',
        todayBtn: "linked",
        todayHighlight:true
    });

    var toDp = $('#DateTo').datepicker({
        language: "en-GB",
        autoClose: true,
        format: 'd-M-yyyy',
        todayBtn: "linked",
        todayHighlight: true
    });

    $("#btnViewReport").click(function () {
        $('#errorMsgs').html("");
        $('#msgs').html("");

        var id = $('#FuelTypeId').val();
        var brandName = $('#Brand').val();
        var dt1 = forDp ? forDp.val() : $('#DateFrom').val();
        var dt2 = toDp ? toDp.val() : $('#DateTo').val();
        if (id == 0) {
            $('#errorMsgs').html("Please select a fuel");
            $('#fuelTypes').focus();
            return false;
        }
        
        window.location.href = '/PriceReports/PriceMovement?DateFrom=' + dt1 + "&DateTo=" + dt2 + "&FuelTypeId=" + id + "&BrandName=" + brandName;
        return true;
    });
    $("#btnExportReport").click(function () {
        $('#errorMsgs').html("");
        $('#msgs').html("");

        var id = $('#FuelTypeId').val();
        var brandName = $('#Brand').val();
        var dt1 = forDp ? forDp.val() : $('#DateFrom').val();
        var dt2 = toDp ? toDp.val() : $('#DateTo').val();
        if (id == 0) {
            $('#errorMsgs').html("Please select a fuel");
            $('#fuelTypes').focus();
            return false;
        }

        window.location.href = '/PriceReports/ExportPriceMovement?DateFrom=' + dt1 + "&DateTo=" + dt2 + "&FuelTypeId=" + id + "&BrandName=" + brandName;
        return true;
    });

});

