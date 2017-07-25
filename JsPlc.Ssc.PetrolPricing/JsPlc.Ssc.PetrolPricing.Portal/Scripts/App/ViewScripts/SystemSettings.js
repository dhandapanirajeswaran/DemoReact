define(["infotips", "notify", "PetrolPricingService", "busyloader", "validation", "bootbox"],
    function (infotips, notify, petrolPricingService, busyloader, validation, bootbox) {

        "use strict";

        var controls = {
            emailData: undefined,
            emailList: undefined,
            emailInput: undefined,
            addButton: undefined,
            testWarning: undefined,
            liveWarning: undefined,
            enableMenu: undefined
        };

        function findControls() {
            controls.emailData = $('#SiteEmailTestAddresses');
            controls.emailList = $('#SiteTestEmailList');
            controls.emailInput = $('#txtSiteTestEmail');
            controls.addButton = $('#btnSiteTestEmailAdd');
            controls.testWarning = $('#divTestEmailInfo');
            controls.liveWarning = $('#divLiveEmailInfo');
            controls.enableMenu = $('#EnableSiteEmails');
        };

        function trim(str) {
            return (str + '').replace(/^\s+|\s+$/, '');
        };

        function getEmailIndex(email) {
            var emails = getEmailList(),
                i;
            for (i = 0; i < emails.length; i++) {
                if (email.toUpperCase() == emails[i].toUpperCase())
                    return i;
            }
            return -1;
        };

        function addButtonClick() {
            var email = trim(controls.emailInput.val()).toLowerCase(),
                emails = getEmailList(),
                i;

            if (email == '') {
                bootbox.alert('Please enter a valid Email address');
                controls.emailInput.focus();
                return;
            }

            if (email.indexOf('@') == -1) {
                email = email + '@sainsburys.co.uk';
            }

            if (!validation.isSainsburysEmail(email)) {
                bootbox.alert('Please enter a valid Sainsburys email!');
                controls.emailInput.focus();
                return;
            }
            if (getEmailIndex(email) != -1) {
                bootbox.alert('List already contains the email: ' + email);
                controls.emailInput.focus();
                return;
            }
            emails.push(email);
            emails.sort();
            setEmailList(emails);
            renderEmailList();
            controls.emailInput.val('');
            controls.emailInput.focus();
        };

        function confirmDeleteTestEmail() {
            var email = $(this).data('email');
            if (email) {
                bootbox.confirm({
                    title: 'Delete Confirmation',
                    message: 'Are you sure you wish to delete this test Email ?<br /><br />'
                        + '<strong>' + email + '</strong>',
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
                            removeTestEmail(email);
                            renderEmailList();
                        }
                    }
                });
            }
        };

        function removeTestEmail(email) {
            var emails = getEmailList(),
                index = getEmailIndex(email);

            if (index == -1)
                return;
            emails.splice(index, 1);
            setEmailList(emails);
        };


        function emailInputKeyup(ev) {
            if (ev.keyCode == 13) {
                addButtonClick();
                ev.preventDefault();
                return false;
            }
            return true;
        };

        function redrawEnableWarnings() {
            var index = controls.enableMenu[0].selectedIndex;
            if (index == 1) {
                controls.enableMenu.removeClass('live-email-disabled').addClass('live-email-enabled');
                controls.testWarning.hide();
                controls.liveWarning.show();
            } else {
                controls.enableMenu.removeClass('live-email-enabled').addClass('live-email-disabled');
                controls.testWarning.show();
                controls.liveWarning.hide();
            }
        };

        function bindEvents() {
            controls.addButton.off().click(addButtonClick);
            controls.emailInput.on('keypress', emailInputKeyup);
            controls.enableMenu.on('change', redrawEnableWarnings);

            $(document.body).on('click', '[data-click="deleteSiteTestEmail"]', confirmDeleteTestEmail);
        };

        function getEmailList() {
            var val = controls.emailData.val();
            return val == '' ? [] : val.split(';');
        };

        function setEmailList(emails) {
            var val = emails.join(';');
            controls.emailData.val(val);
        };

        function renderEmailList() {
            var html = [],
                emails = getEmailList(),
                count = emails.length,
                i;

            if (count == 0) {
                html.push('<div class="alert alert-danger text-center no-margin"><i class="fa fa-warning"></i> There are NO test emails &mdash; Please add one or more</div>');
            } else {
                html.push('<table class="table table-condensed table-striped no-margin">');
                html.push('<tbody>');
                for (i = 0; i < count; i++) {
                    html.push('<tr>');
                    html.push('<td>' + emails[i] + '</td>');
                    html.push('<td><button type="button" class="btn btn-danger btn-xs pull-right" data-click="deleteSiteTestEmail" data-email="' + emails[i] + '" data-infotip="Delete this Email"><i class="fa fa-times"></i></button></td>');
                    html.push('</tr>');
                }
                html.push('</tbody>');
                html.push('</table>');
            }
            controls.emailList.html(html.join(''));
        };

        function docReady() {
            findControls();
            renderEmailList();
            redrawEnableWarnings();
            bindEvents();

            var error = $('#hdnPageErrorMessage').val(),
                success = $('#hdnPageSuccessMessage').val()

            if (/\S/.test(error))
                notify.error(error);

            if (/\S/.test(success)) {
                notify.success(success);

                // mark Price Snapshot outdate (we don't care about result)
                var slacker = function () { };
                petrolPricingService.triggerDailyPriceRecalculation(slacker, slacker);
            }
        };

        function init() {
            $(docReady);
        };

        // API
        return {
            init: init
        }
    }
);
