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
                sc.Add(new ServiceDescriptor(typeof(ICatalogDb<TId, TFilterQuery, TData>), catalog));

                foreach (var sp in searches.Select(sr => new ServiceDescriptor(sr._type, sr._searchProvider)))
                {
                    sc.Add(sp);
                    var tSearchQuery = sp.ServiceType.GetGenericArguments()[1];
                    var tid = sp.ServiceType.GetGenericArguments()[0];
                    var tSearchResult = sp.ServiceType.GetGenericArguments()[2];
                    sc.Add(new ServiceDescriptor(
                        typeof(ISearchStrategy<,>).MakeGenericType(tSearchQuery, typeof(TFilterQuery)),
                        typeof(DefaultSearchStrategy<,,,,>).MakeGenericType(tid, tSearchQuery, typeof(TFilterQuery), tSearchResult, typeof(TData)),
                        ServiceLifetime.Singleton));
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
