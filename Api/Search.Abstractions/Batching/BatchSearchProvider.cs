using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Search.Abstractions.Batching
{
    public class BatchSearchProvider<TId> : ISearchProvider<TId>
    {
        readonly IBatchSearcher<TId> _searcher;

        readonly Dictionary<string, ISearchResult<TId>> _cache = new Dictionary<string, ISearchResult<TId>>();
        readonly object _cacheSync = new object();

        public BatchSearchProvider(IBatchSearcher<TId> searcher)
        {
            _searcher = searcher;
        }

        public Task<ISearchResult<TId>> FindAsync(SearchQuery searchQuery, int fastFetchCount)
        {
            ISearchResult<TId> result;

            lock (_cacheSync)
            {
                var key = $"{searchQuery.Type}|{searchQuery.SearchText}";

                if (_cache.ContainsKey(key))
                {
                    result = _cache[key];
                }
                else
                { 
                    _cache[key] = result = new Result(_searcher.FindAsync(searchQuery), fastFetchCount);
                }
            }

            return Task.FromResult(result);
        }

        private class Result : ISearchResult<TId>
        {
            volatile SemaphoreSlim _semaphore;
            readonly List<TId[]> loadedBatches = new List<TId[]>();

            /// <summary>
            /// mock for possible strategy in future
            /// </summary>
            /// <returns></returns>
            int GetHitLimit() => 200000;

            readonly int _batchSize;
            int GetBatchSize() => _batchSize;

            public Result(Task<IBatchedSearchResult<TId>> searchTask, int batchSize)
            {
                _batchSize = batchSize;
                _semaphore = new SemaphoreSlim(1);
                _semaphore.Wait(); // block
                searchTask.ContinueWith(async t =>
                {
                    if (t.IsCompleted)
                    {
                        var r = t.Result;
                        var leftToFetch = GetHitLimit();
                        _semaphore.Release();
                        do
                        {
                            _semaphore = new SemaphoreSlim(1);
                            var expectedSize = GetBatchSize();

                            await _semaphore.WaitAsync();
                            var batch = await r.Next(expectedSize).ConfigureAwait(false);

                            loadedBatches.Add(batch);
                            leftToFetch -= batch.Length;

                            if (batch.Length < expectedSize) break;
                            if (leftToFetch <= 0) break;

                            _semaphore.Release();
                        } while (true);

                        // must be released after exit from loop
                        _semaphore.Release();
                        _semaphore = null;

                        if (r is IDisposable disposable)
                        {
                            disposable.Dispose();
                        }
                    }
#warning must be properly exception-handled in else { ... }
                }, TaskContinuationOptions.LongRunning);
            }

            public IEnumerator<TId> GetEnumerator()
            {
                int i = 0;

                for (; i < loadedBatches.Count; i++)
                {
                    foreach (var item in loadedBatches[i])
                    {
                        yield return item;
                    }
                }

                while (_semaphore != null)
                {
                    _semaphore.Wait();
                    _semaphore.Release();
                    for (; i < loadedBatches.Count; i++)
                    {
                        foreach (var item in loadedBatches[i])
                        {
                            yield return item;
                        }
                    }
                }
            }

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }
    }
}
