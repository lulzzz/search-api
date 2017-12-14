using MongoDB.Bson;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Driver;
using SearchV2.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SearchV2.Api.MadfastMongo
{
    public interface IFilterCreator<TFilter, TData>
    {
        FilterDefinition<TData> Create(TFilter filters);
    }

    public class MongoCatalog<TId, TFilterQuery, TData> : ICatalogDb<TId, TFilterQuery, TData> where TData : IWithReference<TId>
    {
        readonly MongoClient _client;
        readonly IMongoCollection<TData> _mols;
        readonly IFilterCreator<TFilterQuery, TData> _filterCreator;
        readonly FilterDefinitionBuilder<TData> _filterBuilder = Builders<TData>.Filter;
        readonly string _idPropName;

        static MongoCatalog()
        {
            BsonDefaults.GuidRepresentation = GuidRepresentation.Standard;
        }

        public MongoCatalog(string connectionString, string dbName, IFilterCreator<TFilterQuery, TData> filterCreator)
        {
            _idPropName = nameof(IWithReference<TId>.Ref);

            var cp = new ConventionPack();
            cp.AddClassMapConvention("ids", bcm => bcm.MapIdProperty(_idPropName));
            ConventionRegistry.Register("idsPack", cp, t => typeof(TData) == t);

            _client = new MongoClient(connectionString);
            var db = _client.GetDatabase(dbName);
            _mols = db.GetCollection<TData>("mols");

            _filterCreator = filterCreator;
        }

        async Task<IEnumerable<TData>> ICatalogDb<TId, TFilterQuery, TData>.GetAsync(IEnumerable<TId> ids)
        {
            var filter = _filterBuilder.In(_idPropName, ids);
            var res = await(await _mols.FindAsync(filter)).ToListAsync();
            return res;
        }

        async Task<IEnumerable<TData>> ICatalogDb<TId, TFilterQuery, TData>.GetFilteredAsync(IEnumerable<TId> ids, TFilterQuery filters)
        {
            var filter = _filterBuilder.In(_idPropName, ids) & _filterCreator.Create(filters);
            var res = await(await _mols.FindAsync(filter)).ToListAsync();
            return res;
        }

        async Task<TData> ICatalogDb<TId, TFilterQuery, TData>.OneAsync(TId id)
        {
            return (await _mols.FindAsync(_filterBuilder.Eq(_idPropName, id))).First();
        }
    }
}
