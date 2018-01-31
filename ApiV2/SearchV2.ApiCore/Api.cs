using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace SearchV2.ApiCore
{
    public static partial class Api
    {

        public static IWebHost BuildHost(string routePrefix, params ActionDescriptor[] descriptors)
            => BuildHost(sc =>
                {
                    sc.Add(new ServiceDescriptor(typeof(ControllerDescriptor), new ControllerDescriptor { ControllerType = ControllerBuilder.CreateControllerClass(routePrefix, descriptors) }));
                });
        
        static IWebHost BuildHost(Action<IServiceCollection> populateServicesDelegate)
            => WebHost.CreateDefaultBuilder()
                .CaptureStartupErrors(true)
                .ConfigureServices(populateServicesDelegate)
                .UseStartup<Startup>()
                .Build();


    }
}
