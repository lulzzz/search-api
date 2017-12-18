using System.Collections.Generic;
using System.Threading.Tasks;

namespace SearchV2.Abstractions
{
    public interface ICatalogDb<TId, TFilterQuery, TData> where TData : IWithReference<TId>
    {
        Task<IEnumerable<TData>> GetFilteredAsync(IEnumerable<TId> ids, TFilterQuery filters);
        Task<IEnumerable<TData>> GetAsync(IEnumerable<TId> ids);
        Task<TData> OneAsync(TId id);
    }
}
