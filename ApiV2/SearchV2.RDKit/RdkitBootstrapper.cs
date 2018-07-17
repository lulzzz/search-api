using SearchV2.Abstractions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace SearchV2.RDKit
{
    class RdkitBootstrapper
    {
        public static int Main(string[] args)
        {
            if (args[0].IndexOf("ref:") != 0 || !int.TryParse(args[0].Substring("ref:".Length), out int refColNumber))
            {
                return Terminate("ref column number not provided");
            }
            if (args[1].IndexOf("smiles:") != 0 || !int.TryParse(args[0].Substring("smiles:".Length), out int smilesColNumber))
            {
                return Terminate("smiles column number not provided");
            }
            if (args[2].IndexOf("db_host:") != 0)
            {
                return Terminate("db_host not provided");
            }
            if (args[3].IndexOf("db_name:") != 0)
            {
                return Terminate("db_name not provided");
            }
            if (!File.Exists(args[4]))
            {
                return Terminate("filepath not provided");
            }

            var running = MainAsync(refColNumber, smilesColNumber, args[2].Substring("db_host:".Length), args[3].Substring("db_name:".Length), args[4]);
            running.Wait();
            if (running.IsCompletedSuccessfully)
            {
                return 0;
            }
            else
            {
                Console.WriteLine(running.Exception);
                return -1;
            }
        }

        public static async Task MainAsync(int refColNumber, int smilesColNumber, string dbHost, string dbName, string filepath)
        {
            var index = await RDKitIndex.OpenOrInit(dbHost, dbName);
            using (var streamReader = new StreamReader(filepath))
            {
                await streamReader.ReadLineAsync();
                var buffer = new List<SearchIndexItem>(100000);
                while (!streamReader.EndOfStream)
                {
                    var line = (await streamReader.ReadLineAsync()).Split('\t');
                    buffer.Add(new SearchIndexItem { Ref = line[refColNumber], Smiles = line[smilesColNumber] });
                    if (buffer.Count == 100000)
                    {
                        await index.Add(buffer);
                        buffer.Clear();
                    }
                }
                if (buffer.Count > 0)
                {
                    await index.Add(buffer);
                }
            }
        }

        static int Terminate(string message)
        {
            Console.WriteLine(message);
            Console.WriteLine("usage: dotnet RdkitBootstrapper.dll ref:[colnumber] smiles:[colnumber] db_host:[hostname] db_name:[dbName] [csv_filepath]");
            Environment.Exit(-1);
            return -1;
        }
    }
}
