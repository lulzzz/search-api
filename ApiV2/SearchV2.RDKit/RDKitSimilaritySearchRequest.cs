using SearchV2.Generics;

namespace SearchV2.RDKit
{
    public class RDKitSimilaritySearchRequest : ICacheKey
    {
        public string Query { get; set; }
        public double SimilarityThreshold { get; set; }

        string ICacheKey.ToStringKey() => Query + SimilarityThreshold.ToString();
    }
}