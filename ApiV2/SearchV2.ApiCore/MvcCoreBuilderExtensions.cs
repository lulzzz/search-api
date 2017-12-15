using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using static SearchV2.ApiCore.Api;

namespace SearchV2.ApiCore
{
    public static class MvcCoreBuilderExtensions
    {
        public static IMvcCoreBuilder AddSearchFeature(this IMvcCoreBuilder b, IEnumerable<ServiceDescriptor> services)
        {
            var controllerType = ((ControllerDescriptor)services.Single(sd => sd.ServiceType == typeof(ControllerDescriptor)).ImplementationInstance).ControllerType;
            return b.AddApplicationPart(controllerType.Assembly);
                //.ConfigureApplicationPartManager(apm =>
                //{
                //    apm.FeatureProviders.Add(new MoleculesControllerFeatureProvider(controllerType));
                //});
        }

        //public class MoleculesControllerFeatureProvider : IApplicationFeatureProvider<ControllerFeature>
        //{
        //    readonly TypeInfo _controllerType;

        //    public MoleculesControllerFeatureProvider(TypeInfo controllerType)
        //    {
        //        _controllerType = controllerType;
        //    }

        //    public void PopulateFeature(IEnumerable<ApplicationPart> parts, ControllerFeature feature)
        //    {
        //        feature.Controllers.Add(_controllerType);
        //    }
        //}
    }
}
