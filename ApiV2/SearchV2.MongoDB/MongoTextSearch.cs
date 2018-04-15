using MongoDB.Driver;
using SearchV2.Abstractions;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SearchV2.MongoDB
{
    public class MongoTextSearch<TFilterQuery, TData> : ISearchService<string, TFilterQuery> where TData : class
    {
        readonly MongoClient _client;
        readonly IMongoCollection<TData> _mols;
        readonly IFilterCreator<TFilterQuery, TData> _filterCreator;
        readonly string _idPropName;

        public MongoTextSearch(string connectionString, string dbName, string idPropName, IFilterCreator<TFilterQuery, TData> filterCreator)
        {
            _idPropName = idPropName;
            Init.ForType<TData>(idPropName);

            _client = new MongoClient(connectionString);
            var db = _client.GetDatabase(dbName);
            _mols = db.GetCollection<TData>("mols");
            _filterCreator = filterCreator;
        }

        async Task<ResponseBody> ISearchService<string, TFilterQuery>.FindAsync(string searchQuery, TFilterQuery filters, int skip, int take)
        {
            throw new NotImplementedException();
            //var filter = Builders<TData>.Filter.Text(searchQuery);
            //if (filters != null)
            //{
            //    filter &= _filterCreator.Create(filters);
            //}
            //var res = await _mols.Find(filter)
            //    .Project(Builders<TData>.Projection.MetaTextScore("score"))
            //    .Sort(Builders<TData>.Sort.MetaTextScore("score"))
            //    .Skip(skip)
            //    .Limit(take)
            //    .ToListAsync();

            //return res.Select(r => r.ToDictionary());
        }
    }
}
