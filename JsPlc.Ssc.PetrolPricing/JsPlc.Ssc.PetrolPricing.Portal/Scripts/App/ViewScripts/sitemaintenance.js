define(["SitePricing", "notify", "infotips", "cookieSettings"],
    function (prices, notify, infotip, cookieSettings) {
        "use strict";

        var counts = {
            activeSites: '@(activeSiteCount)',
            inactiveSites: '@(inactiveSiteCount)',
            trialSites: '@(trialPriceSiteCount)',
            matchCompetitorSites: '@(matchCompetitorSiteCount)',
            nonePriceSite: '@(siloSiteCount)'
        };

        var siteFilters = {
            showActiveSites: true,
            showInactiveSites: true,
            highlightTrialPrices: true,
            highlightMatchCompetitors: true,
            highlightNonePrices: true
        };

        var settingsMap = {
            showActiveSites: 'sites.showActiveSites',
            showInactiveSites: 'sites.showInactiveSites',
            highlightTrialPrices: 'sites.highlightTrialPrices',
            highlightMatchCompetitors: 'sites.highlightMatchCompetitors',
            highlightNonePrices: 'sites.highlightNonePrices'
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

        function redrawPriceFilterButtons() {
            var toggleTrialPriceButton = $('#btnToggleHighlightTrialPriceSites'),
                toggleMatchCompetitorsButton = $('#btnToggleHighlightMatchCompetitors'),
                toggleNonePriceButton = $('#btnToggleHighlightNonePrices');

            setButtonClassState(toggleTrialPriceButton, siteFilters.highlightTrialPrices);
            setButtonClassState(toggleMatchCompetitorsButton, siteFilters.highlightMatchCompetitors);
            setButtonClassState(toggleNonePriceButton, siteFilters.highlightNonePrices);
            applyRowFilters();
        };

        function commonToggleButtonFilter(opts) {
            var newState = !siteFilters[opts.state],
                message = newState ? opts.showing : opts.hiding;

            siteFilters[opts.state] = newState;
            redrawPriceFilterButtons();
            redrawSiteFilterButtons();
            applyRowFilters();
            notify.info(message);

            updateCookieSettings();
        };

        function restoreCookieSettings() {
            cookieSettings.restore(settingsMap, siteFilters);

            redrawPriceFilterButtons();
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

            siteFilters.highlightNonePrices
                ? table.addClass('highlight-none-prices')
                : table.removeClass('highlight-none-prices');
        };


        function commonToggleHighlights(opts) {
            var newState = !siteFilters[opts.state];

            siteFilters[opts.state] = newState;

            redrawPriceFilterButtons();
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
                showing: 'Highlighting ' + counts.matchCompetitors + ' Match Competitor Sites',
                hiding: ''
            };
            commonToggleHighlights(opts);
        };

        function toggleHighlightNonePrices() {
            var enabled = !siteFilters.highlightNonePrices,
                opts = {
                    state: 'highlightNonePrices',
                    showing: 'Highlight ' + counts.nonePrices + ' Non Price-matched Sites',
                    hiding: ''
                };
            commonToggleHighlights(opts);
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
                showing: 'Showing ' + counts.active + ' Active Sites',
                hiding: 'Hiding ' + counts.active + ' Active Sites'
            };
            commonToggleButtonFilter(opts);
        };

        function toggleShowInActiveSites() {
            var opts = {
                state: 'showInactiveSites',
                showing: 'Showing ' + counts.inactive + ' Inactive Sites',
                hiding: 'Hiding ' + counts.inactive + ' Inactive Sites'
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
            $('#btnToggleHighlightNonePrices').off().on('click', toggleHighlightNonePrices);

            $('#btnShowActiveSites').off().on('click', toggleShowActiveSites);
            $('#btnShowInActiveSites').off().on('click', toggleShowInActiveSites);
            $('#btnResetActiveSites').off().on('click', resetActiveSites);
        };

        function docReady() {
            prices.go();
            bindEvents();
            restoreCookieSettings();
            redrawSiteFilterButtons();
            redrawPriceFilterButtons();
        };

        $(docReady);

        // API
        return {

        };
    });
