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
    using System;
    using System.Threading.Tasks;

    class Program
    {
        class Env
        {
            public string PathToCsvSource { get; set; }
            public string PathToPriceCats { get; set; }
            public string PostgresConnection { get; set; }
            public string SmtpAddress { get; set; }
            public string SmtpPort { get; set; }
            public string EmailFrom { get; set; }
            public string InquiryNotificationEmail { get; set; }
            public string SmtpUsername { get; internal set; }
            public string SmtpPassword { get; internal set; }
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

            var inquiryService = new InquiryService(env.SmtpAddress, int.Parse(env.SmtpPort), env.SmtpUsername, env.SmtpPassword, env.EmailFrom, env.InquiryNotificationEmail);

            Func<InquiryRequest, Task<InquiryData>> MapFromRequest = async r1 =>
            {
                var r = r1.Body;
                return new InquiryData
                {
                    Comments = r.Comments,
                    Email = r.Email,
                    Institution = r.Institution,
                    Name = r.Name,
                    InquiryItems = await Task.WhenAll(r.InquiryItems.Select(async item =>
                    {
                        var catalogItem = await catalog.OneAsync(item.Key);
                        var priceCategory = priceCategories.Single(pc => pc.Id == catalogItem.PriceCategory);
                        string amount, formattedPrice;
                        if (string.IsNullOrEmpty(item.Value.Amount))
                        {
                            if (item.Value.AmountId.HasValue)
                            {
#warning item.Value.AmountId MUST be a key, not an index in enumeration
                                var pc = priceCategory.WeightsAndPrices.Skip(item.Value.AmountId.Value).First();
                                amount = pc.Weight;
                                formattedPrice = pc.Price;
                            }
                            else
                            {
                                throw new Exception("bad amountId");
                            }
                        }
                        else
                        {
                            amount = item.Value.Amount;
#warning do something with "POA"
                            formattedPrice = priceCategory.WeightsAndPrices.SingleOrDefault(wp => wp.Weight == amount)?.Price ?? "POA"; 
                        }

                        return new InquiryData.InquiryItem
                        {
                            Id = item.Key,
                            Amount = amount,
                            FormattedPrice = formattedPrice
                        };
                    }))
                };
            };

            ApiCore.Api.BuildHost("",
                Get("molecules/{id}", (string id) => catalog.OneAsync(id)),
#warning needs smiles->inchi conversion
                Post("molecules/exact", (SearchRequest<string> r) => catalog.OneAsync(r.Query.Search).ContinueWith(t => new { Data = new[] { t.Result } })),
                Post("molecules/sub", (SearchRequest<string, FilterQuery> r) => Find(subSearch, r)),
                Post("molecules/sim", (SearchRequest<RDKitSimilaritySearchRequest, FilterQuery> r) => Find(simSearch, r)),
#warning needs CAS, inchikey and id? validation and smiles->inchi conversion
                Post("molecules/text", async (SearchRequest<IEnumerable<string>> r) => (await catalog.GetAsync(r.Query.Search)).Skip((r.PageNumber.Value - 1) * r.PageSize.Value).Take(r.PageSize.Value)),
#warning should be in different controller and should be fitted with molecules as a dictionary according to openapi
                Get("price-categories", () => priceCategories),
                //Post("make-ids-list", (IEnumerable<string> ids) => makeFileResponse(ids)),
                //Post("get-sdf", (IEnumerable<string> ids) => makeFileResponse(createSdf(ids))),
                Post("inquire", async (InquiryRequest r) => await inquiryService.Inquire(await MapFromRequest(r)))
            ).Run();
        }
    }
}
