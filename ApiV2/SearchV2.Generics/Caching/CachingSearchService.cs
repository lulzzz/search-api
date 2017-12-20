using SearchV2.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SearchV2.Generics
{
    public static class CachingSearchService
    {
        public static ISearchService<TId, TSearchQuery, TSearchResult> Wrap<TId, TSearchQuery, TSearchResult>(this ISearchService<TId, TSearchQuery, TSearchResult> service, int maxCount)
            where TSearchResult : IWithReference<TId>
            where TSearchQuery : ICacheKey
        {
            return new CachingSearchService<TId, TSearchQuery, TSearchResult>(service, maxCount);
        }
    }

    class CachingSearchService<TId, TSearchQuery, TSearchResult> : ISearchService<TId, TSearchQuery, TSearchResult> 
        where TSearchResult : IWithReference<TId>
        where TSearchQuery : ICacheKey
    {
        readonly ISearchService<TId, TSearchQuery, TSearchResult> _service;
        readonly int _maxCount;

        readonly Dictionary<string, (ISearchResult<TSearchResult> data, DateTime lastRequestDate)> _cache = new Dictionary<string, (ISearchResult<TSearchResult>, DateTime)>();
        readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1);

        public CachingSearchService(ISearchService<TId, TSearchQuery, TSearchResult> service, int maxCount)
        {
            _service = service;
            _maxCount = maxCount;
        }

        async Task<ISearchResult<TSearchResult>> ISearchService<TId, TSearchQuery, TSearchResult>.FindAsync(TSearchQuery query, int fastFetchCount)
        {
            try
            {
                await _semaphore.WaitAsync();
                var key = query.ToStringKey();
                
                if (_cache.TryGetValue(key, out var cacheItem))
                {
                    var result = cacheItem.data;
                    if (!(result is IAsyncOperation resWithStatus && resWithStatus.Status == AsyncOperationStatus.Faulted))
                    {
                        _cache[key] = (result, DateTime.Now);
                        return result;
                    }
                }

                var res = await _service.FindAsync(query, fastFetchCount);
                _cache[key] = (res, DateTime.Now);
                return res;
            }
            finally
            {
                _semaphore.Release();
                var _ = Task.Run(async () =>
                {
                    try
                    {
                        await _semaphore.WaitAsync();
                        if (_cache.Count > _maxCount)
                        {
                            _cache.Remove(
                                _cache.Aggregate(
                                    (key: "", date: DateTime.Now),
                                    (acc, item) => item.Value.lastRequestDate <= acc.date ? (item.Key, item.Value.lastRequestDate) : acc
                                ).key
                            );
                        }
                    }
                    finally
                    {
                        _semaphore.Release();
                    }
                }).Status;
            }
        }
    }
}
