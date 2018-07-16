using Microsoft.Extensions.DependencyInjection;
using System.Linq;

namespace SearchV2.ApiCore
{
    public static class MvcCoreBuilderExtensions
    {
        public static IMvcBuilder AddControllersByDescriptors(this IMvcBuilder b)
            => b.Services
                .Where(sd => sd.ServiceType == typeof(ControllerDescriptor))
                .Select(sd => sd.ImplementationInstance)
                .Cast<ControllerDescriptor>()
                .Select(cd => cd.ControllerType.Assembly)
                .Distinct()
                .Aggregate(b, (builder, assembly) => builder.AddApplicationPart(assembly));

        public static IMvcCoreBuilder AddControllersByDescriptors(this IMvcCoreBuilder b)
            => b.Services
                .Where(sd => sd.ServiceType == typeof(ControllerDescriptor))
                .Select(sd => sd.ImplementationInstance)
                .Cast<ControllerDescriptor>()
                .Select(cd => cd.ControllerType.Assembly)
                .Distinct()
                .Aggregate(b, (builder, assembly) => builder.AddApplicationPart(assembly));
    }
}
