$(function () {

    $("#SiteId").focus();

    $("#SiteId").change(function () {
        var siteId = $(this).val();
        window.location.href = '/PriceReports/CompetitorSites/?siteId=' + siteId;
    });
});
