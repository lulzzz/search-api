using MongoDB.Driver;
using MoreLinq;
using SearchV2.Abstractions;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SearchV2.MongoDB
{
    public sealed class MongoCatalog<TId, TFilterQuery, TData> : ICatalogDb<TId, TFilterQuery, TData> where TData : IWithReference<TId>
    {
        readonly MongoClient _client;
        readonly IMongoCollection<TData> _mols;
        readonly IFilterCreator<TFilterQuery, TData> _filterCreator;
        readonly FilterDefinitionBuilder<TData> _filterBuilder = Builders<TData>.Filter;
        readonly string _idPropName;

        public MongoCatalog(string connectionString, string dbName, IFilterCreator<TFilterQuery, TData> filterCreator)
        {
            _idPropName = nameof(IWithReference<TId>.Ref);

            Init.ForType<TData>(_idPropName);

            _client = new MongoClient(connectionString);
            var db = _client.GetDatabase(dbName);
            _mols = db.GetCollection<TData>("mols");

            _filterCreator = filterCreator;
        }
        
        #region ICatalogDb

        async Task ICatalogDb<TId, TFilterQuery, TData>.AddAsync(IEnumerable<TData> items)
        {
            foreach (var batch in items.Batch(10000))
            {
                await _mols.BulkWriteAsync(batch.Select(i => new InsertOneModel<TData>(i)));
            }
        }

        async Task ICatalogDb<TId, TFilterQuery, TData>.DeleteAsync(IEnumerable<TId> ids)
        {
            foreach (var batch in ids.Batch(10000))
            {
                await _mols.DeleteManyAsync(_filterBuilder.In(i => i.Ref, ids));
            }
        }

        async Task<IEnumerable<TData>> ICatalogDb<TId, TFilterQuery, TData>.GetAsync(IEnumerable<TId> ids)
        {
            var filter = _filterBuilder.In(_idPropName, ids);
            var res = await (await _mols.FindAsync(filter)).ToListAsync();
            return res;
        }

        async Task<IEnumerable<TData>> ICatalogDb<TId, TFilterQuery, TData>.GetFilteredAsync(IEnumerable<TId> ids, TFilterQuery filters)
        {
            var filter = _filterBuilder.In(_idPropName, ids);
            if (filters != null)
            {
                filter &= _filterCreator.Create(filters);
            }
            var res = await (await _mols.FindAsync(filter)).ToListAsync();
            return res;
        }

        async Task<TData> ICatalogDb<TId, TFilterQuery, TData>.OneAsync(TId id)
        {
            return (await _mols.FindAsync(_filterBuilder.Eq(_idPropName, id))).First();
        }
        #endregion
    }
}
