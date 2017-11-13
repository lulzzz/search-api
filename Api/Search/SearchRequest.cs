using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Search
{
    public enum CompareType { Eq, Gt, Lt }

    public class ComparisonDescriptor<T>
    {
        public T Value { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public CompareType CompareType { get; set; }
    }

    public class SearchRequest
    {
        public string Smiles { get; set; }
        public string Name { get; set; }
        public ComparisonDescriptor<double> Mw { get; set; }
        public ComparisonDescriptor<double> Logp { get; set; }
        public ComparisonDescriptor<int> Hba { get; set; }
        public ComparisonDescriptor<int> Hbd { get; set; }
        public ComparisonDescriptor<int> Rotb { get; set; }
        public ComparisonDescriptor<double> Tpsa { get; set; }
        public ComparisonDescriptor<double> Fsp3 { get; set; }
        public ComparisonDescriptor<int> Hac { get; set; }
    }
}