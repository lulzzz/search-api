using System.Collections.Generic;
using System.Threading.Tasks;

namespace Search.Abstractions
{
    public interface ICatalog<TId, TFilterQuery, TData>
    {
        Task<TData> ItemAsync(TId id);
        Task<CatalogResult<TData>> FindAsync(SearchQuery searchQuery, TFilterQuery filters, int skip, int take);
    }
}
