using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace SearchV2.ApiCore
{
    public static class Api
    {

        public static IWebHostBuilder AddEndpoints(this IWebHostBuilder b, params ActionDescriptor[] descriptors)
            => AddEndpoints(b, "", descriptors);

        public static IWebHostBuilder AddEndpoints(this IWebHostBuilder b, string routePrefix, params ActionDescriptor[] descriptors)
            => AddEndpoints(b, routePrefix, $"C" + Guid.NewGuid().ToString("N"), descriptors);

        public static IWebHostBuilder AddEndpoints(this IWebHostBuilder b, string routePrefix, string controllerName, params ActionDescriptor[] descriptors)
            => !string.IsNullOrEmpty(controllerName) 
                ? b.ConfigureServices(sc =>
                    {
                        sc.Add(new ServiceDescriptor(typeof(ControllerDescriptor), new ControllerDescriptor { ControllerType = ControllerBuilder.CreateControllerClass(routePrefix, controllerName, descriptors) }));
                    })
                : throw new ArgumentException();
    }
}
