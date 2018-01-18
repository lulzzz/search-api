using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace SearchV2.Generics
{
    public static class CsvOps
    {
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
    }
}
