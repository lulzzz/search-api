using MongoDB.Driver;
using SearchV2.Abstractions;

namespace SearchV2.MongoDB
{
    public static class MongoConnector
    {
        public static ICatalogDb<string, TFilterQuery, TData> CreateCatalogDb<TFilterQuery, TData>(this MongoConnector<TData> connector, IFilterCreator<TFilterQuery, TData> filterCreator)
            => new MongoCatalog<TFilterQuery, TData>(connector, filterCreator);

        public static ITextSearch<TData> CreateTextSearch<TData>(this MongoConnector<TData> connector, int hitLimit, params string[] textIndexFields)
            => new MongoTextSearch<TData>(connector, hitLimit, textIndexFields);
    }

    public sealed class MongoConnector<TData>
    {
        readonly MongoClient _client;

        public IMongoCollection<TData> Mols { get; }
        public string IdProdName { get; }

        public MongoConnector(string connectionString, string dbName, string idPropName)
        {
            IdProdName = idPropName;
            Init.ForType<TData>(idPropName);

            _client = new MongoClient(connectionString);
            var db = _client.GetDatabase(dbName);
            Mols = db.GetCollection<TData>("mols");
        }
    }
}
