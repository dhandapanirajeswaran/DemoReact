require(["jquery", "common", "busyloader", "bootstrap-datepicker", "notify", "infotips", "downloader"],
    function ($, common, busyloader, bsdatepicker, notify, infotips, downloader) {

        "use strict";

        $("document").ready(function () {

            var rootFolder = common.reportRootFolder();

            var highlights = {
                complies: true,
                negative: true,
                positive: true,
                na: true
            };

            var counts = {
                complies: 0,
                negative: 0,
                positive: 0,
                na: 0
            };

            var selectors = {
                resetHighlightButton: '#btnResetHighlighting',
                toggleHighlightComplies: '#btnToggleHighlightComplies',
                toggleHighlightNegative: '#btnToggleHighlightNegative',
                toggleHighlightPositive: '#btnToggleHighlightPositive',
                toggleHighlightNA: '#btnToggleHighlightNA',
                reportGrid: '#ReportGrid',
                diffComplies: '.diff-complies',
                diffNegative: '.diff-negative',
                diffPositive: '.diff-positive',
                diffNA: '.diff-na',
                exportButton: '#btnExportReport'
            };

            var toggleMap = {
                'btnToggleHighlightComplies': {
                    message: 'Highlighting sites which comply',
                    name: 'complies'
                },
                'btnToggleHighlightNegative': {
                    message: 'Highlighting sites which have a Negative difference',
                    name: 'negative'
                },
                'btnToggleHighlightPositive': {
                    message: 'Highlighting sites which have a Positive difference',
                    name: 'positive'
                },
                'btnToggleHighlightNA': {
                    message: 'Highlighting sites which have a N/A difference',
                    name: 'na'
                }
            };


            var forDp = $('.datepicker').datepicker({
                language: "en-GB",
                autoClose: true,
                format: 'd-M-yyyy',
                todayBtn: "linked",
                todayHighlight: true,
                orientation: 'auto top',
                endDate: '1d'
            });

            $("#btnViewReport").click(function () {
                var dt = forDp.val();
                busyloader.showViewingReport();
                window.location.href = rootFolder + '/PriceReports/Compliance?For=' + dt;
            });

            $('#btnResetReport').click(function () {
                window.location.href = rootFolder + '/PriceReports/Compliance';
            });


            function redrawCounts() {
                var grid = $(selectors.reportGrid);
                counts.complies = grid.find(selectors.diffComplies).length;
                counts.negative = grid.find(selectors.diffNegative).length;
                counts.positive = grid.find(selectors.diffPositive).length;
                counts.na = grid.find(selectors.diffNA).length;

                $(selectors.toggleHighlightComplies).find('.badge').text(counts.complies);
                $(selectors.toggleHighlightNegative).find('.badge').text(counts.negative);
                $(selectors.toggleHighlightPositive).find('.badge').text(counts.positive);
                $(selectors.toggleHighlightNA).find('.badge').text(counts.na);
            };

            function setHighlightButton(gridCss, selector, isActive) {
                var button = $(selector),
                    grid = $(selectors.reportGrid);
                if (isActive) {
                    button.removeClass('btn-default').addClass('btn-primary');
                    grid.addClass(gridCss);
                }
                else {
                    button.removeClass('btn-primary').addClass('btn-default');
                    grid.removeClass(gridCss);
                }
            };

            function redrawHighlightButtons() {
                var resetButton = $(selectors.resetHighlightButton);
                setHighlightButton('highlight-complies', selectors.toggleHighlightComplies, highlights.complies);
                setHighlightButton('highlight-negative', selectors.toggleHighlightNegative, highlights.negative);
                setHighlightButton('highlight-positive', selectors.toggleHighlightPositive, highlights.positive);
                setHighlightButton('highlight-na', selectors.toggleHighlightNA, highlights.na);

                if (highlights.complies && highlights.negative && highlights.positive && highlights.na)
                    resetButton.hide();
                else
                    resetButton.show();
            };

            function commonToggle() {
                var button = $(this),
                    id = button.attr('id'),
                    toggler = toggleMap[id],
                    newState;

                if (!toggler) return;

                 newState = !highlights[toggler.name];
                 highlights[toggler.name] = newState;
                 redrawHighlightButtons();
            };

            function resetHighlighting() {
                highlights.complies = true;
                highlights.negative = true;
                highlights.positive = true;
                highlights.na = true;
                redrawHighlightButtons();
            };

            function exportClick() {
                var dt = forDp.val(),
                    downloadId = downloader.generateId();

                busyloader.showExportToExcel(1000);

                downloader.start({
                    id: downloadId,
                    element: selectors.exportButton,
                    complete: function (download) {
                        notify.success('Export complete - took ' + download.friendlyTimeTaken);
                    }
                });

                window.location.href = rootFolder + '/PriceReports/ExportCompliance?downloadId=' + downloadId + '&For=' + dt;
            };

            function bindEvents() {
                $(selectors.resetHighlightButton).off().on('click', resetHighlighting);
                $(selectors.toggleHighlightComplies).off().on('click', commonToggle);
                $(selectors.toggleHighlightNegative).off().on('click', commonToggle);
                $(selectors.toggleHighlightPositive).off().on('click', commonToggle);
                $(selectors.toggleHighlightNA).off().on('click', commonToggle);

                $(selectors.exportButton).off().on('click', exportClick);
            };

            function init() {
                bindEvents();
                redrawCounts();
                redrawHighlightButtons();
            };

            init();
        });
    }
);

