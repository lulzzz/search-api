using Common;
using System;
using System.Globalization;
using System.Linq;
using Uorsy.Data;

namespace SearchV2.Uorsy.MongoBootstrapper
{
    class Program
    {

        static MoleculeData FromString(string s)
        {
            var lineItems = s.Split('\t');

            var mw = double.Parse(lineItems[3]);
            var logp = double.Parse(lineItems[4]);
            var hba = int.Parse(lineItems[5]);
            var hbd = int.Parse(lineItems[6]);
            var rotb = int.Parse(lineItems[7]);
            var tpsa = double.Parse(lineItems[8]);
            var fsp3 = double.Parse(lineItems[9]);
            var hac = int.Parse(lineItems[10]);

            var md = new MoleculeData
            {
                Smiles = lineItems[0],
                Ref = lineItems[1],
                Name = lineItems[2],
                Mw = mw,
                Logp = logp,
                Hba = hba,
                Hbd = hbd,
                Rotb = rotb,
                Tpsa = tpsa,
                Fsp3 = fsp3,
                Hac = hac
            };

            return md;
        }

        class Env
        {
            public string MongoConnection { get; set; }
            public string MongoDbname { get; set; }
            public string CsvPath { get; set; }
        }

        static void Main(string[] args)
        {
            CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;

            var env = EnvironmentHelper.Read<Env>();

            MongoDB.MongoBootstrapper.Load<MoleculeData, string>(
                connectionString: env.MongoConnection,
                dbName: env.MongoDbname, 
                items: MongoDB.MongoBootstrapper.LoadFromCsv<MoleculeData, string>(
                    path: env.CsvPath,
                    factory: FromString).Skip(2399993),
                drop: false).Wait();

            
        }
    }
}
