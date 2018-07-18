using System.Collections.Generic;
using System.Threading.Tasks;

namespace SearchV2.Abstractions
{
    public interface ICatalogDb<TId, TFilterQuery, TData>
    {
        Task<IEnumerable<TData>> GetFilteredAsync(IEnumerable<TId> ids, TFilterQuery filters);
        Task<IEnumerable<TData>> GetAsync(IEnumerable<TId> ids);
        Task<TData> OneAsync(TId id);

        Task AddAsync(IEnumerable<TData> items);
        Task DeleteAsync(IEnumerable<TId> ids);
    }
}
