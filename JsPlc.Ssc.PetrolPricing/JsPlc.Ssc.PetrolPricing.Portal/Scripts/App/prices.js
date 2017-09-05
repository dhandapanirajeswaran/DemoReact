$("#viewingDate,#viewingStoreNo,#viewingStoreName,#viewingStoreTown,#viewingCatNo").keyup(function (event) {
    if (event.keyCode == 13) {
        $("#btnGO").click();
    }
});

define(["SitePricing", "notify", "busyloader", "downloader", "infotips", "cookieSettings", "bootbox", "PetrolPricingService"],
    function (sitepricing, notify, busyloader, downloader, infotips, cookieSettings, bootbox, petrolPricingService) {

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
                $("#btnExportCompPrices").prop("disabled", true);

                $("#viewingDate").focus();
            });

        $(window).on('exporting-all-click', function (ev, download) {
            busyloader.show({
                message: 'Exporting All - Please wait (ETA: 3 minutes)',
                showtime: 8000
            });

            downloader.start({
                id: download.id,
                element: '#btnExportAll',
                complete: function (download) {
                    notify.success('Export completed - took ' + download.friendlyTimeTaken);
                }
            });
        });

        $("#btnExportAll").click(function () {
            var downloadId = downloader.generateId(),
                url = "Sites/ExportPrices"
                + "?downloadId=" + downloadId
                + "&date=" + $('#viewingDate').val()
                + "&storeName=" + $('#viewingStoreName').val()
                + "&catNo=" + $('#viewingCatNo').val()
                + "&storeNo=" + $('#viewingStoreNo').val()
                + "&storeTown=" + $('#viewingStoreTown').val();

            busyloader.show({
                message: 'Exporting All - Please wait (ETA 3 minutes)',
                showtime: 4000,
                dull: true
            });

            downloader.start({
                id: downloadId,
                element: '#btnExportAll',
                complete: function (download) {
                    notify.success('Export All completed - took ' + download.friendlyTimeTaken);
                }
            });

            window.location.href = getRootSiteFolder() + url;
        });

        $("#btnExportCompPrices").click(function () {
            var downloadId = downloader.generateId(),
                url = "Sites/ExportCompPrices"
                + "?downloadId=" + downloadId
                + "&date=" + $('#viewingDate').val()
                + "&storeName=" + $('#viewingStoreName').val()
                + "&catNo=" + $('#viewingCatNo').val()
                + "&storeNo=" + $('#viewingStoreNo').val()
                + "&storeTown=" + $('#viewingStoreTown').val();

            busyloader.show({
                message: 'Exporting Competitors - Please wait (ETA 1 minute)',
                showtime: 4000,
                dull: true
            });

            downloader.start({
                id: downloadId,
                element: '#btnExportCompPrices',
                complete: function (download) {
                    notify.success('Export Competitors completed - took ' + download.friendlyTimeTaken);
                }
            });

            window.location.href = getRootSiteFolder() + url;
        });

        $("#btnExportSites").click(function () {
            var downloadId = downloader.generateId(),
                url = "Sites/ExportSiteswithPrices"
                + "?downloadId=" + downloadId
                + "&date=" + $('#viewingDate').val()
                + "&storeName=" + $('#viewingStoreName').val()
                + "&catNo=" + $('#viewingCatNo').val()
                + "&storeNo=" + $('#viewingStoreNo').val()
                + "&storeTown=" + $('#viewingStoreTown').val();

            busyloader.show({
                message: 'Exporting JS Sites - Please wait. (ETA 10 seconds)',
                showtime: 2000,
                dull: true
            });
            downloader.start({
                id: downloadId,
                element: '#btnExportSites',
                complete: function (download) {
                    notify.success('Export JS completed - took ' + download.friendlyTimeTaken);
                }
            });

            window.location.href = getRootSiteFolder() + url;
        });

        $('[data-click="setExpandMode"').off().click(function () {
            var button = $(this),
                mode = button.data('mode'),
                selector = button.data('target'),
                message = button.data('message');

            redrawExpandModes(selector, mode);

            $(selector).trigger('expand-mode-change', [mode]);

            notify.info(message);
        });

        function redrawExpandModes(selector, mode) {
            var panel = $(selector),
                clones = $('[data-click="setExpandMode"][data-target="' + selector + '"]'),
                states = clones.first().data('states').split(',');

            clones.each(function () {
                var item = $(this);
                if (item.data('mode') == mode)
                    item.addClass('btn-primary');
                else
                    item.removeClass('btn-primary');
            });

            $.each(states, function (i, value) {
                if (value == mode)
                    panel.addClass(value);
                else
                    panel.removeClass(value);
            });
        };

        $('#PricingPanelScroller').on('expand-mode-change', function (ev, mode) {
            cookieSettings.write('pricing.expandGrid', mode);
        });

        function applyPricingPanelScrollerMode() {
            var selector = '#PricingPanelScroller',
                mode = cookieSettings.read('pricing.expandGrid', '');
            if (mode)
                redrawExpandModes(selector, mode);
        };

        function applyUserSettings() {
            applyPricingPanelScrollerMode();
        };

        function disableExportButtons() {
            $("#btnExportAll").prop("disabled", true);
            $("#btnExportSites").prop("disabled", true);
            $("btnExportCompPrices").prop("disabled", true);
        };

        $("#viewingStoreNo, #viewingStoreName, #viewingStoreNo, #viewingStoreTown").change(disableExportButtons);


        $('#btnRecalculateDailyPrices').off().click(function () {
            bootbox.confirm({
                title: '<i class="fa fa-question"></i> Reset Confirmation ',
                message: 'Are you sure you wish to recalculate the <strong>Daily Prices</strong> ?<br />'
                    + '<br />'
                    + '<strong>Note:</strong> This can take a <strong>1 - 2 minutes</strong> to perform the recalculation.',
                buttons: {
                    confirm: {
                        label: '<i class="fa fa-check"></i> Yes',
                        className: 'btn btn-danger'
                    },
                    cancel: {
                        label: '<i class="fa fa-times"></i> No',
                        className: 'btn btn-default'
                    }
                },
                callback: function (result) {
                    if (result) {
                        triggerRecalculation();
                    }
                }
            });
        });

        function triggerRecalculation() {

            function failure() {
                notify.error('Unable to trigger Daily Price recalculation');
            };

            function success() {
                setTimeout(function () {
                    $("#btnGO").click();
                }, 1000);

                notify.info('Daily Price recalculation started...');
            };

            petrolPricingService.triggerDailyPriceRecalculation(success, failure);
        };


        function docReady() {
            applyUserSettings();
        };

        $(docReady);

        function init(pagedata) {
            sitepricing.initPricingPage(pagedata);
        };

        // API
        return {
            init: init
        };
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

    switch (unit.toLowerCase()) {
        case 'd':
        case 'dd':
        case 'day':
            day += delta;
            break;
        case 'm':
        case 'mm':
        case 'month':
            month += delta;
            break;
        case 'y':
        case 'yy':
        case 'year':
            year += delta;
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

function getDaysAgo(datetime) {
    var now = new Date(),
        timeDiff = new Date(datetime).getTime() - now.getTime(),
        diffDays = Math.ceil(timeDiff / (1000 * 3600 * 24));
    return diffDays;
};

function buildRelativeDayHtml(datetime) {
    var diffDays = getDaysAgo(datetime);

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
        currentDay,
        nextDay,
        previousDay,
        minus2Day,
        selectedDaysAgo;

    if (isUKDate(selectedDate)) {
        dateParts = selectedDate.split('/');
        currentDate = new Date(parseInt(dateParts[2], 10), parseInt(dateParts[1], 10) - 1, parseInt(dateParts[0], 10));
    } else {
        currentDate = new Date();
    }

    selectedDaysAgo = getDaysAgo(currentDate);

    minus2Day = buildDateHeadingAndMarkup(dateAdd('d', -2, currentDate));
    previousDay = buildDateHeadingAndMarkup(dateAdd('d', -1, currentDate));
    currentDay = buildDateHeadingAndMarkup(currentDate);
    nextDay = buildDateHeadingAndMarkup(dateAdd('d', +1, currentDate));

    $('#today1').html('Today<br />' + currentDay.formatted);
    $('#today2').html('Today<br />' + currentDay.formatted);
    $('#today3').html('Today<br />' + currentDay.formatted);

    $('#tomorrow1').html('Tomorrow<br />' + nextDay.formatted);
    $('#tomorrow2').html('Tomorrow<br />' + nextDay.formatted);
    $('#tomorrow3').html('Tomorrow<br />' + nextDay.formatted);

    // competitor headings
    var minus2InfoTip = (selectedDaysAgo - 2) + ' days ago',
        minus1Infotip = (selectedDaysAgo - 1) + ' days ago';

    $(".compyday").html(minus2Day.formatted).attr('data-infotip', minus2InfoTip);
    $(".comptoday").html(previousDay.formatted).attr('data-infotip', minus1Infotip);
}