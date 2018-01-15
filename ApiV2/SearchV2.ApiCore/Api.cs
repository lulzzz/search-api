using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using SearchV2.Abstractions;
using SearchV2.Generics;
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

                var sNew = searches.Select(s => {
                    var t = s._search.GetType();
                    var tSearchComponent = t.GetInterface(typeof(ISearchComponent<,,>).Name);
                    if(tSearchComponent != null)
                    {
                        var tSearchQuery = tSearchComponent.GetGenericArguments()[1];
                        var tid = tSearchComponent.GetGenericArguments()[0];
                        var tSearchResult = tSearchComponent.GetGenericArguments()[2];

                        var tStrategy = typeof(CompositeSearchService<,,,,>).MakeGenericType(tid, tSearchQuery, typeof(TFilterQuery), tSearchResult, typeof(TData));

                        return new SearchDescriptor
                        {
                            RouteSuffix = s._routeSuffix,
                            SearchService = tStrategy.GetConstructor(new[] { tCatalog, tSearchComponent }).Invoke(new object[] { catalog, s._search }),
                            TSearchQuery = tSearchQuery
                        };
                    }

                    var tSearchService = t.GetInterface(typeof(ISearchService<,>).Name);
                    if (tSearchService != null)
                    {
                        var tSearchQuery = tSearchService.GetGenericArguments()[0];
                        return new SearchDescriptor
                        {
                            RouteSuffix = s._routeSuffix,
                            SearchService = s._search,
                            TSearchQuery = tSearchQuery
                        };
                    }

                    else throw new ArgumentException("no way!");
                }).ToArray();

                sc.Add(new ServiceDescriptor(typeof(ControllerDescriptor), new ControllerDescriptor { ControllerType = ControllerBuilder.CreateControllerClass(catalog, sNew) }));
            })
            .UseStartup<Startup>()
            .Build();

        public sealed class SearchRegistration<TId>
        {
            readonly internal string _routeSuffix;
            readonly internal object _search;

            internal SearchRegistration(string routeSuffix, object search)
            {
                _routeSuffix = routeSuffix;
                _search = search;
            }

            public static implicit operator SearchRegistration<TId>(SearchRegistration sr) => new SearchRegistration<TId>(sr._routeSuffix, sr._search);
        }

        public sealed class SearchRegistration
        {
            readonly internal string _routeSuffix;
            readonly internal object _search;

            internal SearchRegistration(string routeSuffix, object search)
            {
                _routeSuffix = routeSuffix;
                _search = search;
            }
        }

        public static SearchRegistration<TId> RegisterSearch<TId, TSearchQuery, TSearchResult>(string routeSuffix, ISearchComponent<TId, TSearchQuery, TSearchResult> searchComponent) 
            where TSearchResult : IWithReference<TId>
        {
            return new SearchRegistration<TId>(routeSuffix, searchComponent);
        }
        
        public static SearchRegistration RegisterSearch<TSearchQuery, TFilterQuery>(string routeSuffix, ISearchService<TSearchQuery, TFilterQuery> searchService)
        {
            return new SearchRegistration(routeSuffix, searchService);
        }
    }
}
