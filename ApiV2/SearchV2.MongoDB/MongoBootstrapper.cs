using MongoDB.Bson;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Driver;
using MoreLinq;
using SearchV2.Abstractions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace SearchV2.MongoDB
{

    public static class MongoBootstrapper
    {
        readonly static HashSet<Type> initialized = new HashSet<Type>();

        public static IEnumerable<TDoc> LoadFromCsv<TDoc, TId>(string path, Func<string, TDoc> factory)
        {
            using (StreamReader @in = new StreamReader(path))
            using (StreamWriter @out = new StreamWriter("bad.txt"))
            {
                const int batchSize = 50000;
                var batch = new List<TDoc>(batchSize);
                var lineNum = 0;
                var bad = new List<string>();
                Task running = Task.CompletedTask;
                while (!@in.EndOfStream)
                {
                    lineNum++;
                    var line = @in.ReadLine();
                    TDoc doc;
                    try
                    {
                        doc = factory(line);
                    }
                    catch (Exception e)
                    {
                        @out.WriteLine(line);
                        Console.WriteLine(e);
                        Console.WriteLine("---------------------------------------------------------");
                        Console.WriteLine($"in line {lineNum}");
                        continue;
                    }
                    yield return doc;
                }
            }

        }

        public static async Task Load<TDoc, TId>(string connectionString, string dbName, IEnumerable<TDoc> items, bool drop = false) where TDoc : IWithReference<TId>
        {
            var opt = new InsertManyOptions { IsOrdered = false };
            var tDoc = typeof(TDoc);
            if (initialized.Add(tDoc))
            {
                var cp = new ConventionPack();
                cp.AddClassMapConvention(tDoc.Name + "ids", bcm => bcm.MapIdProperty(nameof(IWithReference<TId>.Ref)));
                ConventionRegistry.Register(tDoc.Name + "idsPack", cp, t => tDoc == t);
            }

            var client = new MongoClient(connectionString);
            var db = client.GetDatabase(dbName);
            const string collectionName = "mols";
            var collections = db.ListCollections(new ListCollectionsOptions { Filter = new BsonDocument("name", "mols") });
            if (collections.Any())
            {
                if (drop)
                {
                    db.DropCollection(collectionName);
                    db.CreateCollection(collectionName);
                }
            }
            else
            {
                db.CreateCollection(collectionName);
            }
            
            var mols = db.GetCollection<TDoc>(collectionName).WithWriteConcern(WriteConcern.Unacknowledged);

            var batches = items.Batch(50000);

            var running = Task.CompletedTask;

            foreach (var b in batches)
            {
                await running;
                running = mols.InsertManyAsync(b, opt);
            }
            await running;
        }
    }
}
