using Search.Abstractions;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Collections;
using System.Threading;
using System.Linq;

namespace Search.Caching
{
    public class CachedSearchProvider<TId> : ISearchProvider<TId>
    {
        readonly ISearchProvider<TId> _search;
        readonly Dictionary<SearchQuery, RunningResult> _runningResults = new Dictionary<SearchQuery, RunningResult>();
        readonly Dictionary<SearchQuery, CompleteResult> _completedResults = new Dictionary<SearchQuery, CompleteResult>();
        readonly object syncRoot = new object();



        public CachedSearchProvider(ISearchProvider<TId> underlyingProvider)
        {
            _search = underlyingProvider;
        }

        public async Task<ISearchResult<TId>> FindAsync(SearchQuery searchQuery, int fastFetchCount)
        {
            lock (syncRoot)
            {
                if (_completedResults.ContainsKey(searchQuery))
                {
                    return _completedResults[searchQuery];
                }

                if (_runningResults.ContainsKey(searchQuery))
                {
                    return _runningResults[searchQuery];
                }
            }

            var result = await _search.FindAsync(searchQuery, fastFetchCount);

            lock (syncRoot)
            {
                if (!_runningResults.ContainsKey(searchQuery))
                {
                    var runningResult = new RunningResult(result, fastFetchCount, r => {
                        lock (syncRoot)
                        {
                            _runningResults.Remove(searchQuery);
                            _completedResults.Add(searchQuery, new CompleteResult(r.Items));
                        }
                    });
                    _runningResults.Add(searchQuery, runningResult);
                    return runningResult;
                }
                else
                {
                    return _runningResults[searchQuery];
                }
            }
        }

        private class CompleteResult : ISearchResult<TId>
        {
            readonly TId[] _values;
            public CompleteResult(IEnumerable<TId> values)
            {
                _values = values.ToArray();
            }

            public void Dispose() { }

            public IEnumerator<TId> GetEnumerator()
            {
                foreach (var item in _values)
                {
                    yield return item;
                }
            }

            IEnumerator IEnumerable.GetEnumerator() => _values.GetEnumerator();
        }

        private class RunningResult : ISearchResult<TId>
        {
            readonly List<TId> _results = new List<TId>();
            volatile bool _ready;

            public RunningResult(ISearchResult<TId> result, int fastFetch, Action<RunningResult> readyCallback)
            {

                Task.Run(() =>
                {
                    var enumerator = result.GetEnumerator();
                    while (fastFetch > 0 && enumerator.MoveNext())
                    {
                        fastFetch--;
                        _results.Add(enumerator.Current);
                    }

                    while (enumerator.MoveNext())
                    {
                        lock (_results)
                        {
                            _results.Add(enumerator.Current);
                        }
                    }
                    _ready = true;
                    result.Dispose();
                    readyCallback(this);
                });
            }

            public void Dispose() { }

            public IEnumerator<TId> GetEnumerator()
            {
                if (_ready)
                {
                    return _results.GetEnumerator();
                }
                else
                {
                    return Enumerate();
                }
            }

            public TId[] Items
            {
                get
                {
                    if (!_ready)
                    {
                        throw new InvalidOperationException();
                    }
                    return _results.ToArray();
                }
            }

            int AvailableCount()
            {
                lock (_results)
                {
                    return _results.Count;
                }
            }

            public IEnumerator<TId> Enumerate()
            {
                var c = 0;
                while (c < AvailableCount())
                {
                    yield return _results[c];
                }
                while (!_ready)
                {
                    Thread.Sleep(100);
                    while (c < AvailableCount())
                    {
                        yield return _results[c];
                    }
                }
            }

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }
    }

    
}
