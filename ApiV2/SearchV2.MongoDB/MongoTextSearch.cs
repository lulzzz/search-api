using MongoDB.Driver;
using SearchV2.Abstractions;
using System;
using System.Threading.Tasks;

namespace SearchV2.MongoDB
{
    class MongoTextSearch<TSearchQuery, TData> : ISearchService<string, TSearchQuery>
    {
        readonly MongoClient _client;
        readonly IMongoCollection<TData> _mols;
        readonly FilterDefinitionBuilder<TData> _filterBuilder = Builders<TData>.Filter;
        static readonly string _idPropName = nameof(IWithReference<string>.Ref);

        public MongoTextSearch(string connectionString, string dbName)
        {
            _client = new MongoClient(connectionString);
            var db = _client.GetDatabase(dbName);
            _mols = db.GetCollection<TData>("mols");
        }

        public Task<object> FindAsync(string searchQuery, TSearchQuery filters, int skip, int take)
        {
            throw new NotImplementedException();
        }
    }
}
