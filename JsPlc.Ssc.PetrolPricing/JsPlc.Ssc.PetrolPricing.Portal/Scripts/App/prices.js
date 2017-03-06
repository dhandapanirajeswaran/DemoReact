﻿
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

function padZero2Digit(n) {
    return (n < 10 ? '0' : '') + n;
}

function dateAdd(unit, delta, datetime) {
    var day = datetime.getDate(),
        month = datetime.getMonth() + 1,
        year = datetime.getFullYear();

    switch(unit.toLowerCase()) {
        case 'd':
        case 'dd':
        case 'day':
            day+=delta;
            break;
        case 'm':
        case 'mm':
        case 'month':
            month+=delta;
            break;
        case 'y':
        case 'yy':
        case 'year':
            year+=delta;
            break;
        default:
            throw new Error('dateAdd - unsupported unit type: ' + unit);
            break;
    }

    return new Date(year, month - 1, day);
}

function isUKDate(text) {
    return /\d{1,2}\/\d{1,2}\/\d{4}/.test(text);
}

function isSameDate(datetime1, datetime2) {
    return datetime1.getFullYear() == datetime2.getFullYear()
        && datetime1.getMonth() == datetime2.getMonth()
        && datetime1.getDate() == datetime2.getDate();
}

///
/// formatUKDate(date)
/// formatUKDate(year, month, day)
///
function formatUKDate() {
    var year, month, day, datetime;

    if (arguments.length == 1 && arguments[0] instanceof Date) {
        datetime = arguments[0];
    } else if (arguments.length == 3) {
        day = parseInt(arguments[0], 10);
        month = parseInt(arguments[1], 10);
        year = parseInt(arguments[2], 10);
        datetime = new Date(year, month - 1, day);
    }

    if (datetime == undefined) 
        throw new Error('formatUKDate - unsupported argument signature');

    day = datetime.getDate(),
    month = datetime.getMonth() + 1;
    year = datetime.getFullYear();

    return padZero2Digit(day) + '/' + padZero2Digit(month) + '/' + year;
};

function buildRelativeDayHtml(datetime) {
    var today = new Date(),
        timeDiff = datetime.getTime() - today.getTime(),
        diffDays = Math.ceil(timeDiff / (1000 * 3600 * 24));

        switch (diffDays) {
            case -1:
                return '<b class="yesterday-date">Yesterday</b>';
            case 0:
                return '<b class="today-date">Today</b>';
            case 1:
                return '<b class="tomorrow-date">Tomorrow</b>';
            default:
                if (diffDays < 0)
                    return '<b class="past-date">' + diffDays + ' Days</b>';
                else
                    return '<b class="future-date">' + diffDays + ' Days</b>';
        }
};

function buildDateHeadingAndMarkup(datetime) {
    return {
        datetime: new Date(datetime),
        formatted: formatUKDate(datetime),
        markup: buildRelativeDayHtml(datetime)
    };
}

function refreshDates(selectedDate) {
    selectedDate = selectedDate == "" ? $('#viewingDate').val() : selectedDate;

    var currentDate,
        previousDay,
        currentDay,
        nextDay;

    if (isUKDate(selectedDate)) {
        dateParts = selectedDate.split('/');
        currentDate = new Date(parseInt(dateParts[2], 10), parseInt(dateParts[1], 10) - 1, parseInt(dateParts[0], 10));
    } else {
        currentDate = new Date();
    }

    currentDay = buildDateHeadingAndMarkup(currentDate);
    previousDay = buildDateHeadingAndMarkup(dateAdd('d', -1, currentDate));
    nextDay = buildDateHeadingAndMarkup(dateAdd('d', +1, currentDate));

    $('#today1').html(previousDay.markup + '<br />' + previousDay.formatted );
    $('#today2').html(previousDay.markup + '<br />' + previousDay.formatted);
    $('#today3').html(previousDay.markup + '<br />' + previousDay.formatted);

    $(".comptoday").html(currentDay.formatted);

    $('#tomorrow1').html(nextDay.markup + '<br />' + nextDay.formatted);
    $('#tomorrow2').html(nextDay.markup + '<br />' + nextDay.formatted);
    $('#tomorrow3').html(nextDay.markup + '<br />' + nextDay.formatted);

    $(".compyday").html(previousDay.formatted);
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
