using SearchV2.Abstractions;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SearchV2.ApiCore
{
    public interface ISearchStrategy<TSearchQuery, TFilterQuery>
    {
        Task<object> FindAsync(TSearchQuery searchQuery, TFilterQuery filters, int skip, int take);
    }

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
            var result = await _search.FindAsync(searchQuery, limitFixed);
            if (filters != null)
            {
                var ids = new List<TId>(take);
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
                            ids.Add(id.Ref);
                        }
                    }
                    i++;
                    return _trueTask;
                });

                return new { Data = await _catalog.GetFilteredAsync(ids, filters) };
            }
            else
            {
                var buffer = new List<TId>();
                var results = new List<TData>();
                var fetchCount = skip + take;
                await result.ForEachAsync(async item =>
                {
                    buffer.Add(item.Ref);
                    if (buffer.Count == fetchCount)
                    {
                        var filtered = (await _catalog.GetFilteredAsync(buffer, filters)).ToList();
                        var filteredCount = filtered.Count;
                        results.AddRange(filtered);
                        buffer.Clear();
                        if (results.Count >= limitFixed)
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
                            var max = limitFixed > 1000 ? limitFixed : 1000;
                            if (fetchCount > max)
                            {
                                fetchCount = max;
                            }
                        }
                    }
                    return true;
                });

                if (buffer.Count > 0)
                {
                    results.AddRange(await _catalog.GetFilteredAsync(buffer, filters));
                }

                return new { Data = results.Skip(skip).Take(take).ToArray() };
            }
        }
    }
}
