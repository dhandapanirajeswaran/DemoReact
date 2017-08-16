define(["jquery", "waiter", "notify", "text!App/HistoricSitePrices.html"],
    function ($, waiter, notify, modalHtml) {
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

        function show(siteItem, data) {
            modal.modal('show');
            modal.find('.site-name').text(siteItem.StoreName);
            var tbody = modal.find('tbody'),
                rows = tbody.find('tr'),
                row,
                i,
                j,
                index = 0,
                item,
                colClass,
                rowClass,
                day;

            rows.remove();
            while (index < data.length) {
                day = readJsonDate(data[index].PriceDate);
                rowClass = isWeekend(day) ? 'row-weekend' : 'row-weekday';

                row = $('<tr class="' + rowClass + '">');
                $('<td>').text(formatDateWeekDay(day)).appendTo(row);
                $('<td>').text(formatDateDDMMYYYY(day)).appendTo(row);
                for (j = 0; j < 3; j++) {
                    colClass = j == 1 ? 'alt-col' : '';
                    item = data[index];
                    if (item == undefined)
                        item = { TodayPrice: '', PriceSource: '' };
                    $('<td class="' + colClass + '">').html(formatPrice(item.TodayPrice)).appendTo(row);
                    $('<td class="' + colClass + '">').html(item.PriceSource).appendTo(row);
                    index++;
                }
                tbody.append(row);
            }

            modal.find('.historic-price-scroller').scrollTop(0);
        };

        function formatPrice(price) {
            if (price == '' || price == 0)
                return '&mdash;';
            return (price / 10).toFixed(1);
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