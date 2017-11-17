using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Search.Abstractions;

namespace Search.ApiCore
{
    public static class Api
    {
        public static IWebHost BuildHost(string[] args, ISearchProvider searchProvider) =>
            WebHost.CreateDefaultBuilder(args)
                .ConfigureServices(sc => sc.Add(new ServiceDescriptor(typeof(ISearchProvider), searchProvider)))
                .UseStartup<Startup>()
                .Build();
    }
}
