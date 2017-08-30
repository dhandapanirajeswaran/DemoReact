define(["jquery", "common", "text!busyloader.html"],
    function ($, common, busyloaderHtml) {

        "use strict";

        var PriceReasonFlags = {
            CheapestPriceFound: 0x00000001,
            Rounded: 0x00000002,
            InsidePriceVariance: 0x00000004,
            OutsidePriceVariance: 0x00000008,

            TodayPriceSnapBack: 0x00000010,
            HasGrocers: 0x00000020,
            HasIncompleteGrocers: 0x00000040,
            BasedOnUnleaded: 0x00000080,

            MissingSiteCatNo: 0x00000100,
            MissingDailyCatalist: 0x00000200,
            NoMatchCompetitorPrice: 0x00000400,
            NoSuggestedPrice: 0x00000800,

            PriceStuntFreeze: 0x00001000,
            LatestJSPrice: 0x00002000,
            ManualOverride: 0x00004000
        };

        var descriptions = {}
        descriptions[PriceReasonFlags.CheapestPriceFound] = { 
            name: 'Cheapest', 
            infotip: '[u]Cheapest[/u]', 
            text: 'Cheapest Price' 
        };
        descriptions[PriceReasonFlags.Rounded] = { 
            name: 'Rounded', 
            infotip: '[q]Rounded[/q]',
            text: 'Decimal-Rounded' 
        };
        descriptions[PriceReasonFlags.InsidePriceVariance] = { 
            name: 'Inside Variance', 
            infotip: '[q]Inside Variance[/q]',
            text: 'Inside Price Variance' 
        };
        descriptions[PriceReasonFlags.OutsidePriceVariance] = { 
            name: 'Outside Variance', 
            infotip: '',
            text: 'Outside Price Variance' 
        };

        descriptions[PriceReasonFlags.TodayPriceSnapBack] = { 
            name: 'Snapback', 
            infotip: '[i]Price Snapback[/i]',
            text: 'Today Price Snapback' 
        };
        descriptions[PriceReasonFlags.HasGrocers] = { 
            name: 'Grocers', 
            infotip: '[u]Grocers[/u]',
            text: 'Has Grocers' 
        };
        descriptions[PriceReasonFlags.HasIncompleteGrocers] = { 
            name: 'Incomplete Grocers',
            infotip: '[em]Incomplete Grocers[/em]',
            text: 'Grocers but incomplete data' 
        };
        descriptions[PriceReasonFlags.BasedOnUnleaded] = { 
            name: 'Unleaded',
            infotip: '[em]Based on Unleaded[/em]',
            text: 'Price based on Unleaded' 
        };

        descriptions[PriceReasonFlags.MissingSiteCatNo] = { 
            name: 'No CatNo',
            infotip: '[i]No CatNo[/i]',
            text: 'Missing CatNo' 
        };
        descriptions[PriceReasonFlags.MissingDailyCatalist] = { 
            name: 'No Catalist',
            infotip: '[i]No Daily Catalist[/i]',
            text: 'No Daily Catalist file' 
        };
        descriptions[PriceReasonFlags.NoMatchCompetitorPrice] = { 
            name: 'No Competitors',
            infotip: '[q]No Competitors[/q]',
            text: 'No Competitor prices found' 
        };
        descriptions[PriceReasonFlags.NoSuggestedPrice] = { 
            name: 'No Suggested',
            infotip: '[em]No Suggested Price[/em]',
            text: 'No Suggested price' 
        };

        descriptions[PriceReasonFlags.PriceStuntFreeze] = { 
            name: 'Freeze',
            infotip: '[q]Price Freeze[/q]',
            text: 'Price Stunt Freeze' 
        };
        descriptions[PriceReasonFlags.LatestJSPrice] = { 
            name: 'Latest JS', 
            infotip: '[em]Latest JS[/em]',
            text: 'Latest JS Price' 
        };
        descriptions[PriceReasonFlags.ManualOverride] = {
            name: 'Override',
            infotip: '[em]Manual Price Override[/em]',
            text: 'Manual Override'
        };

        function getReasonFromFlags(flags, prop) {
            var reasons = [],
                flag;

            if (flags == 0)
                flags = PriceReasonFlags.NoSuggestedPrice;

            for (flag in descriptions) {
                if ((flags & Number(flag)))
                    reasons.push(descriptions[flag][prop]);
            }
            return reasons;
        };

        function simpleInfotip(flags) {
            var reasons = getReasonFromFlags(flags, 'infotip');
            return reasons.join(' and ');
        };

        // API
        return {
            flags: PriceReasonFlags,
            simpleInfotip: simpleInfotip
        };
    }
);