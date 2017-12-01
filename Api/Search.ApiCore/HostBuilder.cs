using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Search.Abstractions;

namespace Search.ApiCore
{
    public static class Api
    {
        public static IWebHost BuildHost<TId, TFilterQuery, TData>(ICatalog<TId, TFilterQuery, TData> searchProvider) =>
            WebHost.CreateDefaultBuilder()
                .ConfigureServices(sc => sc.Add(new ServiceDescriptor(typeof(ICatalog<TId, TFilterQuery, TData>), searchProvider)))
                .UseStartup<Startup>()
                .Build();
    }
}
