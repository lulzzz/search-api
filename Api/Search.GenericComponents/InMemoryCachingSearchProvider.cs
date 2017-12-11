using Search.Abstractions;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Search.GenericComponents
{
    public class InMemoryCachingSearchProvider<TId> : ISearchProvider<TId>
    {
        readonly ISearchProvider<TId> _inner;

        readonly Dictionary<string, ISearchResult<TId>> _cache = new Dictionary<string, ISearchResult<TId>>();
        readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1);

        public InMemoryCachingSearchProvider(ISearchProvider<TId> inner)
        {
            _inner = inner;
        }

        async Task<ISearchResult<TId>> ISearchProvider<TId>.FindAsync(SearchQuery searchQuery, int fastFetchCount)
        {
            ISearchResult<TId> result;

            await _semaphore.WaitAsync();
            var key = $"{searchQuery.Type}|{searchQuery.SearchText}";

            if (_cache.ContainsKey(key))
            {
                result = _cache[key];
            }
            else
            {
                _cache[key] = result = await _inner.FindAsync(searchQuery, fastFetchCount);
            }
            _semaphore.Release();

            return result;
        }
    }
}
