define(["jquery", "notify", "bootbox"],
    function ($, notify, bootbox) {

        "use strict";

        var charsPerLine = 50,
            lineHeight = 1.25, //em units
            minEditorLines = 2,
            maxEditorLines = 10,
            placeholder = 'Edit!!';

        function slacker() { };

        var editModes = {
            none: 'none',
            choose: 'choose',
            edit: 'edit'
        };

        var editing = {
            mode: editModes.none,
            container: undefined,
            cssClasses: '',
            originalContent: '',
            item: undefined,
            isPlainText: false,
            options: {
                selector: '',
                save: slacker,
                close: slacker
            }
        };

        var editableTags = {
            'li': {text: 'Bullet point'},
            'p': {text: 'Paragraph'},
            'td': {text: 'Table Row'},
            'th': {text: 'Table Heading Row'},
            'h1': {text: 'Heading 1'},
            'h2': {text: 'Heading 2'},
            'h3': {text: 'Heading 3'},
            'h4': {text: 'Heading 4'},
            'h5': {text: 'Heading 5'},
            'h6': {text: 'Heading 6'}
        };

        var importanceStyles = {
            'importance-normal': {text: 'Normal'},
            'importance-info': {text: 'Information'},
            'importance-warning': {text: 'Warning'},
            'importance-danger': { text: 'Urgent!' }
        };

        var borderOverlays = function() {
            var html = '<div class="edit-in-place-border" />',
                element,
                north = $(html),
                south = $(html),
                east = $(html),
                west = $(html),
                margin = 1,
                visible = false;

            function show(ele, click) {
                element = ele;
                hide();

                $(document.body).append(north).append(west).append(east).append(south);

                moveTo(ele);
                bindEvents(click);

                visible = true;

                $(window.document).scroll(windowScrolled);
            };

            function windowScrolled() {
                setTimeout(redraw, 100);
            };

            function hide() {
                if (visible) {
                    visible = false;
                    north.off().detach();
                    west.off().detach();
                    east.off().detach();
                    south.off().detach();
                }
            };

            function bindEvents(click) {
                north.off().click(click);
                east.off().click(click);
                west.off().click(click);
                south.off().click(click);
            };

            function redraw() {
                if (!element)
                    return;
                moveTo(element);
            };

            function halfRound(value) {
                return Math.floor(value + 0.5);
            };

            function moveTo(ele) {
                if (!ele)
                    hide();

                var eleRect = ele[0].getBoundingClientRect(),
                    bodyRect = document.body.getBoundingClientRect(),
                    x1 = halfRound(eleRect.left - 1),
                    y1 = halfRound(eleRect.top - 1),
                    x2 = halfRound(eleRect.right + 1),
                    y2 = halfRound(eleRect.bottom + 1),
                    win = $(window),
                    winWidth = halfRound(win.width()),
                    winHeight = halfRound(win.height());

                setArea(north, 0, 0, winWidth, y1);
                setArea(west, 0, y1, x1, y2);
                setArea(east, x2, y1, winWidth, y2);
                setArea(south, 0, y2, winWidth, winHeight);
            };

            function setArea(ele, left, top, right, bottom) {
                ele.css({
                    left: left,
                    top: top,
                    width: right - left,
                    height: bottom - top
                });
            };

            // API
            return {
                show: show,
                hide: hide,
                redraw: redraw
            };
        }();

        var controlPanel = function () {
            var element;

            var prompt = $('<div class="edit-in-place-prompt">Please click an item to edit</div>');

            var defaultOptions = {
                container: undefined,
                layout: 'empty',
                canAddElements: false,
                canChangeImportance: false,
                canMoveUpDown: false
            };

            var options = $.extend({}, defaultOptions);

            var events = {
                'helpClick': slacker,
                'moveUp': slacker,
                'moveDown': slacker,
                'importanceChange': slacker,
                'columnLeft': slacker,
                'columnRight': slacker,
                'columnAdd': slacker,
                'columnDelete': slacker,
                'elementAdd': slacker,
                'save': slacker,
                'close': slacker
            };

            var panel = $('<div class="edit-in-place-panel"></div>');

            var markup = {
                lineBreak:
                    '<br />',
                divider: 
                    '<span class="vert-divider">|</span>',
                formStart: 
                    '<div class="form-inline">',
                formEnd: 
                    '</div>',
                help:
                    '<button id="btnEditInPlaceHelp" class="btn btn-success" data-infotip="Open or Close the [b]Help[/b] window"><i class="fa fa-question"></i> Help</button>',
                tokens: 
                    '<label class="navbar-btn">Insert Token:</label>&nbsp;'
                    + '<select class="form-control" id="mnuEditInPlaceToken" data-infotip="Choose the [b]Token[/b] you wish to add"></select>'
                    + '<button id="btnEditInPlaceInsertToken" class="btn btn-primary navbar-btn" data-infotip="Insert the selected [b]{ token }[/b]."><i class="fa fa-tag"></i> Insert</button>',
                moveUp:
                    '<button id="btnEditInPlaceMoveUp" class="btn btn-default btn-xs navbar-btn" data-infotip-dock="below" data-infotip="Move selected item [b]up[/b]"><i class="fa fa-arrow-up"></i></button>',
                moveDown:
                    '<button id="btnEditInPlaceMoveDown" class="btn btn-default btn-xs navbar-btn" data-infotip-dock="below" data-infotip="Move selected item [b]down[/b]"><i class="fa fa-arrow-down"></i></button>',
                importance:
                    '<select class="form-control" id="mnuEditInPlaceImportance" data-infotip-dock="below" data-infotip="Choose the [b]Importance/Style[/b] of the selected item"></select>',
                columns:
                    '<label class="navbar-btn">Columns:</label>&nbsp;'
                    + '<button id="btnEditInPlaceTableColumnLeft" type="button" class="btn btn-default btn-xs navbar-btn" data-infotip-dock="below" data-infotip="Move the selected table column [b]left[/b]"><i class="fa fa-arrow-left"></i></button>'
                    + '<button id="btnEditInPlaceTableColumnRight" type="button" class="btn btn-default btn-xs navbar-btn" data-infotip-dock="below" data-infotip="Move the selected table column [b]right[/b]"><i class="fa fa-arrow-right"></i></button>'
                    + '&nbsp;'
                    + '<button id="btnEditInPlaceAddTableColumn" type="button" class="btn btn-primary btn-xs navbar-btn" data-infotip-dock="below" data-infotip="Add a new table column"><i class="fa fa-plus"></i></button>'
                    + '&nbsp;'
                    + '<button id="btnEditInPlaceDeleteTableColumn" type="button" class="btn btn-danger btn-xs navbar-btn" data-infotip-dock="below" data-infotip="[em]Remove[/em] the selected table column "><i class="fa fa-minus"></i></button>',
                element:
                    '<select class="form-control" id="mnuEditInPlaceElement" data-infotip-dock="below" data-infotip="Choose the [b]type of item[/b] you wish to add"></select>'
                    + '<button id="btnEditInPlaceAddElement" type="button" class="btn btn-primary navbar-btn" data-infotip-dock="below" data-infotip="Adds the new [b]item[/b]"><i class="fa fa-plus"></i> Add</button>',
                save:
                    '<button id="btnEditInPlaceSave" class="btn btn-success navbar-btn" data-infotip-dock="below" data-infotip="Save the changes"><i class="fa fa-check"></i> Save</button>',
                close:
                    '<button id="btnEditInPlaceClose" class="btn btn-danger btn-sm navbar-btn" data-infotip-dock="below" data-infotip="Close the editor"><i class="fa fa-times"></i></button>'
            };

            var layouts = {
                'empty': markup.close,
                'emailSubject': markup.formStart + markup.help + markup.divider + markup.tokens + markup.divider + markup.save + markup.divider + markup.close + markup.formEnd,
                'emailBody': markup.formStart + markup.tokens + markup.divider + markup.element + markup.lineBreak
                    + markup.help + markup.divider + markup.moveUp + markup.moveDown + markup.divider + markup.importance + markup.divider + markup.columns  + markup.divider + markup.save + markup.divider + markup.close + markup.formEnd
            };

            function populateMenu(menu, obj, text) {
                menu.children().remove();
                for (var key in obj) {
                    if (text)
                        menu.append($('<option />').val(key).text(obj[key][text]));
                    else
                        menu.append($('<option />').val(key).text(key));
                }
            };

            function populateStyleMenu() {
                populateMenu($('#mnuEditInPlaceImportance'), importanceStyles, 'text');
            };

            function populateElementMenu() {
                populateMenu($('#mnuEditInPlaceElement'), editableTags, 'text');
            };

            function populateTokensMenu() {
                populateMenu($('#mnuEditInPlaceToken'), editing.options.tokens, 'text');
            };

            function bindEvents() {
                $('#btnEditInPlaceHelp').off().click(function () {
                    events.helpClick();
                });

                $('#btnEditInPlaceInsertToken').off().click(function () {
                    var token = $('#mnuEditInPlaceToken').val(),
                        editor,
                        text;
                    if (element) {
                        editor = element.find('.edit-in-place-editor');
                        if (editor) {
                            text = editor.val();
                            text = text == placeholder
                                ? ''
                                : text + ' '
                            editor.val(text + '{' + token + '} ');
                        }
                    }
                });

                $('#btnEditInPlaceMoveUp').off().click(function () {
                    events.moveUp();
                });
                $('#btnEditInPlaceMoveDown').off().click(function () {
                    events.moveDown();
                });
                $('#mnuEditInPlaceImportance').off().change(function () {
                    events.importanceChange();
                });
                $('#btnEditInPlaceTableColumnLeft').off().click(function () {
                    events.columnLeft();
                });
                $('#btnEditInPlaceTableColumnRight').off().click(function () {
                    events.columnRight();
                });
                $('#btnEditInPlaceAddTableColumn').off().click(function () {
                    events.columnAdd();
                });
                $('#btnEditInPlaceDeleteTableColumn').off().click(function () {
                    events.columnDelete();
                });
                $('#btnEditInPlaceAddElement').off().click(function () {
                    events.elementAdd();
                });
                $('#btnEditInPlaceSave').off().click(function () {
                    events.save();
                });
                $('#btnEditInPlaceClose').off().click(function () {
                    events.close();
                });
            };

            function event(evt, handler) {
                if (evt in events) {
                    events[evt] = $.isFunction(handler)
                        ? handler
                        : slacker;
                } else
                    console.log('Unknown event: ' + evt);
            };

            function resetEvents() {
                for (var key in events)
                    events[key] = slacker;
            };

            function reshow() {
                show(options);
            };

            function show(opts) {
                if (!opts.container) {
                    console.log('No container!');
                    return;
                }

                if (!opts.layout in layouts) {
                    console.log('Layout not found: ' + opts.layout);
                    return;
                }

                options = $.extend({}, defaultOptions, opts);

                var offs = options.container.offset(),
                    conLeft = offs.left,
                    conTop = offs.top,
                    conWidth = options.container.outerWidth(),
                    conHeight = options.container.outerHeight(),
                    conRight = conLeft + conWidth,
                    conBottom = conTop + conHeight,
                    panelWidth,
                    panelHeight;

                prompt.hide();

                panel.html(layouts[opts.layout]);

                panel.appendTo(document.body);

                populateElementMenu();
                populateStyleMenu();
                populateTokensMenu();

                panel.show().css('visibility', 'hidden');

                redrawControls();

                bindEvents();

                setTimeout(function () {
                    var panelWidth = panel.outerWidth(),
                        panelHeight = panel.outerHeight();
                    panel.css({
                        'left': conLeft + conWidth / 2 - panelWidth / 2,
                        'top': conTop - panelHeight,
                        'visibility': 'visible'
                    });
                }, 20);
            };

            function hide() {
                unbind();
                resetEvents();

                panel.hide().off().detach();
                prompt.hide().off().detach();
            };

            function redrawControls() {
                $('#btnEditInPlaceMoveUp').attr('disabled', !options.canMoveUpDown);
                $('#btnEditInPlaceMoveDown').attr('disabled', !options.canMoveUpDown);
                $('#mnuEditInPlaceImportance').attr('disabled', !options.canChangeImportance);
                $('#mnuEditInPlaceElement').attr('disabled', !options.canAddElements);
                $('#btnEditInPlaceAddElement').attr('disabled', !options.canAddElements);
            };

            function bindTo(ele) {
                if (element)
                    unbind();

                element = ele;
                var html = ele.html(),
                    lineCount = Math.floor((charsPerLine - 1 + html.length) / charsPerLine),
                    tag = ele.prop('tagName').toLowerCase(),
                    isTableCell = tag == 'td' || tag == 'th',
                    height = Math.min(maxEditorLines, Math.max(minEditorLines, lineCount));

                juggler.injectTextArea(ele);

                $('#btnEditInPlaceTableColumnLeft').attr('disabled', !isTableCell);
                $('#btnEditInPlaceTableColumnRight').attr('disabled', !isTableCell);
                $('#btnEditInPlaceAddTableColumn').attr('disabled', !isTableCell);
                $('#btnEditInPlaceDeleteTableColumn').attr('disabled', !isTableCell);

                $('#txtEditInPlace').css('height', (height * lineHeight) + 'em')
                    .focus()
                    .off().click(function (event) {
                        event.stopImmediatePropagation();
                    });

                setNewItemTag(tag);
            };

            function unbind() {
                if (!element)
                    return;
                var editor = element.find('.edit-in-place-editor'),
                    html = editor.val();

                if (/\S/.test(html)) {
                    element.html(html);
                } else
                    juggler.removeEmptyElement(element);
                element = undefined;
            };

            function getImportance() {
                return $('#mnuEditInPlaceImportance').val();
            };

            function setImportance(value) {
                return $('#mnuEditInPlaceImportance').val(value);
            };

            function getNewItemTag() {
                return $('#mnuEditInPlaceElement').val();
            };

            function setNewItemTag(tag) {
                $('#mnuEditInPlaceElement').val(tag);
            };

            function showPrompt(container, message) {
                var offs = container.offset(),
                    conLeft = offs.left,
                    conTop = offs.top,
                    conWidth = container.outerWidth(),
                    conHeight = container.outerHeight();

                prompt.html(message).appendTo(document.body);

                prompt.css({
                    left: conLeft + conWidth / 2 - prompt.outerWidth() / 2,
                    top: conTop - prompt.outerHeight()
                });
                prompt.show();
            };

            function hidePrompt() {
                prompt.hide();
            };

           
            // API
            return {
                show: show,
                reshow: reshow,
                hide: hide,
                bindTo: bindTo,
                unbind: unbind,
                event: event,
                resetEvents: resetEvents,
                getImportance: getImportance,
                setImportance: setImportance,
                getNewItemTag: getNewItemTag,
                setNewItemTag: setNewItemTag,
                showPrompt: showPrompt,
                hidePrompt: hidePrompt
            }
        }();

        //
        // Dom manipulation helper
        //
        var juggler = function () {

            function createElement(tag) {
                return $('<' + tag + '>').html(placeholder);
            };

            function getTag(ele) {
                if (!ele)
                    return 'NULL';
                return ele.prop('tagName').toLowerCase();
            };

            function isTableCell(tag) {
                return tag == 'td' || tag == 'td';
            };

            function findBaseElement(ele) {
                var tag = getTag(ele);
                switch (tag) {
                    case 'li':
                        return ele.closest('ul,ol');
                    case 'td':
                    case 'th':
                        return ele.closest('table');
                    default:
                        return ele;
                }
            };

            function swapElements(first, second) {
                if (!first || !second)
                    return;
                var temp = $('<div />').insertAfter(first);
                first.insertAfter(second);
                second.insertBefore(temp);
                temp.remove();
            };

            function swapSibling(ele, up) {
                if (!ele)
                    return;
                var tag = getTag(ele),
                    sibling;

                switch (tag) {
                    case 'li':
                        sibling = up ? ele.prev('li') : ele.next('li');
                        swapElements(ele, sibling);
                        break;
                    case 'td':
                    case 'th':
                        sibling = up ? ele.prev('tr') : ele.next('tr');
                        ele = ele.closest('tr');
                        swapElements(ele.closest('tr'), sibling);
                        break;
                    case 'p':
                    case 'h1':
                    case 'h2':
                    case 'h3':
                    case 'h4':
                    case 'h5':
                    case 'h6':
                        sibling = up ? ele.prev() : ele.next();
                        swapElements(ele, sibling);
                        break;
                }
            };

            function reorderCell(ele, delta) {
                if (!ele)
                    return;

                var tag = getTag(ele);
                if (!isTableCell(tag))
                    return;

                var table = ele.closest('table'),
                    rows = table.find('tr'),
                    cells,
                    cellCount = $(rows[0]).find('td,th').length,
                    srcIndex = ele.index(),
                    dstIndex = srcIndex + delta,
                    i;

                if (cellCount < 2 || dstIndex < 0 || dstIndex >= cellCount)
                    return;

                for (i = 0; i < rows.length; i++) {
                    cells = $(rows[i]).find('th,td');
                    swapElements(cells.eq(srcIndex), cells.eq(dstIndex));
                }
            };

            function moveUp(ele) {
                swapSibling(ele, true);
            };

            function moveDown(ele) {
                swapSibling(ele, false);
            };

            function moveCellLeft(ele) {
                reorderCell(ele, -1);
            };

            function moveCellRight(ele) {
                reorderCell(ele, 1);
            };

            function addTag(dstTag, ele) {
                if (!ele) {
                    console.log('Ele is null');
                    return;
                }

                var newEle = createElement(dstTag),
                    srcTag = getTag(ele),
                    list,
                    base,
                    container,
                    row,
                    index,
                    count,
                    table,
                    i;

                switch (dstTag) {
                    case 'li':
                        if (srcTag == 'li') {
                            newEle.insertAfter(ele);
                        } else {
                            base = findBaseElement(ele);
                            container = $('<ul>');
                            container.append(newEle);
                            container.insertAfter(base);
                        }
                        break;
                    case 'th':
                    case 'td':
                        if (isTableCell(srcTag)) {
                            index = ele.index();
                            base = ele.closest('tr');
                            count = base.find('td,th').length;
                            row = $('<tr>');
                            row.insertAfter(base);
                            for (i = 0; i < count; i++) {
                                row.append('<' + dstTag + '>');
                            }
                            newEle = row.find(dstTag).eq(index);
                            newEle.html(placeholder);
                        } else {
                            base = findBaseElement(ele);
                            table = $('<table>');
                            row = $('<tr>');
                            table.append(row);
                            row.append(newEle);
                            table.insertAfter(base);
                        }
                        break;
                    default:
                        base = findBaseElement(ele);
                        base.after(newEle);
                        break;
                }
                return newEle;
            };

            function deleteTag(ele) {
                if (!ele)
                    return;
                var tag = getTag(ele);
                switch (tag) {
                    case 'li':
                        ele.remove();
                        break;
                    case 'th':
                    case 'td':
                        ele.parent().remove();
                        break;
                    default:
                        ele.remove();
                        break;
                }
            };

            function removeEmptyElement(ele) {
                if (!ele)
                    return;

                var tag = getTag(ele),
                    container,
                    count;
                switch (tag) {
                    case 'li':
                        container = ele.closest('ul,ol');
                        ele.remove();
                        count = container.find('li').length;
                        if (count == 0)
                            container.remove();
                        break;
                    case 'th':
                    case 'td':
                        // do not remove
                        break;
                    case 'p':
                    case 'h1':
                    case 'h2':
                    case 'h3':
                    case 'h4':
                    case 'h5':
                    case 'h6':
                        ele.remove();
                        break;
                }
            };

            function removeTableColumn(ele) {
                if (!ele)
                    return;

                var tag = ele.prop('tagName').toLowerCase(),
                    table = ele.closest('table'),
                    rows = table.find('tr'),
                    index = ele.index(),
                    count = $(rows[0]).find('th,td').length,
                    i;

                if (count < 2) {
                    table.remove();
                } else {
                    for (i = 0; i < rows.length; i++) {
                        $(rows[i]).find('th,td').eq(index).remove();
                    }
                }
            };

            function addTableColumn(tag, ele) {
                if (!ele)
                    return;

                var index = ele.index(),
                    rows,
                    count,
                    cell,
                    i;

                rows = ele.closest('table').find('tr');
                count = rows.length;
                for (i = 0; i < count; i++) {
                    cell = $(rows[i]).find('td,th').eq(index);
                    $('<' + tag + '>').text(placeholder).insertAfter(cell);
                }
                return ele.next(tag);
            };

            function injectTextArea(ele) {
                if (!ele)
                    return;
                var html = ele.html();
                ele.html('<textarea id="txtEditInPlace" class="edit-in-place-editor">' + html + '</textarea>');
            };

            function removeTextArea(ele) {
                if (!ele)
                    return;
                var editor = ele.find('.edit-in-place-editor'),
                    html = editor.val();
                ele.html(html);
            };

            // API
            return {
                addTag: addTag,
                moveUp: moveUp,
                moveDown: moveDown,
                moveCellLeft: moveCellLeft,
                moveCellRight: moveCellRight,
                deleteTag: deleteTag,
                removeEmptyElement: removeEmptyElement,
                removeTableColumn: removeTableColumn,
                addTableColumn: addTableColumn,
                injectTextArea: injectTextArea,
                removeTextArea: removeTextArea
            };
        }();

        function chooseBorderClick() {
            var action = function () {
                discardAndClose();
            };
            confirmUnsavedChanges(action);
        };

        function editBorderClick() {
            if (!editing.isPlainText)
                enterChooseMode();
        };

        function closeEditor() {
            if (editing.container) {
                editing.container.removeClass('editing-email-field', 'edit-in-place-choose', 'edit-in-place-editing')
                    .addClass('editable-email-field');
            }
            controlPanel.hide();
            deselectItem();
            borderOverlays.hide();

            editing.item = undefined;
            editing.isPlainText = false;
        };

        function deselectItem() {
            controlPanel.hide();
            if (editing.item)
                editing.item.removeClass('edit-in-place-item');
            editing.item = undefined;
        };

        function editItem(ele) {
            if (editing.item == ele)
                return;

            // already editing this ele ?
            if (ele.find('textarea').length)
                return;

            var isPlainText = editing.isPlainText;

            deselectItem();

            editing.item = ele;

            ele.addClass('edit-in-place-item');

            var opts = {
                container: editing.container,
                layout: editing.options.layout,
                canAddElements: !editing.isPlainText,
                canChangeImportance: !editing.isPlainText,
                canMoveUpDown: !editing.isPlainText
            };

            controlPanel.show(opts);
            controlPanel.bindTo(ele);
            controlPanel.setImportance(extractImportance(ele));
            bindControlEvents();
            setTimeout(borderOverlays.redraw, 10);
        };

        function extractImportance(ele) {
            for (var key in importanceStyles) {
                if (ele.hasClass(key))
                    return key
            }
            return 'importance-normal';
        };

        function bindControlEvents() {
            controlPanel.event('helpClick', function () {
                editing.options.helpClick();
            });

            controlPanel.event('moveUp', function () {
                juggler.moveUp(editing.item);
            });
            controlPanel.event('moveDown', function () {
                juggler.moveDown(editing.item);
            });
            controlPanel.event('importanceChange', function () {
                var importance = controlPanel.getImportance(),
                    key,
                    ele = editing.item;
                if (!ele)
                    return;
                for (key in importanceStyles) {
                    ele.removeClass(key);
                }
                ele.addClass(importance);
            });
            controlPanel.event('columnLeft', function () {
                juggler.moveCellLeft(editing.item);
            });
            controlPanel.event('columnRight', function () {
                juggler.moveCellRight(editing.item);
            });
            controlPanel.event('columnAdd', function () {
                var item = editing.item,
                    ele;

                deselectItem();
                ele = juggler.addTableColumn('td', item);
                if (ele) {
                    focusOnNewElement(ele, "Added a new table column");
                } else 
                    notify.error('Unable to add table column');
            });
            controlPanel.event('columnDelete', function () {
                var ele = editing.item;

                if (!ele)
                    return;

                bootbox.confirm({
                    title: '<i class="fa fa-warning"></i> Delete Confirmation',
                    message: 'Are you sure you want to remove this table column?',
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
                            deselectItem();
                            juggler.removeTableColumn(ele);
                        }
                    }
                });
            });
            controlPanel.event('elementAdd', function () {
                var newTag = controlPanel.getNewItemTag(),
                    text = editableTags[newTag].text,
                    item = editing.item,
                    ele;
                
                deselectItem();
                
                ele = juggler.addTag(newTag, item);

                if (ele) {
                    controlPanel.reshow();
                    openEditor(ele);
                    //focusOnNewElement(ele, "Added a new " + text);
                } else {
                    notify.error('Unable to add a new ' + text);
                }
            });
            controlPanel.event('save', function () {
                saveAndClose();
            });
            controlPanel.event('close', function () {
                confirmUnsavedChanges(discardAndClose);
            });
        };

        function discardAndClose() {
            var content = editing.originalContent,
                callback = editing.options.close;

            destroy();
          
            callback(content);
        };

        function saveAndClose() {
            var content = getContentWithoutEditor(),
                callback = editing.options.save;

            destroy();

            callback(content);
        };

        function confirmUnsavedChanges(action) {
            if (hasUnsavedChanges()) {
                bootbox.confirm({
                    title: '<i class="fa fa-warning"></i> Close Confirmation',
                    message: 'Do you wish to close and <strong>lose any unsaved changes<?strong>?',
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
                            action();
                        }
                    }
                });
            } else {
                action();
            }
        };

        function focusOnNewElement(ele, message) {
            controlPanel.bindTo(ele);
            editing.item = ele;
            notify.success(message);
            borderOverlays.redraw();
        };

        function containerClick() {
            if (!editing.isPlainText) {
                enterChooseMode();
            }
        };

        function getContentWithoutEditor() {
            var content = '',
                ele = editing.item,
                cleaned = $('<div />'),
                items;

            if (ele) {
                juggler.removeTextArea(ele);
                content = editing.container.html();
                juggler.injectTextArea(ele)
            } else {
                content = editing.container.html();
            }
            cleaned.html(content);
            cleaned.find('.edit-in-place-item').removeClass('edit-in-place-item');
            return cleaned.html();
        };

        function hasUnsavedChanges() {
            var content = getContentWithoutEditor();
            return content != editing.originalContent;
        };

        function chooseClick(event) {
            if (editing.mode != editModes.choose)
                return;

            var ele = $(this),
                tag;
            while (ele.length) {
                tag = ele.prop('tagName').toLowerCase();

                if (tag in editableTags) {
                    event.stopImmediatePropagation();
                    enterEditMode(ele);
                    break;
                }
                ele = ele.parent();
            }
        };

        function openEditor(ele) {
            editing.container.removeClass('edit-in-place-choose').addClass('edit-in-place-editing');
            editItem(ele);
        };

        function destroy() {
            var container = editing.container;

            if (container) {
                unbindContainerEvents(container);
                container.attr('class', editing.cssClasses);
                borderOverlays.hide();
                closeEditor();
                controlPanel.hide();
                controlPanel.hidePrompt();
            }
            editing.container = undefined;
            editing.cssClasses = '';
            editing.item = undefined;
            editing.tag = undefined;
            editing.options = undefined;
            editing.isPlainText = false;
            editing.mode = editModes.none;
        };

        function unbindContainerEvents(container) {
            container.off('click', chooseClick);
        };

        function enterChooseMode() {
            editing.mode = editModes.choose;

            var container = editing.container;

            deselectItem();

            container.addClass('edit-in-place-choose');
            unbindContainerEvents(container);
            container.on('click', 'li,p,td,th,h1,h2,h3,h4,h5,h6', chooseClick);

            controlPanel.hide();
            borderOverlays.show(container, chooseBorderClick);
            controlPanel.showPrompt(container, "Please click an item to edit...");
        };

        function enterEditMode(ele) {
            editing.mode = editModes.edit;

            var container = editing.isPlainText
                ? ele
                : editing.container;

            unbindContainerEvents(container);
            controlPanel.hide();
            controlPanel.hidePrompt();
            borderOverlays.show(container, editBorderClick);
            openEditor(ele);

            if (!editing.isPlainText) {
                unbindContainerEvents(container);
                container.click(function () {
                    unbindContainerEvents(container);
                    enterChooseMode();
                });
            }
        };

        function edit(opts) {
            if (!opts.selector) {
                console.log('No selector!');
                return;
            }
            var ele = $(opts.selector);
            if (!ele.length) {
                console.log('Unable to find selector: ' + opts.selector);
                return;
            }
            editing.container = ele;
            editing.cssClasses = ele.attr('class');
            editing.originalContent = ele.html();
            editing.item = undefined;
            editing.tag = undefined;
            editing.options = opts;
            editing.isPlainText = ele.find('li,p,td,th,h1,h2,h3,h4,h5,h6').length == 0;

            if (editing.isPlainText) {
                enterEditMode(ele);
            } else {
                enterChooseMode();
            }
        };

        // API
        return {
            edit: edit
        };
    }
);
