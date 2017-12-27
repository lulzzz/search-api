using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using SearchV2.Abstractions;
using System;
using System.Linq;
using System.Reflection;

namespace SearchV2.ApiCore
{
    public static class Api
    {
        internal class ControllerDescriptor
        {
            public TypeInfo ControllerType { get; set; }
        }
        public static IWebHost BuildHost<TId, TFilterQuery, TData>(ICatalogDb<TId, TFilterQuery, TData> catalog, params SearchRegistration<TId>[] searches) where TData : IWithReference<TId>
        => WebHost.CreateDefaultBuilder()
            .CaptureStartupErrors(true)
            .ConfigureServices(sc =>
            {
                var tCatalog = typeof(ICatalogDb<TId, TFilterQuery, TData>);
                sc.Add(new ServiceDescriptor(tCatalog, catalog));

                foreach(var s in searches)
                {
                    var tSearchService = s._type;

                    var tSearchQuery = tSearchService.GetGenericArguments()[1];
                    var tid = tSearchService.GetGenericArguments()[0];
                    var tSearchResult = tSearchService.GetGenericArguments()[2];

                    var tStrategy = typeof(DefaultSearchStrategy<,,,,>).MakeGenericType(tid, tSearchQuery, typeof(TFilterQuery), tSearchResult, typeof(TData));
                    s._strategy = tStrategy.GetConstructor(new[] { tCatalog, tSearchService }).Invoke(new object[] { catalog, s._searchProvider });
                }
                sc.Add(new ServiceDescriptor(typeof(ControllerDescriptor), new ControllerDescriptor { ControllerType = ControllerBuilder.CreateControllerClass(catalog, searches) }));
            })
            .UseStartup<Startup>()
            .Build();

        public sealed class SearchRegistration<TId>
        {
            internal readonly object _searchProvider;
            internal readonly Type _type;
            internal readonly string _routeSuffix;
            internal object _strategy;

            internal SearchRegistration(string routeSuffix, object searchProvider, Type type)
            {
                _searchProvider = searchProvider;
                _type = type;
                _routeSuffix = routeSuffix;
            }
        }

        public static SearchRegistration<TId> RegisterSearch<TId, TSearchQuery, TSearchResult>(string routeSuffix, ISearchService<TId, TSearchQuery, TSearchResult> searchService) where TSearchResult : IWithReference<TId>
        {
            return new SearchRegistration<TId>(routeSuffix, searchService, typeof(ISearchService<TId, TSearchQuery, TSearchResult>));
        }
    }
}
