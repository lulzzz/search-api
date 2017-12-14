using MongoDB.Bson;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Driver;
using Search.Abstractions;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Search.MongoDB
{
    public interface IFilterCreator<TFilter, TData>
    {
        FilterDefinition<TData> Create(TFilter filters);
    }

    public class MongoDBFilterEnricher<TId, TFilterQuery, TData> : IFilterEnricher<TId, TFilterQuery, TData>
    {
        readonly MongoClient _client;
        readonly IMongoCollection<TData> _mols;
        readonly IFilterCreator<TFilterQuery, TData> _filterCreator;
        readonly FilterDefinitionBuilder<TData> _filterBuilder = Builders<TData>.Filter;
        readonly string _idPropName;

        static MongoDBFilterEnricher()
        {
            BsonDefaults.GuidRepresentation = GuidRepresentation.Standard;
        }

        public MongoDBFilterEnricher(string connectionString, string idPropName, IFilterCreator<TFilterQuery, TData> filterCreator)
        {
            var idProp = typeof(TData).GetProperty(idPropName);
            if (idProp == null || idProp.PropertyType != typeof(TId))
            {
                throw new ArgumentException($"Type {typeof(TData).FullName} doesn't contain property with name {idPropName} and type {typeof(TId).FullName}");
            }
            var cp = new ConventionPack();
            cp.AddClassMapConvention("shit", bcm => bcm.MapIdProperty(idPropName));
            ConventionRegistry.Register("def", cp, t => typeof(TData) == t);

            _idPropName = idPropName;
            _client = new MongoClient(connectionString);
            var db = _client.GetDatabase("search_350m");
            _mols = db.GetCollection<TData>("mols");

            _filterCreator = filterCreator;
        }

        async Task<IEnumerable<TData>> IFilterEnricher<TId, TFilterQuery, TData>.FilterAndEnrich(IEnumerable<TId> ids, TFilterQuery filters)
        {
            var filter = _filterBuilder.In(_idPropName, ids) & _filterCreator.Create(filters);
            var res = await (await _mols.FindAsync(filter)).ToListAsync();
            return res;
        }

        async Task<TData> IFilterEnricher<TId, TFilterQuery, TData>.One(TId id)
        {
            return (await _mols.FindAsync(_filterBuilder.Eq(_idPropName, id))).First();
        }
    }
}
