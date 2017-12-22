using Common;
using Microsoft.AspNetCore.Hosting;
using SearchV2.Api.MadfastMongo;
using SearchV2.Generics;
using SearchV2.RDKit;
using System;
using System.Globalization;
using Uorsy.Data;

namespace SearchV2.Api.Uorsy
{
    class Program
    {
        class Env
        {
            public string MongoConnection { get; set; }
            public string MongoDbname { get; set; }
            public string PostgresConnection { get; set; }
        }

        public static void Main(string[] args)
        {
            CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;

            var env = EnvironmentHelper.Read<Env>();

            ApiCore.Api.BuildHost(
                new MongoCatalog<string, FilterQuery, MoleculeData>(env.MongoConnection, env.MongoDbname, new FilterQuery.Creator()),
                ApiCore.Api.RegisterSearch("sub", CachingSearchService.Wrap(RDKitSearchService.Substructure(env.PostgresConnection, 1000), 1000)),
                //ApiCore.Api.RegisterSearch("sup", CachingSearchService.Wrap(RDKitSearchService.Superstructure(env.PostgresConnection, 1000), 1000))
                ApiCore.Api.RegisterSearch("sim", RDKitSearchService.Similar(env.PostgresConnection, 1000))
                ).Run();
        }
    }
}
