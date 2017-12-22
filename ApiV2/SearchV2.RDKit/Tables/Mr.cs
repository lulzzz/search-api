using SearchV2.Abstractions;

namespace SearchV2.RDKit
{
    public class Mr : IWithReference<string>
    {
        public int Id { get; set; }
        public string Ref { get; set; }
    }
}
