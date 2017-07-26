define(["jquery", "common", "notify", "busyloader", "bootbox", "EmailTemplateService", "text!App/Popups/SiteEmailPopup.html", "text!App/Popups/SiteEmailHelp.html", "EditInPlace"],
    function ($, common, notify, busyloader, bootbox, emailTemplateService, popupHtml, siteEmailHelpHtml, editInPlace ) {

        "use strict";

        var popup = $(popupHtml);
        var help = $(siteEmailHelpHtml).hide();

        var dimensions = {
            minModalHeight: 0
        };

        var params = {
            siteIdList: [],
            siteName: '',
            siteId: 0,
            dayMonthYear: '',
            prices: {
                unleaded: 0,
                diesel: 0,
                superUnleaded: 0
            },
            isViewingHistorical: false,
            hasUnsavedChanges: false,
            save: function () { },
            enableSiteEmails: false,
            settingsPageUrl: ''
        };

        var states = {
            IsLoading: false,
            HasUnsavedChanges: false,
            WasTemplateModified: false,
            SubjectLine: renderSubjectLine,
            EmailBody: renderEmailBody,
            IsEditingSubjectLine: false,
            IsEditingEmailBody: false,
            IsCopyDisabled: renderCopyDisabled,
            IsDeleteDisabled: renderDeleteDisabled,
            EnableSiteEmails: false,
            SiteEmailTestAddresses: '',
            SettingsPageUrl: '',
            SiteEmailTestsInfotip: ''
        };

        var templateOptions = [];

        var tokenOptions = {
            'SiteName': { text: 'Site Name' },
            'DayMonthYear': { text: 'Current date' },
            'UnleadedPrice': { text: 'Unleaded Price' },
            'SuperPrice': { text: 'Super-Unleaded Price' },
            'DieselPrice': { text: 'Diesel Price' }
        };

        var emailTemplate = {
            EmailTemplateId: 0,
            TemplateName: '',
            SubjectLine: '',
            EmailBody: ''
        };

        var defaultEmailTemplate;

        var selectors = {
            menu: '#SiteEmailPopup_EmailTemplateMenu',
            subjectLine: '#SiteEmailPopup_SubjectLine',
            emailBody: '#SiteEmailPopup_EmailBody',
            helpButton: '#btnSiteEmailHelp',
            closeHelpButton: '#btnSiteEmailCloseHelp',
            resetButton: '#btnSiteEmailResetTemplate',
            sendButton: '#btnEmailSite',
            copyButton: '#btnSiteEmailCopy',
            deleteButton: '#btnSiteEmailDelete',
            templateMenu: '#mnuSiteEmailTemplate'
        };

        var controls = {
            menu: null,
            helpButton: null,
            closeHelpButton: null,
            sendButton: null
        };

        var escapeRegEx = function (value) {
            return value.replace(/[\-\[\]{}()*+?.,\\\^$|#\s]/g, "\\$&");
        };

        var replaceTokens = function (format, tokens) {
            var result = format,
                key,
                regex;
            for (key in tokens) {
                if (tokens.hasOwnProperty(key)) {
                    regex = new RegExp(escapeRegEx(key), 'g');
                    result = result.replace(regex, tokens[key]);
                }
            }
            return result;
        };

        function getStandardTokens() {
            var tokens = {
                '{SiteName}': params.siteName,
                '{DayMonthYear}': params.dayMonthYear,
                '{UnleadedPrice}': params.prices.unleaded,
                '{SuperPrice}': params.prices.superUnleaded,
                '{DieselPrice}': params.prices.diesel
            };
            return tokens;
        };

        function renderSubjectLine() {
            if (states.IsEditingSubjectLine)
                return emailTemplate.SubjectLine;
            else {
                var tokens = getStandardTokens();
                return replaceTokens(emailTemplate.SubjectLine, tokens);
            }
        };

        function renderEmailBody() {
            if (states.IsEditingEmailBody)
                return emailTemplate.EmailBody;
            else {
                var tokens = getStandardTokens();
                return replaceTokens(emailTemplate.EmailBody, tokens);
            }
        };

        function renderCopyDisabled() {
            return states.IsLoading || templateOptions.length == 0;
        };

        function renderDeleteDisabled() {
            return states.IsLoading || isViewingDefaultTemplate();
        };

        function highlightTokens(template) {
            return template.replace(/{/g, '<span class="token">{').replace(/}/g, '}</span>');
        };

        function startLoading() {
            states.IsLoading = true;
            refresh();
        };

        function endLoading() {
            states.IsLoading = false;
            refresh();
        };

        function openPopup(parameters) {
            params = $.extend({}, parameters);

            popup.find(selectors.emailBody).html('');

            $(document.body).append(popup);
            redrawTemplateButtons();
            redrawEditableZones();
            refresh();
            states.IsLoading = true;
            states.EnableSiteEmails = params.enableSiteEmails;
            states.SiteEmailTestAddresses = params.siteEmailTestAddresses.replace(/;/g, '<br />');
            states.SettingsPageUrl = params.settingsPageUrl;
            states.SiteTestEmailsInfotip = '[u]' + params.siteEmailTestAddresses.replace(/;/g, '[br /]') + '[/u]';

            var opts = {
                show: true,
                keyboard: true,
            };
            popup.modal(opts);

            popup.on('shown.bs.modal', afterModalShown);

            $(document.body).append(help);
        };

        function afterModalShown() {
            limitEmailBodyHeight();
            showContent();
            $(window).on('resize', windowResized);
        };

        function windowResized() {
            limitEmailBodyHeight();
        };

        function limitEmailBodyHeight() {
            $(selectors.emailBody).hide();

            var dialog = popup.find('.modal-dialog'),
                dialogHeight = dialog.outerHeight(),
                winHeight = $(window).height(),
                safetyGap = 80,
                maxContentHeight = winHeight - dialogHeight - safetyGap;

            $(selectors.emailBody).css({ 'maxHeight': maxContentHeight, 'overflow': 'auto' });

            $(selectors.emailBody).show();
        };

        function showContent() {
            loadTemplateOptions();
            loadDefaultEmailTemplate();
            bindEvents();
        };

        function closePopup() {
            help.hide().detach();
            unbindEvents();
            popup.modal('hide');
        };

        var refreshActions = {
            'data-visible': function (cond, ele) {
                cond ? ele.show() : ele.hide();
            },
            'data-not-visible': function (cond, ele) {
                cond ? ele.hide() : ele.show();
            },
            'data-disable': function (cond, ele) {
                ele.attr('disabled', cond);
            },
            'data-enabled': function (cond, ele) {
                ele.attr('disabled', !cond);
            },
            'data-html': function (html, ele) {
                ele.html(html);
            },
            'data-text': function (text, ele) {
                ele.text(text);
            },
            'data-val': function (value, ele) {
                ele.val(value);
            },
            'data-checked': function (cond, ele) {
                ele.attr('checked', cond);
            },
            'data-link': function (url, ele) {
                ele.attr('href', url);
            },
            'data-set-infotip': function (text, ele) {
                ele.attr('data-infotip', text);
            }
        };

        function refresh() {
            for (var key in refreshActions) {
                popup.find('[' + key + ']').each(function () {
                    var ele = $(this),
                        expr = ele.attr(key),
                        value;
                    if (expr in states) {
                        value = states[expr];
                        value = $.isFunction(value) ? value(ele) : value;
                        refreshActions[key](value, ele);
                    } else
                        console.log('Unable to find ' + expr + ' in states');
                });
            }
        };

        function findControls() {
            controls.menu = popup.find(selectors.menu);
            controls.helpButton = popup.find(selectors.helpButton);
            controls.sendButton = popup.find(selectors.sendButton);
        };

        function isViewingDefaultTemplate() {
            return $(selectors.templateMenu).val() == getDefaultTemplateId();
        };

        function getDefaultTemplateId() {
            var id = 0;
            $.each(templateOptions, function() {
                if (this.IsDefault)
                    id = this.EmailTemplateId;
            });
            return id;
        };

        function isDefaultTemplateId(emailTemplateId) {
            return getDefaultTemplateId() == emailTemplateId;
        };

        function getSelectedTemplateId() {
            return $(selectors.templateMenu).val();
        };

        function redrawTemplateButtons() {
            var copyButton = $(selectors.copyButton),
                deleteButton = $(selectors.deleteButton),
                isDefaultTemplate = isDefaultTemplateId(getSelectedTemplateId());

            copyButton.attr('disabled', templateOptions.length == 0);
            deleteButton.attr('disabled', isDefaultTemplate);
        };

        function populateOptionsMenu() {
            var menu = $('#mnuSiteEmailTemplate');
            menu.children().remove();
            $.each(templateOptions, function () {
                var item = this;
                menu.append($('<option />').val(item.EmailTemplateId).text(item.TemplateName));
            });
        };

        function loadTemplateOptions() {
            var success = function (data) {
                templateOptions = data.JsonObject
                populateOptionsMenu();
                redrawTemplateButtons();
                redrawEditableZones();
                endLoading();
            };

            var failure = function () {
                notify.error('Unable to load Email Template options');
                endLoading();
            };

            startLoading();
            emailTemplateService.getTemplateNames(success, failure);
        };

        function loadDefaultEmailTemplate() {
            loadEmailTemplate(0);
        };

        function loadEmailTemplate(emailTemplateId) {
            startLoading();

            emailTemplateService.getTemplate(function (serverData) {
                commonLoadedEmailTemplate(serverData);
            },
                function () {
                    notify.error('Unable to Load Email Template');
                    $(selectors.templateMenu).val(getDefaultTemplateId());
                    endLoading();
                },
            emailTemplateId);
        };

        function commonLoadedEmailTemplate(serverData) {
            var model = serverData.JsonObject;
            if (serverData && model) {
                delete model.IsDefault;
                delete model.PPUserId;
                emailTemplate = model;
                defaultEmailTemplate = $.extend({}, model);
                states.WasTemplateModified = false;
                refresh();
                bindEmailTemplateEvents();
                redrawEditableZones();
                endLoading();
            } else
                notify.error('Unable to Load Email template');
        };

        function bindEmailTemplateEvents() {
            popup.find(selectors.subjectLine).off().click(subjectLineClick);
            popup.find(selectors.emailBody).off().click(emailBodyClick);
            popup.find(selectors.resetButton).off().click(resetButtonClick);
            popup.find(selectors.sendButton).off().click(sendButtonClick);
            popup.find(selectors.templateMenu).off().on('change', templateMenuChange);
            popup.find(selectors.copyButton).off().click(copyButtonClick);
            popup.find(selectors.deleteButton).off().click(deleteButtonClick);
        };

        function enterEditMode(ele) {
            ele.removeClass('editable-email-field').addClass('editing-email-field');
        };

        function exitEditMode(ele) {
            ele.removeClass('editing-email-field').addClass('editable-email-field');
        };

        function subjectLineClick() {
            if (isViewingDefaultTemplate())
                return;
            if (states.IsEditingSubjectLine)
                return;
            var ele = $(selectors.subjectLine);
            enterEditMode(ele);
            states.IsEditingSubjectLine = true;
            states.IsEditingEmailBody = false;
            refresh();
            editInPlace.edit({
                selector: selectors.subjectLine,
                layout: 'emailSubject',
                helpClick: helpButtonClick,
                tokens: tokenOptions,
                save: function (content) {
                    emailTemplate.SubjectLine = content;
                    saveEmailTemplate(emailTemplate);
                    states.IsEditingSubjectLine = false;
                    states.WasTemplateModified = true;
                    refresh();
                },
                close: function () {
                    exitEditMode(ele);
                    states.IsEditingSubjectLine = false;
                    refresh();
                }
            });
        };

        function emailBodyClick() {
            if (isViewingDefaultTemplate())
                return;
            if (states.IsEditingEmailBody)
                return;
            var ele = $(selectors.emailBody);
            ele.removeClass('editable-email-field').addClass('editing-email-field');
            states.IsEditingEmailBody = true;
            states.IsEditingSubjectLine = false;
            refresh();
            editInPlace.edit({
                selector: selectors.emailBody,
                layout: 'emailBody',
                helpClick: helpButtonClick,
                tokens: tokenOptions,
                save: function (content) {
                    exitEditMode(ele);
                    emailTemplate.EmailBody = content;
                    saveEmailTemplate(emailTemplate);
                    states.WasTemplateModified = true;
                    states.IsEditingEmailBody = false;
                    refresh();
                },
                close: function () {
                    exitEditMode(ele);
                    states.IsEditingEmailBody = false;
                    refresh();
                }
            });
        };

        function sanitiseTemplate(template) {
            var clean = {
                EmailTemplateId: template.EmailTemplateId,
                TemplateName: template.TemplateName.replace(/[<>]/g, ''),
                SubjectLine: common.htmlEncode(template.SubjectLine),
                EmailBody: common.htmlEncode(template.EmailBody)
            }
            return clean;
        };

        function saveEmailTemplate(template) {
            var safeTemplate = sanitiseTemplate(template);
            function success(serverData) {
                notify.success('Saved Email Template');
            };
            function failure() {
                notify.error('Unable to save Email Template');
            };
            emailTemplateService.updateTemplate(success, failure, safeTemplate);
        };

        function resetButtonClick() {
            bootbox.confirm({
                title: '<i class="fa fa-question"></i> Reset Email Template Confirmation',
                message: 'Do you want to reset the Email Template back to the default?<br /><br /><strong>Note:</strong> This will lose any changes you have made.',
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
                        emailTemplate = $.extend({}, defaultEmailTemplate);
                        states.WasTemplateModified = false;
                        refresh();
                        notify.info('Email Template has been reset to the default template');
                    }
                }
            });
        };

        function sendButtonClick() {
            var count = params.siteIdList.length,
                message = count == 1
                ? 'Do you want to send this email to ' + params.siteName + '?'
                : 'Do you want to send this email to ' + count + ' selected Sites ?',
                sending = count == 1
                ? 'Sending email to ' + params.siteName + '. Please wait...'
                : 'Sending emails to ' + count + ' selected sites. Please wait...';

            bootbox.confirm({
                title: '<i class="fa fa-question"></i> Send Email Confirmation',
                message: message,
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
                        $('#SiteEmailPopup').modal('hide');
                        params.send(getSelectedTemplateId(), params.siteIdList);
                        busyloader.show({
                            message: sending,
                            showtime: 2000,
                            dull: true
                        });
                    }
                }
            });
        };

        function templateMenuChange() {
            if (states.IsLoading)
                return;
            var emailTemplateId = getSelectedTemplateId();
            redrawTemplateButtons();
            loadEmailTemplate(emailTemplateId);
            redrawEditableZones();
        };

        function redrawEditableZones() {
            var subjectLine = $(selectors.subjectLine),
                emailBody = $(selectors.emailBody),
                editClass = 'editable-email-field',
                readonlyClass = 'readonly-email-field';

            if (isViewingDefaultTemplate()) {
                subjectLine.removeClass(editClass).addClass(readonlyClass).attr('data-infotip', '');
                emailBody.removeClass(editClass).addClass(readonlyClass).attr('data-infotip', '');
            } else {
                subjectLine.removeClass(readonlyClass).addClass(editClass).attr('data-infotip', 'Click to edit the [b]Email Subject line[/b].');
                emailBody.removeClass(readonlyClass).addClass(editClass).attr('data-infotip', 'Click to edit the [b]Email Body[/b].');
            }
        };

        function copyButtonClick() {
            var emailTemplateId = getSelectedTemplateId();
            if (emailTemplateId == 0)
                return;

            bootbox.prompt('Please enter a name for the new Email Template', function (templateName) {
                if (templateName != null) {
                    templateName = templateName.replace(/^\s+|\s+$/g, '');
                    if (templateName == '') {
                        notify.error('Please enter a name for the Email Template');
                    } else
                        createEmailTemplateClone(emailTemplateId, templateName);
                }
            });
        };

        function createEmailTemplateClone(emailTemplateId, templateName) {
            function success(serverData) {
                var model = serverData.JsonObject,
                    menu = $('#mnuSiteEmailTemplate');
                templateOptions.push(model);
                menu.append($('<option />').val(model.EmailTemplateId).text(model.TemplateName));
                menu.val(model.EmailTemplateId);
                redrawTemplateButtons();
                commonLoadedEmailTemplate(serverData);
                endLoading();
                notify.success('Created a new Email Template');
            };
            function failure() {
                endLoading();
                notify.error('Unable to clone the Email Template');
            };
            startLoading();
            emailTemplateService.createEmailTemplateClone(success, failure, emailTemplateId, templateName);
        };

        function deleteButtonClick() {
            var emailTemplateId = getSelectedTemplateId();
            if (emailTemplateId == 0)
                return;

            bootbox.confirm({
                title: '<i class="fa fa-question"></i> Delete Confirmation',
                message: 'Are you sure you wish to delete this Email Template?',
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
                        deleteEmailTemplate(emailTemplateId);
                    }
                }
            });
        };

        function removeTemplateOption(emailTemplateId) {
            for (var i = 0; i < templateOptions.length; i++) {
                if (templateOptions[i].EmailTemplateId == emailTemplateId);
                {
                    templateOptions.splice(i, 1);
                }
            }
        };

        function deleteEmailTemplate(emailTemplateId) {
            function success() {
                loadDefaultEmailTemplate();
                loadTemplateOptions();
                notify.success('Email Template has been deleted.');
            };

            function failure() {
                endLoading();
                notify.error('Unable to delete Email Template');
            };
            startLoading();
            emailTemplateService.deleteTemplate(success, failure, emailTemplateId);
        };

        function helpButtonClick() {
            var button = controls.helpButton;
            if (help.is(':visible'))
                hideHelp();
            else {
                button.removeClass('btn-default').addClass('btn-success');
                help.fadeIn(1000);
                $(selectors.closeHelpButton).off().click(hideHelp);
            }
        };

        function hideHelp() {
            help.off();
            help.fadeOut(1000);
            controls.helpButton.removeClass('btn-success').addClass('btn-default');
        };

        function bindEvents() {
            findControls();
            controls.helpButton.off().click(helpButtonClick);
        };

        function unbindEvents() {
            popup.off();
            $(window).off('resize', windowResized);
        };

        // API
        return {
            openPopup: openPopup
        };
    }
);