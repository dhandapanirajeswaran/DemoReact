define(["jquery", "common", "notify", "PetrolPricingService", "waiter", "DateUtils", "text!App/EmailLogViewer.html"],
    function ($, common, notify, petrolPricingService, waiter, dateUtils, modalHtml) {
        "use strict";

        var modal = $(modalHtml);

        function show(emailSendLogId) {
            if (emailSendLogId == 0) {
                hide();
                notify.error('Missing EmailSendLogId');
                return;
            }

            function failure() {
                waiter.hide();
                notify.error('Unable to load Email Log');
            };

            function success(data) {
                populateModal(data);
                waiter.hide();
                modal.modal('show');
            };

            function populateModal(data) {
                modal.find('[data-id="subject-line"]').text(data.EmailSubject);
                modal.find('[data-id="end-of-trade-date"]').html(dateUtils.format("DD MMM YYYY", dateUtils.convertJsonDate(data.EndTradeDate)));
                modal.find('[data-id="error-message"]').html(formatOrNone(data.ErrorMessage));
                modal.find('[data-id="is-error"]').html(formatYesNo(data.IsError));
                modal.find('[data-id="is-success"]').html(formatYesNo(data.IsSuccess));
                modal.find('[data-id="is-warning"]').html(formatYesNo(data.IsWarning));
                modal.find('[data-id="send-date"]').html(dateUtils.format("DD MMM YYYY", dateUtils.convertJsonDate(data.SendDate)));
                modal.find('[data-id="site-name"]').html(data.SiteName);
                modal.find('[data-id="email-body"]').html(formatHTML(data.EmailBody));
                modal.find('[data-id="warning-message"]').html(formatOrNone(data.WarningMessage));
            };

            waiter.show({
                title: 'Please wait',
                message: 'Loading Email Log. Please wait',
                icon: 'clock'
            });

            petrolPricingService.getEmailSendLog(success, failure, emailSendLogId);
        };

        function formatOrNone(value) {
            return /\S/.test(''+value)
            ? value + '&nbsp;'
            : '-- none --';
        };

        function formatYesNo(value) {
            return value
            ? '<fa class="fa fa-check text-success"></i> Yes'
            : '<fa class="fa fa-times text-danger"></i> No';
        };

        function formatHTML(value) {
            var cleaned = $('<div>').html(value);
            return cleaned.html();
        };

        function hide() {
            modal.modal('hide');
        };

        function injectDom() {
            modal.appendTo(document.body);
        };

        function docReady() {
            injectDom();
        };

        $(docReady);

        // API
        return {
            show: show,
            hide: hide
        };
    }
);