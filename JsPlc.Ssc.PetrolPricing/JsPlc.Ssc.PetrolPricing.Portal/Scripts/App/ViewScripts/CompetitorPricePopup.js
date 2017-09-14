define(["jquery", "knockout", "common", "competitorPriceNotePopup", "notify", "cookieSettings", "bootbox"],
function ($, ko, common, compNotePopup, notify, cookieSettings, bootbox) {
    "use strict";
    var state = {
        showNotes: false,
        showGrocers: true,
        showNonGrocers: true,
        insideDriveTime: true,
        outsideDriveTime: true,
        includeDriveTime: false,
        showNoPrices: true
    };

    var viewingSiteId = 0;

    var config = {
        messages: {
            editSiteNote: 'Edit Site Note',
            createSiteNote: 'Create Site Note',
            showingAllSiteNotes: 'Showing all Site Notes',
            hidingAllSiteNotes: 'Hiding all Site Notes'
        },
        classes: {
            activeToggleButton: 'btn-primary',
            inactiveToggleButton: 'btn-default',
            activeEditIcon: 'btn-success',
            inactiveEditIcon: 'btn-default',
            highlightRow: 'highlight',
            expandedNote: 'fa-sort-up',
            collapsedNote: 'fa-sort-down',
            viewingSiteCompetitors: 'viewing-site-competitors',
            competitorGrocerRow: 'competitor-grocer',
            competitorNonGrocerRow: 'competitor-non-grocer',
            insideDriveTime: 'inside-drive-time',
            outsideDriveTime: 'outside-drive-time'
        },
        selectors: {
            popup: '#competitorPricesPopup',
            loading: '#competitorPricePopupLoading',
            header: '#competitorPricePopupHeader',
            pricesGrid: '#competitorPricePopupPrices',
            firstDataRow: '#competitorPricePopupPrices .competitorTable tbody tr:first',
            storeName: '#competitorPricePopupStoreName',
            gridWrapper: '.compitatorData',
            noteRows: '.note-row',
            showAllNotesButton: '#competitorPricesShowAllNotesButton',
            hideAllNotesButton: '#competitorPricesHideAllNotesButton',
            noteRow: '.note-row',
            notePanel: '.note-panel',
            notePanelText: '.note-panel-text',
            triggerEditNote: '.triggerEditSiteNote',
            triggerToggleNote: '.triggerToggleNote',
            closeButton: '#competitorPricePopupCloseButton',
            competitorRow: '.competitor-row',
            modalBackground: '.modal-backdrop',
            minimiseModalButton: '#btnMinimiseModal',
            maximiseModalButton: '#btnMaximiseModal',
            toggleGrocersButton: '#competitorPricesToggleGrocersButton',
            toggleNonGrocersButton: '#competitorPricesToggleNonGrocersButton',
            resetFiltersButton: '#competitorPricesResetFilters',
            insideFilterButton: '#competitorPricesToggleInsideButton',
            outsideFilterButton: '#competitorPricesToggleOutsideButton',
            resetDriveTimeFiltersButton: '#competitorPricesResetDriveTimeFilters',
            visibleCount: '#competitorPricePopupVisibleCount',
            totalCount: '#competitorPricePopupTotalCount',
            excludeDriveTimeButton: '#competitorPriceExcludeDriveTime',
            includeDriveTimeButton: '#competitorPriceIncludeDriveTime',
            searchBrandInput: '[data-click="competitorSearchBrand"]',
            searchNameInput: '[data-click="competitorSearchStoreName"]',
            resetBrandSearchButton: '[data-click="competitorSearchBrandClear"]',
            resetNameSearchButton: '[data-click="competitorSearchStoreNameClear"]',
            showNoPriceButton: '#competitorShowNoPriceButton',
            hideNoPriceButton: '#competitorHideNoPriceButton'
        },
        naming: {
            editButton: '#EditSiteNoteButton_',
            competitorNote: '#CompetitorNote_',
            toggleNoteButton: '#ToggleSiteNoteButton_',
            competitorSiteRow: '#CompetitorSiteRow_'
        }
    };

    function extractSiteId(ele) {
        var id = $(ele).attr('id');
        return id.split('_')[1];
    };

    function getPopup() {
        return $(config.selectors.popup);
    };

    function bindPopup() {
        resetState();
        bindEvents();
    };

    function resetState() {
        state.showGrocers = true;
        state.showNonGrocers = true;
        state.insideDriveTime = true;
        state.outsideDriveTime = true;
        state.includeDriveTime = false;
    };

    function showLoading() {
        $(config.selectors.loading).show();
    };

    function hideLoading() {
        $(config.selectors.loading).fadeOut(2000);
    };

    function hidePrices() {
        $(config.selectors.pricesGrid).html('');
    };

    function populate(siteItem) {
        var popup = $(config.selectors.popup);
                        
        popup.find('.storeNumber').text(siteItem.StoreNo);
        popup.find('.storeName').text(siteItem.StoreName);
        popup.find('.storeTown').text(siteItem.Town);
        popup.find('.catNo').text(siteItem.CatNo);
        popup.find('.pfsNo').text(siteItem.PfsNo);

        $(config.selectors.storeName).text(siteItem.StoreName)

        $('.compitatorData .storeName').text(siteItem.StoreName);

        $('.chosen-date').text($('#viewingDate').val());
        hideAllNotes(true);
    };

    function confirmEditSite() {
        var popup = $(config.selectors.popup),
            editSiteLink = popup.data('edit-site-link').replace('{SITEID}', viewingSiteId);

        bootbox.confirm({
            title: "Confirm navigation",
            message: 'Are you sure you with to proceed and <strong>Edit this Site</strong>?'
                + '<br />'
                + '<br />'
                + 'This will <strong>lose any unsaved Overrides</strong>.',
            buttons: {
                confirm: {
                    label: '<i class="fa fa-check"></i> Yes - Edit the Site',
                    className: 'btn btn-danger'
                },
                cancel: {
                    label: '<i class="fa fa-times"></i> No',
                    className: 'btn btn-default'
                }
            },
            callback: function (result) {
                if (result)
                    window.location = editSiteLink
            }
        });
    };

    function commonSetNoPriceState(opts) {
        state.showNoPrices = opts.state;
        redrawNoPriceButtons();
        applyFilters();
        notify.info(opts.message);
        cookieSettings.writeBoolean('pricing.hideNoPrices', opts.state);
    };

    function showNoPriceClick() {
        commonSetNoPriceState({
            state: true,
            message: 'Showing all Prices (including N/A ones)'
        });
    };

    function hideNoPriceClick() {
        commonSetNoPriceState({
            state: false,
            message: 'Hiding N/A prices'
        });
    };

    function populateReadOnlyPopupPrices(siteId) {
        var row = $('#SiteHeading' + siteId),
            cells = row.find('>td'),
            tomorrow = $('#tomorrow1').text().replace(/[a-z ]/g,''),
            today = $('#today1').text().replace(/[a-z]/g, '');

        cloneAsReadonlyHtml('.readonlyUnleadedYesterday', cells.get(4));
        cloneAsReadonlyHtml('.readonlyUnleadedToday', cells.get(5));
        cloneAsReadonlyHtml('.readonlyUnleadedTodayPriceChange', cells.get(6));

        cloneAsReadonlyHtml('.readonlyDieselYesterday', cells.get(8));
        cloneAsReadonlyHtml('.readonlyDieselToday', cells.get(9));
        cloneAsReadonlyHtml('.readonlyDieselTodayPriceChange', cells.get(10));

        cloneAsReadonlyHtml('.readonlySuperUnleadedYesterday', cells.get(12));
        cloneAsReadonlyHtml('.readonlySuperUnleadedToday', cells.get(13));
        cloneAsReadonlyHtml('.readonlySuperUnleadedTodayPriceChange', cells.get(14));
    };
    function cloneAsReadonlyHtml(selector, ele) {
        var popup = $(config.selectors.popup),
            src = $(ele),
            dst = popup.find(selector),
            value = src.find('input').val() || '',
            html = (src.html() + '')
            .replace(/data-bind="[^"]+"/i, '')
            .replace(/<!--[^>]+-->/g, '')
            .replace(/\s+/g, ' ')
            .replace(/<input/i, '<input readonly="readonly" value="' + value + '"');

        dst.html(html);

        if (src.hasClass('price-snapback'))
            dst.addClass('price-snapback');
        else
            dst.removeClass('price-snapback');

        if (src.hasClass('price-freeze'))
            dst.addClass('price-freeze');
        else
            dst.removeClass('price-freeze');
    };

    function drawPopup(siteItem) {
        hideLoading();
        viewingSiteId = siteItem.SiteId;

        var dstHeader = $(config.selectors.header),
            srcDiv = $('#AjaxCollapseCompetitorsforsite' + siteItem.SiteId + ' .compitatorData'),
            srcThead = srcDiv.find('table thead'),
            clonedDiv = srcDiv.clone(false),
            popup = $(config.selectors.popup),
            priceMatchInfo = '',
            matchCompetitorMarkup = siteItem.FuelPrices[0].MatchCompetitorMarkup,
            fuelMarkupInfotip = '',
            markups = {
                unleaded: 0,
                diesel: 0,
                superUnleaded: 0
            };


        clonedDiv.css('visibility', 'hidden');

        $(config.selectors.pricesGrid).html(clonedDiv);

        restoreCookieSettings();

        redrawAllNoteIcons();
        redrawAllNoteToggles();
        redrawFilterButtons();
        redrawDriveTimeButtons();
        redrawIncludeDriveTimeButtons();
        redrawNoPriceButtons();
        applyFilters();

        dstHeader.hide().html(srcThead.clone(false));

        maximiseModal();

        switch (siteItem.PriceMatchType) {
            case 1:
                priceMatchInfo = '<div data-infotip="Standard"><span class="price-match-strategy"><i class="fa fa-dot-circle-o"></i> Standard</span></div>';
                markups.unleaded = siteItem.FuelPrices[1].Markup;
                markups.diesel = siteItem.FuelPrices[2].Markup;
                markups.superUnleaded = siteItem.FuelPrices[0].Markup;
                fuelMarkupInfotip = '';
                break;
            case 2:
                priceMatchInfo = '<div data-infotip="Trial Price"><span class="price-match-strategy"><i class="fa fa-flask"></i> Trial Price</span> <big class="markup-price">' + formatFuelMarkUp(siteItem.FuelPrices[0].CompetitorPriceOffset) + '</big></div>';
                markups.unleaded = siteItem.FuelPrices[1].CompetitorPriceOffset;
                markups.diesel = siteItem.FuelPrices[2].CompetitorPriceOffset;
                markups.superUnleaded = siteItem.FuelPrices[0].CompetitorPriceOffset;
                fuelMarkupInfotip = 'Any markup from a [br /][b]Match Competitor[/b] or [b]Trial Price[/b][br /] [em]Price Match Strategy[/em]';
                fuelMarkupInfotip = '[b class="fa fa-flask"][/b] [b]Trial Price[/b] Markup of [em]' + formatFuelMarkUp(siteItem.FuelPrices[0].CompetitorPriceOffset) + '[/em]';
                break;
            case 3:
                priceMatchInfo = '<div data-infotip="Match Competitor"><span class="price-match-strategy"><i class="fa fa-clone"></i> Match:</span> ' + siteItem.FuelPrices[0].CompetitorName + '<big class="markup-price">' + formatFuelMarkUp(matchCompetitorMarkup) + '</big></div>';
                markups.unleaded = matchCompetitorMarkup;
                markups.diesel = matchCompetitorMarkup;
                markups.superUnleaded = matchCompetitorMarkup;
                fuelMarkupInfotip = '[b class="fa fa-clone"][/b] [b]Match Competitor[/b] Markup of [em]' + formatFuelMarkUp(matchCompetitorMarkup) + '[/em]';
                break;
        }

        popup.find('.price-match-info').html(priceMatchInfo);
        popup.find('.fuel-markup-unleaded').text(formatFuelMarkUp(markups.unleaded));
        popup.find('.fuel-markup-diesel').text(formatFuelMarkUp(markups.diesel));
        popup.find('.fuel-markup-super-unleaded').text(formatFuelMarkUp(markups.superUnleaded));
        popup.find('.storeName').text(siteItem.StoreName);

        popup.find('.fuel-markup-unleaded').attr('data-infotip', fuelMarkupInfotip)[fuelMarkupInfotip == '' ? 'hide': 'show']();
        popup.find('.fuel-markup-diesel').attr('data-infotip', fuelMarkupInfotip)[fuelMarkupInfotip == '' ? 'hide': 'show']();
        popup.find('.fuel-markup-super-unleaded').attr('data-infotip', fuelMarkupInfotip)[fuelMarkupInfotip == '' ? 'hide': 'show']();
        
        setTimeout(afterDrawnPopup, 200);

        highlightSainsburysSiteRow();

        bindSearchEvents();
    };

    function formatFuelMarkUp(markup) {
        return markup < 0
            ? '-' + Math.abs(markup) + 'p'
            : '+' + Math.abs(markup) + 'p';
    };

    function highlightSainsburysSiteRow() {
        var row = $('#SiteHeading' + viewingSiteId);
        row.addClass(config.classes.viewingSiteCompetitors);
    };

    function unhighlightSainsburysSiteRow() {
        var row = $('#SiteHeading' + viewingSiteId);
        row.removeClass(config.classes.viewingSiteCompetitors);
    };

    function showOrHideNotePanels(showNotes, expandSiteId) {
        var noteRows = $(config.selectors.popup).find(config.selectors.noteRow);
        noteRows.each(function (index) {
            var row = $(this),
                siteId = extractSiteId(row),
                note = $(config.naming.competitorNote + siteId),
                panel = note.find(config.selectors.notePanelText),
                text = panel.text(),
                hasNote = isNotWhitespace(text),
                visible = hasNote && (siteId == expandSiteId || showNotes);
            visible ? row.show() : row.hide();
            visible ? panel.show() : panel.hide();
        });
    };

    function afterDrawnPopup() {
        resizeCompetitorPricePopup();
        bindNoteEvents();

        showOrHideNotePanels(state.showNotes, null);
        redrawAllNoteIcons();
        redrawAllNoteToggles();
        bindOutsideModalEvents();

        populateReadOnlyPopupPrices(viewingSiteId);

        $(config.selectors.popup).on('click', '.comp-brand-name', compBrandNameClick);

        applyNoPricesStyles();
    };

    function setHeaderWidths() {
        fixedTableHeaders(config.selectors.pricesGrid + ' table:first');
    };

    function setScrollingTableHeight() {
        var innerHeight = window.innerHeight,
            popup = $(config.selectors.popup).find('.modal-content'),
            table = $(config.selectors.pricesGrid).find(config.selectors.gridWrapper),
            tableHeight,
            popupHeightWithoutTable,
            gap = 50,
            minHeight = 100,
            targetHeight;

        table.css('position', 'absolute')
            .height('auto');
        tableHeight = table.height();
        table.hide();
        popupHeightWithoutTable = popup.height();

        targetHeight = innerHeight - popupHeightWithoutTable - gap;
        targetHeight = Math.min(targetHeight, tableHeight);
        targetHeight = Math.max(targetHeight, minHeight);

        table.height(targetHeight + 'px')
            .css({
                'visibility': 'visible',
                'position': 'relative'
            })
            .show();
    };

    function fixedTableHeaders(tableSelector) {
        var table = $(tableSelector),
            thead = table.find('thead:first'),
            tableWidth = table.width(),
            theadHeight = 1 + thead.outerHeight(),
            tbodyWidth = table.find('tbody:first').width(),
            priceGridWidth = $(config.selectors.pricesGrid).width();

        if (tbodyWidth > priceGridWidth)
            tbodyWidth = priceGridWidth;

        table.css({'margin-top': theadHeight + 'px', 'width': '100%' });
        thead.css({ 'position': 'fixed', 'margin-top': '-' + theadHeight + 'px', 'width': tbodyWidth + 'px', 'overflow': 'hidden' });

        var cols = table.find('colgroup col');

        for (var i = 0; i < cols.length; i++) {
            var percent = parseInt(cols[i].width, 10);
            var wide = Math.floor(tableWidth * percent / 100);
            table.find('thead .num' + (i + 1)).width(wide + 'px');
        }
    };

    function resizeCompetitorPricePopup() {
        setHeaderWidths();
        setScrollingTableHeight();

        setTimeout(setHeaderWidths, 100);
    };

    function windowResized() {
        var popup = $(config.selectors.popup)
        if (popup.is(':visible')) {
            setTimeout(resizeCompetitorPricePopup, 100);
        }
    };

    function closePopup() {
        unhighlightSainsburysSiteRow();
        $(config.selectors.modalBackground).show();
        $(config.selectors.popup).modal('hide');
        unbindOutsideModalEvents();
    };

    function redrawAllNoteIcons() {
        var notes = $(config.selectors.popup).find(config.selectors.notePanelText);
        notes.each(function (item) {
            var note = $(this),
                tr = note.closest('tr'),
                id = extractSiteId(tr);

            redrawNoteEditButton(id);
        });
    };

    function redrawNoteEditButton(siteId) {
        var editButton = $(config.naming.editButton + siteId),
            note = $(config.naming.competitorNote + siteId).find(config.selectors.notePanelText),
            hasNote = isNotWhitespace(note.text());

        if (hasNote)
            editButton.removeClass(config.classes.inactiveEditIcon)
                .addClass(config.classes.activeEditIcon)
                .attr('title', config.messages.editSiteNote);
        else
            editButton.removeClass(config.classes.activeEditIcon)
                .addClass(config.classes.inactiveEditIcon)
                .attr('title', config.messages.createSiteNote);
    };

    function redrawAllNoteToggles() {
        var rows = $(config.selectors.popup).find(config.selectors.competitorRow);
        rows.each(function () {
            var row = $(this),
                siteId = extractSiteId(row);
            redrawNoteToggle(siteId);
        });
    };

    function redrawNoteToggle(siteId, isExpanded) {
        var button = $(config.naming.toggleNoteButton + siteId),
            icon = button.find('.fa'),
            note = $(config.naming.competitorNote + siteId),
            text = note.find(config.selectors.notePanelText),
            hasNote = isNotWhitespace(note.text()),
            isNoteVisible = note.is(':visible');

        if (isExpanded != undefined)
            isNoteVisible = isExpanded;

        if (hasNote) {
            if (isNoteVisible)
                replaceClass(icon, config.classes.collapsedNote, config.classes.expandedNote);
            else
                replaceClass(icon, config.classes.expandedNote, config.classes.collapsedNote);
            button.show();
        } else {
            button.hide();
        }
    };
    
    function maximiseModal() {
        var grid = $(config.selectors.pricesGrid),
            backdrop = $(config.selectors.modalBackground),
            maxButton = $(config.selectors.maximiseModalButton),
            minButton = $(config.selectors.minimiseModalButton);
        backdrop.fadeTo(1000, 0.5);
        grid.slideDown(1000);
        maxButton.removeClass('btn-default').addClass('btn-primary');
        minButton.removeClass('btn-primary').addClass('btn-default');
    };

    function minimiseModal() {
        var grid = $(config.selectors.pricesGrid),
            backdrop = $(config.selectors.modalBackground),
            maxButton = $(config.selectors.maximiseModalButton),
            minButton = $(config.selectors.minimiseModalButton);
        grid.slideUp(1000);
        backdrop.fadeTo(1000, 0);
        maxButton.removeClass('btn-primary').addClass('btn-default');
        minButton.removeClass('btn-default').addClass('btn-primary');
    };

    function bindEvents() {
        $(window).on('resize', windowResized);

        $(config.selectors.showAllNotesButton).off().click(showAllNotesClick);
        $(config.selectors.hideAllNotesButton).off().click(hideAllNotesClick);

        $(config.selectors.toggleGrocersButton).off().click(toggleGrocersClick);
        $(config.selectors.toggleNonGrocersButton).off().click(toggleNonGrocersClick);
        $(config.selectors.resetFiltersButton).off().click(resetFiltersClick);

        $(config.selectors.insideFilterButton).off().click(insideFilterClick);
        $(config.selectors.outsideFilterButton).off().click(outsideFilterClick);
        $(config.selectors.resetDriveTimeFiltersButton).off().click(resetDriveTimeClick);

        $(config.selectors.closeButton).off().click(closePopup);
        $(config.selectors.popup).find('.modal-header .close').off().click(closePopup);

        $(config.selectors.maximiseModalButton).off().click(maximiseModal);
        $(config.selectors.minimiseModalButton).off().click(minimiseModal);

        $(config.selectors.excludeDriveTimeButton).off().click(excludeDriveTimeClick);
        $(config.selectors.includeDriveTimeButton).off().click(includeDriveTimeClick);

        $(config.selectors.storeName).off().click(confirmEditSite);

        $(config.selectors.showNoPriceButton).off().click(showNoPriceClick);
        $(config.selectors.hideNoPriceButton).off().click(hideNoPriceClick);
    };

    function bindSearchEvents() {
        var modal = $(config.selectors.popup);

        // clear the seach boxes
        $(config.selectors.searchNameInput).val('');
        $(config.selectors.searchNameInput).val('');

        // wire up events
        modal.find(config.selectors.searchBrandInput).off().on('keyup change', peformCompetitorSearch);
        modal.find(config.selectors.searchNameInput).off().on('keyup change', peformCompetitorSearch);
        modal.find(config.selectors.resetBrandSearchButton).off().click(resetBrandSearchClick);
        modal.find(config.selectors.resetNameSearchButton).off().click(resetNameSearchClick);
    };

    function bindOutsideModalEvents() {
        $(config.selectors.modalBackground).off().click(closePopup)
            .css('cursor', 'pointer')
            .attr('title', 'Close popup');
    };

    function unbindOutsideModalEvents() {
        return $(config.selectors.modalBackground).off()
            .css('cursor', 'default')
            .attr('title', '');
    };
    
    function showAllNotesClick() {
        showAllNotes();
    };

    function hideAllNotesClick() {
        hideAllNotes();
    };

    function commonToggleFilterState(opts) {
        var newstate = !state[opts.name],
            message = newstate ? opts.showing : opts.hiding;
        state[opts.name] = newstate;
        applyFilters();
        redrawFilterButtons();
        redrawDriveTimeButtons();
        notify.info(message);
    };

    function toggleGrocersClick() {
        commonToggleFilterState({
            name: 'showGrocers',
            showing: 'Showing Grocers',
            hiding: 'Hiding Grocers'
        });
    };

    function toggleNonGrocersClick() {
        commonToggleFilterState({
            name: 'showNonGrocers',
            showing: 'Showing Non-Grocers',
            hiding: 'Hiding Non-Grocers'
        });
    };

    function resetFiltersClick() {
        state.showGrocers = true;
        state.showNonGrocers = true;
        applyFilters();
        redrawFilterButtons();
        notify.info('Filters have been reset &mdash; Showing all items');
    };

    function insideFilterClick() {
        commonToggleFilterState({
            name: 'insideDriveTime',
            showing: 'Showing Grocers inside the Drive Time limit',
            hiding: 'Hiding Grocers inside the Drive Time limit'
        });
    };

    function outsideFilterClick() {
        commonToggleFilterState({
            name: 'outsideDriveTime',
            showing: 'Showing Grocers outside the Drive Time limit',
            hiding: 'Hiding Grocers outside the Drive Time limit'
        });
    };

    function resetDriveTimeClick() {
        state.insideDriveTime = true;
        state.outsideDriveTime = true;
        redrawDriveTimeButtons();
        applyFilters();
        notify.info('Reset Drive Time Filters (showing all Drive Times)')
    };

    function excludeDriveTimeClick() {
        state.includeDriveTime = false;
        redrawIncludeDriveTimeButtons();
        applyFilters();
        notify.info('Showing Competitor Prices (not including Drive Time Markup)');
        cookieSettings.writeBoolean('pricing.includeDriveTime', false);
    };

    function includeDriveTimeClick() {
        state.includeDriveTime = true;
        redrawIncludeDriveTimeButtons();
        applyFilters();
        notify.info('Showing Competitor Prices Including Drive Time Markup');
        cookieSettings.writeBoolean('pricing.includeDriveTime', true);
    };

    function peformCompetitorSearch() {
        applyFilters();
    };
    
    function resetBrandSearchClick() {
        $(config.selectors.searchBrandInput).val('');
        peformCompetitorSearch();
        notify.info('Cleared the Brand search');
    };
    function resetNameSearchClick() {
        $(config.selectors.searchNameInput).val('');
        peformCompetitorSearch();
        notify.info('Cleared the Competitor Name search');
    };

    function compBrandNameClick() {
        var brandName = $(this).text();
        $(config.selectors.searchBrandInput).val(brandName);
        peformCompetitorSearch();
        notify.info('Filtered by Brand: ' + brandName);
    };

    function bindNoteEvents() {
        $(config.selectors.popup).off().on('click', config.selectors.notePanel, notePanelClick);
        $(config.selectors.triggerEditNote).off().click(triggerEditNote);
        $(config.selectors.triggerToggleNote).off().click(triggerToggleNote);
        $(config.selectors.competitorRow).off().on('dblclick', triggerEditNote);
    };

    function triggerEditNote() {
        var btn = $(this),
            siteId = extractSiteId(btn);
        unbindOutsideModalEvents();
        compNotePopup.editSiteNote(siteId);
        highlightSiteRow(siteId, true);
        expandSiteNote(siteId);
    };

    function expandSiteNote(siteId) {
        var note = $(config.naming.competitorNote + siteId),
            panel = note.find(config.selectors.notePanelText),
            text = panel.text(),
            hasNote = isNotWhitespace(text),
            isNoteVisible = note.is(':visible');

        if (hasNote && !isNoteVisible) {
            panel.hide();
            note.show();
            panel.slideDown(1000);
            redrawNoteToggle(siteId, true);
        }
    };

    function triggerToggleNote() {
        var btn = $(this),
            siteId = extractSiteId(btn),
            note = $(config.naming.competitorNote + siteId),
            panel = note.find(config.selectors.notePanelText),
            isNoteVisible = note.is(':visible');

        if (isNoteVisible) {
            panel.slideUp(1000, function () { note.hide(); });
            redrawNoteToggle(siteId, false);
        } else {
            panel.hide();
            note.show();
            panel.slideDown(1000);
            redrawNoteToggle(siteId, true);
        }
    };

    function highlightSiteRow(siteId, isHighlight) {
        var cssClass = config.classes.highlightRow;
        if (siteId) {
            var row = $(config.naming.competitorSiteRow + siteId);
            isHighlight ? row.addClass(cssClass) : row.removeClass(cssClass);
        } else {
            $(config.selectors.pricesGrid).find('.' + cssClass).removeClass(cssClass);
        }
    };

    function notePanelClick() {
        var row = $(this).closest('tr' + config.selectors.noteRow),
            siteId = extractSiteId(row);

        compNotePopup.editSiteNote(siteId);
        highlightSiteRow(siteId, true);
    };

    function isPopupVisible() {
        return $('#competitorPricesPopup').is(':visible');
    };

    function showAllNotes() {
        if (!isPopupVisible())
            return;

        state.showNotes = true;
        showOrHideNotePanels(true);
        setActiveToggleButtons(config.selectors.showAllNotesButton, config.selectors.hideAllNotesButton);
        notify.info(config.messages.showingAllSiteNotes);
    };

    function hideAllNotes(hideNotify) {
        state.showNotes = false;
        showOrHideNotePanels(false);
        setActiveToggleButtons(config.selectors.hideAllNotesButton, config.selectors.showAllNotesButton);
        if (!hideNotify)
            notify.info(config.messages.hidingAllSiteNotes);
    };

    function replaceClass(ele, oldClass, newClass) {
        $(ele).removeClass(oldClass).addClass(newClass);
    };

    function setActiveToggleButtons(activeSelector, inactiveSelector) {
        replaceClass(activeSelector, config.classes.inactiveToggleButton, config.classes.activeToggleButton);
        replaceClass(inactiveSelector, config.classes.activeToggleButton, config.classes.inactiveToggleButton);
    };

    function bindNotePopup(settings) {
        compNotePopup.bindPopup(settings);
    };

    function redrawSiteNote(siteNote) {
        var html = common.htmlEncode(siteNote.Note).replace(/\n/g, '<br />');
        $(config.naming.competitorNote + siteNote.SiteId).find(config.selectors.notePanelText).html(html);
    }

    function afterNoteUpdate(siteNote) {
        state.showNotes = false;
        setActiveToggleButtons(config.selectors.hideAllNotesButton, config.selectors.showAllNotesButton);
        redrawSiteNote(siteNote);
        redrawNoteEditButton(siteNote.SiteId);
        redrawNoteToggle(siteNote.SiteId);
        showOrHideNotePanels(state.showNotes, siteNote.SiteId);
    };

    function afterNoteDelete(siteNote) {
        redrawSiteNote(siteNote);
        redrawAllNoteIcons();
        redrawNoteToggle(siteNote.SiteId);
        showOrHideNotePanels(state.showNotes, siteNote.SiteId);
    };

    function afterNoteHide() {
        highlightSiteRow();
        bindOutsideModalEvents();
    };

    function redrawFilterButtons() {
        setButtonActiveState(config.selectors.toggleGrocersButton, state.showGrocers);
        setButtonActiveState(config.selectors.toggleNonGrocersButton, state.showNonGrocers);
    };

    function redrawDriveTimeButtons() {
        setButtonActiveState(config.selectors.insideFilterButton, state.insideDriveTime);
        setButtonActiveState(config.selectors.outsideFilterButton, state.outsideDriveTime);
    };

    function redrawIncludeDriveTimeButtons() {
        setButtonActiveState(config.selectors.excludeDriveTimeButton, !state.includeDriveTime);
        setButtonActiveState(config.selectors.includeDriveTimeButton, state.includeDriveTime);
    };

    function redrawNoPriceButtons() {
        setButtonActiveState(config.selectors.showNoPriceButton, state.showNoPrices);
        setButtonActiveState(config.selectors.hideNoPriceButton, !state.showNoPrices);
    };
    
    function applyNoPricesStyles() {
        var popup = $(config.selectors.popup),
            tables = popup.find('table.competitorTable');

        if (state.showNoPrices) {
            tables.removeClass('hide-competitor-no-price-data').addClass('show-competitor-no-price-data')
        } else {
            tables.removeClass('show-competitor-no-price-data').addClass('hide-competitor-no-price-data');
        }
    };

    function applyFilters() {
        var grid = $(config.selectors.pricesGrid),
            popup = $(config.selectors.popup),
            table = popup.find('table.competitorTable'),
            rows = grid.find(config.selectors.competitorRow),
            totalCount = 0,
            visibleCount = 0,
            brandSearch = '',
            hasBrandSearch = false,
            storeNameSearch = '',
            hasStoreNameSearch = false;

        applyNoPricesStyles();

        // workaround for weird issue with KO
        if (popup.find(config.selectors.searchBrandInput).length == 2) {
            brandSearch = (popup.find(config.selectors.searchBrandInput).eq(1).val() + '').toUpperCase();
            hasBrandSearch = brandSearch != '';
            storeNameSearch = (popup.find(config.selectors.searchNameInput).eq(1).val() + '').toUpperCase();
            hasStoreNameSearch = storeNameSearch != '';
        }

            if (state.includeDriveTime)
                grid.removeClass('show-competitors-excluding-drive-time').addClass('show-competitors-including-drive-time');
            else
                grid.removeClass('show-competitors-including-drive-time').addClass('show-competitors-excluding-drive-time');

            rows.each(function () {
                var row = $(this),
                    rowBrandName = '',
                    rowStoreName = '',
                    isGrocer = row.hasClass(config.classes.competitorGrocerRow),
                    isNonGrocer = row.hasClass(config.classes.competitorNonGrocerRow),
                    isInside = row.hasClass(config.classes.insideDriveTime),
                    isOutside = row.hasClass(config.classes.outsideDriveTime),
                    visibleGrocer = (isGrocer && state.showGrocers) || (isNonGrocer && state.showNonGrocers),
                    visibleDriveTime = (isInside && state.insideDriveTime) || (isOutside && state.outsideDriveTime),
                    visibleBrand = true,
                    visibleStoreName = true;

                if (hasBrandSearch) {
                    rowBrandName = row.find('.comp-brand-name').text().toUpperCase();
                    visibleBrand = rowBrandName.indexOf(brandSearch) != -1;
                }

                if (hasStoreNameSearch) {
                    rowStoreName = row.find('.comp-store-name').text().toUpperCase();
                    visibleStoreName = rowStoreName.indexOf(storeNameSearch) != -1;
                }

                if (visibleGrocer && visibleDriveTime && visibleBrand && visibleStoreName) {
                    row.show();
                    visibleCount++;
                }
                else
                    row.hide();
                totalCount++;
            });

        $(config.selectors.visibleCount).text(visibleCount);
        $(config.selectors.totalCount).text(totalCount);
    };

    function setButtonActiveState(selector, isActive) {
        var button = $(selector);
        isActive
        ? button.removeClass(config.classes.inactiveToggleButton).addClass(config.classes.activeToggleButton)
        : button.removeClass(config.classes.activeToggleButton).addClass(config.classes.inactiveToggleButton);
    };

    function isNotWhitespace(note) {
        return /\S/.test(note);
    };

    function restoreCookieSettings() {
        state.includeDriveTime = cookieSettings.readBoolean('pricing.includeDriveTime', false);
        state.showNoPrices = cookieSettings.readBoolean('pricing.hideNoPrices', false);
    };

    return {
        bindPopup: bindPopup,
        drawPopup: drawPopup,
        populate: populate,
        showLoading: showLoading,
        hideLoading: hideLoading,
        hidePrices: hidePrices,
        bindNotePopup: bindNotePopup,
        afterNoteUpdate: afterNoteUpdate,
        afterNoteDelete: afterNoteDelete,
        afterNoteHide: afterNoteHide
    };
});