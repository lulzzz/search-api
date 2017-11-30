using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Search.Abstractions
{
    public interface ISearchProvider
    {
        Task<MoleculeData> ItemAsync(string id);
        Task<IEnumerable<MoleculeData>> FindAsync(SearchQuery searchQuery, FilterQuery filters, int skip, int take);
    }

    public abstract class SearchResult<TId> : IEnumerable<TId>, IDisposable
    {

    }

    public interface ISearchProvider<TId>
    {
        Task<IEnumerable<TId>>
    }
}
