define(["jquery"],
    function ($) {
        "use strict";
        
        function addChartData(chart, items) {
            var i,
                item,
                delta,
                obj;
            for (i = 0; i < items.length; i++) {
                item = items[i];
                delta = Number(item.delta);

                if (chart.min == undefined || delta < chart.min )
                    chart.min = delta;
                if (chart.max == undefined || delta > chart.max)
                    chart.max = delta;

                if (delta in chart.keys) {
                    obj = chart.keys[delta];
                    obj.count += item.count;
                }
                else {
                    obj = {
                        count: item.count,
                        delta: delta
                    };
                    chart.data.push(obj);
                    chart.keys[delta] = obj;
                }

                if (delta < 0)
                    chart.stats.down += item.count;
                else if (delta == 0)
                    chart.stats.none += item.count;
                else
                    chart.stats.up += item.count;
            }
        };

        function sortChartData(chart) {
            var i,
                obj,
                origin = 1000000,
                sorter = function (a, b) {
                    var value1 = origin + Number(a.delta),
                        value2 = origin + Number(b.delta);
                    return (value1 > value2) - (value1 < value2);
                };
            chart.data.sort(sorter);
            chart.keys = {};
            for (i = 0; i < chart.data.length; i++) {
                obj = chart.data[i];
                chart.keys[obj.delta] = obj;
            }
        };

        function getChartCountStats(chart) {
            var i,
                obj,
                count;

            for (i = 0; i < chart.data.length; i++) {
                obj = chart.data[i];
                count = obj.count;
                if (i == 0 || count < chart.counts.min)
                    chart.counts.min = count;
                if (i== 0 || count > chart.counts.max)
                    chart.counts.max = count;
            }

            chart.counts.origin = 0 - chart.counts.min;
            chart.counts.range = (chart.counts.max - chart.counts.min) + 1;
        }

        function render(opts, statsData) {
            var chart = {
                data: [],
                keys: {},
                deltas: {
                    min: undefined,
                    max: undefined
                },
                counts: {
                    min: undefined,
                    max: undefined,
                    origin: undefined,
                    range: undefined
                },
                stats: {
                    up: 0,
                    none: 0,
                    down: 0
                }
            };

            if (opts.show.unleaded)
                addChartData(chart, statsData.unleaded.tomorrow.priceChangeDataset.data);
            if (opts.show.diesel)
                addChartData(chart, statsData.diesel.tomorrow.priceChangeDataset.data);
            if (opts.show.superUnleaded)
                addChartData(chart, statsData.superUnleaded.tomorrow.priceChangeDataset.data);

            sortChartData(chart);

            getChartCountStats(chart);

            switch (opts.show.view) {
                case 0:
                    renderBarChart(chart);
                    break;
                case 1:
                    renderTable(chart);
                    break;
                case 2:
                    renderTags(chart);
                    break;
                default:
                    console.log('Unknown view: ' + opts.show.view);
                    break;
            }
        };

        function renderTags(chart) {
            var html = [],
                groups = {
                    down: [],
                    none: [],
                    up: []
                },
                i = 0,
                obj;

            for (i = 0; i < chart.data.length; i++) {
                obj = chart.data[i];
                if (obj.delta < 0) {
                    groups.down.push('<span class="tag tag-down">' + obj.count + ' fuels <strong>-' + Math.abs(obj.delta).toFixed(1) + '</strong></span>');
                } else if (obj.delta == 0) {
                    groups.none.push('<span class="tag tag-none">' + obj.count + ' fuels <strong>0</strong></span>');
                } else {
                    groups.up.push('<span class="tag tag-up">' + obj.count + ' fuels <strong>+' + Math.abs(obj.delta).toFixed(1) + '</strong></span>');
                }
            }

            if (groups.down.length == 0)
                groups.down.push('<div class="text-center font125pc">There are 0 downward Price Changes</div>');
            if (groups.none.length == 0)
                groups.none.push('<div class="text-center font125pc">There are 0 non-moving Price Changes</div>');
            if (groups.up.length == 0)
                groups.up.push('<div class="text-center font125pc">There are 0 upward Price Changes</div>');

            html.push('<div class="price-change-tags">')

            html.push('<div class="panel panel-success">');
            html.push('<div class="panel-heading text-center font125pc">');
            html.push('<strong>Our Price Changes &mdash; moving up <i class="fa fa-arrow-up"></i> <span class="badge">' + groups.up.length + '</span></strong>');
            html.push('</div>');
            html.push('<div class="panel-body">' + groups.up.join('') + '</div>');
            html.push('</div>');

            html.push('<div class="panel panel-warning">');
            html.push('<div class="panel-heading text-center font125pc">');
            html.push('<strong>Our Price Changes &mdash; non-movers <i class="fa fa-arrow-right"></i> <span class="badge">' + groups.none.length + '</span></strong>');
            html.push('</div>');
            html.push('<div class="panel-body">' + groups.none.join('') + '</div>');
            html.push('</div>');

            html.push('<div class="panel panel-danger">');
            html.push('<div class="panel-heading text-center font125pc">');
            html.push('<strong>Our Price Changes &mdash; moving down <i class="fa fa-arrow-down"></i> <span class="badge">' + groups.down.length + '</span></strong>');
            html.push('</div>');
            html.push('<div class="panel-body">' + groups.down.join('') + '</div>');
            html.push('</div>');

            html.push('</div>');

            $('#divPriceChangeBarChart').html(html.join(''));
        };

        function renderTable(chart) {
            var html = [],
                i = 0,
                rowIndex,
                colIndex,
                columns = 6,
                rowsPerColumn = Math.ceil(chart.data.length / columns),
                obj,
                css;

            html.push('<div class="price-change-table col-md-12">');

            for (colIndex = 0; colIndex < columns; colIndex++) {
                html.push('<div class="col-md-2">');
                if (i < chart.data.length) {
                    html.push('<table class="table table-striped table-condensed">');
                    html.push('<thead>');
                    html.push('<tr>');
                    html.push('<th>Fuels</th>');
                    html.push('<th></th>');
                    html.push('<th>Change</th>');
                    html.push('</tr>');
                    html.push('</thead>');

                    html.push('<tbody>');
                    for (rowIndex = 0; rowIndex < rowsPerColumn; rowIndex++) {
                        if (i < chart.data.length) {
                            obj = chart.data[i++];
                            html.push('</tr>');
                            html.push('<td class="text-right">' + obj.count + '</td>');
                            if (obj.delta < 0) {
                                html.push('<td class="price-change-down"><i class="fa fa-arrow-down"></i></td>');
                                css = 'price-change-down';
                            }
                            else if (obj.delta == 0) {
                                html.push('<td class="price-change-none"><i class="fa fa-arrow-right"></i></td>');
                                css = 'price-change-none';
                            }
                            else {
                                html.push('<td class="price-change-up"><i class="fa fa-arrow-up"></i></td>');
                                css = 'price-change-up';
                            }
                            html.push('<td class="text-center ' + css + '">' + (obj.delta < 0 ? '-' : '+') + Math.abs(obj.delta).toFixed(1) + '</td>');
                            html.push('</tr>');
                        }
                    }
                    html.push('</tbody>');
                    html.push('</table>');
                }
                html.push('</div>');
            }
            html.push('</div>');

            $('#divPriceChangeBarChart').html(html.join(''));
        };

        function renderBarChart(chart) {
            var html = [],
                row1 = [],
                row2 = [],
                i,
                barHeight,
                maxBarHeight = 60,
                obj,
                css;

            row1.push('<th class="text-center"><i class="fa fa-arrow-up"></i><br />Fuels</th>');
            row2.push('<th>Change</th>');

            for (i = 0; i < chart.data.length; i++) {
                obj = chart.data[i];

                if (obj.delta < 0)
                    css = 'down';
                else if (obj.delta == 0)
                    css = 'none';
                else
                    css = 'up';

                if (chart.counts.range <= 1)
                    barHeight = maxBarHeight
                else {
                    barHeight = Math.floor(obj.count * maxBarHeight / chart.counts.range);
                }
                if (barHeight == 0 && obj.count != 0)
                    barHeight = 1;

                row1.push('<td><div data-infotip="[b]' + obj.count + '[/b] with a change of [u]' + obj.delta + '[/u]" class="bar bar-' + css + '" style="border-bottom-width: ' + barHeight + 'px; padding-top: ' + (maxBarHeight - barHeight) + 'px">' + obj.count + '</div></td>');
                row2.push('<td class="key key-' + css + '">' + (obj.delta < 0 ? '-' : '+') + Math.abs(obj.delta).toFixed(1) + '</td>');
            }
            html.push('<div class="price-change-bar-chart">');
            html.push('<div class="price-change-bar-chart-scroller">');
            html.push('<table>');
            html.push('<tbody>');
            html.push('<tr class="bar-row">' + row1.join('') + '</tr>');
            html.push('<tr class="key-row">' + row2.join('') + '</tr>');
            html.push('</tbody>');
            html.push('</table>');
            html.push('</div>');

            html.push('<span class="title title-down">' + chart.stats.down + ' fuels with <i class="fa fa-arrow-down"></i> price change</span>');
            html.push('<span class="title title-none">' + chart.stats.none + ' fuels with <i class="fa fa-arrow-right"></i> price change</span>');
            html.push('<span class="title title-up">' + chart.stats.up + ' fuels with <i class="fa fa-arrow-up"></i> price change</span>');
            html.push('</div>');

            $('#divPriceChangeBarChart').html(html.join(''));
        };

        function init(selector) {
            // TODO
        };

        // API
        return {
            init: init,
            render: render
        };
    }
);