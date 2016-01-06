$(function () {

    $("#SiteId").focus();

    $("#SiteId").change(function () {
        var siteId = $(this).val();
        window.location.href = '/Reports/CompetitorSites/?siteId=' + siteId;
    });
});
