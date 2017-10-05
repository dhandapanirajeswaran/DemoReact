define(["jquery", "waiter", "notify", "validation", "PetrolPricingService", "text!App/JsPriceOverrides.html"],
    function ($, waiter, notify, validation, petrolPricingService ,modalHtml) {
        "use strict";

        var modal = $(modalHtml);
        var templates = {
            row: undefined,
            empty: undefined
        };

        var priceOverridesModel = {
            siteIds: [],
            siteModels: {}
        };

        var callbacks = {
            getPrices: function () { },
            setOverride: function() {}
        };

        var state = {
            includeUnleaded: true,
            includeDiesel: true,
            includeSuperUnleaded: true
        };

        var selectors = {
            includeUnleaded: '[data-id="include-unleaded"]',
            includeDiesel: '[data-id="include-diesel"]',
            includeSuperUnleaded: '[data-id="include-super-unleaded"]',
            applyButton: '[data-id="apply-price-overrides"]'
        };

        function extractTemplates() {
            var row,
                key,
                ele,
                mapping = {
                    'row': 'row-template',
                    'empty': 'row-empty-template'
                };

            for (key in mapping) {
                row = modal.find('[jq-template-id="' + mapping[key] + '"]').attr('jq-template-id', null)[0].outerHTML;
                templates[key] = row.replace(/\s+/, ' ');
            }
        };

        function redrawIncludeButtons() {
            redrawRadioControl(selectors.includeUnleaded, state.includeUnleaded);
            redrawRadioControl(selectors.includeDiesel, state.includeDiesel);
            redrawRadioControl(selectors.includeSuperUnleaded, state.includeSuperUnleaded);
        };

        function redrawRadioControl(selector, isChecked) {
            var checkbox = modal.find(selector),
                label = checkbox.closest('.radio-label');

            checkbox.attr('checked', isChecked);
            if (isChecked) 
                label.removeClass('radio-unchecked').addClass('radio-checked');
            else
                label.removeClass('radio-checked').addClass('radio-unchecked');
        };

        function redrawJsPriceOverrides(model) {
            var table = modal.find('#JsPriceOverridesTable'),
                tbody = table.find('tbody'),
                html = [],
                siteModel,
                row,
                tokens,
                prices,
                siteId,
                i;
            tbody.children().remove();

            if (model.siteIds.length == 0) {
                html.push(templates.empty);
                modal.find('tfoot').hide();
                modal.find(selectors.applyButton).hide();
            }

            for (i = 0; i < model.siteIds.length; i++) {
                siteId = model.siteIds[i];
                siteModel = model.siteModels[siteId];

                tokens = {
                    '{CATNO}': siteModel.catNo,
                    '{SITENAME}': siteModel.siteName,
                    '{SITEID}': siteModel.siteId,
                    '{UNLEADED-INC}': siteModel.unleaded.increaseHtml,
                    '{UNLEADED-ABS}': siteModel.unleaded.absoluteHtml,
                    '{UNLEADED-PREVIEW}': siteModel.unleaded.previewHtml,
                    '{DIESEL-INC}': siteModel.diesel.increaseHtml,
                    '{DIESEL-ABS}': siteModel.diesel.absoluteHtml,
                    '{DIESEL-PREVIEW}': siteModel.diesel.previewHtml,
                    '{SUPER-UNLEADED-INC}': siteModel.superUnleaded.increaseHtml,
                    '{SUPER-UNLEADED-ABS}': siteModel.superUnleaded.absoluteHtml,
                    '{SUPER-UNLEADED-PREVIEW}': siteModel.superUnleaded.previewHtml
                };

                row = replaceTokens(templates.row, tokens);

                html.push(row);
            }

            tbody.append(html.join(''));
        };

        function buildSiteModel(todayPrice, increase, absolute) {
            var obj = {
                todayPrice: validation.isNonZeroNumber(todayPrice) ? Number(todayPrice) / 10 : 0,
                todayPriceIsValid: validation.isNonZeroNumber(todayPrice),
                increaseValue: validation.isNumber(increase) ? Number(increase) / 10 : 0,
                increaseIsValid: validation.isNumber(increase),
                increaseHtml: '&mdash;',
                absoluteValue: validation.isNonZeroNumber(absolute) ? Number(absolute / 10) : 0,
                absoluteIsValid: validation.isNonZeroNumber(absolute),
                absoluteHtml: '&mdash;',
                previewValue: 0,
                previewHtml: '&mdash;'
            },
            parts;

            if (obj.increaseIsValid && obj.todayPriceIsValid) {
                obj.increaseHtml = formatIncreaseHtml(obj.increaseValue);
                obj.previewValue = (obj.increaseValue + obj.todayPrice).toFixed(1);
                obj.previewHtml = formatForecourtPrice(obj.previewValue);
            }

            if (obj.absoluteIsValid) {
                obj.absoluteHtml = formatForecourtPrice(obj.absoluteValue);
                obj.previewValue = obj.absoluteValue.toFixed(1);
                obj.previewHtml = formatForecourtPrice(obj.absoluteValue);
            }

            return obj;
        };

        function formatForecourtPrice(price) {
            var parts = (Number(price).toFixed(1) + '.0').split('.');
            return '<big>' + parts[0] + '</big><small>.' + parts[1] + '</small>';
        };

        function formatIncreaseHtml(increase) {
            if (increase < 0)
                return '-' + (Math.abs(increase) / 10).toFixed(1);
            return increase == 0
                ? '0.0'
                : '+' + (Math.abs(increase) / 10).toFixed(1);
        };

        function formatAbsoluteHtml(absolute) {
            return absolute.toFixed(1);
        };
       
        function toActualPrice(modalPrice) {
            return (modalPrice / 10).toFixed(1);
        };

        function replaceTokens(template, tokens) {
            var formatted = template + '',
                key;
            for (key in tokens) {
                formatted = formatted.replace(key, tokens[key]);
            }
            return formatted;
        };

        function buildDataModel(items) {
            var siteIds = [],
                item,
                i,
                prices,
                price,
                siteModels = {},
                siteId,
                siteModel,
                unleadedToday,
                dieselToday,
                superUnleadedToday;
            
            for (i = 0; i < items.length; i++) {
                siteIds.push(items[i].SiteId);
            }
            // communicate with main pricing page for sites Today/Override prices...
            prices = callbacks.getPrices(siteIds);

            for (i = 0; i < items.length; i++) {
                item = items[i];
                siteId = item.SiteId;

                if (siteId in prices) {
                    price = prices[siteId];
                    unleadedToday = price.unleaded.today;
                    dieselToday = price.diesel.today;
                    superUnleadedToday = price.superUnleaded.today;
                } else {
                    unleadedToday = 0;
                    dieselToday = 0;
                    superUnleadedToday = 0;
                }

                siteModel = {
                    siteId: siteId,
                    siteName: item.SiteName,
                    catNo: item.CatNo,
                    unleaded: buildSiteModel(unleadedToday, item.UnleadedIncrease, item.UnleadedAbsolute),
                    diesel: buildSiteModel(dieselToday, item.DieselIncrease, item.DieselAbsolute),
                    superUnleaded: buildSiteModel(superUnleadedToday, item.SuperUnleadedIncrease, item.SuperUnleadedAbsolute)
                };

                siteModels[siteId] = siteModel;
            }
            return {
                siteIds: siteIds,
                siteModels: siteModels
            };
        };

        function detectFileUpload(fileUploadId) {
            if (!fileUploadId || Number(fileUploadId) == 0)
                return;

            waiter.show({
                title: 'Js Price Overrides',
                message: 'Detecting JS Price Overrides...',
                icon: 'clock'
            });

            function failure() {
                waiter.hide();
                notify.info('No JS Price Override file was uploaded.')
            };

            function success(data) {
                waiter.hide();
                if (!data || data.ErrorMessage)
                    return failure();

                priceOverridesModel = buildDataModel(data.Items);

                redrawIncludeButtons();

                modal.modal('show');
                redrawJsPriceOverrides(priceOverridesModel);
            };

            petrolPricingService.getJsPriceOverrides(success, failure, fileUploadId);
        };


        function applyClick() {
            var i,
                siteId,
                site,
                model = priceOverridesModel,
                count = 0,
                fuels = [],
                opts = {
                    unleaded: !!modal.find(selectors.includeUnleaded).is(':checked'),
                    diesel: !!modal.find(selectors.includeDiesel).is(':checked'),
                    superUnleaded: !!modal.find(selectors.includeSuperUnleaded).is(':checked')
                };

            if (opts.unleaded)
                fuels.push('Unleaded');
            if (opts.diesel)
                fuels.push('Diesel');
            if (opts.superUnleaded)
                fuels.push('Super-Unleaded');

            if (!fuels.length) {
                notify.warning('Please select one or more Fuel Grades - or press [Close] to ignore Price Overrides');
                return;
            }

            for (i = 0; i < model.siteIds.length; i++) {
                siteId = model.siteIds[i];
                site = model.siteModels[siteId];

                if (opts.unleaded && validation.isNonZeroNumber(site.unleaded.previewValue)) {
                    callbacks.setOverride(siteId, 2, site.unleaded.previewValue);
                    count++;
                }

                if (opts.diesel && validation.isNonZeroNumber(site.diesel.previewValue)) {
                    callbacks.setOverride(siteId, 6, site.diesel.previewValue);
                    count++;
                }

                if (opts.superUnleaded && validation.isNonZeroNumber(site.superUnleaded.previewValue)) {
                    callbacks.setOverride(siteId, 1, site.superUnleaded.previewValue);
                    count++;
                }
            }

            notify.success('Updated ' + count + ' Price Overrides for ' + fuels.join(' and '));
            hide();
        };

        function includeUnleadedClick() {
            state.includeUnleaded = $(this).is(':checked');
            redrawIncludeButtons();
        };

        function includeDieselClick() {
            state.includeDiesel = $(this).is(':checked');
            redrawIncludeButtons();
        };

        function includeSuperUnleadedClick() {
            state.includeSuperUnleaded = $(this).is(':checked');
            redrawIncludeButtons();
        };

        function hide() {
            modal.modal('hide');
        };

        function injectDom() {
            modal.hide().appendTo(document.body);
        };

        function bind(actions) {
            callbacks = actions;
        };

        function bindEvents() {
            modal.find(selectors.applyButton).off().click(applyClick);
            modal.find(selectors.includeUnleaded).off().click(includeUnleadedClick);
            modal.find(selectors.includeDiesel).off().click(includeDieselClick);
            modal.find(selectors.includeSuperUnleaded).off().click(includeSuperUnleadedClick);
        };

        function docReady() {
            extractTemplates();
            injectDom();
            bindEvents();
        };

        $(docReady);

        // API
        return {
            bind: bind,
            detectFileUpload: detectFileUpload
        };
    }
);