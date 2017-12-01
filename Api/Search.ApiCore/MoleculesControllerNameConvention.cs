using Microsoft.AspNetCore.Mvc.ApplicationModels;
using System;

namespace Search.ApiCore
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class MoleculesControllerNameConvention : Attribute, IControllerModelConvention
    {
        public void Apply(ControllerModel controller) => controller.ControllerName = "Molecules";
    }
}
