using SearchV2.Generics;
using System.ComponentModel.DataAnnotations;

namespace SearchV2.RDKit
{
    public class RDKitSimilaritySearchRequest : ICacheKey
    {
        [Required]
        public string Query { get; set; }
        public double? SimilarityThreshold { get; set; } = 0.5;

        string ICacheKey.ToStringKey() => Query + SimilarityThreshold.ToString();
    }
}