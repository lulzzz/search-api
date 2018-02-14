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
    using SearchResult = ISearchResult<MadfastResultItem>;

    public class MadfastSimilaritySearchService : ISearchComponent<string, MadfastSearchQuery, MadfastResultItem>
    {
        readonly static HttpClient _httpClient = new HttpClient();
        readonly string _url;
        readonly int _hitLimit;

        public MadfastSimilaritySearchService(string url, int hitLimit)
        {
            _url = url;
            _hitLimit = hitLimit;
        }

        class MadfastQueryResult
        {
            public IEnumerable<Target> Targets { get; set; }

            public class Target
            {
                public double Dissimilarity { get; set; }
                public string Targetid { get; set; }
            }
        }

        Task<SearchResult> ISearchComponent<string, MadfastSearchQuery, MadfastResultItem>.FindAsync(MadfastSearchQuery query, int fastFetchCount)
        => Task.FromResult<SearchResult>(
            new AsyncResult<MadfastResultItem>(
                pushState => 
                    Task.Run(() =>
                        {
                            async Task LoadAndUpdate(int count, int skip, Func<int, Task> nextFactory)
                            {
                                var r = await _httpClient.PostAsync(_url, new FormUrlEncodedContent(new Dictionary<string, string>
                                {
                                    { "max-count", count.ToString() },
                                    { "query", query.Query },
                                    { "max-dissimilarity", (1.0 - query.SimilarityThreshold).ToString() }
                                }));

                                var res = r.IsSuccessStatusCode
                                    ? JsonConvert
                                        .DeserializeObject<MadfastQueryResult>(await r.Content.ReadAsStringAsync())
                                        .Targets
                                        .Skip(skip)
                                        .Select(t => new MadfastResultItem { Ref = t.Targetid, Similarity = 1.0 - t.Dissimilarity })
                                        .ToList()
                                    : Enumerable.Empty<MadfastResultItem>();
                    
                                pushState(nextFactory?.Invoke(res.Count()), res);
                            }

                            return LoadAndUpdate(fastFetchCount, 0, count => count < _hitLimit ? LoadAndUpdate(_hitLimit, count, null) : null);
                        }
                    )
            )
        );
    }
}
