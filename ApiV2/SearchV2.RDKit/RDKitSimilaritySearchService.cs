using System.Threading.Tasks;
using SearchV2.Abstractions;

namespace SearchV2.RDKit
{
    internal class RDKitSimilaritySearchService : ISearchService<string, RDKitSimilaritySearchRequest, RDKitSimilaritySearchResult>
    {
        private string _connectionString;
        private int _hitLimit;

        public RDKitSimilaritySearchService(string connectionString, int hitLimit)
        {
            _connectionString = connectionString;
            _hitLimit = hitLimit;
        }

        Task<ISearchResult<RDKitSimilaritySearchResult>> ISearchService<string, RDKitSimilaritySearchRequest, RDKitSimilaritySearchResult>.FindAsync(RDKitSimilaritySearchRequest query, int fastFetchCount)
        {
            throw new System.NotImplementedException();
        }
    }
}