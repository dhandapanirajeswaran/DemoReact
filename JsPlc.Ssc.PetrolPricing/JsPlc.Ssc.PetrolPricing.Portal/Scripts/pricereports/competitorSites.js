$("document").ready(function () {

    var rootFolder = /\/petrolpricing\//i.test(window.location.href) ? "/petrolpricing" : "";

    $("#SiteId").focus();

    $("#SiteId").change(function () {
        var siteId = $(this).val();
        window.location.href = rootFolder + '/PriceReports/CompetitorSites/?siteId=' + siteId;
    });


    $("#btnExportReport").click(function () {


        var siteId = $("#SiteId").val();
        window.location.href = rootFolder + '/PriceReports/ExportCompetitorSites/?siteId=' + siteId;
    });
});
