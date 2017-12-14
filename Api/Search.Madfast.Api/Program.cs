using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Globalization;
using Search.GenericComponents;
using Search.MongoDB;

namespace Search.Madfast.Api
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

            //var searchProvider = new PostgresRDKitCatalog(postgresConnectionString);

            var catalog =
                new GenericCatalog<string, FilterQuery, MoleculeData>(
                    //new InMemoryCachingSearchProvider<string>(
                        new MadfastSearchProvider(madfastUrl, 1000, 0.3)
                        //)
                        ,
                    new MongoDBFilterEnricher<string, FilterQuery, MoleculeData>(mongoConnectionString, nameof(MoleculeData.IdNumber), new FilterQuery.Creator()));

            ApiCore.Api.BuildHost(catalog).Run();
            return 0;
        }
    }
}
