using Microsoft.Extensions.DependencyInjection;
using System.Linq;
using static SearchV2.ApiCore.Api;

namespace SearchV2.ApiCore
{
    public static class MvcCoreBuilderExtensions
    {
        public static IMvcCoreBuilder AddSearchFeature(this IMvcCoreBuilder b)
        {
            var controllerType = ((ControllerDescriptor)b.Services.Single(sd => sd.ServiceType == typeof(ControllerDescriptor)).ImplementationInstance).ControllerType;
            return b.AddApplicationPart(controllerType.Assembly);
        }
    }
}
