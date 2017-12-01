using Search.Abstractions;
using System;
using System.Threading.Tasks;

namespace Search.Caching
{
    public class CachedSearchProvider<TId> : ISearchProvider<TId>
    {
        readonly ISearchProvider<TId> _search;

        public CachedSearchProvider(ISearchProvider<TId> underlyingProvider)
        {
            _search = underlyingProvider;
        }

        public Task<ISearchResult<TId>> FindAsync(SearchQuery searchQuery, int fastFetchCount)
        {
            
        }
    }
}
