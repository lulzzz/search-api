using Microsoft.AspNetCore.Mvc.ApplicationModels;
using System;

namespace SearchV2.ApiCore
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class MoleculesControllerNameConventionAttribute : Attribute, IControllerModelConvention
    {
        public void Apply(ControllerModel controller) => controller.ControllerName = "Molecules";
    }
}