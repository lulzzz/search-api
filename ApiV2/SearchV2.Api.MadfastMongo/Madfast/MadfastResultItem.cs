using SearchV2.Abstractions;

namespace SearchV2.Api.MadfastMongo
{
    public class MadfastResultItem : IWithReference<string>
    {
        public string Ref { get; set; }
        public double Similarity { get; set; }
    }
}
