using Microsoft.AspNetCore.Hosting;
using Search.PostgresRDKit;
using System;
using System.Globalization;

namespace Search.RDKit.Api
{
    public class Program
    {
        public static int Main(string[] args)
        {
            CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
            var postgresConnectionString = Environment.GetEnvironmentVariable("postgres_connection");
            if (string.IsNullOrEmpty(postgresConnectionString))
            {
                Console.WriteLine("Connection string to PostgreSQL instance must be passed as environment variable 'postgres_connection'");
                return 1;
            }

            var searchProvider = new PostgresRDKitSearchProvider(postgresConnectionString);

            ApiCore.Api.BuildHost(args, searchProvider).Run();
            return 0;
        }
    }
}
