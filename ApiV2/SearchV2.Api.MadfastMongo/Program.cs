﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Globalization;
using SearchV2.Generics;

namespace SearchV2.Api.MadfastMongo
{
    public class Program
    {
        public static int Main(string[] args)
        {
            CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;

            var madfastUrl = Environment.GetEnvironmentVariable("madfast_url");
            if (string.IsNullOrEmpty(madfastUrl))
            {
                Console.WriteLine("URL to the 'find-most-similars' endpoint of Madfast service must be passed as environment variable 'madfast_url'");
                return 1;
            }

            var mongoConnectionString = Environment.GetEnvironmentVariable("mongo_connection");
            if (string.IsNullOrEmpty(mongoConnectionString))
            {
                Console.WriteLine("Connection string to MongoDB instance must be passed as environment variable 'mongo_connection'");
                return 1;
            }

            var mongoDbName = Environment.GetEnvironmentVariable("mongo_dbname");
            if (string.IsNullOrEmpty(mongoDbName))
            {
                Console.WriteLine("DB name must be passed as environment variable 'mongo_dbname'");
                return 1;
            }

            //var searchProvider = new PostgresRDKitCatalog(postgresConnectionString);

            ApiCore.Api.BuildHost(
                new MongoCatalog<string, FilterQuery, MoleculeData>(mongoConnectionString, mongoDbName, new FilterQuery.Creator()),
                ApiCore.Api.RegisterSearch("sim", CachingSearchService.Wrap(new MadfastSimilaritySearchService(madfastUrl, 1000)))
                ).Run();
            return 0;
        }
    }
}
