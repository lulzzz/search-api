using SearchV2.ApiCore;
using SearchV2.ApiCore.SearchExtensions;
using SearchV2.Generics;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SearchV2.Api.Uorsy
{
    public class PriceCategory
    {
        public class WeightPricePair
        {
            public string Weight { get; set; }
            public string Price { get; set; }
        }
        public int Id { get; set; }
        public string ShippedWithin { get; set; }
        public IEnumerable<WeightPricePair> WeightsAndPrices { get; set; }

        public static IEnumerable<PriceCategory> ReadFromFile(string path)
        {
            const int commonHeaderLength = 2;
            using (var reader = new StreamReader(path))
            {
                var weights = reader.ReadLine().Split('\t').Skip(commonHeaderLength).ToArray();
                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine().Split('\t');
                    yield return new PriceCategory
                    {
                        Id = int.Parse(line[0]),
                        ShippedWithin = line[1],
                        WeightsAndPrices = weights.Select((w, i) => new PriceCategory.WeightPricePair { Weight = w, Price = line[i + commonHeaderLength] }).ToArray()
                    };
                }
            }
        }
    }
}
