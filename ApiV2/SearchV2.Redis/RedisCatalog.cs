using SearchV2.Abstractions;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SearchV2.Redis
{
    public class RedisCatalog<TFilterQuery, TData> : ICatalogDb<string, TFilterQuery, TData> 
        where TData : IWithReference<string>
    {
        readonly IDatabase _db;
        readonly ISerializer<TData> _serializer;
        readonly Func<TFilterQuery, Func<TData, bool>> _createFilterDelegate;

        public RedisCatalog(string connectionString, int database, ISerializer<TData> serializer, Func<TFilterQuery, Func<TData, bool>> createFilterDelegate)
        {
            _db = ConnectionMultiplexer.Connect(connectionString).GetDatabase(database);
            _serializer = serializer;
            _createFilterDelegate = createFilterDelegate;
        }

        async Task<IEnumerable<TData>> ICatalogDb<string, TFilterQuery, TData>.GetAsync(IEnumerable<string> ids)
            => (await _db.StringGetAsync(ids.Cast<RedisKey>().ToArray()))
            .Select(v => _serializer.Deserialize(v))
            .ToArray();

        async Task<IEnumerable<TData>> ICatalogDb<string, TFilterQuery, TData>.GetFilteredAsync(IEnumerable<string> ids, TFilterQuery filters)
        {
            var fulfillsFilter = _createFilterDelegate(filters);

            return (await _db.StringGetAsync(ids.Cast<RedisKey>().ToArray()))
                .Select(v => _serializer.Deserialize(v))
                .Where(fulfillsFilter)
                .ToArray();
        }

        async Task<TData> ICatalogDb<string, TFilterQuery, TData>.OneAsync(string id)
            => _serializer.Deserialize(await _db.StringGetAsync(id));
    }
}
