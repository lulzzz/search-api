using SearchV2.Abstractions;

namespace SearchV2.RDKit
{
    public class RDKitSimpleSearchResult : IWithReference<string>
    {
        public string Ref { get; set; }
    }
}
