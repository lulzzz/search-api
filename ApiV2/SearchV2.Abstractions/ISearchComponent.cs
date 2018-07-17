using System.Threading.Tasks;

namespace SearchV2.Abstractions
{
    public interface ISearchComponent<TSearchQuery, TSearchResult>
    {
        Task<ISearchResult<TSearchResult>> FindAsync(TSearchQuery query, int fastFetchCount);
    }
}
