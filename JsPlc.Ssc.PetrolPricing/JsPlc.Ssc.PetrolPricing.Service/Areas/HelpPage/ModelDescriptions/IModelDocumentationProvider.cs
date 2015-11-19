using System;
using System.Reflection;

namespace JsPlc.Ssc.PetrolPricing.Service.Areas.HelpPage.ModelDescriptions
{
    public interface IModelDocumentationProvider
    {
        string GetDocumentation(MemberInfo member);

        string GetDocumentation(Type type);
    }
}