using System.Threading.Tasks;

namespace SearchV2.Abstractions
{
    public interface ISearchService<TId, TSearchQuery, TSearchResult> where TSearchResult : IWithReference<TId>
    {
        Task<ISearchResult<TSearchResult>> FindAsync(TSearchQuery query, int fastFetchCount);
    }
}
