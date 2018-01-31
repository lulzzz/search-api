using Common;
using Microsoft.AspNetCore.Hosting;
using SearchV2.Abstractions;
using SearchV2.ApiCore;
using SearchV2.ApiCore.SearchExtensions;
using SearchV2.Generics;
using SearchV2.InMemory;
using SearchV2.MongoDB;
using SearchV2.RDKit;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
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
            public string MongoConnection { get; set; }
            public string MongoDbname { get; set; }
            public string PostgresConnection { get; set; }
        }

        public static void Main(string[] args)
        {
            CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;

            var env = EnvironmentHelper.Read<Env>();

            var filterCreator = new FilterQuery.Creator();

            var checker = new HashSet<string>();
            var mols = ReadFromFile(@"D:\linux_share\Bases\UORSY.txt").Where(md => checker.Add(md.Ref)).ToArray();
            checker = null;

            ICatalogDb<string, FilterQuery, MoleculeData> catalog = new InMemoryCatalogDb<string, FilterQuery, MoleculeData>(mols, filter => data => true);

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

        static IEnumerable<MoleculeData> ReadFromFile(string path)
        {
            using (var reader = new StreamReader(path))
            {
                reader.ReadLine();
                while (!reader.EndOfStream)
                {
                    yield return FromString(reader.ReadLine());
                }
            }
        }

        static MoleculeData FromString(string s)
        {
            var lineItems = s.Split('\t');

            var mw = double.Parse(lineItems[3]);
            var logp = double.Parse(lineItems[4]);
            var hba = int.Parse(lineItems[5]);
            var hbd = int.Parse(lineItems[6]);
            var rotb = int.Parse(lineItems[7]);
            var tpsa = double.Parse(lineItems[8]);
            var fsp3 = double.Parse(lineItems[9]);
            var hac = int.Parse(lineItems[10]);

            var md = new MoleculeData
            {
                Smiles = lineItems[0],
                Ref = lineItems[1],
                Name = lineItems[2],
                Mw = mw,
                Logp = logp,
                Hba = hba,
                Hbd = hbd,
                Rotb = rotb,
                Tpsa = tpsa,
                Fsp3 = fsp3,
                Hac = hac
            };

            return md;
        }
    }
}
