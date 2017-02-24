define(["jquery", "knockout", "common", "competitorPriceNotePopup", "notify"],
function ($, ko, common, compNotePopup, notify) {
    "use strict";
    var state = {
        showNotes: false
    };

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
            collapsedNote: 'fa-sort-down'
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
            competitorRow: '.competitor-row'
        },
        naming: {
            editButton: '#EditSiteNoteButton_',
            competitorNote: '#CompetitorNote_',
            toggleNoteButton: '#ToggleSiteNoteButton_',
            competitorSiteRow: '#CompetitorSiteRow_'
        }
    };

    function extractSiteId(ele) {
        return $(ele).attr('id').split('_')[1];
    };

    function getPopup() {
        return $(config.selectors.popup);
    };

    function bindPopup() {
        bindEvents();
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

        $(config.selectors.showAllNotesButton).hide();
        $(config.selectors.hideAllNotesButton).hide();

        hideAllNotes(true);
    };

    function drawPopup(siteItem) {
        hideLoading();

        var dstHeader = $(config.selectors.header),
            srcDiv = $('#AjaxCollapseCompetitorsforsite' + siteItem.SiteId + ' .compitatorData'),
            srcThead = srcDiv.find('table thead'),
            clonedDiv = srcDiv.clone(false);

        clonedDiv.css('visibility', 'hidden');

        $(config.selectors.pricesGrid).html(clonedDiv);

        redrawAllNoteIcons();
        redrawAllNoteToggles();

        dstHeader.hide().html(srcThead.clone(false));

        setTimeout(afterDrawnPopup, 200);
    };

    function showOrHideNotePanels(showNotes, expandSiteId) {
        var noteRows = $(config.selectors.popup).find(config.selectors.noteRow);
        noteRows.each(function (index) {
            var row = $(this),
                siteId = extractSiteId(row),
                note = row.find(config.selectors.notePanelText).text(),
                hasNote = isNotWhitespace(note),
                visible = hasNote && (siteId == expandSiteId || showNotes);
            visible ? row.show() : row.hide();
        });
    };

    function afterDrawnPopup() {
        resizeCompetitorPricePopup();
        bindNoteEvents();
        $(config.selectors.showAllNotesButton).fadeIn(2000);
        $(config.selectors.hideAllNotesButton).fadeIn(2000);
        showOrHideNotePanels(state.showNotes, null);
        redrawAllNoteIcons();
        redrawAllNoteToggles();
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
        $(config.selectors.popup).modal('hide');
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

    function redrawNoteToggle(siteId) {
        var button = $(config.naming.toggleNoteButton + siteId),
            icon = button.find('i:first'),
            note = $(config.naming.competitorNote + siteId),
            text = note.find(config.selectors.notePanelText),
            hasNote = isNotWhitespace(note.text()),
            isNoteVisible = note.is(':visible');

        if (hasNote) {
            if (isNoteVisible)
                icon.removeClass(config.classes.collapsedNote).addClass(config.classes.expandedNote)
            else
                icon.removeClass(config.classes.expandedNote).addClass(config.classes.collapsedNote);
            button.show();
        } else {
            button.hide();
        }
    };

    function bindEvents() {
        $(window).on('resize', windowResized);

        $(config.selectors.showAllNotesButton).off().on('click', showAllNotesClick).hide();
        $(config.selectors.hideAllNotesButton).off().on('click', hideAllNotesClick).hide();

        $(config.selectors.closeButton).off().on('click', closePopup);
        $(config.selectors.popup).find('.modal-header .close').off().on('click', closePopup);
    };

    function showAllNotesClick() {
        showAllNotes();
    };

    function hideAllNotesClick() {
        hideAllNotes();
    };

    function bindNoteEvents() {
        var that = this,
            handler = function () {
                notePanelClick.call(this);
            };
        $(config.selectors.popup).off().on('click', config.selectors.notePanel, handler);
        $(config.selectors.triggerEditNote).off().on('click', triggerEditNote);
        $(config.selectors.triggerToggleNote).off().on('click', triggerToggleNote);
        $(config.selectors.competitorRow).off().on('dblclick', triggerEditNote);
    };

    function triggerEditNote() {
        var btn = $(this),
            siteId = extractSiteId(btn);
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
            redrawNoteToggle(siteId);
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
        } else {
            panel.hide();
            note.show();
            panel.slideDown(1000);
        }

        redrawNoteToggle(siteId);
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

    function showAllNotes() {
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
    };

    function isNotWhitespace(note) {
        return /\S/.test(note);
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