using System.Threading.Tasks;

namespace Search.Abstractions.Batching
{
    public interface IBatchSearcher<TId>
    {
        Task<IBatchedSearchResult<TId>> FindAsync(SearchQuery searchQuery);
    }
}
