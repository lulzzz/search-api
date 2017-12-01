using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Search.Abstractions
{
    public interface ICatalog<TId, TData>
    {
        Task<TData> ItemAsync(TId id);
        Task<IEnumerable<TData>> FindAsync(SearchQuery searchQuery, FilterQuery filters, int skip, int take);
    }

    public interface ISearchResult<TId> : IEnumerable<TId>, IDisposable { }

    public interface ISearchProvider<TId>
    {
        Task<ISearchResult<TId>> FindAsync(SearchQuery searchQuery);
    }

    //public interface ICatalog<TId, TData>
    //{
    //    Task<IEnumerable<TData>> FindAsync(SearchQuery searchQuery, FilterQuery filters, int skip, int take);
    //    Task<TData> ItemAsync(TId id);
    //}
}
