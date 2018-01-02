using System.Threading.Tasks;

namespace SearchV2.Abstractions
{
    public interface ISearchComponent<TId, TSearchQuery, TSearchResult> where TSearchResult : IWithReference<TId>
    {
        Task<ISearchResult<TSearchResult>> FindAsync(TSearchQuery query, int fastFetchCount);
    }
}
