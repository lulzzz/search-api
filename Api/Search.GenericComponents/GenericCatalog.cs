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
            var limit = skip + take;
            var result = await _search.FindAsync(searchQuery, limit);
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

                await result.ForEach(async item =>
                {
                    buffer.Add(item);
                    if (buffer.Count == limit)
                    {
                        var filtered = (await _filterEnricher.FilterAndEnrich(buffer, filters)).ToList();
                        var filteredCount = filtered.Count;
                        results.AddRange(filtered);
                        if (results.Count >= skip + take)
                        {
                            buffer.Clear();
                            return false;
                        }
                        limit = filteredCount != 0
                            ? limit * limit / filteredCount - limit
                            : limit * 10;
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
