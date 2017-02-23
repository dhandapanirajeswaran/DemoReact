
$("#viewingDate,#viewingStoreNo,#viewingStoreName,#viewingStoreTown,#viewingCatNo").keyup(function (event) {
    if (event.keyCode == 13) {
        $("#btnGO").click();
    }
});

require(["SitePricing", "notify", "busyloader"],
    function (prices, notify, busyloader) {
    prices.go();

    $('.datepicker')
        .datepicker({
            language: "en-GB",
            format: 'dd/mm/yyyy',
            orientation: 'auto top',
            autoclose: true,
            todayHighlight: true,
            endDate: '1d'
        }).on('changeDate', function (e) {
            $("#btnExportAll").prop("disabled", true);
            $("#btnExportSites").prop("disabled", true);

            $("#viewingDate").focus();
        });

    $(window).on('exporting-all-click', function () {
        busyloader.show({
            message: 'Exporting All - Please wait',
            showtime: 8000
        });
    });

    $(window).on('exporting-js-sites-click', function () {
        busyloader.show({
            message: 'Exporting JS Sites - Please wait.',
            showtime: 8000
        });
    });
});

function getRootSiteFolder() {
    var rootFolder = /\/petrolpricing\//i.test(window.location.href) ? "/petrolpricing/" : "/";
    return rootFolder;
}

function refreshDates(selectedDate) {
    selectedDate = selectedDate == "" ? $('#viewingDate').val() : selectedDate;
    var yesterday = new Date();
    var tomorrow = new Date();
    var strtoday = "";
    var stryday = "";
    var strtomorrow = "";
    if (selectedDate == "") {
        var d = new Date();

        var month = d.getMonth() + 1;
        var day = d.getDate();
        var year = d.getFullYear();
        d = new Date(yearnum, monnum, daynum - 1);
        strtoday = (('' + day).length < 2 ? '0' : '') + day + '/' +
            (('' + month).length < 2 ? '0' : '') + month + '/' +
            d.getFullYear();

        yesterday = new Date(d.getFullYear(), d.getMonth(), d.getDate() - 1);
        tomorrow = new Date(d.getFullYear(), d.getMonth(), d.getDate() + 1);
    } else {
        strtoday = selectedDate;
        var tmp = selectedDate.split("/");
        var yearnum = parseInt(tmp[2]);
        var monnum = parseInt(tmp[1]) - 1;
        var daynum = parseInt(tmp[0]);

        var d = new Date(yearnum, monnum, daynum - 1);
        yesterday = new Date(d.getFullYear(), d.getMonth(), d.getDate() - 1);
        tomorrow = new Date(d.getFullYear(), d.getMonth(), d.getDate() + 2);

        var month1 = d.getMonth() + 1;
        var day1 = d.getDate();
        var year1 = d.getFullYear();
        strtoday = (('' + day1).length < 2 ? '0' : '') + day1 + '/' +
            (('' + month1).length < 2 ? '0' : '') + month1 + '/' +
            d.getFullYear();
    }

    var month = yesterday.getMonth() + 1;
    var day = yesterday.getDate();
    stryday = (('' + day).length < 2 ? '0' : '') + day + '/' +
        (('' + month).length < 2 ? '0' : '') + month + '/' +
        yesterday.getFullYear();

    month = tomorrow.getMonth() + 1;
    day = tomorrow.getDate();
    strtomorrow = (('' + day).length < 2 ? '0' : '') + day + '/' +
        (('' + month).length < 2 ? '0' : '') + month + '/' +
        tomorrow.getFullYear();
    $('#today1').text(strtoday);
    $('#today2').text(strtoday);
    $('#today3').text(strtoday);

    $(".comptoday").text(strtoday);

    $('#tomorrow1').text(strtomorrow);
    $('#tomorrow2').text(strtomorrow);
    $('#tomorrow3').text(strtomorrow);
    $(".compyday").text(stryday);
}

$("#viewingStoreNo").change(function () {
    $("#btnExportAll").prop("disabled", true);
    $("#btnExportSites").prop("disabled", true);
});

$("#viewingStoreName").change(function () {
    $("#btnExportAll").prop("disabled", true);
    $("#btnExportSites").prop("disabled", true);
});

$("#viewingStoreNo").change(function () {
    $("#btnExportAll").prop("disabled", true);
    $("#btnExportSites").prop("disabled", true);
});

$("#viewingStoreTown").change(function () {
    $("#btnExportAll").prop("disabled", true);
    $("#btnExportSites").prop("disabled", true);
});

$("#btnExportAll").click(function () {
    var url = "Sites/ExportPrices?date=" + $('#viewingDate').val()
        + "&storeName=" + $('#viewingStoreName').val()
        + "&catNo=" + $('#viewingCatNo').val()
        + "&storeNo=" + $('#viewingStoreNo').val()
        + "&storeTown=" + $('#viewingStoreTown').val();

    $(window).trigger('exporting-all-click');

    window.location.href = getRootSiteFolder() + url;
});

$("#btnExportSites").click(function () {
    var url = "Sites/ExportSiteswithPrices?date=" + $('#viewingDate').val()
        + "&storeName=" + $('#viewingStoreName').val()
        + "&catNo=" + $('#viewingCatNo').val()
        + "&storeNo=" + $('#viewingStoreNo').val()
        + "&storeTown=" + $('#viewingStoreTown').val();

    $(window).trigger('exporting-js-sites-click');

    window.location.href = getRootSiteFolder() + url;
});
