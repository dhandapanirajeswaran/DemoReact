define(["SitePricing", "notify", "infotips", "cookieSettings"],
    function (prices, notify, infotip, cookieSettings) {
        "use strict";

        var counts = {
            activeSites: $('#hdnActiveSitesCount').val() || 0,
            inactiveSites: $('#hdnInactiveSitesCount').val() || 0,
            trialSites: $('#hdnTrialSitesCount').val() || 0,
            matchCompetitorSites: $('#hdnMatchCompetitorSitesCount').val() || 0,
            standardSites: $('#hdnStandardPriceSitesCount').val() || 0,
            hasEmails: $('#hdnHasEmailsSiteCount').val() || 0,
            hasNoEmails: $('#hdnHasNoEmailsSiteCount').val() || 0,
            withDataSites: $('#hdnHasDataSitesCount').val() || 0,
            missingDataSites: $('#hdnMissingDataSitesCount').val() || 0
        };

        var siteFilters = {
            showHasEmails: true,
            showNoEmails: true,
            showActiveSites: true,
            showInactiveSites: true,
            highlightTrialPrices: true,
            highlightMatchCompetitors: true,
            highlightStandardPrices: true,
            withData: true,
            missingData: true
        };

        var settingsMap = {
            showHasEmails: 'sites.showHasEmails',
            showNoEmails: 'sites.showNoEmails',
            showActiveSites: 'sites.showActiveSites',
            showInactiveSites: 'sites.showInactiveSites',
            highlightTrialPrices: 'sites.highlightTrialPrices',
            highlightMatchCompetitors: 'sites.highlightMatchCompetitors',
            highlightStandardPrices: 'sites.highlightStandardPrices'
        };

        function applyRowFilters() {
            var rows = $('#SiteListTable>tbody>tr'),
                noresults = $('#divNoResultsAlert');

            rows.each(function (index, item) {
                var row = $(item),
                    visible = (
                            (
                                (row.hasClass('site-active') && siteFilters.showActiveSites)
                                || (row.hasClass('site-inactive') && siteFilters.showInactiveSites)
                            )
                            && (
                                (row.hasClass('has-emails') && siteFilters.showHasEmails)
                                || (row.hasClass('no-email') && siteFilters.showNoEmails)
                            )
                            && (
                                (row.hasClass('has-missing-data') && siteFilters.missingData)
                                || (row.hasClass('has-complete-data') && siteFilters.withData)
                            )
                        );

                visible ? row.show() : row.hide();
            });

            if (rows.find(':visible').length == 0)
                noresults.show();
            else
                noresults.hide();
        };

        function setButtonClassState(button, isOn) {
            if (isOn)
                button.removeClass('btn-default').addClass('btn-primary');
            else
                button.removeClass('btn-primary').addClass('btn-default');
        }

        function redrawHighlightingButtons() {
            var toggleTrialPriceButton = $('#btnToggleHighlightTrialPriceSites'),
                toggleMatchCompetitorsButton = $('#btnToggleHighlightMatchCompetitors'),
                toggleStandardPriceButton = $('#btnToggleHighlightStandardPrices'),
                isResetVisible = siteFilters.highlightMatchCompetitors || siteFilters.highlightStandardPrices || siteFilters.highlightTrialPrices,
                resetButton = $('#btnResetHighlighting');

            setButtonClassState(toggleTrialPriceButton, siteFilters.highlightTrialPrices);
            setButtonClassState(toggleMatchCompetitorsButton, siteFilters.highlightMatchCompetitors);
            setButtonClassState(toggleStandardPriceButton, siteFilters.highlightStandardPrices);
            applyRowFilters();

            resetButton[isResetVisible ? 'show' : 'hide']();
        };

        function commonToggleButtonFilter(opts) {
            var newState = !siteFilters[opts.state],
                message = newState ? opts.showing : opts.hiding;

            siteFilters[opts.state] = newState;
            redrawHighlightingButtons();
            redrawSiteFilterButtons();
            applyRowFilters();
            notify.info(message);

            updateCookieSettings();
        };

        function restoreCookieSettings() {
            cookieSettings.restore(settingsMap, siteFilters);

            redrawHighlightingButtons();
            redrawSiteFilterButtons();
            redrawHighlighting();
            applyRowFilters();
        };

        function updateCookieSettings() {
            cookieSettings.update(settingsMap, siteFilters);
        };

        function redrawHighlighting() {
            var table = $('#SiteListTable');

            siteFilters.highlightMatchCompetitors
                ? table.addClass('highlight-match-competitors')
                : table.removeClass('highlight-match-competitors');

            siteFilters.highlightTrialPrices
                ? table.addClass('highlight-trial-prices')
                : table.removeClass('highlight-trial-prices');

            siteFilters.highlightStandardPrices
                ? table.addClass('highlight-standard-prices')
                : table.removeClass('highlight-standard-prices');
        };


        function commonToggleHighlights(opts) {
            var newState = !siteFilters[opts.state];

            siteFilters[opts.state] = newState;

            redrawHighlightingButtons();
            redrawHighlighting();

            notify.info(newState ? opts.showing : opts.hiding);

            updateCookieSettings();
        };

        function toggleHighlightShowTrialPrices() {
            var opts = {
                state: 'highlightTrialPrices',
                showing: 'Highlighting ' + counts.trialSites + ' Trial Price Sites',
                hiding: ''
            };
            commonToggleHighlights(opts);
        };

        function toggleHighlightShowMatchCompetitors() {
            var opts = {
                state: 'highlightMatchCompetitors',
                showing: 'Highlighting ' + counts.matchCompetitorSites + ' Match Competitor Sites',
                hiding: ''
            };
            commonToggleHighlights(opts);
        };

        function toggleHighlightStandardPrices() {
            var enabled = !siteFilters.highlightStandardPrices,
                opts = {
                    state: 'highlightStandardPrices',
                    showing: 'Highlight ' + counts.standardSites + ' Standard Price Sites',
                    hiding: ''
                };
            commonToggleHighlights(opts);
        };

        function resetHighlighting() {
            siteFilters.highlightMatchCompetitors = false;
            siteFilters.highlightStandardPrices = false;
            siteFilters.highlightTrialPrices = false;
            redrawHighlightingButtons();
            redrawHighlighting();
            notify.info('Removed all Price highlighting');
            updateCookieSettings();
        };

        function redrawSiteFilterButtons() {
            var activeButton = $('#btnShowActiveSites'),
                inactiveButton = $('#btnShowInActiveSites'),
                withEmailsButton = $('#btnShowWithEmails'),
                withNoEmailsButton = $('#btnShowNoEmails'),
                resetButton = $('#btnResetActiveSites'),
                activeRows = $('tr.site-active'),
                inactiveRows = $('tr.site-inactive'),
                withDataButton = $('#btnShowWithData'),
                missingDataButton = $('#btnShowMissingData'),
                dataChooseGroup = $('[data-choose-group="with-data-group"]'),
                emailChooseGroup = $('[data-choose-group="with-email-group"]'),
                activeChooseGroup = $('[data-choose-group="active-site-group"]');

            if (siteFilters.showActiveSites && siteFilters.showInactiveSites && siteFilters.showHasEmails && siteFilters.showNoEmails && siteFilters.withData && siteFilters.missingData)
                resetButton.hide();
            else
                resetButton.show();

            setButtonClassState(activeButton, siteFilters.showActiveSites);
            setButtonClassState(inactiveButton, siteFilters.showInactiveSites);
            setButtonClassState(withEmailsButton, siteFilters.showHasEmails);
            setButtonClassState(withNoEmailsButton, siteFilters.showNoEmails);
            setButtonClassState(withDataButton, siteFilters.withData);
            setButtonClassState(missingDataButton, siteFilters.missingData);

            setChooseGroup(dataChooseGroup, !siteFilters.withData && !siteFilters.missingData);
            setChooseGroup(emailChooseGroup, !siteFilters.showHasEmails && !siteFilters.showNoEmails);
            setChooseGroup(activeChooseGroup, !siteFilters.showActiveSites && !siteFilters.showInactiveSites);

            applyRowFilters();
        };

        function setChooseGroup(groupEle, show ) {
            if (show) 
                groupEle.removeClass('pick-one-hide').addClass('pick-one-show');
             else
                groupEle.removeClass('pick-one-show').addClass('pick-one-hide');
        };

        function toggleShowActiveSites() {
            var opts = {
                state: 'showActiveSites',
                showing: 'Showing ' + counts.activeSites + ' Active Sites',
                hiding: 'Hiding ' + counts.activeSites + ' Active Sites'
            };
            commonToggleButtonFilter(opts);
        };

        function toggleShowInActiveSites() {
            var opts = {
                state: 'showInactiveSites',
                showing: 'Showing ' + counts.inactiveSites + ' Inactive Sites',
                hiding: 'Hiding ' + counts.inactiveSites + ' Inactive Sites'
            };
            commonToggleButtonFilter(opts);
        };

        function toggleShowWithEmails() {
            var opts = {
                state: 'showHasEmails',
                showing: 'Showing ' + counts.hasEmails + ' Sites with Emails',
                hiding: 'Hiding ' + counts.hasEmails + ' Sites with Emails'
            };
            commonToggleButtonFilter(opts);
        };

        function toggleShowWithNoEmails() {
            var opts = {
                state: 'showNoEmails',
                showing: 'Showing ' + counts.hasNoEmails + ' Sites with NO emails',
                hiding: 'Hiding ' + counts.hasNoEmails + ' Sites with NO emails'
            };
            commonToggleButtonFilter(opts);
        };

        function toggleWithData() {
            var opts = {
                state: 'withData',
                showing: 'Showing ' + counts.withDataSites + ' Sites with Data',
                hiding: 'Hiding ' + counts.withDataSites + ' Sites with Data'
            };
            commonToggleButtonFilter(opts);
        };

        function toggleMissingData() {
            var opts = {
                state: 'missingData',
                showing: 'Showing ' + counts.missingDataSites + ' Sites with Missing Data',
                hiding: 'Hiding ' + counts.missingDataSites + ' Sites with Missing Data'
            };
            commonToggleButtonFilter(opts);
        };

        function resetActiveSites() {
            siteFilters.missingData = true;
            siteFilters.withData = true;
            siteFilters.showHasEmails = true;
            siteFilters.showNoEmails = true;
            siteFilters.showActiveSites = true;
            siteFilters.showInactiveSites = true;
            redrawSiteFilterButtons();
            notify.info('Showing all Sites - all filters removed');
            updateCookieSettings();
        };

        function clickGoOnEnter(ev) {
            if (ev.keyCode == 13)
                $("#btnGO").click();
        };

        function resetActiveFilters() {
            siteFilters.showActiveSites = true;
            siteFilters.showInactiveSites = true;
            siteFilters.showHasEmails = true;
            siteFilters.showNoEmails = true;
            siteFilters.withData = true;
            siteFilters.missingData = true;
            redrawSiteFilterButtons();
            applyRowFilters();
            notify.info('Reset the Active/Inactive filters');
            updateCookieSettings();
        };

        function resetSearchCriteria() {
            $('#viewingCatNo').val('');
            $('#viewingStoreNo').val('');
            $('#viewingStoreName').val('');
            $('#viewingStoreTown').val('');
            $('#btnGo').trigger('click');
        };

        function bindEvents() {
            $("#viewingStoreTown, #viewingStoreName, #viewingStoreNo, #viewingCatNo").keyup(clickGoOnEnter);

            $('#btnToggleHighlightTrialPriceSites').off().on('click', toggleHighlightShowTrialPrices);
            $('#btnToggleHighlightMatchCompetitors').off().on('click', toggleHighlightShowMatchCompetitors);
            $('#btnToggleHighlightStandardPrices').off().on('click', toggleHighlightStandardPrices);
            $('#btnResetHighlighting').off().on('click', resetHighlighting);
            $('#btnShowWithData').off().click(toggleWithData);
            $('#btnShowMissingData').off().click(toggleMissingData);

            $('#btnShowWithEmails').off().on('click', toggleShowWithEmails);
            $('#btnShowNoEmails').off().on('click', toggleShowWithNoEmails);
            $('#btnShowActiveSites').off().on('click', toggleShowActiveSites);
            $('#btnShowInActiveSites').off().on('click', toggleShowInActiveSites);
            $('#btnResetActiveSites').off().on('click', resetActiveSites);

            $('#btnResetActiveFilters').off().on('click', resetActiveFilters);

            $('#btnResetSearchCriteria').off().on('click', resetSearchCriteria);
        };

        function docReady() {
            prices.go();
            var waiters = $('.wait-for-js').removeClass('wait-for-js');
            bindEvents();
            restoreCookieSettings();
            redrawSiteFilterButtons();
            redrawHighlightingButtons();
            $('.loading-js').hide();
        };

        $(docReady);

        // API
        return {

        };
    });
