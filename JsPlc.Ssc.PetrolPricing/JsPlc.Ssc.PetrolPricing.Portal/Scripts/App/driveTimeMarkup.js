define(["jquery", "infotips", "bootbox", "notify", "validation"],
    function ($, infotips, bootbox, notify, validation) {
        "use strict";

        function create(opts) {

            return (function() {

                var panel = $('#' + opts.panelId),
                    warningPanel = $('#' + opts.warningPanelId),
                    fuelTypeId = opts.fuelTypeId,
                    friendlyFuelName = opts.friendlyFuelName,
                    callback = opts.callback || function () { };

                var events = {
                    dataChanged: 'data-changed'
                };

                var errors = {
                    empty: true,
                    unordered: true,
                    input: true
                };

                var templates = {
                    row: ''
                };

                var limits = {
                    drivetime: { min: 0, max: 60 },
                    markup: {min: 0, max: 50}
                };

                var controls = {
                    driveTime: null,
                    markup: null,
                    tbody: null
                };

                var driveTimeMarkups = [];

                function addItem(drivetime, markup) {
                    driveTimeMarkups.push({driveTime: drivetime, markup: markup});
                };

                function sortMarkups() {
                    driveTimeMarkups.sort(function(a, b) {
                        return a.driveTime - b.driveTime;
                    });
                };

                function replaceTokens(template, tokens) {
                    var key,
                        html = '' + template;
                    for(var key in tokens) {
                        html = html.split(key).join(tokens[key]);
                    }
                    return html;
                };

                function render(pingIndex) {
                    var tbody = controls.tbody,
                        pingTr,
                        lastMarkup = 0;
                    tbody.find('tr').remove();
                    $.each(driveTimeMarkups, function(index, item) {
                        var tr,
                            isWarningRequired = item.markup < lastMarkup,
                            tokens = {
                                '{driveTime}': item.driveTime,
                                '{markup}': item.markup,
                                '{index}': index,
                                '{warning}': isWarningRequired
                                    ? '<i class="fa fa-warning text-danger" data-infotip="Markup value [b]' + item.markup + '[/b] is lower than the previous [b]' + lastMarkup + '[/b]"></i>'
                                    : ''
                            };

                        lastMarkup = item.markup;

                        tr = $(replaceTokens(templates.row, tokens));
                        tr.appendTo(tbody);
                    });

                    if (pingIndex != undefined) {
                        setTimeout(function() {
                            pingTr = tbody.find('tr').eq(pingIndex);
                            pingTr.addClass('ping-background');
                        }, 100);
                    }
                };

                function renderErrors() {
                    var ul = $('<ul />'),
                        messages = [];

                    if (errors.empty)
                        messages.push('Drive Time list is empty');
                    if (errors.unordered)
                        messages.push('Markup values are incorrectly ordered');
                    if (errors.input)
                        messages.push('Invalid Drive Time and/or Markup');

                    if (messages.length) {
                        $.each(messages, function (i, item) {
                            $('<li>').html(item).appendTo(ul);
                        });
                        warningPanel.html(ul).show();

                    } else
                        warningPanel.hide();
                };

                function findByDriveTime(driveTime) {
                    var index = -1;

                    $.each(driveTimeMarkups, function(i, item) {
                        if (item.driveTime == driveTime)
                            index = i;
                    });
                    return index;
                };

                function addOrUpdate(driveTime, markup) {
                    var index = findByDriveTime(driveTime);

                    if (index != -1) {
                        driveTimeMarkups[index].markup = markup;
                        return {
                            found: true,
                            index: index

                        };
                    }

                    addItem(driveTime, markup);
                    sortMarkups();
                    index = findByDriveTime(driveTime);

                    return {
                        found: false,
                        index: index
    
                    };
                };

                function validateDriveTime(value, allowEmpty) {
                    if (value != '' || !allowEmpty) {
                        if (!validation.isDriveTime(value))
                            return {
                                value: value,
                                isValid: false,
                                error: 'Please enter a value for Drive-Time'
                            };

                        if (value < limits.drivetime.min || value > limits.drivetime.max)
                            return {
                                value: value,
                                isValid: false,
                                error: 'Drive-time can be between ' + limits.drivetime.min + ' and ' + limits.drivetime.max
                            };

                    }
                    return {
                        value: Math.floor(value),
                        isValid: true,
                        error: ''
                    };
                };

                function validateMarkup(value, allowEmpty) {
                    if (value != '' || !allowEmpty) {
                        if (!validation.isMarkup(value))
                            return {
                                value: value,
                                isValid: false,
                                error: 'Please enter a value for the Markup value (e.g. 5)'
                            };

                        if (value < limits.markup.min || value > limits.markup.max)
                            return {
                                value: value,
                                isValid: false,
                                error: 'Markup can be between ' + limits.markup.min + ' and ' + limits.markup.max
                            };
                    }
                    return {
                        value: Math.floor(value),
                        isValid: true,
                        error: ''
                    };
                };

                function addClick() {
                    var driveTime = validateDriveTime(controls.driveTime.val()),
                        markup = validateMarkup(controls.markup.val()),
                        result;

                    if (!driveTime.isValid) {
                        controls.driveTime.focus();
                        notify.error(driveTime.error);
                        validateAndShowErrors();
                        return;
                    }

                    if (!markup.isValid) {
                        controls.markup.focus();
                        notify.error(markup.error);
                        validateAndShowErrors();
                        return;
                    }

                    clearInputs();
                    result = addOrUpdate(driveTime.value, markup.value);
                    if (result.found)
                        notify.info('Updated item with Drive-Time: ' + driveTime.value + ' with Markup:' + markup.value);
                    else
                        notify.info('Added Drive-Time: ' + driveTime.value + ' with Markup: ' + markup.value);

                    render(result.index);
                    validateAndShowErrors();
                    callback(events.dataChanged);
                };

                function deleteClick() {
                    var ele = $(this),
                        row = ele.closest('tr'),
                        index = row.index();

                    inspectRow(row);

                    bootbox.confirm({
                        title: 'Delete confirmation',
                        message: 'Are you sure you wish to delete this item for <strong>' + friendlyFuelName + '</strong> ?',
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
                                driveTimeMarkups.splice(index, 1);
                                row.fadeOut(1000, function () { row.remove() });
                                validateAndShowErrors();
                                callback(events.dataChanged);
                            }
                        }
                    });
                };

                function removeAllHighlights() {
                    controls.tbody.find('tr').removeClass('highlight');
                };

                function tableCellClick() {
                    var td = $(this),
                        index = td.index(),
                        row = td.closest('tr');

                    inspectRow(row);
                    switch (index) {
                        case 0:
                            controls.driveTime.focus();
                            break;
                        case 1:
                            controls.markup.focus();
                            break;
                    }
                };

                function inspectRow(row) {
                    removeAllHighlights();

                    var rowIndex = row.index();

                    if (rowIndex < 0 || rowIndex >= driveTimeMarkups.length) {
                        clearInputs();
                        return;
                    }

                    var record = driveTimeMarkups[rowIndex];

                    controls.driveTime.val(record.driveTime);
                    controls.markup.val(record.markup);

                    row.addClass('highlight');
                };

                function driveTimeChange() {
                    validateAndShowErrors();
                };

                function markupChange() {
                    validateAndShowErrors();
                };

                function clearInputs() {
                    controls.driveTime.val('');
                    controls.markup.val('');
                };

                function findControls() {
                    controls.driveTime = panel.find('[data-input="drivetime"]');
                    controls.markup = panel.find('[data-input="markup"]');
                    controls.tbody = panel.find('tbody');
                };

                function bindEvents() {
                    panel.find('[data-action="add"]').off().click(addClick);
                    panel.on('click','[data-action="delete"]', deleteClick);
                    panel.on('click', '.clickable', tableCellClick);
                    controls.driveTime.off().on('change keyup', driveTimeChange);
                    controls.markup.off().on('change keyup', markupChange);
                };

                function extractTemplate(ele) {
                    return $('<div>').append(ele.removeClass('hide').detach()).html();
                };

                function fetchTemplates() {
                    templates.row = extractTemplate(panel.find('[data-id="row-template"]').attr('data-id', null));
                };

                function serialise() {
                    var data = [];
                    $.each(driveTimeMarkups, function (i, item) {
                        data.push({
                            Id: 0,
                            FuelTypeId: fuelTypeId,
                            DriveTime: item.driveTime,
                            Markup: item.markup
                        });
                    });
                    return data;
                };

                function validateAndShowErrors() {
                    isValid();
                    renderErrors();
                };

                function isValid() {
                    var driveTime = validateDriveTime(controls.driveTime.val()),
                        markup = validateMarkup(controls.markup.val()),
                        anyInput = driveTime.value != '' || markup.value != '',
                        lastMarkup;

                    errors.input = anyInput && (!driveTime.isValid || !markup.isValid);
                    errors.empty = driveTimeMarkups.length == 0;

                    errors.unordered = false;
                    $.each(driveTimeMarkups, function (i, item) {
                        if (item.markup < lastMarkup)
                            errors.unordered = true;
                        lastMarkup = item.markup;
                    });

                    return !errors.empty && !errors.unordered && !errors.input;
                };
       
                function populate(items) {
                    fetchTemplates();
                    findControls();

                    $.each(items, function (i, item) {
                        addItem(item.DriveTime, item.Markup);
                    });

                    sortMarkups();
                    render();
                    bindEvents();
                };

                // API
                return {
                    populate: populate,
                    serialise: serialise,
                    isValid: isValid
                };

            })(opts);
        };
        
        // API
        return {
            create: create
        };
    });