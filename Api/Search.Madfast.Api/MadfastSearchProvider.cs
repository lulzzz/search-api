using Search.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Search.Madfast.Api
{
    public class MadfastSearchProvider : ISearchProvider<string>
    {
        readonly static HttpClient _httpClient = new HttpClient();
        readonly string _url;
        readonly int _hitLimit;
        readonly double _maxDissimilarity;

        public MadfastSearchProvider(string url, int hitLimit, double maxDissimilarity)
        {
            _url = url;
            _hitLimit = hitLimit;
            _maxDissimilarity = maxDissimilarity;
        }

        Task<ISearchResult<string>> ISearchProvider<string>.FindAsync(SearchQuery searchQuery, int fastFetchCount)
        {
            switch (searchQuery.Type)
            {
                case SearchType.Similar:
                    return Task.FromResult<ISearchResult<string>>(new Result(this, fastFetchCount, _hitLimit, _maxDissimilarity, searchQuery.SearchText));
                case SearchType.Exact:
                case SearchType.Substructure:
                case SearchType.Superstructure:
                case SearchType.Smart:
                    throw new NotImplementedException();
                default:
                    throw new ArgumentException();
            }
        }

        class Result : ISearchResult<string>
        {
            static readonly string[] empty = new string[0];

            volatile Task _runningTask;
            readonly List<string> loaded = new List<string>();
            readonly object _syncObj = new object();

            public IEnumerable<string> ReadyResult
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

            public Result(MadfastSearchProvider creator, int fastFetch, int hitLimit, double maxDissimilarity, string query)
            {
                _hitLimit = hitLimit;
                _runningTask = Task.Run(async () =>
                {
                    var r = await _httpClient.PostAsync(creator._url, new FormUrlEncodedContent(new Dictionary<string, string>
                    {
                            { "max-count", fastFetch.ToString() },
                            { "query", query },

                    }));


                    dynamic a = r.Content;
                    var res = ((IEnumerable<dynamic>)a.targets).Select(t => (string)t.targetid).ToList();

                    var loadNext = res.Count > fastFetch
                        ? Task.Run(async () =>
                            {
                                var r1 = await _httpClient.PostAsync(creator._url, new FormUrlEncodedContent(new Dictionary<string, string>
                                {
                                        { "max-count", hitLimit.ToString() },
                                        { "query", query }
                                }));
                                dynamic a1 = r1.Content;
                                IEnumerable<string> res1 = ((IEnumerable<dynamic>)a.targets).Select(t => (string)t.targetid);
                                lock (_syncObj)
                                {
                                    loaded.Clear();
                                    loaded.AddRange(res);
                                    _runningTask = null;
                                }
                            })
                         : null; 

                    lock (_syncObj)
                    {
                        loaded.AddRange(res);
                        _runningTask = loadNext;
                    }
                });

            }

            Task<string> Next(int index, Task runningTask)
            => runningTask.ContinueWith(t =>
               {
                   return loaded[index];
               });

            async Task ISearchResult<string>.ForEach(Func<string, Task<bool>> body)
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

            public IEnumerable<Task<string>> AsyncResult
            {
                get
                {
                    int i = 0; // count of returned elements-1
                    string[] ready;

                    do
                    {
                        lock (_syncObj)
                        {
                            var len = loaded.Count - i;
                            ready = new string[len];
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
