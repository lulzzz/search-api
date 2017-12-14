using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using SearchV2.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SearchV2.ApiCore
{
    public static class Api
    {
        public static IWebHost BuildHost<TId, TFilterQuery, TData>(ICatalogDb<TId, TFilterQuery, TData> catalog, params SearchRegistration<TId>[] searches) where TData : IWithReference<TId>
        => WebHost.CreateDefaultBuilder()
            .ConfigureServices(sc =>
            {
                sc.Add(new ServiceDescriptor(typeof(ICatalogDb<TId, TFilterQuery, TData>), catalog));
                foreach(var sp in searches.Select(sr => new ServiceDescriptor(sr._type, sr._searchProvider)))
                {
                    sc.Add(sp);
                }
            })
            .UseStartup<Startup>()
            .Build();

        public sealed class SearchRegistration<TId>
        {
            internal readonly object _searchProvider;
            internal readonly Type _type;

            internal SearchRegistration(object searchProvider, Type type)
            {
                _searchProvider = searchProvider;
                _type = type;
            }
        }

        public static SearchRegistration<TId> RegisterSearch<TId, TSearchQuery, TSearchResult>(ISearchService<TId, TSearchQuery, TSearchResult> searchService) where TSearchResult : IWithReference<TId>
        {
            return new SearchRegistration<TId>(searchService, typeof(ISearchService<TId, TSearchQuery, TSearchResult>));
        }
    }
}
