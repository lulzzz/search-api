using System.Collections.Generic;
using System.Threading.Tasks;

namespace Search.Abstractions
{
    public interface IFilterEnricher<TId, TFilterQuery, TData>
    {
        Task<IEnumerable<TData>> FilterAndEnrich(IEnumerable<TId> ids, TFilterQuery filters);
        Task<TData> One(TId id);
    }
}
