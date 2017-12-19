using SearchV2.Abstractions;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SearchV2.Generics
{
    public static class CachingSearchService
    {
        public static ISearchService<TId, TSearchQuery, TSearchResult> Wrap<TId, TSearchQuery, TSearchResult>(this ISearchService<TId, TSearchQuery, TSearchResult> service)
            where TSearchResult : IWithReference<TId>
            where TSearchQuery : ICacheKey
        {
            return new CachingSearchService<TId, TSearchQuery, TSearchResult>(service);
        }
    }

    class CachingSearchService<TId, TSearchQuery, TSearchResult> : ISearchService<TId, TSearchQuery, TSearchResult> 
        where TSearchResult : IWithReference<TId>
        where TSearchQuery : ICacheKey
    {
        readonly ISearchService<TId, TSearchQuery, TSearchResult> _service;

        readonly Dictionary<string, ISearchResult<TSearchResult>> _cache = new Dictionary<string, ISearchResult<TSearchResult>>();
        readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1);

        public CachingSearchService(ISearchService<TId, TSearchQuery, TSearchResult> service)
        {
            _service = service;
        }

        async Task<ISearchResult<TSearchResult>> ISearchService<TId, TSearchQuery, TSearchResult>.FindAsync(TSearchQuery query, int fastFetchCount)
        {
            ISearchResult<TSearchResult> result;

            try
            {
                await _semaphore.WaitAsync();
                var key = query.ToStringKey();

                if (_cache.ContainsKey(key))
                {
                    result = _cache[key];
                }
                else
                {
                    _cache[key] = result = await _service.FindAsync(query, fastFetchCount);
                }
            }
            finally
            {
                _semaphore.Release();
            }

            return result;
        }
    }

    public interface ICacheKey
    {
        string ToStringKey();
    }
}
