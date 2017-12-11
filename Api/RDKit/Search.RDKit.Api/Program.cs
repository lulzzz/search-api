using Microsoft.AspNetCore.Hosting;
using Search.GenericComponents;
using Search.MongoDB;
using Search.RDKit.Postgres;
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

            var mongoConnectionString = Environment.GetEnvironmentVariable("mongo_connection");
            if (string.IsNullOrEmpty(postgresConnectionString))
            {
                Console.WriteLine("Connection string to MongoDB instance must be passed as environment variable 'mongo_connection'");
                return 1;
            }

            //var searchProvider = new PostgresRDKitCatalog(postgresConnectionString);

            var catalog = 
                new GenericCatalog<string, FilterQuery, MoleculeData>(
                    new InMemoryCachingSearchProvider<string>(
                        new BatchSearchProvider<string>(
                            new PostgresRDKitBatchSearcher(postgresConnectionString),
                            10000)),
                    new MongoDBFilterEnricher<string, FilterQuery, MoleculeData>(mongoConnectionString, nameof(MoleculeData.IdNumber), new KekFilterCreator()));

            ApiCore.Api.BuildHost(catalog).Run();
            return 0;
        }
    }
}
