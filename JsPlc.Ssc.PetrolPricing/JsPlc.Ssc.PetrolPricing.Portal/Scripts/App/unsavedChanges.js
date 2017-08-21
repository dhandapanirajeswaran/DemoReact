define(["jquery"],
    function ($) {
        "use strict";

        function slacker() {};

        var state = {
            ele: undefined,
            changed: false,
            snapshot: '',
            frequency: 1000,
            events: {
                unsaved: slacker
            }
        };

        function bind(opts) {
            state.ele = opts.ele,
            state.snapshot = getSnapshot();

            state.events.unsaved = $.isFunction(opts.unsaved) ? opts.unsaved : slacker;

            setTimeout(detectChanges, state.frequency);
        };

        function detectChanges() {
            detect();
            setTimeout(detectChanges, state.frequency);
        };

        function getSnapshot() {
            return state.ele.serialize();
        };

        function markAsChanged() {
            if (state.changed)
                return;
            state.changed = true;
            state.events.unsaved();
        };

        function hasChanges() {
            return state.changed || detect();
        };

        function detect() {
            if (!state.changed) {
                if (state.snapshot != getSnapshot())
                    markAsChanged();
            }
            return state.changed;
        };

        function saved() {
            state.changed = false;
            state.snapshot = getSnapshot();
        };

        // API
        return {
            bind: bind,
            markAsChanged: markAsChanged,
            hasChanges: hasChanges,
            saved: saved
        };
    }
);