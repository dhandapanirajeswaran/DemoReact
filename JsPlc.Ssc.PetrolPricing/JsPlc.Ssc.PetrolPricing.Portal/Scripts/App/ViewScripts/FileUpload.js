define(['jquery', 'common', 'notify', 'busyloader', 'DateUtils', 'bootstrap-datepicker', 'waiter', "PriceFreezeEventService"],
    function ($, common, notify, busyloader, dateUtils, bsDatePicker, waiter, priceFreezeEventService) {
        "use strict";

        var options = {
            datePicker: false
        };

        var lastPriceFreezeEventDate = '';

        var uploadTypeDefs = {
            '1': {
                message: 'Daily Price Data',
                accept: ".csv, .txt",
                prompt: 'Please select a CSV or TXT file',
                templateId: null,
                filenameRegex: /^2\d{7}_\d{4}-DailyPriceData[\. ]/i,
                submit: {
                    title: 'Uploading Daily Price Data',
                    message: 'ETA: between 1 and 2 minutes'
                },
                help: '#divDailyPriceHelp',
                mostRecent: '#divMostRecentDailyCatalistInfo'
            },
            '2': {
                message: 'Quarterly Site Data',
                accept: ".xls, .xlsx",
                prompt: 'Please select an Excel file',
                templateId: 'DownloadQuarterlyTemplateButton',
                filenameRegex: /^2\d{7}_\d{4}-QuarterlySiteData[\. ]/i,
                submit: {
                    title: 'Uploading Quarterly Site Data File',
                    message: 'ETA: between 1 and 2 minutes'
                },
                help: '#divQuarterlyFileHelp',
                mostRecent: '#divMostRecentQuarterlyInfo'
            },
            '3': {
                message: 'Latest JS Price Data',
                accept: ".xls, .xlsx",
                prompt: 'Please select an Excel file',
                templateId: 'DownloadLatestPriceTemplateButton',
                filenameRegex: /^2\d{7}_\d{4}-LatestJsPriceData[\. ]/i,
                submit: {
                    title: 'Uploading Latest JS Price Data',
                    message: 'ETA: less than 1 minute'
                },
                help: '#divLatestJsHelp',
                mostRecent: '#divMostRecentJSPriceDataInfo'
            },
            '4': {
                message: 'Latest Competitors Price Data',
                accept: ".xls, .xlsx",
                prompt: 'Please select an Excel file',
                templateId: 'DownloadLatestCompPriceTemplateButton',
                filenameRegex: /^2\d{7}_\d{4}-LatestCompPriceData[\. ]/i,
                submit: {
                    title: 'Latest Competitors Price Data',
                    message: 'ETA: less than 1 minute'
                },
                help: '#divLatestCompHelp',
                mostRecent: '#divMostRecentCompPriceDataInfo'
            }
        };

        var fileTypes = {
            'txt': {image: '/images/txtfileicon.png', title: 'Text file'},
            'csv': {image: '/images/excelcsvicon.png', title: 'CSV file'},
            'xls': {image: '/images/excelicon.png', title: 'Excel file'},
            'xlsx': {image: '/images/excelicon.png', title: 'Excel file'},
        };

        var selectors = {
            active: 'active',
            fileButton: '#FileButton',
            uploadTypeName: '#UploadTypeName',
            inputTypeFile: 'input[type=file]',
            chosenDate: '#caldattimepicker',
            chooseDatePanel: '#ChooseDatePanel',
            uploadButton: '#UploadFileButton',
            calculatedAsDate: '#calcultedasdate',
            filenameAndIcon: '#divfilenameplusicon',
            uploadButtonPanel: '#divuploadbtn',
            downloadButton: '#divdownloadbtn',
            fileUploadingDialog: '#FileUploadingDialog',
            step1Panel: '#Step1Panel',
            step2Panel: '#Step2Panel',
            step3Panel: '#Step3Panel',
            step2Label: '#Step2Label',
            errorPanel: '#diverror',
            fileUploadTitle: '#FileUploadTitle',
            selectFilePrompt: '#SelectFilePrompt',
            calculateDateInfo: '#CalculateDateInfo',
            calculateDateInfo2: '#CalculateDateInfo2',

            downloadQuarterlyTemplateButton: '#DownloadQuarterlyTemplateButton',
            downloadLatestPriceTemplateButton: '#DownloadLatestPriceTemplateButton',
            downloadLatestCompPriceTemplateButton: '#DownloadLatestCompPriceTemplateButton'
        };

        function bindEvents() {
            $(selectors.uploadTypeName).off().on('change', uploadTypeNameChange);
            $(selectors.inputTypeFile).off().on('click', inputTypeFileClick).on('change', inputTypeFileChange);

            // TODO: merge handler!
            $(selectors.uploadTypeName).on('change', uploadTypeNameChange2);

            $(selectors.chosenDate).off().on('change', calDateTimeChange);
            $(selectors.uploadButton).off().on('click', uploadfileButtonClick);

            $(selectors.downloadLatestCompPriceTemplateButton).off().on('click', downloadLatestCompPriceTemplate);
            $(selectors.downloadLatestPriceTemplateButton).off().on('click', downloadLatestPriceTemplate);
            $(selectors.downloadQuarterlyTemplateButton).off().on('click', downloadQuarterlyTemplateButton);

            $(selectors.showDatePickerCheckbox).off().on('change click', redrawStepLabels);
        };

        function downloadLatestCompPriceTemplate() {
            busyloader.show({
                message: 'Downloading Latest Competitor Price Template...',
                showtime: 2000
            });
        };

        function downloadLatestPriceTemplate() {
            busyloader.show({
                message: 'Downloading Latest Price Template...',
                showtime: 2000
            });
        };

        function downloadQuarterlyTemplateButton() {
            busyloader.show({
                message: 'Downloading Quarterly Template...',
                showtime: 2000
            });
        };

        function uploadTypeNameChange() {
            var uploadtype = $(selectors.uploadTypeName).val();
            redrawTemplateButtons(uploadtype);
            redrawHelp();
            showSteps(2);
        };

        function redrawHelp() {
            var uploadtype = $(selectors.uploadTypeName).val(),
                key,
                def;

            for (key in uploadTypeDefs) {
                def = uploadTypeDefs[key];
                showOrHide($(def.help), uploadtype == key);
                showOrHide($(def.mostRecent), uploadtype == key);
            }
        };

        function showOrHide(ele, show) {
            if (show)
                ele.show();
            else
                ele.hide();
        };

        function redrawTemplateButtons(uploadType) {
            for (var key in uploadTypeDefs) {
                var defin = uploadTypeDefs[key],
                    template = $('#' + defin.templateId);
                if (key == uploadType) {
                    $("#file").attr({ "accept": defin.accept });
                    $(selectors.selectFilePrompt).text(defin.prompt);
                    template.show();
                    $(selectors.fileUploadTitle).text(' - ' + defin.message);
                } else {
                    template.hide();
                }
            };
        };

        function inputTypeFileClick() {
            this.value = null;
        };

        function extractDateFromFilename(filename) {
            if (/^2\d{7}[\._]/.test(filename))
                return filename.substring(6, 8) + '/' + filename.substring(4, 6) + '/' + filename.substring(0, 4);
            else
                return null;
        };

        function extractFileTypeFromFilename(filename) {
            var key;
            for (key in uploadTypeDefs) {
                if (uploadTypeDefs[key].filenameRegex.test(filename))
                    return key;
            }
            return null;
        };

        function inputTypeFileChange() {
            showSteps(3);
            var uploadtype = $('#UploadTypeName').val(),
                filename = this.files[0].name.toLowerCase(),
                ext = '.' + this.files[0].name.split(".").pop().toLowerCase(),
                typeDef = uploadTypeDefs[uploadtype],
                isValid = typeDef && typeDef.accept.split(/\s*,\s*/).indexOf(ext) != -1,
                fileType = fileTypes[ext.replace('.', '')],
                filenameDate = extractDateFromFilename(filename),
                filenameType = extractFileTypeFromFilename(filename),
                datePicker = $(selectors.chosenDate);

            if (isValid) {
                $(selectors.errorPanel).hide();
                $(selectors.filenameAndIcon).show();
                $(selectors.uploadButton).show();
                $("#fileimage").attr("src", common.reportRootFolder() + fileType.image).attr('title', fileType.title);

                if (filenameType) {
                    $('#UploadTypeName').val(filenameType);
                    redrawStepLabels();
                }

                if (filenameDate && options.datePicker) {
                    if (!$('#chkShowDatePicker').is(':checked'))
                        $('#chkShowDatePicker').trigger('click');
                    datePicker.val(filenameDate);
                    datePicker.trigger('change');
                    notify.info('Date changed ' + filenameDate + ' to match upload filename');
                }
            } else {
                $(selectors.errorPanel).show().text("Please Select valid file...").delay(3000).fadeOut();
                $(selectors.filenameAndIcon).hide();
                $(selectors.uploadButton).hide();
                this.value = null;
            }

            $('#filename').text(this.files[0].name);
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
                    endDate: '1d'
                });
        };

        function formatDDMMYYYY(year, month, day) {
            return (day < 10 ? '0' + day : day) + '/' + (month < 10 ? '0' + month : month) + '/' + year;
        };

        function uploadTypeNameChange2() {
            var value = $(this).val();
            if (value != 1) {
                var d = new Date();
                var month = d.getMonth() + 1;
                var day = d.getDate();

                var output = formatDDMMYYYY(d.getFullYear(), month, day);
                drawCalculatedAsDate(output);
            }
            redrawStepLabels();
        };

        function redrawStepLabels() {
            var value = $(selectors.uploadTypeName).val();
            if (options.datePicker || value == 1) {
                $(selectors.chooseDatePanel).show();
                $(selectors.step2Label).text('Date & File:');
            } else {
                $(selectors.chooseDatePanel).hide();
                $(selectors.step2Label).text('File:');
            }
        };

        function calDateTimeChange() {
            var value = $(selectors.chosenDate).val()
            drawCalculatedAsDate(value);
            if (value != lastPriceFreezeEventDate) {
                lastPriceFreezeEventDate = value;
                reloadPriceFreezeWarning(value);
            }
        };

        function formatDaysRemaining(dateTo) {
            var days = dateUtils.dayDiff(new Date(), dateTo);
            return days == 1 ? '1 Day' : days + ' Days';
        };

        function redrawPriceFreezeEvent(event) {
            var panel = $('#divPriceFreezeWarning');

            if (event && event.PriceFreezeEventId > 0) {
                panel.find('[data-value="DateFrom"]').text(common.formatDateDDMMYYY(event.DateFrom));
                panel.find('[data-value="DateTo"]').text(common.formatDateDDMMYYY(event.DateTo));
                panel.find('[data-value="DaysRemaining"]').text(formatDaysRemaining(event.DateTo));
                panel.find('[data-value="CreatedBy"]').text(event.CreatedBy.split('@')[0].replace('.', ' '));
                panel.show();
            } else {
                panel.hide();
            }
        };

        function reloadPriceFreezeWarning(forDate) {
            function failure() {
                notify.error('Unable to load Price Freeeze Event for ' + forDate);
            };

            function success(data) {
                data.DateFrom = common.convertJsonDate(data.DateFrom);
                data.DateTo = common.convertJsonDate(data.DateTo);
                redrawPriceFreezeEvent(data);
            };

            priceFreezeEventService.getPriceFreezeEventForDate(success, failure, forDate);
        };

        function drawCalculatedAsDate(ddmmyyyy) {
            var today = new Date(),
                parts = ddmmyyyy.split('/'),
                date = new Date(parseInt(parts[2], 10), parseInt(parts[1], 10)-1, parseInt(parts[0], 10)),
                timeDiff = date.getTime() - today.getTime(),
                diffDays = Math.ceil(timeDiff / (1000 * 3600 * 24)),
                friendly1 = '',
                friendly2 = ''

            $(selectors.calculatedAsDate).text(ddmmyyyy);

            switch (diffDays) {
                case 0:
                    friendly1 = '<span class="badge font125pc">Today</span>';
                    friendly2 = '<span class="date-indicator today">Today</span>';
                    break;
                case -1:
                    friendly1 = '<span class="badge font125pc">Yesterday</span>';
                    friendly2 = '<span class="date-indicator yesterday">Yesterday</span>';
                    break;
                case 1:
                    friendly1 = '<span class="badge font125pc">Tomorrow</span>';
                    friendly2 = '<span class="date-indicator tomorrow">Tomorrow</span>';
                    break;
                default:
                    if (diffDays < 0) {
                        friendly1 = '<span class="badge font125pc">' + Math.abs(diffDays) + ' Days Ago</span>';
                        friendly2 = '<span class="date-indicator past">' + Math.abs(diffDays) + ' Days Ago</span>';
                    }
                    else {
                        friendly1 = '<span class="badge font125pc">' + diffDays + ' Days</span>';
                        friendly2 = '<span class="date-indicator future">' + diffDays + ' Days</span>';
                    }
                    break;
            }
            $(selectors.calculateDateInfo).html(friendly1);
            $(selectors.calculateDateInfo2).html(friendly2);
        };

        function uploadfileButtonClick() {
            var uploadtype = $('#UploadTypeName').val(),
                uploadDef = uploadTypeDefs[uploadtype],
                fileButton = $(selectors.uploadButton),
                delayed = function () {
                    $(fileButton).attr('disabled', true);
                };

            fileButton.removeClass('btn-primary').addClass('btn-danger');

            waiter.show({
                title: uploadDef.submit.title,
                message: uploadDef.submit.message,
                icon: 'upload'
            });

            setTimeout(delayed, 100);
        };

        function showSteps(currentStep) {
            var fileButton = $(selectors.fileButton),
                fileIcon = fileButton.find('.fa');

            setStepClass(1, currentStep, selectors.step1Panel);
            setStepClass(2, currentStep, selectors.step2Panel);
            setStepClass(3, currentStep, selectors.step3Panel);

            if (currentStep == 2) {
                $(selectors.selectFilePrompt).show();
                fileButton.removeClass('btn-success').addClass('btn-danger');
                fileIcon.removeClass('fa-check').addClass('fa-times');
            } else {
                $(selectors.selectFilePrompt).hide();
                fileButton.removeClass('btn-danger').addClass('btn-success');
                fileIcon.removeClass('fa-times').addClass('fa-check');
            }
        };

        function setStepClass(index, currentStep, selector) {
            var current = index == currentStep,
                visible = currentStep >= index;

            $(selector)[visible ? 'show' : 'hide']()[current ? 'addClass' : 'removeClass'](selectors.active);
        };

        function removeHideClass() {
            $('.hide').hide().removeClass('hide');
        };

        function docReady() {
            var uploadTypeMenu = $(selectors.uploadTypeName),
                selectedUploadType = $('#hdnSelectedFileUploadType').val() || '',
                forDate = $(selectors.chosenDate).val();
            removeHideClass();
            showSteps(2);
            bindEvents();
            initDatePickers();
            redrawTemplateButtons(1);
            redrawStepLabels();
            redrawHelp();
            drawCalculatedAsDate($(selectors.calculatedAsDate).text());
            if (selectedUploadType != '') {
                uploadTypeMenu.val(selectedUploadType);
                uploadTypeMenu.trigger('change');
            }
            lastPriceFreezeEventDate = forDate;
            reloadPriceFreezeWarning(forDate);
        };

        function init(opts) {
            options = $.extend(options, opts);
            $(docReady);
        };

        // API
        return {
            init: init
        };
    }
);