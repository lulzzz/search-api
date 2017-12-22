using SearchV2.Abstractions;

namespace SearchV2.RDKit
{
    public class RDKitSearchResult : IWithReference<string>
    {
        public string Ref { get; set; }
    }
}
