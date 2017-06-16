define(["jquery"],
    function ($) {
        "use strict";

        function extractTemplate(ele) {
            ele.attr('data-template', null);
            return $('<div>').append(ele.detach()).html();
        };

        function replaceTokens(template, tokens) {
            var key,
                html = '' + template;
            for (var key in tokens) {
                html = html.split(key).join(tokens[key]);
            }
            return html;
        };

        // API
        return {
            extractTemplate: extractTemplate,
            replaceTokens: replaceTokens
        };
    }
);