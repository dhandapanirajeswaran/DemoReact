define(["jquery", "EmailScheduleService", "notify", "busyloader", "bootbox"],
    function ($, emailScheduleService, notify, busyloader, bootbox) {
        "use strict";

        var editing = {
            id: undefined,
            model: undefined
        };

        var months = [
            'Jan',
            'Feb',
            'Mar',
            'Apr',
            'May',
            'Jun',
            'Jul',
            'Aug',
            'Sep',
            'Oct',
            'Nov',
            'Dec'
        ];
        
        function parseJsonDate(value) {
            if (!value)
                return '';
            var ticks = Number(value.split('Date(')[1].split(')')[0]);
            return new Date(ticks);
        };

        function nullToEmptyString(value) {
            return value == null ? "" : value;
        };

        function formatDateHHMM(value) {
            var hours = value.getHours(),
                mins = value.getMinutes();
            return (hours < 10 ? '0' + hours : hours) + ':' + (mins < 10 ? '0' + mins : mins);
        };

        function pad2(value) {
            return ((100+value) + '').substring(1);
        };

        function formatDDMMMYYYY_HHMMSS(value) {
            if (!value)
                return '';
            var dt = new Date(value),
                dd = dt.getDate(),
                mo = dt.getMonth(),
                yyyy = dt.getFullYear(),
                hh = dt.getHours(),
                mi = dt.getMinutes(),
                ss = dt.getSeconds();
            return pad2(dd) + ' ' + months[mo] + ' ' + yyyy + ' at ' + pad2(hh) + ':' + pad2(mi) + ':' + pad2(ss);
        }

        function dateToCsModel(value) {
            var item = JSON.stringify({ date: value });
            return item.split('date":"')[1].split('"')[0];
        };

        function reloadPage() {
            window.location.reload();
        };

        function isSainsburysEmail(value) {
            return /@sainsburys\.co\.uk$/i.test(value);
        };

        function updateScheduleClick() {
            var modal = $('#EditScheduleItemModal'),
                hhmm = modal.find('#txtScheduledForHour').val(),
                now = new Date(),
                scheduledFor,
                emailAddress = modal.find('#txtEmailAddress').val();

            if (!/^\d{1,2}:\d{1,2}$/.test(hhmm)) {
                notify.error('Please enter a valid time (e.g. 18:20)');
                modal.find('#txtScheduledForHour').focus();
                return;
            }

            if (emailAddress != '' && !isSainsburysEmail(emailAddress)) {
                notify.error('Please enter a valid Sainsburys email address');
                modal.find('#txtEmailAddress').focus();
                return;
            }
            scheduledFor = new Date(now.getFullYear(), now.getMonth(), now.getDate(), hhmm.split(':')[0], hhmm.split(':')[1], 0, 0);

            editing.model.IsActive = modal.find('#radIsActiveYes').is(':checked');
            editing.model.ScheduledFor = dateToCsModel(scheduledFor);
            editing.model.EmailAddress = emailAddress;

            function failure() {
                notify.error('Unable to save Schedule Item');
            };
            function success() {
                $('#EditScheduleItemModal').modal('hide');
                messageAndReload('Saved Schedule Item. Reloading page');
            };

            emailScheduleService.saveSchedule(success, failure, editing.model);
        };

        function messageAndReload(message, showtime) {
            busyloader.show({
                message: message,
                showtime: showtime || 3000,
                dull: true
            });
            setTimeout(reloadPage, showtime || 3000);
        };

        function showLoader() {
            $('#divEditScheduleLoader').show();
        };

        function hideLoader() {
            $('#divEditScheduleLoader').hide();
        };

        function editScheduleClick() {
            var id = $(this).data('item-id');

            function failure() {
                hideLoader();
                notify.error('Unable to load Email Schedule Item: ' + id);
            };

            function success(data) {
                editing.id = id;
                editing.model = data;

                var modal = $('#EditScheduleItemModal'),
                    scheduledForDate = parseJsonDate(data.ScheduledFor);
                modal.find('.event-type').text(data.EventTypeName);
                if (data.IsActive)
                    modal.find('#radIsActiveYes').prop('checked', true);
                else
                    modal.find('#radIsActiveNo').prop('checked', true);

                modal.find('#txtScheduledForHour').val(formatDateHHMM(scheduledForDate));
                modal.find('.lastPolledOn').text(formatDDMMMYYYY_HHMMSS(parseJsonDate(data.LastPolledOn)));
                modal.find('.lastStartedOn').text(formatDDMMMYYYY_HHMMSS(parseJsonDate(data.LastStartedOn)));
                modal.find('.lastCompletedOn').text(formatDDMMMYYYY_HHMMSS(parseJsonDate(data.LastCompletedOn)));
                modal.find('.status').text(data.EventStatusName);
                modal.find('#txtEmailAddress').val(data.EmailAddress);

                hideLoader();
            };
            showLoader();

            emailScheduleService.loadSchedule(success, failure, id);
        };

        function runScheduleClick() {
            function failure() {
                busyloader.hide();
                notify.error('Unable to run Schedule');
            };
            function success(status) {
                if (status.ErrorMessage) {
                    busyloader.hide();
                    notify.error(status.ErrorMessage);
                    return;
                }
                messageAndReload(status.SuccessMessage ||'Schedule Ran successfully. Reloading page');
            };

            busyloader.show({
                message: 'Running Schedule. Please wait...',
                showtime: 1000,
                dull: true
            })

            emailScheduleService.runSchedule(success, failure);
        };

        function clearEventLogEntries() {
            function success(status) {
                if (status.ErrorMessage)
                    failure();
                else
                    messageAndReload(status.SuccessMessage || 'Schedule Event Log cleared');
            };
            function failure(status) {
                busyloader.hide();
                notify.error(status.ErrorMessage || 'Unable to clear Schedule Event Log');
            };

            busyloader.show({
                message: 'Clearing the Schedule Event Log. Please wait...',
                showtime: 1000,
                dull: true
            });

            emailScheduleService.clearEventLog(success, failure);
        };

        function clearEventLogClick() {
            bootbox.confirm({
                title: 'Delete Event Log Confirmation',
                message: 'Are you sure you wish to remove <strong>all</strong> the Event Log Entries?',
                buttons: {
                    cancel: {
                        label: '<i class="fa fa-times"></i> Close',
                        className: 'btn-default'
                    },
                    confirm: {
                        label: '<i class="fa fa-check"></i> Delete',
                        className: 'btn-danger'
                    }
                },
                callback: function (result) {
                    if (result) {
                        clearEventLogEntries();
                    }
                }
            });
        };

        function resetLastCompletedOnClick() {
            bootbox.confirm({
                title: 'Confirmation Reset Last Completed On',
                message: 'Are you sure you want to reset the <strong>Last Completed On</strong> for the <strong>Daily Price Email</strong>?<br />'
                    + '<br />'
                    + 'This will allow the email to be <strong class="text-danger">re-sent today</strong> (even if it has already been sent)',
                buttons: {
                    confirm: {
                        label: '<i class="fa fa-check"></i> Reset',
                        className: 'btn-warning'
                    },
                    cancel: {
                        label: '<i class="fa fa-times"></i> Close',
                        className: 'btn-default'
                    },
                },
                callback: function (result) {
                    if (result)
                        clearEmailLastCompletedOn();
                }
            })
        };

        function clearEmailLastCompletedOn() {
            function failure() {
                busyloader.hide();
                notify.error('Unable to reset the Last Completed On');
            };

            function success() {
                busyloader.hide();
                messageAndReload(status.SuccessMessage || 'Last Completed On reset. Reloading page');
            };

            busyloader.show({
                message: 'Clearing Email Last Completed On...',
                showtime: 1000,
                dull: true
            });

            emailScheduleService.markEmailPendingForToday(success, failure);
        };

        function bindEvents() {
            $(document.body).on('click', '#ScheduleItemModalUpdateButton', updateScheduleClick );
            $('[data-click="EditScheduleItem"]').off().click(editScheduleClick);
            $('#btnRunSchedule').off().click(runScheduleClick);
            $('#btnClearEventLog').off().click(clearEventLogClick);
            $('#btnResetLastCompletedOn').off().click(resetLastCompletedOnClick);
        };

        function init() {
            bindEvents();
        };

        // API
        return {
            init: init
        };
    }
);