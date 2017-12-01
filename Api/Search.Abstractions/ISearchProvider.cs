using System.Threading.Tasks;

namespace Search.Abstractions
{
    public interface ISearchProvider<TId>
    {
        Task<ISearchResult<TId>> FindAsync(SearchQuery searchQuery);
    }
}
