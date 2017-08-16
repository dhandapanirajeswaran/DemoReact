define(["jquery", "waiter", "notify", "text!App/HistoricSitePrices.html"],
    function ($, waiter, notify, modalHtml) {
        "use strict";

        var modal = $(modalHtml),
            lightbox = $('<div class="lightbox-overlay"></div>');

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
                colClass;

            rows.remove();
            while (index < data.length) {
                row = $('<tr>');
                $('<td>').text(formatJsonDate(data[index].PriceDate)).appendTo(row);
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
        };

        function formatPrice(price) {
            if (price == '' || price == 0)
                return '&mdash;';
            return (price / 10).toFixed(1);
        };

        function formatJsonDate(date) {
            if (!date)
                return '';
            var ticks = ('' + date).replace(/\D/g, ''),
                dt = new Date(Number(ticks)),
                dd = dt.getDate(),
                mm = dt.getMonth() + 1,
                yyyy = dt.getFullYear();
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