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

        class MadfastQueryResult
        {
            public IEnumerable<Target> Targets { get; set; }

            public class Target
            {
                public double Dissimilarity { get; set; }
                public string Targetid { get; set; }
            }
        }

        Task<ISearchResult<MadfastResultItem>> ISearchService<string, MadfastSearchQuery, MadfastResultItem>.FindAsync(MadfastSearchQuery query, int fastFetchCount)
        => Task.FromResult<ISearchResult<MadfastResultItem>>(
            new AsyncResult<MadfastResultItem>(
                updateState => 
                    Task.Run(() =>
                        {
                            async Task LoadAndUpdate(int count, Func<int, Task> nextFactory)
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
                                        .Select(t => new MadfastResultItem { Ref = t.Targetid, Similarity = 1.0 - t.Dissimilarity })
                                        .ToList()
                                    : Enumerable.Empty<MadfastResultItem>();
                    
                                updateState(nextFactory?.Invoke(res.Count()), res);
                            }

                            return LoadAndUpdate(fastFetchCount, count => count < _hitLimit ? LoadAndUpdate(_hitLimit, null) : null);
                        }
                    )
            )
        );
    }
}
