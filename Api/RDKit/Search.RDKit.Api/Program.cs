using Microsoft.AspNetCore.Hosting;
using Search.PostgresRDKit;
using Search.ApiCore;
using System;

namespace Search
{
    public class Program
    {
        public static int Main(string[] args)
        {
            var postgresConnectionString = Environment.GetEnvironmentVariable("postgres_connection");
            if (string.IsNullOrEmpty(postgresConnectionString))
            {
                Console.WriteLine("Connection string to PostgreSQL instance must be passed as environment variable 'postgres_connection'");
                return 1;
            }

            var searchProvider = new PostgresRDKitSearchProvider(postgresConnectionString);

            Api.BuildHost(args, searchProvider).Run();
            return 0;
        }
    }
}
