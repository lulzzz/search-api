using Search.Abstractions;
using Search.Abstractions.Batching;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Search.GenericComponents
{
    public class BatchSearchProvider<TId> : ISearchProvider<TId>
    {
        readonly IBatchSearcher<TId> _searcher;

#warning caching should be separated
        readonly Dictionary<string, ISearchResult<TId>> _cache = new Dictionary<string, ISearchResult<TId>>();
        readonly object _cacheSync = new object();

        readonly int _hitLimit;

        public BatchSearchProvider(IBatchSearcher<TId> searcher, int hitLimit)
        {
            _searcher = searcher;
            _hitLimit = hitLimit;
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
                    _cache[key] = result = new Result(_searcher.FindAsync(searchQuery), fastFetchCount, _hitLimit);
                }
            }

            return Task.FromResult(result);
        }

        private class Result : ISearchResult<TId>
        {
            volatile Task _runningTask;
            readonly List<TId> loaded = new List<TId>();

            readonly object _syncObj = new object();

            public bool HasReadyResult => _runningTask == null;
            public IEnumerable<TId> ReadyResult
            {
                get
                {
                    lock (_syncObj)
                    {
                        if (_runningTask == null)
                        {
                            return loaded;
                        }
                    }
                    throw new InvalidOperationException("The result is not ready for synchronous consumption");
                }
            }

            /// <summary>
            /// mock for possible strategy in future
            /// </summary>
            /// <returns></returns>
            readonly int _hitLimit;
            int GetHitLimit() => _hitLimit;

            readonly int _batchSize;
            int GetBatchSize() => _batchSize;
            
            public Result(Task<IBatchedSearchResult<TId>> searchTask, int batchSize, int hitLimit)
            {
                _batchSize = batchSize;
                _hitLimit = hitLimit;
                _runningTask = Task.Run(async ()=>
                {
                    var leftToFetch = GetHitLimit();
                    var r = await searchTask;
#warning lacks proper error handling
                    async Task LoadBatchAsync()
                    {
                        var expectedSize = GetBatchSize();
                        var batch = await r.Next(expectedSize).ConfigureAwait(false);
                        lock (_syncObj)
                        {
                            loaded.AddRange(batch);
                            leftToFetch -= batch.Length;
                            if (batch.Length < expectedSize || leftToFetch <= 0)
                            {
                                _runningTask = null;
                                if (r is IDisposable disposable)
                                {
                                    disposable.Dispose();
                                }
                            }
                            else
                            {
                                _runningTask = LoadBatchAsync();
                            }
                        }
                    }

                    await LoadBatchAsync();
                });
            }

            Task<TId> Next(int index, Task runningTask)
            {
                return runningTask.ContinueWith(t =>
                {
#warning lock, rly? there must be another way!
                    lock (_syncObj)
                    {
                        return loaded[index];
                    }
                });
            }

            async Task ISearchResult<TId>.ForEach(Func<TId, Task<bool>> body)
            {
                if (HasReadyResult)
                {
                    foreach (var item in ReadyResult)
                    {
                        var @continue = await body(item);
                        if (!@continue)
                        {
                            return;
                        }
                    }
                }
                else
                {
                    foreach (var itemTask in AsyncResult)
                    {
                        var item = await itemTask;
                        var @continue = await body(item);
                        if (!@continue)
                        {
                            return;
                        }
                    }
                }
            }

            public IEnumerable<Task<TId>> AsyncResult
            {
                get
                {
                    int i = 0; // count of returned elements-1
                    TId[] ready;

                    do
                    {
                        lock (_syncObj)
                        {
                            ready = loaded.Skip(i).ToArray();
                        }

                        foreach (var item in ready)
                        {
                            i++;
                            yield return Task.FromResult(item);
                        }

                        Task running = null;

                        lock (_syncObj) // synchronizedCheck to determine if anything new has been loaded
                        {
                            if (i == loaded.Count) // if nothing new was loaded, then remember current running task to avoid race
                            {
                                running = _runningTask;
                            }
                        }

                        if(running != null) // if running task was remembered in the lock, then no ready elements left and we yield awaiting task
                        {
                            yield return Next(i, running);
                            i++;
                            break; // 
                        }
                    } while (true);

                    do
                    {
                        Task running;
                        lock (_syncObj)
                        {
                            if (_runningTask == null)
                            {
                                break;
                            }
                            running = _runningTask;
                        }
                        yield return Next(i, running);
                        i++;
                    } while (true);
                }
            }
        }
    }
}
