using SearchV2.Abstractions;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SearchV2.Redis
{
    public class RedisCatalog<TId, TFilterQuery, TData> : ICatalogDb<TId, TFilterQuery, TData> 
        where TData : IWithReference<TId>
        where TId : IRedisCacheKey
        where TFilterQuery : IRedisFilterQuery
    {
        readonly ICacheClient _cache;

        public RedisCatalog(string connectionString, int database)
        {
            
        }

        Task<IEnumerable<TData>> ICatalogDb<TId, TFilterQuery, TData>.GetAsync(IEnumerable<TId> ids)
        {
            throw new NotImplementedException();
        }

        Task<IEnumerable<TData>> ICatalogDb<TId, TFilterQuery, TData>.GetFilteredAsync(IEnumerable<TId> ids, TFilterQuery filters)
        {
            throw new NotImplementedException();
        }

        Task<TData> ICatalogDb<TId, TFilterQuery, TData>.OneAsync(TId id)
        {
            throw new NotImplementedException();
        }
    }

    public interface IRedisFilterQuery
    {
    }

    public interface IRedisCacheKey
    {
    }
}
