﻿define(["jquery", "common", "notify", "busyloader", "bootbox", "downloader", "PetrolPricingService"],
    function ($, common, notify, busyloader, bootbox, downloader, petrolPricingService) {
        "use strict";

        var state = {
            activeSites: true,
            inactiveSites: true,
            withEmails: true,
            withNoEmails: true
        };

        var selectors = {
            deleteAllEmnails: '[data-click="deleteAllEmails"]',
            exportEmails: '[data-click="exportEmails"]',

            toggleActiveSites: '[data-click="toggleActiveSites"]',
            toggleInactiveSites: '[data-click="toggleInactiveSites"]',
            clearActiveFilters: '[data-click="clearActiveFilters"]',

            toggleWithEmails: '[data-click="toggleWithEmails"]',
            toggleWithNoEmails: '[data-click="toggleWithNoEmails"]',
            clearEmailFilters: '[data-click="clearEmailFilters"]',

            clearAllFilters: '[data-click="clearAllFilters"]',

            storeNoFilter: '#txtSearchStoreNo',
            siteNameFilter: '#txtSearchSiteName',
            catNoFilter: '#txtSearchCatNo',
            pfsNoFilter: '#txtSearchPfsNo',
            emailFilter: '#txtSearchEmail',

            showingInfo: '#spnShowingInfo'
        };

        var controls = {
            deleteAllEmnails: undefined,
            exportEmails: undefined,

            toggleActiveSites: undefined,
            toggleInactiveSites: undefined,
            clearSiteFilters: undefined,

            toggleWithEmails: undefined,
            toggleWithNoEmails: undefined,
            clearEmailFilters: undefined,

            clearAllFilters: undefined,

            storeNoFilter: undefined,
            siteNameFilter: undefined,
            catNoFilter: undefined,
            pfsNoFilter: undefined,
            emailFilter: undefined
        };

        
        function deleteEmailsClick() {
            bootbox.confirm({
                title: "Delete Confirmation",
                message: "Are you sure you wish to delete All the Site Email Addresses?",
                buttons: {
                    confirm: {
                        label: '<i class="fa fa-check"></i> Yes',
                        className: 'btn btn-danger'
                    },
                    cancel: {
                        label: '<i class="fa fa-times"></i> No',
                        className: 'btn btn-default'
                    }
                },
                callback: function (result) {
                    if (result) {
                        removeAllSiteEmailAddresses();
                    }
                }
            });
        };

        function removeAllSiteEmailAddresses() {
            function failure() {
                notify.error("Unable to remove Email Addresses");
            };
            function success() {
                notify.success("Removed all Site Email Addresses");

                setTimeout(function () {
                    busyloader.show({
                        message: 'Reloading Page. Please wait...',
                        showtime: 2000,
                        dull: true
                    });
                    setTimeout(function () {
                        window.location.reload();
                    }, 1000);
                }, 1000);

            };
            petrolPricingService.removeAllSiteEmailAddresses(success, failure);
        };

        function exportEmailsClick() {
            var downloadId = downloader.generateId(),
                url = "Sites/ExportSiteEmails?downloadId=" + downloadId;

            busyloader.show({
                message: 'Exporting Email Addresses. Please wait...',
                showtime: 1000,
                dull: true
            });
            downloader.start({
                id: downloadId,
                element: selectors.exportButton,
                complete: function (download) {
                    notify.success("Exported Email Addresses - took " + download.friendlyTimeTaken);
                }
            });

            window.location.href = common.getRootSiteFolder() + url;
        };

        function searchKeyup() {
            filterRows();
        };

        function clearAllFiltersClick() {
            controls.storeNoFilter.val('');
            controls.siteNameFilter.val('');
            controls.catNoFilter.val('');
            controls.pfsNoFilter.val('');
            controls.emailFilter.val('');

            state.activeSites = true;
            state.inactiveSites = true;
            state.withEmails = true;
            state.withNoEmails = true;
            redrawFilterButtons();

            filterRows();
            notify.info('Cleared all the filters');
        };

        function trim(str) {
            return ('' + str).replace(/^\s+|\s+$/, '');
        };

        function filterRows() {
            var storeNo = $(selectors.storeNoFilter).val().toUpperCase(),
                siteName = $(selectors.siteNameFilter).val().toUpperCase(),
                catNo = $(selectors.catNoFilter).val().toUpperCase(),
                pfsNo = $(selectors.pfsNoFilter).val().toUpperCase(),
                email = $(selectors.emailFilter).val().toUpperCase(),
                rows = $('.table > tbody > tr'),
                row,
                i,
                cells,
                visible,
                isActive,
                hasEmail,
                count = 0,
                totalCount = 0;

            for (i = 0; i < rows.length; i++) {
                row = $(rows[i]);
                cells = row.find('td');
                isActive = row.hasClass('row-site-active');
                hasEmail = row.hasClass('row-has-email');
                visible = (
                    (storeNo == '' || trim(cells.eq(0).text()).indexOf(storeNo) ==0)
                    && (siteName == '' || trim(cells.eq(1).text()).toUpperCase().indexOf(siteName) != -1)
                    && (catNo == '' || trim(cells.eq(2).text()).indexOf(catNo) == 0)
                    && (pfsNo == '' || trim(cells.eq(3).text()).indexOf(pfsNo) == 0)
                    && (email == '' || trim(cells.eq(4).text()).toUpperCase().indexOf(email) == 0)
                    && (!isActive || (isActive && state.activeSites ))
                    && (isActive || (!isActive && state.inactiveSites))
                    && (!hasEmail || (hasEmail && state.withEmails))
                    && (hasEmail || (!hasEmail && state.withNoEmails))
                    );

                visible ? row.show() : row.hide();
                visible ? count++ : count;
                totalCount++;
            }

            $(selectors.showingInfo).text(count + ' of ' + totalCount);
        };

        function redrawButton(control, isActive) {
            isActive
                ? control.removeClass('btn-default').addClass('btn-primary')
                : control.removeClass('btn-primary').addClass('btn-default');
        };

        function redrawFilterButtons() {
            redrawButton(controls.toggleActiveSites, state.activeSites);
            redrawButton(controls.toggleInactiveSites, state.inactiveSites);
            redrawButton(controls.toggleWithEmails, state.withEmails);
            redrawButton(controls.toggleWithNoEmails, state.withNoEmails);
        };

        function commonToggleFilter(opts) {
            var newstate = !state[opts.name],
                message = newstate ? opts.showing : opts.hiding;

            state[opts.name] = newstate;
            redrawFilterButtons();
            filterRows();
            notify.info(message);
        };


        function toggleActiveSitesClick() {
            commonToggleFilter({
                name: 'activeSites',
                showing: 'Showing Active Sites',
                hiding: 'Hiding Active Sites'
            });
        };

        function toggleInactiveSitesClick() {
            commonToggleFilter({
                name: 'inactiveSites',
                showing: 'Showing Inactive Sites',
                hiding: 'Hiding Inactive sites'
            });
        };

        function clearActiveEmails() {
            state.activeSites = true;
            state.inactiveSites = true;
            redrawButton(controls.toggleActiveSites, state.activeSites);
            redrawButton(controls.toggleInactiveSites, state.inactiveSites);
            filterRows();
            notify.info('Cleared all the search filters');
        };

        function toggleWithEmailsClick() {
            commonToggleFilter({
                name: 'withEmails',
                showing: 'Showing Sites With email addresses',
                hiding: 'Hiding Sites with email addresses'
            });
        };
        
        function toggleWithNoEmailsClick() {
            commonToggleFilter({
                name: 'withNoEmails',
                showing: 'Showing Sites with No email addresses',
                hiding: 'Hiding Sites with No email addresses'
            });
        };

        function clearEmailFiltersClick() {
            state.withEmails = true,
            state.withNoEmails = true;
            redrawButton(controls.toggleWithEmails, state.withEmails);
            redrawButton(controls.toggleWithNoEmails, state.withNoEmails);
            filterRows();
            notify.info('Cleared the Email filters');
        };


        function findControls() {
            var key;
            for(key in selectors) {
                controls[key] = $(selectors[key]);
            }
        };

        function bindEvents() {
            controls.deleteAllEmnails.off().click(deleteEmailsClick);
            controls.exportEmails.off().click(exportEmailsClick);

            controls.toggleActiveSites.off().click(toggleActiveSitesClick);
            controls.toggleInactiveSites.off().click(toggleInactiveSitesClick);
            controls.clearActiveFilters.off().click(clearActiveEmails);

            controls.toggleWithEmails.off().click(toggleWithEmailsClick);
            controls.toggleWithNoEmails.off().click(toggleWithNoEmailsClick);
            controls.clearEmailFilters.off().click(clearEmailFiltersClick);

            controls.clearAllFilters.off().click(clearAllFiltersClick);

            controls.storeNoFilter.off().on('keyup', searchKeyup);
            controls.siteNameFilter.off().on('keyup', searchKeyup);
            controls.catNoFilter.off().on('keyup', searchKeyup);
            controls.pfsNoFilter.off().on('keyup', searchKeyup);
            controls.emailFilter.off().on('keyup', searchKeyup);
        };

        function docReady() {
            findControls();
            bindEvents();
            filterRows();
        };
        
        $(docReady);
    }
);