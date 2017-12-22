using SearchV2.Abstractions;

namespace SearchV2.RDKit
{
    public class RDKitSubstructureSearchResult : IWithReferenceInternal, IWithReference<string>
    {
        public string Ref { get; set; }
    }
}