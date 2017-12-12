using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;

namespace Mongo.Bootstrapper
{
    public class MoleculeData
    {
        public string IdNumber { get; set; }

        public string Smiles { get; set; }

        public double Mw { get; set; }
        public double Logp { get; set; }
        public int Hba { get; set; }
        public int Hbd { get; set; }
        public int Rotb { get; set; }
        public double Tpsa { get; set; }
        public double Fsp3 { get; set; }
        public int Hac { get; set; }
    }

    class Program
    {
        static void Main(string[] args)
        {
            CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
            var cp = new ConventionPack();
            BsonDefaults.GuidRepresentation = GuidRepresentation.Standard;

            cp.AddClassMapConvention("shit", bcm => bcm.MapIdProperty(nameof(MoleculeData.IdNumber)));
            ConventionRegistry.Register("def", cp, t => typeof(MoleculeData) == t);
            MongoClient client = new MongoClient("mongodb://localhost:27017");
            var db = client.GetDatabase("simsearch");
            var filter = new BsonDocument("name", "mols");

            var collections = db.ListCollections(new ListCollectionsOptions { Filter = filter });
            if (collections.Any())
            {
                db.DropCollection("mols");
            }
            db.CreateCollection("mols");
            var col = db.GetCollection<MoleculeData>("mols");
            using (StreamReader @in = new StreamReader(@"D:\rdb_all_337M.smi"))
            {
                const int batchSize = 5000000;
                var batch = new List<MoleculeData>(batchSize);
                var counter = 0;
                //for (int i = 0; i < 203000000; i++)
                //{
                //    @in.ReadLine();
                //}
                var lineNum = 0;
                while (!@in.EndOfStream)
                {
                    lineNum++;
                    var line = @in.ReadLine().Split('\t');
                    //if (loadedIDs.Add(line[1]))
                    //{
                    try
                    {
                        var md = new MoleculeData
                        {
                            Smiles = line[0],
                            IdNumber = line[1],
                            Mw = double.Parse(line[2]),
                            Logp = double.Parse(line[3]),
                            Hba = int.Parse(line[4]),
                            Hbd = int.Parse(line[5]),
                            Rotb = int.Parse(line[6]),
                            Tpsa = double.Parse(line[7]),
                            Fsp3 = double.Parse(line[9]),
                            Hac = int.Parse(line[8])
                        };
                        batch.Add(md);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                        Console.WriteLine("---------------------------------------------------------");
                        Console.WriteLine($"in line {lineNum}");
                    }
                        
                        counter++;
                        if (counter == batchSize)
                        {
                            try
                            {
                                col.InsertMany(batch);
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine(e);
                                Console.WriteLine("---------------------------------------------------------");
                                Console.WriteLine($"failed on batch before line line {lineNum}");
                            }
                            batch.Clear();
                            counter = 0;
                        }
                    //}
                    //else
                    //{
                    //    Console.WriteLine($"{line[1]} is duplicate");
                    //}
                }
                if(batch.Count != 0)
                {
                    col.InsertMany(batch);
                }
            }
            Console.Read();
        }
    }
}
