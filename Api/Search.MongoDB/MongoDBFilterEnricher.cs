using Search.Abstractions;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Search.MongoDB
{
    public class MongoDBFilterEnricher<TId, TFilterQuery, TData> : IFilterEnricher<TId, TFilterQuery, TData>
    {
        Task<IEnumerable<TData>> IFilterEnricher<TId, TFilterQuery, TData>.FilterAndEnrich(IEnumerable<TId> ids, TFilterQuery filters)
        {
            throw new NotImplementedException();
        }
    }
}
