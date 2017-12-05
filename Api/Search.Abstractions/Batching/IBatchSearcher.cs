using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Search.Abstractions
{
    public interface IBatchSearcher<TId>
    {
        Task<IBatchedSearchResult<TId>> FindAsync(SearchQuery searchQuery);
    }
}
