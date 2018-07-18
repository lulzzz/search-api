using MongoDB.Driver;
using MoreLinq;
using SearchV2.Abstractions;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SearchV2.MongoDB
{
    internal sealed class MongoCatalog<TFilterQuery, TData> : ICatalogDb<string, TFilterQuery, TData>
    {
        readonly IMongoCollection<TData> _mols;
        readonly IFilterCreator<TFilterQuery, TData> _filterCreator;
        readonly FilterDefinitionBuilder<TData> _filterBuilder = Builders<TData>.Filter;
        readonly string _idPropName;

        public MongoCatalog(MongoConnector<TData> connector, IFilterCreator<TFilterQuery, TData> filterCreator)
        {
            _idPropName = connector.IdProdName;
            _mols = connector.Mols;

            Init.ForType<TData>(_idPropName);

            _filterCreator = filterCreator;
        }
        
        #region ICatalogDb
        async Task ICatalogDb<string, TFilterQuery, TData>.AddAsync(IEnumerable<TData> items)
        {
            foreach (var batch in items.Batch(10000))
            {
                await _mols.BulkWriteAsync(batch.Select(i => new InsertOneModel<TData>(i)));
            }
        }

        async Task ICatalogDb<string, TFilterQuery, TData>.DeleteAsync(IEnumerable<string> ids)
        {
            foreach (var batch in ids.Batch(10000))
            {
                await _mols.DeleteManyAsync(_filterBuilder.In(_idPropName, ids));
            }
        }

        async Task<IEnumerable<TData>> ICatalogDb<string, TFilterQuery, TData>.GetAsync(IEnumerable<string> ids)
        {
            var filter = _filterBuilder.In(_idPropName, ids);
            var res = await (await _mols.FindAsync(filter)).ToListAsync();
            return res;
        }

        async Task<IEnumerable<TData>> ICatalogDb<string, TFilterQuery, TData>.GetFilteredAsync(IEnumerable<string> ids, TFilterQuery filters)
        {
            var filter = _filterBuilder.In(_idPropName, ids);
            if (filters != null)
            {
                filter &= _filterCreator.Create(filters);
            }
            var res = await (await _mols.FindAsync(filter)).ToListAsync();
            return res;
        }

        async Task<TData> ICatalogDb<string, TFilterQuery, TData>.OneAsync(string id)
        {
            return (await _mols.FindAsync(_filterBuilder.Eq(_idPropName, id))).FirstOrDefault();
        }
        #endregion
        
    }
}
