define(["jquery", "infotips", "text!App/ErrorCatcher.html"],
    function ($, infotips, modalHtml) {
        "use strict";

        var trigger = $('<div id="ErrorCatcherTrigger" class="btn btn-danger" data-infotip-dock="below" data-infotip="Show the Error"><i class="fa fa-warning fa-2x"></i> <br /> Error</div>')
        var modal = $(modalHtml);

        var lastError = {
            message: '',
            stack: ''
        };

        function injectDom() {
            trigger.hide().appendTo(document.body);
            modal.hide().appendTo(document.body);
        };

        function show() {
            var text = 'Error: ' + lastError.message + '\n' + lastError.stack;
            modal.find('[data-id="error-message"]').val(text);
            modal.modal('show');
        };

        function hide() {
            modal.modal('hide');
        };

        function bindEvents() {
            $('#ErrorCatcherTrigger').off().click(show);
        };

        function docReady() {
            injectDom();
            bindEvents();
        };

        $(docReady);

        window.addEventListener('error', function (e) {
            lastError.message = e.error.toString();
            lastError.stack = e.error.stack ? e.error.stack : '';
            trigger.fadeIn(1000);
        });

        // API
        return {
            show: show,
            hide: hide
        };
    }
);