using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SearchV2.Abstractions
{
    public interface ISearchService<TId, TSearchQuery, TSearchResult> where TSearchResult : IWithReference<TId>
    {
        Task<ISearchResult<TSearchResult>> FindAsync(TSearchQuery query, int fastFetchCount);
    }
    public interface IWithReference<TId>
    {
        TId Ref { get; }
    }
    public interface ISearchResult<TSearchResult>
    {
        Task ForEachAsync(Func<TSearchResult, Task<bool>> action);
    }
    public interface ICatalogDb<TId, TFilterQuery, TData> where TData : IWithReference<TId>
    {
        Task<IEnumerable<TData>> GetFilteredAsync(IEnumerable<TId> ids, TFilterQuery filters);
        Task<IEnumerable<TData>> GetAsync(IEnumerable<TId> ids);
        Task<TData> OneAsync(TId id);
    }
}
