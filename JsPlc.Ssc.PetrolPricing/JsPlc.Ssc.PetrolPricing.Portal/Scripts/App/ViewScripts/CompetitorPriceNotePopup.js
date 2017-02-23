define(["jquery", "knockout", "common", "notify", "PetrolPricingService"],
function ($, ko, common, notify, pricingService) {
    "use strict";

    var siteModel = {
        SiteId: 0,
        SiteName: '',
        Note: ''
    };

    var config = {
        messages: {
            siteNoteDeleted: 'Site Note deleted',
            unableToDeleteSiteNote: 'Unable to delete Site Note',
            pleaseEnterTextForNote: 'Please enter some text for the note',
            siteNoteSavedFor: 'Site Note saved for ',
            unableToSaveSiteNoteFor: 'unable to save Site Note for ',
            unableToLoadSiteNote: 'Sorry, unable to load site Note'
        },
        classes: {
            fieldError: 'field-error'
        },
        selectors: {
            popup: '#competitorNotePopup',
            title: '#competitorNotesTitle',
            note: '#competitorNote',
            deleteButton: '#competitorNodeDeleteButton',
            cancelButton: '#competitorNodeCancelButton',
            saveButton: '#competitorNoteSaveButton',
            parentModal: '#competitorPricesPopup',
            deletePrompt: '#competitorPriceNoteDeletePrompt',
            noteEmptyWarning: '#competitorNoteWarning'
        },
        callbacks: {
            afterUpdate: null,
            afterDelete: null,
            afterHide: null
        }
    };

    var bindPopup = function (settings) {
        // event handlers
        config.callbacks.afterUpdate = settings.events.afterNoteUpdate;
        config.callbacks.afterDelete = settings.events.afterNoteDelete;
        config.callbacks.afterHide = settings.events.afterNoteHide;

        bindModalEvents();

        enableOrDisable(false, config.selectors.saveButton);
    };

    function isNotWhitespace(text) {
        return /\S/.test(text);
    };

    function bindModalEvents() {
        var modal = $(config.selectors.popup),
            deletePrompt = modal.find(config.selectors.deletePrompt);

        modal.off('show.bs.modal').on('show.bs.modal', beforeShow);
        modal.off('hide.bs.modal').on('hide.bs.modal', beforeHide);
        modal.off('hidden.bs.modal').on('hidden.bs.modal', afterHide);

        deletePrompt.off().on('click', deletePromptClick);
    };

    function bindEvents() {
        $(config.selectors.deleteButton).off().on('click', deleteButtonClick);
        $(config.selectors.saveButton).off().on('click', saveButtonClick);
        $(config.selectors.note).off().on('keyup change', noteChanged);
    };

    function noteChanged() {
        redrawNoteButtons();
    };

    function redrawNoteButtons() {
        var note = $(config.selectors.note),
            text = note.val(),
            hasNote = /\S/.test(text),
            saveButton = $(config.selectors.saveButton),
            warning = $(config.selectors.noteEmptyWarning);

        if (hasNote) {
            note.removeClass(config.classes.fieldError);
            warning.fadeOut();
            enable(saveButton);
        } else {
            note.addClass(config.classes.fieldError);
            warning.fadeIn();
            disable(saveButton);
        }
    };

    function enable(ele) {
        $(ele).attr('disabled', null);
    };

    function disable(ele) {
        $(ele).attr('disabled', 'disabled');
    };

    function enableOrDisable(isEnabled, ele) {
        isEnabled ? enable(ele) : disable(ele);
    };

    function deletePromptClick(event) {
        var ele = event.target,
            list = $(ele).data('click-action'),
            actions = list ? list.replace(/\s+/g, '').split(',') : [],
            i,
            eventMap = deletePromptActions;

        for (i = 0; i < actions.length; i++) {
            var action = actions[i];
            action && eventMap.hasOwnProperty(action)
            ? eventMap[action].call(ele, event)
            : console.log('Unknown action: "' + action + '"', eventMap);
        }
    };

    function hideDeletePrompt() {
        $(config.selectors.deletePrompt).slideUp();
    };

    function showDeletePrompt() {
        $(config.selectors.deletePrompt).slideDown();
    };

    function deletePromptYesClick() {
        function success(result) {
            siteModel.Note = '';
            config.callbacks.afterDelete(siteModel);
            notify.success(result.Message || config.messages.siteNoteDeleted);
        };

        function failure(result) {
            notify.error(result && result.Message || config.messages.unableToDeleteSiteNote);
        };

        pricingService.deleteSiteNote(siteModel.SiteId, success, failure);
        hideDeletePrompt();
    };

    function deletePromptNoClick() {
        hideDeletePrompt();
    };

    function dismissParentModal() {
        var modal = $(this).closest('.modal').modal('hide');
    };

    var deletePromptActions = {
        'click-yes': deletePromptYesClick,
        'click-no': deletePromptNoClick,
        'hide-prompt': hideDeletePrompt,
        'dismiss-parent-modal': dismissParentModal
    };

    function deleteButtonClick() {
        showDeletePrompt();
    };

    function saveButtonClick() {
        var model = $.extend( {}, siteModel);
        model.Note = $(config.selectors.note).val();

        if (isNotWhitespace(model.Note)) {
            saveSiteNote(model);
        }
        else {
            notify.error(config.messages.pleaseEnterTextForNote);
            $(config.selectors.note).focus();
        }
    };

    function saveSiteNote(model, success) {
        function success(result) {
            siteModel = model;
            $(config.selectors.popup).modal('hide');
            config.callbacks.afterUpdate(model);
            notify.success(config.messages.siteNoteSavedFor + model.SiteName);
        };

        function failure(result) {
            notify.error(config.messages.unableToSaveSiteNoteFor + model.SiteName);
        };

        pricingService.saveSiteNote(model, success, failure);
    };

    function unbindEvents() {
        $(config.selectors.popup).children().off();
    };

    function beforeShow() {
        $(config.selectors.parentModal).css('opacity', 0.8);
        bindEvents();
    };

    function beforeHide() {
        unbindEvents();
        $(config.selectors.parentModal).css('opacity', 1);
    };

    function afterHide() {
        config.callbacks.afterHide();
    };

    function editSiteNote(siteId) {
        siteModel.SiteId = siteId;

        $(config.selectors.popup).modal('show');

        $(config.selectors.noteEmptyWarning).hide();

        function success(siteNote) {
            populateModal(siteNote);
        };

        function failure() {
            notify.error(config.messages.unableToLoadSiteNote);
        };

        pricingService.getSiteNote(siteId, success, failure);
    };

    function populateModal(siteNote) {
        $(config.selectors.title).text(siteNote.SiteName);
        $(config.selectors.note).val(siteNote.Note);
        siteModel = $.extend({}, siteNote);
        redrawNoteButtons();
    };

    return {
        bindPopup: bindPopup,
        editSiteNote: editSiteNote
    };
});