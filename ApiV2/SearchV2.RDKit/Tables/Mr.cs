using SearchV2.Abstractions;

namespace SearchV2.RDKit
{
    class Mr : IWithReference<string>
    {
        public int Id { get; set; }
        public string Ref { get; set; }
    }
}
