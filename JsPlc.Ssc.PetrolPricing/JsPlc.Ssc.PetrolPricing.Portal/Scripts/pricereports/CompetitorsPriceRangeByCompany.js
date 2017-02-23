﻿require(["jquery", "common", "busyloader", "bootstrap-datepicker"],
    function ($, common, busyloader, bsdatepicker) {

        $("document").ready(function () {
            var forDp = $('.datepicker').datepicker({
                language: "en-GB",
                autoClose: true,
                format: 'd-M-yyyy',
                todayBtn: "linked",
                todayHighlight: true,
                orientation: 'auto top',
                endDate: '1d'
            });

            $("#DateFor, #SelectedCompanyName, #SelectedBrandName").each(function () {
                $this = $(this);
                $("#" + $this.attr("name") + "Copy").val($this.val());
            });

            $("#DateFor, #SelectedCompanyName, #SelectedBrandName").change(function () {
                $this = $(this);
                $("#" + $this.attr("name") + "Copy").val($this.val());
            });

            $('#btnExportReport').off().on('click', function () {
                busyloader.showExportToExcel();
                return true;
            });

            $('#btnViewReport').off().on('click', function () {
                busyloader.showViewingReport();
                return true;
            });

        });
    }
);