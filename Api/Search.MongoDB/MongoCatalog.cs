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
            // check cache and return if not empty
            // notify cache about this query
            // start task reading from ISearchProvider to interlocked Queue<TId>. task should periodically update cache?
            // filter results until [skip+take] is not reached
            // return selected [take] results and, if task is completed, count
            // when task is finished, it sets cache record as ready
            throw new NotImplementedException();
        }

        public Task<TData> ItemAsync(TId id)
        {
            throw new NotImplementedException();
        }
    }
}
