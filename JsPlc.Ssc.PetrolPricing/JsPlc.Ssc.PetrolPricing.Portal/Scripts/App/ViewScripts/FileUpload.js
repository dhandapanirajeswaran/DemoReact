define(['jquery', 'common', 'notify', 'busyloader', 'bootstrap-datepicker'],
    function ($, common, notify, busyloader, bsDatePicker) {
        "use strict";

        var uploadTypeDefs = {
            '1': { message: 'Daily Price Data', accept: ".csv, .txt", prompt: 'Please select a CSV or TXT file', templateId: null},
            '2': { message: 'Quarterly Site Data', accept: ".xls, .xlsx", prompt: 'Please select an Excel file', templateId: 'DownloadQuarterlyTemplateButton'},
            '3': { message: 'Latest JS Price Data', accept: ".xls, .xlsx", prompt: 'Please select an Excel file', templateId: 'DownloadLatestPriceTemplateButton'},
            '4': { message: 'Latest Competitors Price Data', accept: ".xls, .xlsx", prompt: 'Please select an Excel file', templateId: 'DownloadLatestCompPriceTemplateButton'}
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
            downloadLatestCompPriceTemplateButton:'#DownloadLatestCompPriceTemplateButton'
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
            showSteps(2);
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

        function inputTypeFileChange() {
            showSteps(3);
            var uploadtype = $('#UploadTypeName').val(),
                ext = '.' + this.files[0].name.split(".").pop().toLowerCase(),
                typeDef = uploadTypeDefs[uploadtype],
                isValid = typeDef && typeDef.accept.split(/\s*,\s*/).indexOf(ext) != -1,
                fileType = fileTypes[ext.replace('.', '')];
                
            if (isValid) {
                $(selectors.errorPanel).hide();
                $(selectors.filenameAndIcon).show();
                $(selectors.uploadButton).show();
                $("#fileimage").attr("src", common.reportRootFolder() + fileType.image).attr('title', fileType.title);
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

        function padZero2Digit(n) {
            return n < 10 ? '0' + n : n;
        };

        function formatDDMMYYYY(year, month, day) {
            return padZero2Digit(day) + '/' + padZero2Digit(month) + '/' + year;
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
            if (value == 1) {
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
        };

        function drawCalculatedAsDate(ddmmyyyy) {
            var today = new Date(),
                parts = ddmmyyyy.split('/'),
                date = new Date(parseInt(parts[2], 10), parseInt(parts[1], 10)-1, parseInt(parts[0], 10)),
                timeDiff = date.getTime() - today.getTime(),
                diffDays = Math.ceil(timeDiff / (1000 * 3600 * 24)),
                friendly = '';

            $(selectors.calculatedAsDate).text(ddmmyyyy);

            switch (diffDays) {
                case 0:
                    friendly = '<span class="today-date">Today</span>';
                    break;
                case -1:
                    friendly = '<span class="yesterday-date">Yesterday</span>';
                    break;
                case 1:
                    friendly = '<span class="tomorrow-date">Tomorrow</span>';
                    break;
                default:
                    if (diffDays < 0)
                        friendly = '<span class="past-days">' + diffDays + ' Days Ago</span>';
                    else
                        friendly = '<span class="future-days">' + diffDays + ' Days</span>';
                    break;
            }
            $(selectors.calculateDateInfo).html(friendly);
            $(selectors.calculateDateInfo2).html(friendly);
        };

        function uploadfileButtonClick() {

            var delayed = function () {
                $(selectors.uploadButton).attr('disabled', true);
            }

            $(selectors.uploadButton).removeClass('btn-primary').addClass('btn-danger');
            $(selectors.fileUploadingDialog).show();

            setTimeout(delayed, 100);
        };

        function showSteps(currentStep) {
            setStepClass(1, currentStep, selectors.step1Panel);
            setStepClass(2, currentStep, selectors.step2Panel);
            setStepClass(3, currentStep, selectors.step3Panel);

            if (currentStep == 2) {
                $(selectors.selectFilePrompt).show();
                $(selectors.fileButton).removeClass('btn-success').addClass('btn-danger');

            } else {
                $(selectors.selectFilePrompt).hide();
                $(selectors.fileButton).removeClass('btn-danger').addClass('btn-success');
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
                selectedUploadType = $('#hdnSelectedFileUploadType').val() || '';
            removeHideClass();
            showSteps(2);
            bindEvents();
            initDatePickers();
            redrawTemplateButtons(1);
            redrawStepLabels();
            drawCalculatedAsDate($(selectors.calculatedAsDate).text());
            if (selectedUploadType != '') {
                uploadTypeMenu.val(selectedUploadType);
                uploadTypeMenu.trigger('change');
            }
        };

        function init() {
            $(docReady);
        };

        // API
        return {
            init: init
        };
    }
);