define(['jquery', 'cookieSettings'],
    function ($, cookieSettings) {

        var currentBuildDate = '',
            overlay,
            markup = '<div class="build-notification">'
            + '<i class"fa fa-info"></i> A new build was deployed onto this website on {CURRENTBUILDDATE}'
            + '<button id="btnAcknowledgeNewBuild" type="button"><i class="fa fa-times"></i> Close</button>'
            + '</div>';

        var animations = {
            'show': { 'bottom': '0' },
            'hide': { 'bottom': '-4em' }
        };

        function showBuildChangedNotification(buildDate) {
            currentBuildDate = buildDate;
            overlay = $(markup.replace(/\{CURRENTBUILDDATE\}/ig, currentBuildDate));
            overlay.css(animations.hide);
            $('body').append(overlay);
            overlay.find('#btnAcknowledgeNewBuild').off().click(acknowledgeNewBuild);
            overlay.animate(animations.show, 2000);
        };

        function acknowledgeNewBuild() {
            cookieSettings.write('build.date', currentBuildDate);
            overlay.animate(animations.hide, 1000, function () {
                overlay.hide();
            });
        };

        function init(buildDate) {
            var lastSeenBuild = cookieSettings.read('build.date', '');
            if (buildDate != lastSeenBuild)
                showBuildChangedNotification(buildDate);
        };

        // API
        return {
            init: init
        };
    }
);