using SearchV2.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace SearchV2.Api.MadfastMongo
{
    public class MadfastSearchQuery
    {
        public string Query { get; set; }
        public double SimilarityThreshold { get; set; }
    }
    public class MadfastResultItem : IWithReference<string>
    {
        public string Ref { get; set; }
        public double Similarity { get; set; }
    }
    public class MadfastSimilaritySearchProvider : ISearchService<string, MadfastSearchQuery, MadfastResultItem>
    {
        readonly static HttpClient _httpClient = new HttpClient();
        readonly string _url;
        readonly int _hitLimit;
        readonly double _maxDissimilarity;

        public MadfastSimilaritySearchProvider(string url, int hitLimit, double maxDissimilarity)
        {
            _url = url;
            _hitLimit = hitLimit;
            _maxDissimilarity = maxDissimilarity;
        }

        Task<ISearchResult<MadfastResultItem>> ISearchService<string, MadfastSearchQuery, MadfastResultItem>.FindAsync(MadfastSearchQuery query, int fastFetchCount)
        {
            throw new NotImplementedException();
        }

        class Result : ISearchResult<MadfastResultItem>
        {
            static readonly string[] empty = new string[0];

            volatile Task _runningTask;
            readonly List<MadfastResultItem> loaded = new List<MadfastResultItem>();
            readonly object _syncObj = new object();

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

            /// <summary>
            /// mock for possible strategy in future
            /// </summary>
            /// <returns></returns>
            readonly int _hitLimit;
            int GetHitLimit() => _hitLimit;

            readonly int _batchSize;
            int GetBatchSize() => _batchSize;

            public Result(MadfastSimilaritySearchProvider creator, int fastFetch, int hitLimit, double maxDissimilarity, string query)
            {
                _hitLimit = hitLimit;
                _runningTask = Task.Run(async () =>
                {
                    var r = await _httpClient.PostAsync(creator._url, new FormUrlEncodedContent(new Dictionary<string, string>
                    {
                            { "max-count", fastFetch.ToString() },
                            { "query", query },

                    }));


                    dynamic a = Newtonsoft.Json.JsonConvert.DeserializeObject(await r.Content.ReadAsStringAsync());
                    var res = ((IEnumerable<dynamic>)a.targets).Select(t => new MadfastResultItem { Ref = t.targetid, Similarity = 1.0 - t.dissimilarity }).ToList();

                    lock (_syncObj)
                    {
                        loaded.AddRange(res);
                        _runningTask = res.Count < hitLimit
                            ? Task.Run(async () =>
                            {
                                var r1 = await _httpClient.PostAsync(creator._url, new FormUrlEncodedContent(new Dictionary<string, string>
                                        {
                                                { "max-count", hitLimit.ToString() },
                                                { "query", query }
                                        }));
                                dynamic a1 = Newtonsoft.Json.JsonConvert.DeserializeObject(await r1.Content.ReadAsStringAsync()); //await r1.Content.ReadAsStreamAsync();
                                IEnumerable<MadfastResultItem> res1 = ((IEnumerable<dynamic>)a1.targets).Select(t => new MadfastResultItem { Ref = t.targetid, Similarity = 1.0 - t.dissimilarity });
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

            Task<MadfastResultItem> Next(int index, Task runningTask)
                => runningTask.ContinueWith(t =>
                {
                    return loaded[index];
                });

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
                        var @continue = await body(item);
                        if (!@continue)
                        {
                            return;
                        }
                    }
                }
            }

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

                    while (i < loaded.Count)
                    {
                        yield return Task.FromResult(loaded[i]);
                        i++;
                    }
                }
            }
        }
    }
}
