﻿using SearchV2.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace SearchV2.ApiCore
{
    public class DefaultSearchStrategy<TId, TSearchQuery, TFilterQuery, TSearchResult, TData> 
        : ISearchStrategy<TSearchQuery, TFilterQuery> 
        where TData : IWithReference<TId>
        where TSearchResult : IWithReference<TId>
    {
        readonly ICatalogDb<TId, TFilterQuery, TData> _catalog;
        readonly ISearchService<TId, TSearchQuery, TSearchResult> _search;

        public DefaultSearchStrategy(ICatalogDb<TId, TFilterQuery, TData> catalog, ISearchService<TId, TSearchQuery, TSearchResult> search)
        {
            _catalog = catalog;
            _search = search;
        }

        static readonly Task<bool> _falseTask = Task.FromResult(false);
        static readonly Task<bool> _trueTask = Task.FromResult(true);

        async Task<object> ISearchStrategy<TSearchQuery, TFilterQuery>.FindAsync(TSearchQuery searchQuery, TFilterQuery filters, int pageNumber, int pageSize)
        {
            int skip = (pageNumber - 1) * pageSize;
            int take = pageSize;
            var limitFixed = skip + take;
            if (filters == null)
            {
                var result = await _search.FindAsync(searchQuery, limitFixed);
                var vals = new List<TSearchResult>(take);
                var i = 0;
                await result.ForEachAsync(id =>
                {
                    if (i >= skip)
                    {
                        if (i == take + skip)
                        {
                            return _falseTask;
                        }
                        else
                        {
                            vals.Add(id);
                        }
                    }
                    i++;
                    return _trueTask;
                });
                
                var filtered = await _catalog.GetFilteredAsync(vals.Select(v => v.Ref), filters);
                return new { Data = Join(filtered, vals) };
            }
            else
            {
                var maxFetch = Math.Max(limitFixed, 1000);
                var fetchCount = Math.Min(limitFixed << 1, maxFetch);

                var searchResult = await _search.FindAsync(searchQuery, fetchCount);

                var buffer = new List<TSearchResult>();
                var allSearchResults = Enumerable.Empty<TSearchResult>();
                var data = new List<TData>();

                await searchResult.ForEachAsync(async item =>
                {
                    buffer.Add(item);
                    if (buffer.Count == fetchCount)
                    {
                        var filtered = (await _catalog.GetFilteredAsync(buffer.Select(i => i.Ref), filters)).ToList();
                        var filteredCount = filtered.Count;
                        data.AddRange(filtered);

                        if (data.Count >= limitFixed)
                        {
                            return false;
                        }

                        fetchCount = filteredCount != 0
                            ? fetchCount * fetchCount / filteredCount - fetchCount
                            : fetchCount * 10;
                        if (fetchCount < take)
                        {
                            fetchCount = take;
                        }
                        else
                        {
                            if (fetchCount > maxFetch)
                            {
                                fetchCount = maxFetch;
                            }
                        }

                        allSearchResults = allSearchResults.Union(buffer);
                        buffer = new List<TSearchResult>(fetchCount);
                    }
                    return true;
                });

                if (buffer.Count > 0)
                {
                    data.AddRange(await _catalog.GetFilteredAsync(buffer.Select(i => i.Ref), filters));
                    allSearchResults = allSearchResults.Union(buffer);
                }
                
                return new { Data = Join(data.Skip(skip).Take(take), allSearchResults) };
            }
        }

        IEnumerable<object> Join(IEnumerable<TData> filtered, IEnumerable<TSearchResult> searchResult)
        {
            var tDataProps = typeof(TData).GetProperties();
            var tSearchResultProps = typeof(TSearchResult).GetProperties(BindingFlags.Instance | BindingFlags.Public);

            var filteredDict = filtered.ToDictionary(f => f.Ref);
            foreach (var searchItem in searchResult)
            {
                if(filteredDict.ContainsKey(searchItem.Ref))
                {
                    var res = new Dictionary<string, object>();
                    foreach (var prop in tDataProps)
                    {
                        res.Add(prop.Name, prop.GetValue(filteredDict[searchItem.Ref]));
                    }

                    foreach (var prop in tSearchResultProps)
                    {
                        if (prop.Name != nameof(IWithReference<TId>.Ref))
                        {
                            res.Add(prop.Name, prop.GetValue(searchItem));
                        }
                    }
                    yield return res;
                }
            }
        }
    }
}
