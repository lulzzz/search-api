using System;
using Microsoft.AspNetCore.Hosting;
using System.Globalization;
using SearchV2.Generics;
using SearchV2.MongoDB;
using static SearchV2.ApiCore.SearchExtensions.SearchApi;
using SearchV2.ApiCore;
using SearchV2.ApiCore.SearchExtensions;

namespace SearchV2.Api.MadfastMongo
{
    using Common;
    using SearchV2.Abstractions;
    using static ActionDescriptor;
    using static SearchApi;

    public class Program
    {
        class Env
        {
            public string MongoConnection { get; set; }
            public string MongoDbname { get; set; }
            public string MadfastUrl { get; set; }
        }

        public static void Main(string[] args)
        {
            CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;

            var env = EnvironmentHelper.Read<Env>();

            ICatalogDb<string, FilterQuery, MoleculeData> catalog = new MongoCatalog<string, FilterQuery, MoleculeData>(env.MongoConnection, env.MongoDbname, new FilterQuery.Creator());
            var simSearch = CompositeSearchService.Compose(catalog, CachingSearchService.Wrap(new MadfastSimilaritySearchService(env.MadfastUrl, 1000), 1000));

            ApiCore.Api.BuildHost("molecules",
                Get("{id}", (string id) => catalog.OneAsync(id)),
                Post("sim", (SearchRequest<MadfastSearchQuery, FilterQuery> r) => Find(simSearch, r))
            ).Run();
        }
    }
}
