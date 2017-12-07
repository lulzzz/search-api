﻿using Microsoft.AspNetCore.Hosting;
using Search.GenericComponents;
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

            //var searchProvider = new PostgresRDKitCatalog(postgresConnectionString);

            var catalog = 
                new GenericCatalog<string, FilterQuery, MoleculeData>(
                    new BatchSearchProvider<string>(
                        new PostgresRDKitBatchSearcher(postgresConnectionString),
                        int.MaxValue), // can be passed from outside
                    new PostgresFilterEnricher(postgresConnectionString));

            ApiCore.Api.BuildHost(catalog).Run();
            return 0;
        }
    }
}
