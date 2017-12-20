using Newtonsoft.Json;
using SearchV2.Abstractions;
using SearchV2.Generics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace SearchV2.Api.MadfastMongo
{
    public class MadfastSimilaritySearchService : ISearchService<string, MadfastSearchQuery, MadfastResultItem>
    {
        readonly static HttpClient _httpClient = new HttpClient();
        readonly string _url;
        readonly int _hitLimit;

        public MadfastSimilaritySearchService(string url, int hitLimit)
        {
            _url = url;
            _hitLimit = hitLimit;
        }

        Task<ISearchResult<MadfastResultItem>> ISearchService<string, MadfastSearchQuery, MadfastResultItem>.FindAsync(MadfastSearchQuery query, int fastFetchCount)
        {
            return Task.FromResult<ISearchResult<MadfastResultItem>>(new Result(this, fastFetchCount, _hitLimit, 1.0 - query.SimilarityThreshold, query.Query));
        }

        class Result : ISearchResult<MadfastResultItem>, IAsyncOperation
        {
            volatile Task _runningTask;
            readonly List<MadfastResultItem> loaded = new List<MadfastResultItem>();
            readonly object _syncObj = new object();

            async Task<IEnumerable<MadfastResultItem>> Query(string url, string query, double maxDissimilarity, int count)
            {
                var r = await _httpClient.PostAsync(url, new FormUrlEncodedContent(new Dictionary<string, string>
                    {
                        { "max-count", count.ToString() },
                        { "query", query },
                        { "max-dissimilarity", maxDissimilarity.ToString() }
                    }));

                return r.IsSuccessStatusCode
                    ? JsonConvert
                        .DeserializeObject<MadfastQueryResult>(await r.Content.ReadAsStringAsync())
                        .Targets
                        .Select(t => new MadfastResultItem { Ref = t.Targetid, Similarity = 1.0 - t.Dissimilarity })
                        .ToList()
                    : Enumerable.Empty<MadfastResultItem>();
            }

            public Result(MadfastSimilaritySearchService creator, int fastFetch, int hitLimit, double maxDissimilarity, string query)
            {
                _runningTask = Task.Run(async () =>
                {
                    var res = await Query(creator._url, query, maxDissimilarity, fastFetch);

                    lock (_syncObj)
                    {
                        loaded.AddRange(res);
                        _runningTask = res.Count() < hitLimit
                            ? Task.Run(async () =>
                            {
                                var res1 = await Query(creator._url, query, maxDissimilarity, hitLimit);
                                lock (_syncObj)
                                {
                                    loaded.Clear();
                                    loaded.AddRange(res1);
                                    _runningTask = null;
                                }
                            })
                            : null;
                    }
                });

            }

#warning needs to be refactored to separate three parts - cross-threading interaction, loading strategy, querying of target system
            #region ISearchResult<MadfastResultItem>.ForEachAsync
            async Task ISearchResult<MadfastResultItem>.ForEachAsync(Func<MadfastResultItem, Task<bool>> body)
            {   
                if (_runningTask == null)
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
                        var @continue = item != null && await body(item);
                        if (!@continue)
                        {
                            return;
                        }
                    }
                }
            }

            public IEnumerable<MadfastResultItem> ReadyResult
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

            Task<MadfastResultItem> TryNext(int index, Task runningTask)
                => runningTask.ContinueWith(t =>
                {
                    lock (_syncObj)
                    {
                        if (_runningTask != null && _runningTask.IsFaulted)
                        {
                            throw new InvalidOperationException("Loading of results was faulted");
                        }
                        return index < loaded.Count
                            ? loaded[index]
                            : null;
                    }
                });

            public IEnumerable<Task<MadfastResultItem>> AsyncResult
            {
                get
                {
                    int i = 0; // count of returned elements-1
                    MadfastResultItem[] ready;

                    do
                    {
                        lock (_syncObj)
                        {
                            var len = loaded.Count - i;
                            ready = new MadfastResultItem[len];
                            loaded.CopyTo(i, ready, 0, len);
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

                        if (running != null) // if running task was remembered in the lock, then no ready elements left and we yield awaiting task
                        {
                            yield return TryNext(i, running);
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
                        yield return TryNext(i, running);
                        i++;
                    } while (true);

                    while (i < loaded.Count)
                    {
                        yield return Task.FromResult(loaded[i]);
                        i++;
                    }
                }
            }

            AsyncOperationStatus IAsyncOperation.Status
            {
                get
                {
                    if(_runningTask == null)
                    {
                        return AsyncOperationStatus.Finished;
                    }
                    if(_runningTask.IsFaulted)
                    {
                        return AsyncOperationStatus.Faulted;
                    }
                    return AsyncOperationStatus.Running;
                }
            }
            #endregion


            class MadfastQueryResult
            {
                public IEnumerable<Target> Targets { get; set; }

                public class Target
                {
                    public double Dissimilarity { get; set; }
                    public string Targetid { get; set; }
                }
            }
        }
    }
}
