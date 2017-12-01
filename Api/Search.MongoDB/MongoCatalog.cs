using Search.Abstractions;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Search.MongoDB
{
    public class MongoCatalog<TId, TFilterQuery, TData> : ICatalog<TId, TFilterQuery, TData>
    {
        readonly ISearchProvider<TId> _search;

        public MongoCatalog(ISearchProvider<TId> search)
        {

        }

        public Task<CatalogResult<TData>> FindAsync(SearchQuery searchQuery, TFilterQuery filters, int skip, int take)
        {
            throw new NotImplementedException();
        }

        public Task<TData> ItemAsync(TId id)
        {
            throw new NotImplementedException();
        }
    }
}
