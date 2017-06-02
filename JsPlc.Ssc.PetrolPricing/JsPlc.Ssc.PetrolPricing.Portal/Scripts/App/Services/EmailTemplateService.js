define(["jquery", "common", "PetrolPricingService"],
    function ($, common, petrolPricingService) {
        "use strict";

        function createEmailTemplateClone(success, failure, emailTemplateid, templateName) {
            var url = "EmailTemplate/CreateEmailTemplateClone?emailTemplateId=" + emailTemplateid + '&templateName=' + templateName;
            var promise = common.callService("get", url, null);
            promise.done(function (response, textStatus, jqXhr) {
                success(response);
            });
            promise.fail(failure);
        };

        function getTemplateNames(success, failure) {
            var url = "EmailTemplate/GetEmailTemplateNames";
            var promise = common.callService("get", url, null);
            promise.done(function (response, textStatus, jqXhr) {
                success(response);
            });
            promise.fail(failure);
        };

        function getTemplate(success, failure, emailTemplateId) {
            var url = "EmailTemplate/GetEmailTemplate?emailTemplateId=" + emailTemplateId;
            var promise = common.callService("get", url, null);
            promise.done(function (response, textStatus, jqXhr) {
                success(response);
            });
            promise.fail(failure);
        };

        function updateTemplate(success, failure, emailTemplate) {
            var url = "EmailTemplate/UpdateEmailTemplate";
            var promise = common.callService("post", url, emailTemplate);
            promise.done(function (response, textStatus, jqXhr) {
                success(response);
            });
            promise.fail(failure);
        };

        function deleteTemplate(success, failure, emailTemplateId) {
            var url = "EmailTemplate/DeleteEmailTemplate?emailTemplateid=" + emailTemplateId;
            var promise = common.callService("get", url, null);
            promise.done(function (response, textStatus, jqXhr) {
                success(response);
            });
            promise.fail(failure);
        }

        // API
        return {
            createEmailTemplateClone: createEmailTemplateClone,
            getTemplateNames: getTemplateNames,
            getTemplate: getTemplate,
            updateTemplate: updateTemplate,
            deleteTemplate: deleteTemplate
        };
    }
);