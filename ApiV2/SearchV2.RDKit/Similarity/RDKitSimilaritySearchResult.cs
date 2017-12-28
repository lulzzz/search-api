using SearchV2.Abstractions;

namespace SearchV2.RDKit
{
    public class RDKitSimilaritySearchResult : IWithReference<string>
    {
        public string Ref { get; set; }
        public double Similarity { get; set; }
    }
}