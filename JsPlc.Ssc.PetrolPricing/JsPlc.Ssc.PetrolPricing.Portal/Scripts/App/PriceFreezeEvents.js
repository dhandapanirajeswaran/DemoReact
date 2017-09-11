define(["jquery", "common", "waiter", "notify", "infotips", "bootbox", "DateUtils", "PriceFreezeEventService", "bootstrap-datepicker", "bootstrap-datepickerGB", "text!App/PriceFreezeEvents.html"],
    function ($, common, waiter, notify, infotips, bootbox, dateUtils, priceFreezeEventService, datepicker, datePickerGb, modalHtml) {
        "use strict";

        var modal = $(modalHtml);

        var model = {
            PriceFreezeEventId: 0,
            DateFrom: new Date(),
            Days: 0,
            IsActive: true
        };

        function reloadPage() {
            window.location.reload();
        };

        function injectDom() {
            modal.hide().appendTo(document.body);
        };

        function redrawEndsOn() {
            var dateFrom = $('#dpDateFrom').val(),
                dt = dateUtils.dateFromDDMMYYYY(dateFrom),
                days = $('#mnuDays').val(),
                endsOn = '';

            if (isDate(dateFrom) && Number(days) > 0)
                endsOn = dateUtils.format('DD/MM/YYYY', dateUtils.addDays(days - 1, dt));

            $('#spnEndsOn').text(endsOn);
        };

        function dateFromDDMMYYYY(value) {
            var parts = value.split('/');
            return new Date(parts[2], parts[1] - 1, parts[0], 0, 0, 0, 0);
        };

        function addEventClick() {
            populate({
                PriceFreezeEventId: 0,
                DateFrom: new Date(),
                Days: 7,
                IsActive: true
            });

            modal.modal('show');
        };

        function populate(data) {
            model.PriceFreezeEventId = data.PriceFreezeEventId;
            model.DateFrom = data.DateFrom;
            model.Days = Number(data.Days);

            $('#dpDateFrom').val(dateUtils.format('DD/MM/YYYY', model.DateFrom));
            $('#mnuDays').val(model.Days);
            $('#mnuIsActive').val(data.IsActive ? '1' : '0');
            redrawEndsOn();
        };

        function isDate(value) {
            return /^\d{1,2}\/\d{1,2}\/\d{4}$/.test(value)
        };

        function dateToCsModel(value) {
            var item = JSON.stringify({ date: value });
            return item.split('date":"')[1].split('"')[0];
        };

        function addClick() {
            var dateFrom = $('#dpDateFrom').val(),
                days = $('#mnuDays').val(),
                isActive = $('#mnuIsActive').val(),
                data;

            if (!isDate(dateFrom)) {
                notify.error('Please enter a Date (DD/MM/YYYY)');
                $('#dpDateFrom').focus();
                return;
            }

            if (isNaN(days)) {
                notify.error('Please choose the number of Days');
                $('#mnuDays').focus();
                return;
            }

            model.DateFrom = dateFrom;
            model.Days = Number(days);

            data = {
                PriceFreezeEventId: model.PriceFreezeEventId,
                DateFrom: dateToCsModel(model.DateFrom),
                Days: days,
                IsActive: Number(isActive) ? 'true' : 'false'
            };

            function failure(response) {
                showResponseStatus(response, 'Unable to update Price Freeze Event');
            };

            function success(response) {
                if (showResponseStatus(response, 'Saved Price Freeze Event')) {
                    $('#PriceFreezeEventsGrid').css('opacity', 0.3);
                    modal.modal('hide');
                    setTimeout(reloadPage, 2000);
                }
            };

            priceFreezeEventService.updatePriceFreezeEvent(success, failure, data);
        };

        function deleteClick() {
            var eventId = $(this).data('event-id');
            if (!eventId)
                return;

            bootbox.confirm({
                title: '<i class="fa fa-question"></i> Delete Confirmation',
                message: 'Are you sure you wish to <strong>delete</strong> this <strong>Price Freeze Event</strong>?',
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
                        priceFreezeEventService.deletePriceFreezeEvent(deleteSuccess, deleteFailure, eventId)
                    }
                }
            })
        };

        function deleteFailure(response) {
            showResponseStatus(response, 'Unable to delete Price Freeze Event');
        };

        function deleteSuccess(response) {
            if (showResponseStatus(response, 'Unable to Delete Price Event'))
                setTimeout(reloadPage, 2000);
        };

        function showResponseStatus(response, bad) {
            if (response && response.SuccessMessage) {
                notify.success(response.SuccessMessage);
                return true;
            }
            if (response && response.ErrorMessage) {
                notify.error(response.ErrorMessage);
                return false;
            }
            notify.error(bad);
            return false;
        };

        function editClick() {
            var eventId = $(this).data('event-id');
            if (!eventId)
                return;

            function success(response) {
                if (!response.PriceFreezeEventId)
                    failure(response);
                else {
                    var data = {
                        PriceFreezeEventId: response.PriceFreezeEventId,
                        DateFrom: common.convertJsonDate(response.DateFrom),
                        Days: response.Days,
                        IsActive: response.IsActive
                    };

                    populate(data);
                    modal.modal('show');
                }
            };

            function failure(response) {
                showResponseStatus(response, 'Unable to edit Price Freeze Event');
            };

            priceFreezeEventService.getPriceFreezeEvent(success, failure, eventId);
        };
        function bindEvents() {
            $('#btnAddEvent').off().click(addEventClick);
            $('#btnPriceFreezeEventAdd').off().click(addClick);
            $('#dpDateFrom').on('change', redrawEndsOn);
            $('#mnuDays').on('change', redrawEndsOn);
            $(document.body).on('click', '[data-click-action="delete-event"]', deleteClick);
            $(document.body).on('click', '[data-click-action="edit-event"]', editClick);
        };

        function initDatePickers() {
            $('.datepicker')
                .datepicker({
                    language: "en-GB",
                    format: "dd/mm/yyyy",
                    orientation: 'auto top',
                    autoclose: true,
                    todayHighlight: true,
                    startDate: '-3m',
                    endDate: '+3m'
                });
        };

        function docReady() {
            bindEvents();
            initDatePickers();
        };

        function init() {
            injectDom();
            $(docReady);
        };

        // API
        return {
            init: init
        };
    }
);