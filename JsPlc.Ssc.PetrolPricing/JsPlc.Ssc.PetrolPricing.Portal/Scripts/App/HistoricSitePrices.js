define(["jquery", "waiter", "notify", "PriceReasons", "text!App/HistoricSitePrices.html"],
    function ($, waiter, notify, priceReasons, modalHtml) {
        "use strict";

        var modal = $(modalHtml),
            lightbox = $('<div class="lightbox-overlay"></div>');

        var weekdays = [
            'Sun',
            'Mon',
            'Tue',
            'Wed',
            'Thu',
            'Fri',
            'Sat'
        ];

        function show(storeName, data, today) {
            modal.modal('show');
            modal.find('.site-name').text(storeName);
            modal.find('.view-date').text(today);
            var tbody = modal.find('tbody'),
                rows = tbody.find('tr'),
                row,
                i,
                j,
                index = 0,
                item,
                colClass,
                rowClass,
                day,
                dayCalculated,
                ddmmyyyy,
                dateInfotip,
                dateWeekday;

            rows.remove();

            while (index < data.length) {
                day = readJsonDate(data[index].PriceDate);
                dayCalculated = addDays(-1, day);
                ddmmyyyy = formatDateDDMMYYYY(day),
                rowClass = (isWeekend(day) ? 'row-weekend' : 'row-weekday')
                    + (ddmmyyyy == today ? ' row-today' : '');

                dateWeekday = formatDateWeekDay(day);

                dateInfotip = 'Prices for [u]' + dateWeekday + ' ' + ddmmyyyy + '[/u] &mdash; Calculated on [em]' + formatDateDDMMYYYY(dayCalculated) + '[/em]';

                row = $('<tr class="' + rowClass + '">');
                $('<td>').text(dateWeekday).appendTo(row);
                $('<td>').text(ddmmyyyy).appendTo(row);
                for (j = 0; j < 3; j++) {
                    colClass = j == 1 ? 'alt-col' : '';
                    item = data[index];
                    if (item == undefined)
                        item = { TodayPrice: '', PriceSource: '', PriceReasonFlags: 0 };
                    $('<td class="' + colClass + '">').html(formatPrice(item.TodayPrice, item.PriceReasonFlags, dateInfotip)).appendTo(row);
                    $('<td class="' + colClass + '">').html(item.PriceSource).appendTo(row);
                    index++;
                }
                tbody.append(row);
            }

            modal.find('.historic-price-scroller').scrollTop(0);
        };

        function formatPrice(price, priceReasonFlags, dateInfotip) {
            if (price == '' || price == 0)
                return '&mdash;';
            var val = (price / 10).toFixed(1),
                parts = val.split('.');

            return '<span data-infotip="' + dateInfotip + '[br /]' + priceReasons.simpleInfotip(priceReasonFlags) + '"><big>' + parts[0] + '</big>.' + parts[1] + '</span>';
        };

        function readJsonDate(date) {
            if (!date)
                return;
            var ticks = ('' + date).replace(/\D/g, '');
            return new Date(Number(ticks));
        };

        function isWeekend(date) {
            if (!date)
                return false;
            var d = date.getDay();
            return d == 0 || d == 6;
        };

        function formatDateWeekDay(date) {
            if (!date)
                return '';
            return weekdays[date.getDay()];
        };

        function addDays(days, date) {
            return new Date(date.getFullYear(), date.getMonth(), date.getDate() + days, 0, 0, 0, 0);
        };

        function formatDateDDMMYYYY(date) {
            if (!date)
                return '';
            var dd = date.getDate(),
                mm = date.getMonth() + 1,
                yyyy = date.getFullYear();
            return (dd < 10 ? '0' + dd : dd)
                + '/' + (mm < 10 ? '0' + mm : mm)
                + '/' + yyyy;
        };

        function initDom() {
            modal.hide().appendTo(document.body);
        };

        initDom();

        // API
        return {
            show: show
        };
    }
);