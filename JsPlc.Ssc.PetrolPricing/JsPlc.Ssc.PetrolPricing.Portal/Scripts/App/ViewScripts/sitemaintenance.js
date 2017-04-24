﻿define(["SitePricing", "notify", "infotips", "cookieSettings"],
    function (prices, notify, infotip, cookieSettings) {
        "use strict";

        var counts = {
            activeSites: $('#hdnActiveSitesCount').val() || 0,
            inactiveSites: $('#hdnInactiveSitesCount').val() || 0,
            trialSites: $('#hdnTrialSitesCount').val() || 0,
            matchCompetitorSites: $('#hdnMatchCompetitorSitesCount').val() || 0,
            soloSites: $('#hdnSoloPriceSitesCount').val() || 0
        };

        console.log(counts);

        var siteFilters = {
            showActiveSites: true,
            showInactiveSites: true,
            highlightTrialPrices: true,
            highlightMatchCompetitors: true,
            highlightSoloPrices: true
        };

        var settingsMap = {
            showActiveSites: 'sites.showActiveSites',
            showInactiveSites: 'sites.showInactiveSites',
            highlightTrialPrices: 'sites.highlightTrialPrices',
            highlightMatchCompetitors: 'sites.highlightMatchCompetitors',
            highlightSoloPrices: 'sites.highlightSoloPrices'
        };

        function applyRowFilters() {
            var rows = $('#SiteListTable>tbody>tr');

            rows.each(function (index, item) {
                var row = $(item),
                    visible = (
                            (row.hasClass('site-active') && siteFilters.showActiveSites)
                            || (row.hasClass('site-inactive') && siteFilters.showInactiveSites)
                        );

                visible ? row.show() : row.hide();
            });
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
                toggleSoloPriceButton = $('#btnToggleHighlightSoloPrices'),
                isResetVisible = siteFilters.highlightMatchCompetitors || siteFilters.highlightSoloPrices || siteFilters.highlightTrialPrices,
                resetButton = $('#btnResetHighlighting');

            setButtonClassState(toggleTrialPriceButton, siteFilters.highlightTrialPrices);
            setButtonClassState(toggleMatchCompetitorsButton, siteFilters.highlightMatchCompetitors);
            setButtonClassState(toggleSoloPriceButton, siteFilters.highlightSoloPrices);
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

            siteFilters.highlightSoloPrices
                ? table.addClass('highlight-solo-prices')
                : table.removeClass('highlight-solo-prices');
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

        function toggleHighlightSoloPrices() {
            var enabled = !siteFilters.highlightSoloPrices,
                opts = {
                    state: 'highlightSoloPrices',
                    showing: 'Highlight ' + counts.soloSites + ' Solo Price Sites',
                    hiding: ''
                };
            commonToggleHighlights(opts);
        };

        function resetHighlighting() {
            siteFilters.highlightMatchCompetitors = false;
            siteFilters.highlightSoloPrices = false;
            siteFilters.highlightTrialPrices = false;
            redrawHighlightingButtons();
            redrawHighlighting();
            notify.info('Removed all Price highlighting');
            updateCookieSettings();
        };

        function redrawSiteFilterButtons() {
            var activeButton = $('#btnShowActiveSites'),
                inactiveButton = $('#btnShowInActiveSites'),
                resetButton = $('#btnResetActiveSites'),
                activeRows = $('tr.site-active'),
                inactiveRows = $('tr.site-inactive');

            if (siteFilters.showActiveSites && siteFilters.showInactiveSites)
                resetButton.hide();
            else
                resetButton.show();

            setButtonClassState(activeButton, siteFilters.showActiveSites);
            setButtonClassState(inactiveButton, siteFilters.showInactiveSites);
            applyRowFilters();
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

        function resetActiveSites() {
            siteFilters.showActiveSites = true;
            siteFilters.showInactiveSites = true;
            redrawSiteFilterButtons();
            notify.info('Showing both Active and Inactive Sites');
        };

        function clickGoOnEnter(ev) {
            if (ev.keyCode == 13)
                $("#btnGO").click();
        };

        function bindEvents() {
            $("#viewingStoreTown, #viewingStoreName, #viewingStoreNo, #viewingCatNo").keyup(clickGoOnEnter);

            $('#btnToggleHighlightTrialPriceSites').off().on('click', toggleHighlightShowTrialPrices);
            $('#btnToggleHighlightMatchCompetitors').off().on('click', toggleHighlightShowMatchCompetitors);
            $('#btnToggleHighlightSoloPrices').off().on('click', toggleHighlightSoloPrices);
            $('#btnResetHighlighting').off().on('click', resetHighlighting);

            $('#btnShowActiveSites').off().on('click', toggleShowActiveSites);
            $('#btnShowInActiveSites').off().on('click', toggleShowInActiveSites);
            $('#btnResetActiveSites').off().on('click', resetActiveSites);
        };

        function docReady() {
            prices.go();
            bindEvents();
            restoreCookieSettings();
            redrawSiteFilterButtons();
            redrawHighlightingButtons();
        };

        $(docReady);

        // API
        return {

        };
    });
