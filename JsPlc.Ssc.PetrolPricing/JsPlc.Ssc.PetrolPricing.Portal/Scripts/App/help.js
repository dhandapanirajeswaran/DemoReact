define(["jquery", "common", "cookieSettings", "text!App/Help.html"],
    function ($, common, cookieSettings, helpHtml) {

        var state = {
            page: '',
            isOpen: false
        };

        var helpPaths = {
            'help/faq': ['/'],
            'help/faq-file-uploads': [
                '/file',
                '/file/upload',
                '/file/details'
            ],
            'help/faq-site-pricing': [
                '/sites/prices'
            ],
            'help/faq-site-management': [
                '/sites',
                '/sites/create',
            ],
            '/help/faq-site-emails': [
                'sites/siteemails'
            ],
            'help/faq-reports': [
                '/pricereports',
            ],
            'help/faq-reports#CompetitorSites': [
                '/pricereports/competitorsites'
            ],
            'help/faq-reports#PricePoints': [
                '/pricereports/pricepoints'
            ],
            'help/faq-reports#NationalAverage': [
                '/pricereports/nationalaverage',
            ],
            'help/faq-reports#NationalAverage2': [
                '/pricereports/nationalaverage2',
            ],
            'help/faq-reports#PriceMovement': [
                '/pricereports/pricemovement',
            ],
            'help/faq-reports#ComplianceCheck': [
                '/pricereports/compliance',
            ],
            'help/faq-reports#CompetitorPriceRange': [
                '/pricereports/competitorspricerange',
            ],
            'help/faq-reports#CompetitorPriceRangeByCompany': [
                '/pricereports/competitorspricerangebycompany',
            ],
            'help/faq-reports#QuarterlySiteAnalysis': [
                '/pricereports/quarterlysiteanalysis',
            ],
            'help/faq-reports#LastSitePrices': [
                '/pricereports/lastsiteprices',
            ],
            'help/faq-settings': [
                '/settings'
            ],
            'help/faq-settings#DriveTime': [
                '/settings/drivetime'
            ],
            'help/faq-settings#BrandsGrocers': [
                '/settings/brands'
            ],
            'help/faq-settings#EmailSchedule': [
                '/settings/schedule'
            ],
            'help/faq-settings#PriceFreezeEvents': [
                '/settings/pricefreeze'
            ],
            'help/faq-user-management': [
                '/ppusers'
            ]
        };

        var helpWindow = $(helpHtml),
            helpLoading = helpWindow.find('.loading');

        function show(page) {
            var hash;
            page = page || cookieSettings.read('help.lastPage', 'index');

            if (!/^help\//i.test(page)) {
                console.log('Unable to open Help page: ' + page);
                return;
            }

            if (page.indexOf('#') != -1) {
                hash = '#' + page.split('#')[1];
                page = page.split('#')[0];
            }

            // already on page ?
            if (state.isOpen && state.page == page) {
                closeClick();
                return;
            }

            // open animation
            if (!state.isOpen) {
                helpWindow.fadeIn(1000);
                helpLoading.show();
                state.isOpen = true;
                cookieSettings.writeBoolean('help.isOpen', true);
            }

            // load the help page
            var url = common.getRootSiteFolder() + page;

            if (!/.htm/i.test(page))
                url += '.html';

            $.get(url, function (data) {
                helpWindow.find('.help-page').replaceWith(data);
                helpLoading.hide();
                state.page = page;
                cookieSettings.write('help.lastPage', page);

                setTimeout(function () {
                    scrollToHash(hash);
                }, 500);
            });
        };

        function hide() {
            state.isOpen = false;
            helpWindow.hide();
            cookieSettings.writeBoolean('help.isOpen', false);
        };

        function closeClick() {
            state.isOpen = false;
            helpWindow.fadeOut(500);
            cookieSettings.writeBoolean('help.isOpen', false);
        };

        function homeClick() {
            show('help/index');
        };

        function faqClick() {
            show('help/faq');
        };

        function uploadsClick() {
            show('help/faq-file-uploads');
        };
        function pricingClick() {
            show('help/faq-site-pricing');
        };

        function settingsClick() { 
            show('help/faq-settings');
        };

        function glossaryClick() {
            show('help/faq-glossary');
        };

        function helpButtonClick() {
            var pathname = window.location.pathname.replace(/\/petrolpricing\//i, '/').toLowerCase(),
                pathname = pathname.replace(/\/\d+/, ''),
                page = findHelpPageForPath(pathname);
            show(page);
        };

        function findHelpPageForPath(pathname) {
            var key,
                paths,
                i;
            for (key in helpPaths) {
                paths = helpPaths[key];
                for (i = 0; i < paths.length; i++) {
                    if (paths[i] == pathname) {
                        return key;
                    }
                }
            }
            return '';
        }

        function topOfPageClick() {
            helpWindow.find('.help-page').animate({ "scrollTop": "0px" }, 500);
        };

        function linkClick(ev) {
            var link = $(this),
                href = link.attr('href'),
                ele;

            ev.preventDefault();

            if (href[0] == '#') {
                scrollToHash(href);
            }
            else if (/^help/i.test(href)) {
                show(href);
            } else {
                window.location = common.getRootSiteFolder() + href;
            }
        };

        function scrollToHash(hash) {
            var ele = $(hash);
            if (!ele.length)
                return;

            helpWindow.find('.help-page').animate({ "scrollTop": (ele.offset().top - 10) + 'px' }, 500, function () {
                ele.addClass('ping-yellow');
            });
        };

        function injectDom() {
            helpWindow.hide().appendTo(document.body);
        };

        function bindEvents() {
            $('#navHelpButton').off().click(helpButtonClick);
            helpWindow.off().on('click', 'a', linkClick);
            helpWindow.on('click', '[data-click="help-close"]', closeClick);
            helpWindow.on('click', '[data-click="help-home"]', homeClick);
            helpWindow.on('click', '[data-click="help-faq"]', faqClick);
            helpWindow.on('click', '[data-click="help-uploads"]', uploadsClick);
            helpWindow.on('click', '[data-click="help-pricing"]', pricingClick);
            helpWindow.on('click', '[data-click="help-settings"]', settingsClick);
            helpWindow.on('click', '[data-click="help-glossary"]', glossaryClick);
            helpWindow.on('click', '[data-click="help-top-of-page"]', topOfPageClick);
        };

        function docReady() {
            injectDom();
            bindEvents();
            if (cookieSettings.readBoolean('help.isOpen', false))
                show();
        };

        $(docReady);

        // API
        return {
            show: show,
            hide: hide
        };
    }
);