using Common;
using Microsoft.AspNetCore.Hosting;
using SearchV2.Abstractions;
using SearchV2.ApiCore;
using SearchV2.ApiCore.SearchExtensions;
using SearchV2.Generics;
using SearchV2.MongoDB;
using SearchV2.RDKit;
using System.Globalization;
using Uorsy.Data;


namespace SearchV2.Api.Uorsy
{
    using static ActionDescriptor;
    using static SearchApi;
    using static CompositeSearchService;

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

            var filterCreator = new FilterQuery.Creator();

            ICatalogDb<string, FilterQuery, MoleculeData> catalog = new MongoCatalog<string, FilterQuery, MoleculeData>(env.MongoConnection, env.MongoDbname, filterCreator);

            var subSearch = Compose(catalog, CachingSearchComponent.Wrap(RDKitSearchService.Substructure(env.PostgresConnection, 1000), 1000));
            var supSearch = Compose(catalog, CachingSearchComponent.Wrap(RDKitSearchService.Superstructure(env.PostgresConnection, 1000), 1000));
            var simSearch = Compose(catalog, RDKitSearchService.Similar(env.PostgresConnection, 1000));
            var smartSearch = new MongoTextSearch<FilterQuery, MoleculeData>(env.MongoConnection, env.MongoDbname, nameof(MoleculeData.Ref), filterCreator);

            ApiCore.Api.BuildHost("molecules",
                Get("{id}", (string id) => catalog.OneAsync(id)),
                Post("sub", (SearchRequest<string, FilterQuery> r) => Find(subSearch, r)),
                Post("sup", (SearchRequest<string, FilterQuery> r) => Find(supSearch, r)),
                Post("sim", (SearchRequest<RDKitSimilaritySearchRequest, FilterQuery> r) => Find(simSearch, r)),
                Post("smart", (SearchRequest<string, FilterQuery> r) => Find(smartSearch, r))
            ).Run();
        }
    }
}
