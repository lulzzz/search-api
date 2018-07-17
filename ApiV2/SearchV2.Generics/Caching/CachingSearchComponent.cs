using SearchV2.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SearchV2.Generics
{
    public static class CachingSearchComponent
    {
        public static ISearchComponent<TSearchQuery, TSearchResult> Wrap<TSearchQuery, TSearchResult>(this ISearchComponent<TSearchQuery, TSearchResult> service, int maxCount)
            where TSearchQuery : ICacheKey
        {
            return new CachingSearchService<TSearchQuery, TSearchResult>(service, maxCount);
        }

        public static ISearchComponent<string, TSearchResult> Wrap<TSearchResult>(this ISearchComponent<string, TSearchResult> service, int maxCount)
        {
            return new CachingSearchService<string, TSearchResult>(service, maxCount);
        }
    }

    class CachingSearchService<TSearchQuery, TSearchResult> : ISearchComponent<TSearchQuery, TSearchResult>
    {
        readonly ISearchComponent<TSearchQuery, TSearchResult> _service;
        readonly int _maxCount;

        readonly Func<TSearchQuery, string> _getStringKey;

        readonly Dictionary<string, (ISearchResult<TSearchResult> data, DateTime lastRequestDate)> _cache = new Dictionary<string, (ISearchResult<TSearchResult>, DateTime)>();
        readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1);

        public CachingSearchService(ISearchComponent<TSearchQuery, TSearchResult> service, int maxCount)
        {
            if(typeof(TSearchQuery).GetInterface(nameof(ICacheKey)) != null)
            {
                _getStringKey = v => ((ICacheKey)v).ToStringKey();
            }
            else if (typeof(TSearchQuery) == typeof(string))
            {
                _getStringKey = (TSearchQuery v) => v as string;
            }
            else
            {
                throw new ArgumentException("TSearchQuery must be either string or ICacheKey");
            }
            _service = service;
            _maxCount = maxCount;
        }

        async Task<ISearchResult<TSearchResult>> ISearchComponent<TSearchQuery, TSearchResult>.FindAsync(TSearchQuery query, int fastFetchCount)
        {
            try
            {
                await _semaphore.WaitAsync();
                var key = _getStringKey(query);
                
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
