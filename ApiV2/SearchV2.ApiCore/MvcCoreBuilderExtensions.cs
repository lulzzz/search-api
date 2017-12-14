using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace SearchV2.ApiCore
{
    public static class MvcCoreBuilderExtensions
    {
        public static IMvcCoreBuilder AddSearchFeature(this IMvcCoreBuilder b, IEnumerable<ServiceDescriptor> services)
        {
            return b.AddApplicationPart(dynAss)
                .ConfigureApplicationPartManager(apm =>
                {
                    apm.FeatureProviders.Add(new MoleculesControllerFeatureProvider(dynType));
                });
        }

        public class MoleculesControllerFeatureProvider : IApplicationFeatureProvider<ControllerFeature>
        {
            readonly TypeInfo _controllerType;

            public MoleculesControllerFeatureProvider(TypeInfo controllerType)
            {
                _controllerType = controllerType;
            }

            public void PopulateFeature(IEnumerable<ApplicationPart> parts, ControllerFeature feature)
            {
                feature.Controllers.Add(_controllerType);
            }
        }
    }
}
