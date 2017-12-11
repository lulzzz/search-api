using Search.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Search.GenericComponents
{
    public interface IFilterQuery
    {
        bool Empty { get; }
    }
    public class GenericCatalog<TId, TFilterQuery, TData> : ICatalog<TId, TFilterQuery, TData> where TFilterQuery : IFilterQuery
    {
        readonly ISearchProvider<TId> _search;
        readonly IFilterEnricher<TId, TFilterQuery, TData> _filterEnricher;

        public GenericCatalog(ISearchProvider<TId> search, IFilterEnricher<TId, TFilterQuery, TData> filterEnricher)
        {
            _search = search;
            _filterEnricher = filterEnricher;
        }

        readonly Task<bool> _falseTask = Task.FromResult(false);
        readonly Task<bool> _trueTask = Task.FromResult(true);

        public async Task<CatalogResult<TData>> FindAsync(SearchQuery searchQuery, TFilterQuery filters, int skip, int take)
        {
            var limitFixed = skip + take;
            var result = await _search.FindAsync(searchQuery, limitFixed);
            if (filters != null && filters.Empty)
            {
                var ids = new List<TId>(take);
                var i = 0;
                await result.ForEach(id =>
                {
                    if (i >= skip)
                    {
                        if (i == take + skip)
                        {
                            return _falseTask;
                        }
                        else
                        {
                            ids.Add(id);
                        }
                    }
                    i++;
                    return _trueTask;
                });
                
                return new CatalogResult<TData> { Data = await _filterEnricher.FilterAndEnrich(ids, filters) };
            }
            else
            {
                var buffer = new List<TId>();
                var results = new List<TData>();
                var fetchCount = skip + take;
                await result.ForEach(async item =>
                {
                    buffer.Add(item);
                    if (buffer.Count == fetchCount)
                    {
                        var filtered = (await _filterEnricher.FilterAndEnrich(buffer, filters)).ToList();
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
                    results.AddRange(await _filterEnricher.FilterAndEnrich(buffer, filters));
                }

                return new CatalogResult<TData> { Data = results.Skip(skip).Take(take).ToArray() };
            }
        }

        public Task<TData> ItemAsync(TId id) => _filterEnricher.One(id);
    }
}
