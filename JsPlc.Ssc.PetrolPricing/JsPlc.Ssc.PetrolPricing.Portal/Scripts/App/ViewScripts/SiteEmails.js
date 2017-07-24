define(["jquery", "common", "notify", "busyloader", "bootbox", "downloader", "PetrolPricingService"],
    function ($, common, notify, busyloader, bootbox, downloader, petrolPricingService) {
        "use strict";
        
        function clearEmailsClick() {
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
                element: '[data-click="exportEmailsClick"]',
                complete: function (download) {
                    notify.success("Exported Email Addresses - took " + download.friendlyTimeTaken);
                }
            });

            window.location.href = common.getRootSiteFolder() + url;
        };

        function searchKeyup() {
            filterRows();
        };

        function clearSearchFilters() {
            $('#txtSearchStoreNo,#txtSearchSiteName,#txtSearchCatNo,#txtSearchPfsNo,#txtSearchEmail').val('');
            filterRows();
        };

        function filterRows() {
            var storeNo = $('#txtSearchStoreNo').val().toUpperCase(),
                siteName = $('#txtSearchSiteName').val().toUpperCase(),
                catNo = $('#txtSearchCatNo').val().toUpperCase(),
                pfsNo = $('#txtSearchPfsNo').val().toUpperCase(),
                email = $('#txtSearchEmail').val(),
                rows = $('.table > tbody > tr'),
                row,
                i,
                cells,
                visible,
                count = 0;

            for (i = 0; i < rows.length; i++) {
                row = $(rows[i]);
                cells = row.find('td');
                visible = (
                    (storeNo == '' || cells.eq(0).text().indexOf(storeNo) ==0)
                    && (siteName == '' || cells.eq(1).text().toUpperCase().indexOf(siteName) != -1)
                    && (catNo == '' || cells.eq(2).text().indexOf(catNo) == 0)
                    && (pfsNo == '' || cells.eq(3).text().indexOf(pfsNo) == 0)
                    && (email == '' || cells.eq(4).text().indexOf(email) == 0)
                    );

                visible ? row.show() : row.hide();
                visible ? count++ : count;
            }
        };

        function bindEvents() {
            $('[data-click="clearEmailsClick"]').off().click(clearEmailsClick);
            $('[data-click="exportEmailsClick"]').off().click(exportEmailsClick);
            $('#txtSearchStoreNo, #txtSearchSiteName, #txtSearchCatNo, #txtSearchPfsNo, #txtSearchEmail').off().on('keyup', searchKeyup);
            $('#btnClearFilters').off().click(clearSearchFilters);
        };

        function docReady() {
            bindEvents();
        };
        
        $(docReady);
    }
);