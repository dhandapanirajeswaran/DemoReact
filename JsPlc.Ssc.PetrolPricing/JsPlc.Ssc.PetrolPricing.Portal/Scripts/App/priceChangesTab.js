define(["jquery"],
    function ($) {
        "use strict";

        var canvas = {
            ele: undefined,
            width: 0,
            height: 0,
            context: undefined,
            padding: {
                top: 20,
                left: 80,
                right: 20,
                bottom: 20
            }
        };

        var graph = {
            left: 0,
            top: 0,
            right: 0,
            bottom: 0,
            width: 0,
            height: 0,
            spacing: {
                columns: 1
            },
            steps: {
                columnGroup: 0,
                column: 0
            }
        };

        var styles = {
            canvas: {
                background: 'rgb(255, 255, 255)',
                text: 'rgb(0,0,0)'
            },
            graph: {
                background: 'rgb(248, 248, 248)',
                text: 'rgb(0,0,0)',
                border: 'rgb(128,128,128)'
            },
            combined: {
                bar: 'rgb(240,173,78)'
            },
            unleaded: {
                bar: 'rgb(92,184,92)'
            },
            diesel: {
                bar: 'rgb(0,0,0)'
            },
            superUnleaded: {
                bar: 'rgb(255, 255, 255)'
            }
        };
        
        function render(opts, statsData) {
            if (canvas == undefined)
                return;
            
            var ctx = canvas.context,
                fuelCount = Number(opts.show.combined) + Number(opts.show.unleaded) + Number(opts.show.diesel) + Number(opts.show.superUnleaded),
                graphData,
                message;

            // clear the entire canvas
            ctx.fillStyle = styles.canvas.background;
            ctx.fillRect(0, 0, canvas.width, canvas.height);

            // clear the graph area
            ctx.fillStyle = styles.graph.background;
            ctx.fillRect(graph.left, graph.top, graph.width, graph.height);

            printChartError(ctx, 'Price Changes coming soon..');
            return false;


            if (fuelCount == 0) {
                printChartError(ctx, 'Please select 1 or more fuels');
                return false;
            }

            // prepare the graph data
            graphData = prepareData(opts, statsData);

            if (graphData.delta.data.length == 0) {
                printChartError(ctx, 'There are no deltas to plot!');
                return false;
            }

            if (graphData.sites.data.length == 0) {
                printChartError(ctx, 'There are no sites to plot!');
                return false;
            }

            // draw the bar charts
            drawBarChart(ctx, graphData, opts, fuelCount);

            return true;
        };

        function printChartError(ctx, message) {
            ctx.font = '24px Times New Roman';
            ctx.fillStyle = styles.canvas.text;
            centreText(ctx, message, canvas.width / 2, canvas.height / 2);
        };

        function centreText(ctx, text, left, top) {
            var size = ctx.measureText(text);
            ctx.fillText(text, left - size.width / 2, top);
        };

        function drawBarChart(ctx, graphData, opts, fuelCount) {
            var columns = Math.max(1, graphData.delta.data.length),
                rows = Math.max(1, graphData.sites.data.length),
                columnGroupWidth = Math.floor(graph.width / columns),
                columnWidth = Math.floor(columnGroupWidth / fuelCount) - graph.spacing.columns,
                rowHeight = Math.floor(graph.height / rows),
                x,
                y,
                i,
                delta,
                siteCount,
                width,
                height;

            debugger;

            ctx.lineWidth = 1;
            ctx.beginPath();
            ctx.moveTo(graph.left, graph.top);
            ctx.lineTo(graph.right, graph.top);
            ctx.lineTo(graph.right, graph.bottom);
            ctx.lineTo(graph.left, graph.bottom);
            ctx.lineTo(graph.left, graph.top);
            ctx.stroke();


            for (i = 0; i < columns; i++) {
                x = graph.left + i * columnGroupWidth;
                delta = graphData.delta.data[i];
                ctx.beginPath();
                ctx.moveTo(x, graph.bottom);
                ctx.lineTo(x, graph.bottom + 10);
                ctx.stroke();

                if (opts.show.combined) {
                    height = delta * barYScalr
                    ctx.fillStyle = styles.combined.bar;
                    ctx.fillRect(x, graph.bottom);
                }
            }

        };

        function prepareData(opts, statsData) {
            var graphData = {
                delta: {
                    keys: {}, // N * {index: 0, delta: n}
                    data: [], // N * {combined: 0, unleaded:0, diesel: 0, superUnleaded: 0}
                    min: undefined,
                    max: undefined
                },
                sites: {
                    keys: {},
                    data: [],
                    totalCount: 0,
                    min: undefined,
                    max: undefined
                }
            };

            if (opts.show.combined)
                addGraphStats('combined', graphData, statsData.combined.tomorrow.priceChangeDataset);
            if (opts.show.unleaded)
                addGraphStats('unleaded', graphData, statsData.unleaded.tomorrow.priceChangeDataset);
            if (opts.show.diesel)
                addGraphStats('diesel', graphData, statsData.diesel.tomorrow.priceChangeDataset);
            if (opts.show.superUnleaded)
                addGraphStats('superUnleaded', graphData, statsData.superUnleaded.tomorrow.priceChangeDataset);

            // aggregate the graph data
            aggregateStats(graphData);

            // sort the x-axis data (delta aka price changes)
            graphData.delta.data.sort(function (a, b) {
                var origin = 1000000,
                    value1 = origin + toNumberOrZero(a.delta),
                    value2 = origin + toNumberOrZero(b.delta);
                return value1 > value2 - value1 < value2;
            });

            // sort the y-axis data (Site counts)
            graphData.sites.data.sort(function (a, b) {
                return Number(a) > Number(b) - Number(a) < Number(b);
            });

            return graphData;
        };

        function aggregateStats(graphData) {
            var i = 0,
                item,
                deltas = graphData.delta.data,
                delta;
            for (i = 0; i < deltas.length; i++) {
                item = deltas[i];
                delta = item.delta;

                graphData.delta.min = Math.min(delta, toNumberElse(graphData.delta.min, delta));
                graphData.delta.max = Math.max(delta, toNumberElse(graphData.delta.max, delta));

                aggregateFuelStat(graphData, delta, item.combined);
                aggregateFuelStat(graphData, delta, item.unleaded);
                aggregateFuelStat(graphData, delta, item.diesel);
                aggregateFuelStat(graphData, delta, item.superUnleaded);
            }
        };

        function aggregateFuelStat(graphData, delta, siteCount) {
            if (!(siteCount in graphData.sites.keys)) {
                graphData.sites.data.push(siteCount);
                graphData.sites.keys[siteCount] = true;

                graphData.sites.min = Math.min(siteCount, toNumberElse(graphData.sites.min, siteCount));
                graphData.sites.max = Math.max(siteCount, toNumberElse(graphData.sites.max, siteCount));
            }
            graphData.sites.totalCount += siteCount;
        };

        function toNumberElse(value, defaultValue) {
            return value == '' || isNaN(value)
                ? Number(defaultValue)
                : Number(value);
        };

        function toNumberOrZero(value) {
            return (value == '' || isNaN(value))
                ? 0
                : Number(value);
        };

        function createGraphGroup() {
            return {
                combined: 0,
                unleaded: 0,
                diesel: 0,
                superUnleaded: 0
            };
        };

        function addGraphStats(fuelName, graphData, dataset) {
            var i,
                index,
                stat,
                delta,
                record,
                item;
            for (i = 0; i < dataset.data.length; i++) {
                item = dataset.data[i]
                delta = item.delta;
                if (delta in graphData.delta.keys) {
                    index = graphData.delta.keys[delta].index;
                } else {
                    index = graphData.delta.data.length;
                    record = {
                        delta: delta,
                        combined: 0,
                        unleaded: 0,
                        diesel: 0,
                        superUnleaded: 0
                    };
                    graphData.delta.keys[delta] = record;
                    graphData.delta.data.push(record);
                }
                graphData.delta.data[index][fuelName] += item.count;
            }
        };

        function init(selector) {
            var ele = $(selector);
            if (ele.length == 0) {
                console.log('Unable to find canvas: ' + selector);
                return;
            }
            canvas.ele = ele;
            canvas.width = ele.attr('width');
            canvas.height = ele.attr('height');
            canvas.context = ele[0].getContext('2d');

            graph.left = canvas.padding.left;
            graph.top = canvas.padding.top;
            graph.width = canvas.width - (canvas.padding.left + canvas.padding.right);
            graph.height = canvas.height - (canvas.padding.top + canvas.padding.bottom);
            graph.right = graph.left + graph.width - 1;
            graph.bottom = graph.top + graph.height - 1;
        };

        // API
        return {
            init: init,
            render: render
        };
    }
);