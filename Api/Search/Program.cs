using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Search.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Search.PostgresRDKit;
using Search.ApiCore;

namespace Search
{
    public class Program
    {
        public static void Main(string[] args)
        {
            BuildWebHost(args, new PostgresRDKitSearchProvider("User ID=postgres;Host=rdkit-postgres;Port=5432;Database=simsearch;Pooling=true;")).Run();
        }

        public static IWebHost BuildWebHost(string[] args, ISearchProvider searchProvider) =>
            WebHost.CreateDefaultBuilder(args)
                .ConfigureServices(sc => sc.Add(new ServiceDescriptor(typeof(ISearchProvider), searchProvider)))
                .UseStartup<Startup>()
                .Build();
    }
}
