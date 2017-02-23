define(['jquery'], function ($) {

    var positions = {
        visible: { 'top': '8px' },
        hidden: {'top': '-100px'}
    };

    var notifyDefs = {
        "success": {selector: "#notification-generic-success", delay: 100, show: 500, pause: 2000, hide: 1000},
        "error": {selector: "#notification-generic-error", delay: 100, show: 500, pause: 4000, hide: 3000},
        "warning": { selector: "#notification-generic-warning", delay: 100, show: 500, pause: 1500, hide: 3000 },
        "info": { selector: "#notification-generic-info", delay: 100, show: 500, pause: 1000, hide: 500 }
    };

    var hideAll = function () {
        $.each(notifyDefs, function (item) {
            $(item.selector).hide();
        });
    };

    var show = function (alertType, message, extratime) {
        hideAll();
        if (!/\S/.test(message)) return;
        var notif = notifyDefs[alertType],
            readingTime = message.replace(/[^a-z]/g, '').length * 100,
            extratime = extratime || 0;

        $(notif.selector).find('.message').html(message).end()
            .stop(true, true)
            .css(positions.hidden)
            .show()
            .delay(notif.delay)
            .animate(positions.visible, notif.show)
            .delay(notif.pause + readingTime + extratime)
            .animate(positions.hidden, notif.hide);
    };

    var showError = function (message, extratime) {
        show("error", message, extratime);
    };

    var showSuccess = function (message, extratime) {
        show("success", message, extratime);
    };

    var showWarning = function (message, extratime) {
        show("warning", message, extratime);
    }

    var showInfo = function (message, extratime) {
        show("info", message, extratime);
    }

    var showAlertType = function (alertType, message, extratime) {
        switch (alertType) {
            case "error":
            case "danger":
                showError(message, extratime);
                break;
            case "success":
                showSuccess(message, extratime);
                break;
            case "warning":
                showWarning(message, extratime);
                break;
            case "info":
                showInfo(message, extratime);
                break;
            default:
                showError(message, extratime);
                break;
        }
    }

    $(function () {
        $('.auto-hide-alert').each(function (index) {
            var item = $(this),
                alertType = item.data('alert-type'),
                alertMessage = item.text();
            showAlertType(alertType, alertMessage);
        });
    });

    $.fn.notify = function () {
        var src = $($(this)[0]),
            alertType = src.find('strong').text().replace(/[^a-z]/ig, '').toLowerCase(),
            alertMessage = src.clone(false).find('a').remove().end().find('strong').remove().end().text().replace(/^\s+|\s+$/, '');
        showAlertType(alertType, alertMessage);
        return this;
    };

    return {
        hide: hideAll,
        error: showError,
        success: showSuccess,
        warning: showWarning,
        info: showInfo
    };
});