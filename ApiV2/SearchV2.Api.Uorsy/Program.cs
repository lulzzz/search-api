using Common;
using Microsoft.AspNetCore.Hosting;
using SearchV2.Abstractions;
using SearchV2.ApiCore;
using SearchV2.ApiCore.SearchExtensions;
using SearchV2.Generics;
using SearchV2.InMemory;
using SearchV2.RDKit;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
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
            public string PathToCsvSource { get; set; }
            public string PathToPriceCats { get; set; }
            public string PostgresConnection { get; set; }
        }

        const int hitLimit = 200;

        public static void Main(string[] args)
        {
            CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;

            var env = EnvironmentHelper.Read<Env>();

            var priceCategories = PriceCategory.ReadFromFile(env.PathToPriceCats).ToArray();

            var checker = new HashSet<string>();
            var mols = MoleculeData.ReadFromFile(env.PathToCsvSource).Where(md => checker.Add(md.Ref)).ToArray();
            
            ICatalogDb<string, FilterQuery, MoleculeData> catalog = new InMemoryCatalogDb<FilterQuery, MoleculeData>(mols, FilterQuery.CreateFilterDelegate,
                md => md.Ref,
                md => md.Smiles,
                md => md.Cas,
                md => md.InChIKey);

            var subSearch = Compose(catalog, CachingSearchComponent.Wrap(RDKitSearchService.Substructure(env.PostgresConnection, hitLimit), 1000));
            var simSearch = Compose(catalog, RDKitSearchService.Similar(env.PostgresConnection, hitLimit));

            ApiCore.Api.BuildHost("molecules",
                Get("{id}", (string id) => catalog.OneAsync(id)),
#warning needs smiles->inchi conversion
                Post("exact", (SearchRequest<string> r) => catalog.OneAsync(r.Query.Search).ContinueWith(t => new { Data = new[] { t.Result } })),
                Post("sub", (SearchRequest<string, FilterQuery> r) => Find(subSearch, r)),
                Post("sim", (SearchRequest<RDKitSimilaritySearchRequest, FilterQuery> r) => Find(simSearch, r)),
#warning needs CAS, inchikey and id? validation and smiles->inchi conversion
                Post("text", async (SearchRequest<IEnumerable<string>> r) => (await catalog.GetAsync(r.Query.Search)).Skip((r.PageNumber.Value - 1) * r.PageSize.Value).Take(r.PageSize.Value)),
#warning should be in different controller and should be fitted with molecules as a dictionary according to openapi
                Get("price-categories", () => priceCategories)
            ).Run();
        }
    }
}
