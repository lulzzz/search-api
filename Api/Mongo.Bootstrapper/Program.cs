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
        public int? Hba { get; set; }
        public int? Hbd { get; set; }
        public int? Rotb { get; set; }
        public double? Tpsa { get; set; }
        public double Fsp3 { get; set; }
        public int Hac { get; set; }
    }

    class Program
    {
        static int? intTryParse(string src) => int.TryParse(src, out int i) ? (int?)i : null;
        static double? doubleTryParse(string src) => double.TryParse(src, out double i) ? (double?)i : null;

        static void Main(string[] args)
        {
            CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
            var cp = new ConventionPack();
            BsonDefaults.GuidRepresentation = GuidRepresentation.Standard;

            cp.AddClassMapConvention("shit", bcm => bcm.MapIdProperty(nameof(MoleculeData.IdNumber)));
            ConventionRegistry.Register("def", cp, t => typeof(MoleculeData) == t);
            MongoClient client = new MongoClient("mongodb://search_350m:a87s9h2uf3F2@mongo:27017/search_350m");
            var db = client.GetDatabase("search_350m");
            var filter = new BsonDocument("name", "mols");

            var collections = db.ListCollections(new ListCollectionsOptions { Filter = filter });
            if (collections.Any())
            {
                db.DropCollection("mols");
            }
            db.CreateCollection("mols");
            var col = db.GetCollection<MoleculeData>("mols");
            using (StreamWriter @out = new StreamWriter("out.txt"))
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
                var bad = new List<string>();
                while (!@in.EndOfStream)
                {
                    lineNum++;
                    var line = @in.ReadLine();
                    var lineItems = line.Split('\t');
                    //if (loadedIDs.Add(line[1]))
                    //{
                    try
                    {
                        var mw = doubleTryParse(lineItems[2]);
                        var logp = doubleTryParse(lineItems[3]);
                        var hba = intTryParse(lineItems[4]);
                        var hbd = intTryParse(lineItems[5]);
                        var rotb = intTryParse(lineItems[6]);
                        var tpsa = doubleTryParse(lineItems[7]);
                        var fsp3 = doubleTryParse(lineItems[9]);
                        var hac = intTryParse(lineItems[8]);

                        if (!(mw.HasValue && logp.HasValue && hba.HasValue && hbd.HasValue && rotb.HasValue && tpsa.HasValue && fsp3.HasValue && hac.HasValue))
                        {
                            var md = new MoleculeData
                            {
                                Smiles = lineItems[0],
                                IdNumber = lineItems[1],
                                Mw = mw.Value,
                                Logp = logp.Value,
                                Hba = hba,
                                Hbd = hbd,
                                Rotb = rotb,
                                Tpsa = tpsa,
                                Fsp3 = fsp3.Value,
                                Hac = hac.Value
                            };

                            batch.Add(md);
                            counter++;
                        }
                        else
                        {
                            Console.WriteLine($"bad line {lineNum}: {line}");
                        }
                    }
                    catch (Exception e)
                    {
                        @out.WriteLine(line);
                        Console.WriteLine(e);
                        Console.WriteLine("---------------------------------------------------------");
                        Console.WriteLine($"in line {lineNum}");
                    }

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
                if (batch.Count != 0)
                {
                    col.InsertMany(batch);
                }
            }
            Console.Read();
        }
    }
}
