using MongoDB.Driver;

namespace SearchV2.MongoDB
{
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
